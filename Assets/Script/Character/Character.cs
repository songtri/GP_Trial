using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

// TODO: need to seperate stats with actor control
public class Character : MonoBehaviour
{
	[SerializeField]
	private float HeavyDmgThreshold = 0.2f;

	[HideInInspector]
	public CharacterStats Stats;
	private Animator animator;

	private const float BodyRemoveWaitTime = 3f;

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
	private bool SinkBody = false;
	private float lastCombatTime = 0f;
	private AnimationState moveType = AnimationState.Walking;

	public event Func<Character, bool> OnAttack;
	public event Action OnEnterCombat;
	public event Action<int, float> OnDamaged;
	public event Action OnOutOfCombat;
	public event Action<Character> OnFinishTarget;
	public event Action<Character> OnDie;

	private bool IsMovable
	{
		get
		{
			var animState = animator.GetCurrentAnimatorStateInfo(0);
			return animState.IsName("Idle") || animState.IsName("Walk");
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
		if (SinkBody)
		{
			transform.localPosition -= new Vector3(0f, Time.deltaTime / 3f, 0f);
		}

		if (lastCombatTime < Player.minSecondToBeOutOfCombat)
		{
			lastCombatTime += Time.deltaTime;
			if (lastCombatTime >= Player.minSecondToBeOutOfCombat)
				OnOutOfCombat?.Invoke();
		}
		//Debug.Log("LastCombatTime: " + lastCombatTime);
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
			//Debug.Log($"movement : {movement.x}, {movement.y}, {movement.z}");
			var actualMovement = movement * Stats.MoveSpeed;
			if (!CharacterManager.Instance.CheckCollisionWithOtherCharacter(this, actualMovement))
				transform.localPosition += actualMovement;
		}
		animator.SetBool(AnimationState.Walking.ToString(), move);
		//animator.SetTrigger(AnimationStateTrigger.AttackCancel.ToString());
	}

	public void Rotate(float xAngle)
	{
		transform.Rotate(0f, xAngle, 0f);
	}

	public void Attack(AttackType type)
	{
		switch (type)
		{
			case AttackType.Slash:
				animator.SetTrigger(AnimationStateTrigger.SlashAttack.ToString());
				break;
			case AttackType.Blunt:
				animator.SetTrigger(AnimationStateTrigger.BluntAttack.ToString());
				break;
			case AttackType.Pierce:
				animator.SetTrigger(AnimationStateTrigger.PierceAttack.ToString());
				break;
			case AttackType.Max:
			default:
				break;
		}
	}

	public void FinishMove(int type)
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
		if (type != AnimationState.Walking || type != AnimationState.Running)
			return;

		moveType = type;
	}

	#region Animation Event

	public void AttackEvent()
	{
		//Debug.Log("Animation Hit");
		// check collision
		bool hitTarget = OnAttack?.Invoke(this) ?? false;
		if (hitTarget)
		{
			if (lastCombatTime >= Player.minSecondToBeOutOfCombat)
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
			if (dmgRatio >= HeavyDmgThreshold)
				animator.SetTrigger(AnimationStateTrigger.HitByAttackerHeavy.ToString());
			else
				animator.SetTrigger(AnimationStateTrigger.HitByAttacker.ToString());
		}
		OnDamaged?.Invoke(damage, dmgRatio);
		if (lastCombatTime >= Player.minSecondToBeOutOfCombat)
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
		//SinkBody = true;

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
}
