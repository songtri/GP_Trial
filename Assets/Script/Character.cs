﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	private Animator animator;
	[SerializeField]
	public int MoveSpeed = 2;
	[SerializeField]
	public int MaxHP = 100;
	[SerializeField]
	public int Damage = 10;
	[SerializeField]
	public int RageDamage = 20;
	[SerializeField]
	private float HeavyDmgThreshold = 0.2f;

	private const float BodyRemoveWaitTime = 3f;

	public bool IsDead { get => CurrentHP <= 0; }
	public int CurrentHP
	{
		get => currentHP;
		set
		{
			currentHP = value;
			if (value <= 0)
				PrepareDie();
		}
	}
	private int currentHP = 0;
	private bool SinkBody = false;
	private float lastCombatTime = 0f;

	public event Func<Character, bool> OnAttack;
	public event Action OnEnterCombat;
	public event Action<int, float> OnDamaged;
	public event Action OnOutOfCombat;
	public event Action<Character> OnDie;

	private bool IsMovable
	{
		get
		{
			var animState = animator.GetCurrentAnimatorStateInfo(0);
			return animState.IsName("Idle") || animState.IsName("Walk");
		}
	}

	public Character()
	{
		CurrentHP = MaxHP;
	}

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (SinkBody)
		{
			transform.localPosition -= new Vector3(0f, Time.deltaTime / 3f, 0f);
		}

		lastCombatTime += Time.deltaTime;
		//Debug.Log("LastCombatTime: " + lastCombatTime);
		if (lastCombatTime > Player.minSecondToBeOutOfCombat)
			OnOutOfCombat?.Invoke();
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
			transform.localPosition += movement * MoveSpeed;
		}
		animator.SetBool(AnimationState.Walking.ToString(), move);
		//animator.SetTrigger(AnimationStateTrigger.AttackCancel.ToString());
	}

	public void Rotate(float xAngle)
	{
		transform.Rotate(0f, xAngle * 1.5f, 0f);
	}

	public void Attack(int button)
	{
		AttackType type = Player.instance.GetAttack(button);
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

	public void OnAttacked(int damage)
	{
		//var smr = transform.GetChild(2).GetComponent<SkinnedMeshRenderer>();
		//smr.materials[0].color = Color.red;
		CurrentHP -= damage;
		float dmgRatio = (float)damage / MaxHP;
		if (CurrentHP <= 0)
		{
			CurrentHP = 0;
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

		Destroy(gameObject);
		OnDie?.Invoke(this);
		lastCombatTime = 0f;
	}
}
