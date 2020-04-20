﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
	Slash,
	Blunt,
	Pierce,
	Max,
}

public class Player
{
	private Player()
	{
		Attack[0] = AttackType.Slash;
		Attack[1] = AttackType.Blunt;
	}

	public static Player instance => Nested.instance;

	public class Nested
	{
		static Nested()
		{
		}

		internal static readonly Player instance = new Player();
	}

	public int RageInt => (int)Rage;

	private AttackType[] Attack = new AttackType[2];
	public int HP = 0;
	public float Rage = 000;
	public float MaxRage = 1000;
	private bool IsOutOfCombat = true;
	public bool IsInBerserkerState = false;
	public bool CanUseFinalBlow = false;

	private const float rageGainPerAttack = 50;
	private const float rageGainPerAttacked = 50;
	private const float rageGainPerLostHpRatio = 23;   // gain per 1% hp loss
	private const float minLostHpRatioToGainRage = 5;
	private const float rageLossPerSecondOutOfCombat = 100;
	private const float rageLossInBerserkState = 200; // determines how long berserk state will last
	public const float minSecondToBeOutOfCombat = 2f;

	public const float moveSpeedBonusMultipliedInBerserk = 2f;
	public const float moveSpeedBonusAddedInBerserk = 0f;
	public const float damageBonusMultipliedInBerserk = 0f;
	public const int damageBonusAddedInBerserk = 0;

	public event Action OnBerserkStateStarted;
	public event Action OnBerserkStateEnded;

	public void SetAttack(int button, int skill)
	{
		if (button == 0 || button == 1)
			Attack[button] = (AttackType)skill;
	}

	public AttackType GetAttack(int button)
	{
		if (button == 0 || button == 1)
			return Attack[button];
		else
			return AttackType.Max;
	}

	private void CheckMaxRage()
	{
		if (Rage >= MaxRage)
		{
			Rage = MaxRage;
			IsInBerserkerState = true;
			OnBerserkStateStarted?.Invoke();
		}
	}

	public void Update(float delta)
	{
		//Debug.Log("Player.Update: " + delta);
		if (IsInBerserkerState)
		{
			Rage -= rageLossInBerserkState * delta;
		}
		else if (IsOutOfCombat)
		{
			if (Rage >= 0)
				Rage -= rageLossPerSecondOutOfCombat * delta;
		}

		if (Rage < 0)
		{
			if (IsInBerserkerState)
			{
				OnBerserkStateEnded?.Invoke();
				IsInBerserkerState = false;
			}

			Rage = 0;
		}
	}

	public void OnAttack()
	{
		if (!IsInBerserkerState)
			Rage += rageGainPerAttack;
		CheckMaxRage();
	}

	public void OnEnterCombat()
	{
		//Debug.Log("In Combat");
		IsOutOfCombat = false;
	}

	public void OnDamaged(int damage, float ratio)
	{
		HP -= damage;
		if (!IsInBerserkerState)
		{
			Rage += rageGainPerAttacked;
			Rage += (int)(rageGainPerLostHpRatio * (ratio - minLostHpRatioToGainRage));
		}
		CheckMaxRage();
	}

	public void OnOutOfCombat()
	{
		//Debug.Log("Out of Combat");
		IsOutOfCombat = true;
	}

	public void OnDie()
	{
		Rage = 0;
	}

	public int GetCurrentDamage(int baseDmg)
	{
		if (IsInBerserkerState)
			return (int)(baseDmg * damageBonusMultipliedInBerserk) + damageBonusAddedInBerserk;
		else
			return baseDmg;
	}
}
