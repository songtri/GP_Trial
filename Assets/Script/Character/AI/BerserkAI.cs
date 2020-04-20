using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkAI : AIComponent
{
	public override void Think(float delta)
	{
		if (!Player.instance.IsInBerserkerState)
			return;

		Vector3 playerToEnemy = Vector3.zero;
		float sqrShortest = float.MaxValue;
		Character nearestEnemy = null;
		foreach (var enemy in CharacterManager.Instance.EnemyList)
		{
			playerToEnemy = enemy.transform.position - Character.transform.position;
			if (sqrShortest > playerToEnemy.sqrMagnitude)
			{
				sqrShortest = playerToEnemy.sqrMagnitude;
				nearestEnemy = enemy;
			}
		}

		Character.transform.forward = playerToEnemy.normalized;
		if (sqrShortest > 2.25)
			Character.Move(Vector2.left, true);
		else
		{
			Character.Move(Vector2.zero, false);
			Character.Attack(AttackType.Slash);
		}
	}
}
