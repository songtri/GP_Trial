using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
	public float Radius = 0.5f;
	public float AttackRange = 0.5f;
	public int MoveSpeed = 2;
	public int MaxHP = 100;
	public int CurrentHP = 0;
	public int Damage = 10;

	public bool IsDead => CurrentHP <= 0;

	private void Start()
	{
		InitStat();
	}

	public void InitStat()
	{
		CurrentHP = MaxHP;
	}
}
