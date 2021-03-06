﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : AIComponent
{
	public enum EnemyAiType
	{
		StandStill,
		//GuardArea,
		RoamSlow,
		RoamFast,
		SearchAndDestroy,
	}

	public EnemyAiType AiType = EnemyAiType.StandStill;
	public float SightRange = 10f;
	public float SightAngle = 120f;
	public float AttackAngle = 120f;
	public float MovePeriod = 3f;

	private float movePeriod = 0;
	private bool move = true;

	protected override void Start()
	{
		base.Start();
		System.Random random = new System.Random();
		AiType = (EnemyAiType)random.Next(0, 4);
	}

	public override void Think(float delta)
	{
		movePeriod += delta;
		if (movePeriod > MovePeriod)
			movePeriod = 0f;
		else
		{
			Character.Move(move ? Vector2.right : Vector2.zero, move);
			return;
		}

		switch (AiType)
		{
			case EnemyAiType.StandStill:
				move = false;
				break;
			case EnemyAiType.RoamSlow:
			{
				if (Character.MoveType != AnimationState.Walking)
					Character.SetMoveType(AnimationState.Walking);

				SetRoam();
				if (IsInRange(Character.transform.position, CharacterManager.Instance.MainPlayer.transform.position, SightRange, SightAngle))
					SetToSearchAndDestroy();
			}
			break;
			case EnemyAiType.RoamFast:
				if (Character.MoveType != AnimationState.Running)
					Character.SetMoveType(AnimationState.Running);
				SetRoam();
				if (IsInRange(Character.transform.position, CharacterManager.Instance.MainPlayer.transform.position, SightRange, SightAngle))
					SetToSearchAndDestroy();
				break;
			case EnemyAiType.SearchAndDestroy:
			{
				if (Character.MoveType != AnimationState.Running)
					Character.SetMoveType(AnimationState.Running);

				Vector3 selfToPlayer = CharacterManager.Instance.MainPlayer.transform.position - Character.transform.position;
				float angle = Vector3.SignedAngle(Character.transform.forward, selfToPlayer, Vector3.up);
				Character.Rotate(angle);
				float attackRange = Character.Stats.AttackRange + CharacterManager.Instance.MainPlayer.Stats.Radius;
				if (IsInRange(Character.transform.position, CharacterManager.Instance.MainPlayer.transform.position, attackRange, AttackAngle) && Character.MoveType == AnimationState.Running)
				{
					move = false;
					Character.Attack(AttackType.Slash);
				}
				else
					move = true;
			}
			break;
			default:
				break;
		}

		Character.Move(move ? Vector2.right : Vector2.zero, move);
	}

	private void SetRoam()
	{
		var rotation = UnityEngine.Random.Range(0f, 90f);
		Character.Rotate(rotation);

		move = UnityEngine.Random.value < 0.8f ? true : false;
	}

	private void SetToSearchAndDestroy()
	{
		AiType = EnemyAiType.SearchAndDestroy;
		MovePeriod = 0.1f;
	}
}
