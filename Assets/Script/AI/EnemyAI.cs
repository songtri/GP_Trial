using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : AIComponent
{
	public override void Think()
	{
		Vector3 selfToPlayer = WorldInfo.instance.MainPlayer.transform.position - Character.transform.position;
		Character.transform.forward = selfToPlayer.normalized;
		if (selfToPlayer.sqrMagnitude > 2.25)
			Character.Move(Vector2.left, true);
		else
		{
			Character.Move(Vector2.zero, false);
			Character.Attack(AttackType.Slash);
		}
	}
}
