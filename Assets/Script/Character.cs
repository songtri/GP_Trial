using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	private Animator animator;
	[SerializeField]
	public int HP = 100;
	[SerializeField]
	public int Damage = 10;
	[SerializeField]
	public int RageDamage = 20;

	public bool IsDead { get => HP <= 0; }
	private bool SinkBody = false;

	public event Action<Character> OnAttack;

	private bool IsMovable
	{
		get
		{
			var animState = animator.GetCurrentAnimatorStateInfo(0);
			return animState.IsName("Idle") || animState.IsName("Walk");
		}
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
	}

	public void Move(Vector2 direction, bool move)
	{
		if (move && IsMovable)
		{
			Vector3 forward = transform.forward;
			Vector3 movement = Vector3.zero;
			if(direction.x != 0)
				movement = new Vector3(forward.x * direction.x, 0f, forward.z * direction.x) * Time.deltaTime;
			else if(direction.y != 0)
				movement = new Vector3(forward.z * direction.y, 0f, - forward.x * direction.y) * Time.deltaTime;
			transform.localPosition += movement;
		}
		animator.SetBool(AnimationState.Running.ToString(), move);
		animator.SetTrigger(AnimationStateTrigger.AttackCancel.ToString());
	}

	public void Rotate(float xAngle)
	{
		transform.Rotate(0f, xAngle * 1.5f, 0f);
	}

	public void Attack()
	{
		animator.SetTrigger(AnimationStateTrigger.BasicAttack.ToString());
	}

	#region Animation Event

	public void AttackEvent()
	{
		//Debug.Log("Animation Hit");
		// check collision
		OnAttack?.Invoke(this);
	}

	public void StartSinkEvent()
	{
		Destroy(gameObject, 5f);
		SinkBody = true;
	}

	#endregion

	public void OnAttacked(int damage)
	{
		//var smr = transform.GetChild(2).GetComponent<SkinnedMeshRenderer>();
		//smr.materials[0].color = Color.red;
		HP -= damage;
		if (HP <= 0)
		{
			HP = 0;
			Die();
		}
		else
			animator.SetTrigger(AnimationStateTrigger.HitByAttacker.ToString());
	}

	public void Die()
	{
		animator.SetTrigger("Die");
	}
}
