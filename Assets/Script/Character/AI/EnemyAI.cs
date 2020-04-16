using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : AIComponent
{
	private int thinkPeriod = 0;
	private bool move = true;

	public override void Think()
	{
		//Vector3 selfToPlayer = CharacterManager.Instance.MainPlayer.transform.position - Character.transform.position;
		//Character.transform.forward = selfToPlayer.normalized;
		//if (selfToPlayer.sqrMagnitude > 2.25)
		//	Character.Move(Vector2.right, true);
		//else
		//{
		//	Character.Move(Vector2.zero, false);
		//	Character.Attack(AttackType.Slash);
		//}
		thinkPeriod++;

		if (thinkPeriod % 400 == 0)
		{
			var rotation = Random.Range(0f, 90f);
			//if (Random.value > 0.5f)
			Character.Rotate(rotation);
		}

		if(thinkPeriod % 300 == 0)
		{
			if (Random.value > 0.8f)
				move = false;
			else
				move = true;
		}

		Character.Move(move ? Vector2.right : Vector2.zero, move);
	}
}
