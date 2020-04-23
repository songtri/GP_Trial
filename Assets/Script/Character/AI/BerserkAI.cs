using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkAI : AIComponent
{
	public float AttackAngle = 120f;

	private bool move = false;

	public override void Think(float delta)
	{
		if (!Player.instance.IsInBerserkerState)
			return;

		Vector3 playerToEnemy = Vector3.zero;
		float sqrShortest = float.MaxValue;
		Character nearestEnemy = null;
		foreach (var enemy in CharacterManager.Instance.EnemyList)
		{
			if (enemy.CurrentHP <= 0)
				continue;

			playerToEnemy = enemy.transform.position - Character.transform.position;
			if (sqrShortest > playerToEnemy.sqrMagnitude)
			{
				sqrShortest = playerToEnemy.sqrMagnitude;
				nearestEnemy = enemy;
			}
		}

		if (nearestEnemy == null)
		{
			Character.Move(Vector2.zero, false);
			return;
		}

		float angle = Vector3.SignedAngle(Character.transform.forward, playerToEnemy, Vector3.up);
		Character.Rotate(angle);
		float attackRange = Character.Stats.AttackRange + CharacterManager.Instance.MainPlayer.Stats.Radius;
		if (IsInRange(CharacterManager.Instance.MainPlayer.transform.position, nearestEnemy.transform.position, attackRange, AttackAngle) && Character.MoveType == AnimationState.Sprint)
		{
			move = false;
			Character.Attack(AttackType.Slash);
		}
		else
		{
			move = true;
		}

		Character.Move(move ? Vector2.right : Vector2.zero, move);
	}
}
