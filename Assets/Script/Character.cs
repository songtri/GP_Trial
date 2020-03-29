using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	private Animator animator;

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
		animator.SetBool("SetWalking", move);
	}

	public void Rotate(float xAngle)
	{
		transform.Rotate(0f, xAngle, 0f);
	}

	public void Attack()
	{
		animator.SetBool("SetAttack", true);
	}
}
