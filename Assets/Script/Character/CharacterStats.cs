using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public float Radius = 0.5f;
	public float AttackRange = 0.5f;
	public float WalkSpeed = 0.5f;
	public float RunSpeed = 1f;
	public int MaxHP = 100;
	public int CurrentHP = 0;
	public int Damage = 10;

	public float MoveSpeed = 2f;

	public bool IsDead => CurrentHP <= 0;

	private void Awake()
	{
		InitStat();
	}

	public void InitStat()
	{
		CurrentHP = MaxHP;
		MoveSpeed = RunSpeed;
	}
}
