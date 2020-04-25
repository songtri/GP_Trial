using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class Character : MonoBehaviour
{
	[SerializeField]
	private float HeavyDmgThreshold = 0.2f;
	[SerializeField]
	private float TurnSpeed = 90f;  // angle(degree) per second
	[SerializeField]
	public bool IgnoreTurnSpeed = false;

	[HideInInspector]
	public CharacterStats Stats = null;
	private Animator animator;

	private const float BodyRemoveWaitTime = 5f;

	public bool IsDead => CurrentHP <= 0;
	public int CurrentHP
	{
		get => Stats?.CurrentHP ?? 0;
		set
		{
			Stats.CurrentHP = value;
			if (value <= 0)
				PrepareDie();
		}
	}
	private float lastCombatTime = 0f;
	[HideInInspector]
	public AnimationState MoveType = AnimationState.Running;
	private float remainRotation = 0f;
	public bool IsTurning { get => remainRotation != 0f; }

	public event Func<Character, bool> OnAttack;
	public event Action OnEnterCombat;
	public event Action<int, float> OnDamaged;
	public event Action OnOutOfCombat;
	public event Action<Character> OnFinishTarget;
	public event Action<Character> OnDie;

	public bool IsMovable
	{
		get
		{
			var animState = animator.GetCurrentAnimatorStateInfo(0);
			return animState.IsName("Idle") || animState.IsName("Walk") || animState.IsName("Running") || animState.IsName("Sprint");
		}
	}

	private void Awake()
	{
		Stats = GetComponent<CharacterStats>();
		if (Stats == null)
			throw new MissingComponentException("Characterstat");
		animator = GetComponent<Animator>();
		if (animator == null)
			throw new MissingComponentException("Animator");
	}

	private void Update()
	{
		if (lastCombatTime < Player.minSecondToBeOutOfCombat)
		{
			lastCombatTime += Time.deltaTime;
			if (lastCombatTime >= Player.minSecondToBeOutOfCombat)
				OnOutOfCombat?.Invoke();
		}
		//Debug.Log("LastCombatTime: " + lastCombatTime);

		if (remainRotation != 0f)
		{
			float rotation;
			if (remainRotation > 0)
				rotation = TurnSpeed * Time.deltaTime;
			else
				rotation = -TurnSpeed * Time.deltaTime;
			if (Math.Sign(remainRotation - rotation) != Math.Sign(remainRotation))
				rotation = remainRotation;
			remainRotation -= rotation;
			transform.Rotate(0f, rotation, 0f);
		}
	}

	public void Init()
	{
		animator.SetTrigger(AnimationStateTrigger.Init.ToString());
		Stats.InitStat();
		transform.localScale = Vector3.one;
		lastCombatTime = 0f;
	}

	public void Move(Vector2 direction, bool move)
	{
		if (move && IsMovable)
		{
			//Debug.Log($"direction : {direction.x}, {direction.y}");
			Vector3 forward = transform.forward;
			Vector3 movement = Vector3.zero;
			if(direction.x != 0)
				movement = new Vector3(forward.x * direction.x, 0f, forward.z * direction.x) * Time.deltaTime;
			else if(direction.y != 0)
				movement = new Vector3(forward.z * direction.y, 0f, - forward.x * direction.y) * Time.deltaTime;
			var actualMovement = movement * Stats.MoveSpeed;
			//if (this == CharacterManager.Instance.MainPlayer)
			//{
			//	Debug.Log($"movement : {movement.x}, {movement.y}, {movement.z}");
			//	Debug.Log($"actualMovement : {actualMovement.x}, {actualMovement.y}, {actualMovement.z}");
			//}
			if (!CharacterManager.Instance.CheckCollisionWithOtherCharacter(this, actualMovement))
				transform.localPosition += actualMovement;
		}
		//Debug.Log("MoveType: " + MoveType);
		animator.SetBool(MoveType.ToString(), move);
		//animator.SetTrigger(AnimationStateTrigger.AttackCancel.ToString());
	}

	public void Rotate(float xAngle)
	{
		if (IgnoreTurnSpeed)
			transform.Rotate(0f, xAngle, 0f);
		else
			remainRotation = xAngle;
	}

	public void Attack(AttackType type)
	{
		string triggerName = string.Empty;
		switch (type)
		{
			case AttackType.Slash:
				triggerName = AnimationStateTrigger.SlashAttack.ToString();
				break;
			case AttackType.Blunt:
				triggerName = AnimationStateTrigger.BluntAttack.ToString();
				break;
			case AttackType.Pierce:
				triggerName = AnimationStateTrigger.PierceAttack.ToString();
				break;
			case AttackType.Max:
			default:
				break;
		}

		if (!string.IsNullOrEmpty(triggerName))
		{
			if (animator.GetCurrentAnimatorStateInfo(0).IsName(triggerName))
				return;
			animator.SetTrigger(triggerName);
		}
	}

	public void FinishingBlow(int type)
	{
		if (type == 1)
		{
			animator.SetTrigger(AnimationStateTrigger.FinalBlowA.ToString());
			transform.localScale *= 1.2f;
		}
		else if (type == 2)
		{
			animator.SetTrigger(AnimationStateTrigger.FinalBlowB.ToString());
			//var weapon = transform.Find("Sword_3");
			//var smr = weapon?.GetComponent<SkinnedMeshRenderer>();
			//if (smr != null)
			//{
			//	Color color = smr.materials[0].color;
			//	smr.materials[0].color = new Color(color.r * 1.2f, color.g, color.b);
			//}
		}
		Stats.Damage += 10;
	}

	public void SetMoveType(AnimationState type)
	{
		if (MoveType == type)
			return;

		Move(Vector2.zero, false);

		if (type == AnimationState.Walking || type == AnimationState.Running || type == AnimationState.Sprint)
			MoveType = type;

		if (MoveType == AnimationState.Walking)
			Stats.MoveSpeed = Stats.WalkSpeed;
		if (MoveType == AnimationState.Running || MoveType == AnimationState.Sprint)
			Stats.MoveSpeed = Stats.RunSpeed;
	}

	#region Animation Event

	public void AttackEvent()
	{
		//Debug.Log("Animation Hit");
		// check collision
		bool hitTarget = OnAttack?.Invoke(this) ?? false;
		if (hitTarget)
		{
			OnEnterCombat?.Invoke();
			lastCombatTime = 0f;
		}
	}

	public void StartSinkEvent()
	{
		StartCoroutine(ProcessDie());
	}

	#endregion

	public void OnAttacked(Character attacker, int damage)
	{
		//var smr = transform.GetChild(2).GetComponent<SkinnedMeshRenderer>();
		//smr.materials[0].color = Color.red;
		CurrentHP -= damage;
		float dmgRatio = (float)damage / Stats.MaxHP;
		if (CurrentHP <= 0)
		{
			CurrentHP = 0;
			attacker.OnFinishTarget?.Invoke(this);
		}
		else
		{
			if (this != CharacterManager.Instance.MainPlayer)
			{
				if (dmgRatio >= HeavyDmgThreshold)
					animator.SetTrigger(AnimationStateTrigger.HitByAttackerHeavy.ToString());
				else
					animator.SetTrigger(AnimationStateTrigger.HitByAttacker.ToString());
			}
		}
		OnDamaged?.Invoke(damage, dmgRatio);
		OnEnterCombat?.Invoke();
		lastCombatTime = 0f;
	}

	private void PrepareDie()
	{
		animator.SetTrigger("Die");
	}

	IEnumerator ProcessDie()
	{
		yield return new WaitForSeconds(BodyRemoveWaitTime);

		while (transform.localPosition.y > -5f)
		{
			transform.localPosition -= new Vector3(0f, Time.deltaTime / 3f, 0f);
			yield return null;
		}

		OnDie?.Invoke(this);
	}

	public void SetMoveAniamtionSpeed(float times)
	{
		AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;
		foreach (var s in animatorController.layers[0].stateMachine.states)
		{
			AnimatorState state = s.state;
			if (state.name == "Walk")
			{
				state.speed *= times;
			}
		}
	}

	public void SetRageMode(bool enabled)
	{
		animator.SetBool(AnimationState.Rage.ToString(), enabled);
	}
}
