using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
	[SerializeField]
	private GameObject CharacterPrefab = null;
	[SerializeField]
	private GameObject EnemyPrefab = null;
	[SerializeField]
	private GameObject EnemySpawnPoints = null;
	[SerializeField]
	private Transform EnemyObjectRoot = null;

	private List<Character> activeCharacters = new List<Character>();
	private Queue<Character> pooledCharacters = new Queue<Character>();
	private List<Transform> enemySpawnPoints = new List<Transform>();
	private int currentEnemySpawnIndex = 0;

	public Character MainPlayer { get; private set; } = null;
	public List<Character> EnemyList { get; } = new List<Character>();

	public static CharacterManager Instance { get; private set; }

	private void Start()
	{
		Instance = this;

		enemySpawnPoints.Clear();
		for (int i = 0; i < EnemySpawnPoints.transform.childCount; ++i)
		{
			var tr = EnemySpawnPoints.transform.GetChild(i);
			enemySpawnPoints.Add(tr);
		}
	}

	public Character CreateMainPlayer()
	{
		MainPlayer = GetCharacter(CharacterPrefab, MainPlayer_OnAttack, Player.instance.OnEnterCombat, MainPlayer_OnDamaged, Player.instance.OnOutOfCombat, MainPlayer_OnFinishTarget, MainPlayer_OnDie);
		Player.instance.HP = MainPlayer.CurrentHP;
		Player.instance.OnBerserkStateStarted += Instance_OnBerserkStateStarted;
		Player.instance.OnBerserkStateEnded += Instance_OnBerserkStateEnded;

		return MainPlayer;
	}

	public Character GetCharacter(GameObject prefab, Func<Character, bool> onAttack, Action onEnterCombat, Action<int, float> onDamaged, Action onOutOfCombat, Action<Character> onFinishTarget, Action<Character> onDie)
	{
		Character character;
		if (pooledCharacters.Count > 0)
		{
			character = pooledCharacters.Dequeue();
			character.gameObject.SetActive(true);
		}
		else
		{
			GameObject go = Instantiate(prefab);
			character = go.GetComponent<Character>();
			character.OnAttack += onAttack;
			character.OnEnterCombat += onEnterCombat;
			character.OnDamaged += onDamaged;
			character.OnOutOfCombat += onOutOfCombat;
			character.OnFinishTarget += onFinishTarget;
			character.OnDie += onDie;
		}
		activeCharacters.Add(character);

		return character;
	}

	private static int enemyNum = 0;
	public void CreateEnemy()
	{
		var enemy = GetCharacter(EnemyPrefab, Enemy_OnAttack, null, null, null, null, Enemy_OnDie);
		enemy.transform.parent = EnemyObjectRoot.transform;
		enemy.transform.position = enemySpawnPoints[currentEnemySpawnIndex++].position;
		if (currentEnemySpawnIndex >= enemySpawnPoints.Count)
			currentEnemySpawnIndex = 0;
		EnemyList.Add(enemy);
		enemy.name += enemyNum++.ToString();
	}

	private bool CheckAttackRange(Character from, Character to)
	{
		var distVec = to.transform.position - from.transform.position;
		float attackRange = (from.Stats.AttackRange + to.Stats.Radius) * (from.Stats.AttackRange + to.Stats.Radius);
		if (distVec.sqrMagnitude < attackRange && Vector3.Dot(distVec, from.transform.forward) > 0)
		{
			//Debug.Log($"{from.name} collides with {to.name}");
			return true;
		}

		return false;
	}

	public bool CheckCollisionWithOtherCharacter(Character character, Vector3 movement)
	{
		var dest = character.transform.position + movement;
		foreach (var c in activeCharacters)
		{
			if (c == character || c.CurrentHP <= 0)
				continue;

			var diff = dest - c.transform.position;
			if (diff.sqrMagnitude < character.Stats.Radius + c.Stats.Radius)
				return true;
		}

		return false;
	}

	public int GetEnemyCount()
	{
		return EnemyList.Count;
	}

	#region Event Handlers

	private bool MainPlayer_OnAttack(Character obj)
	{
		bool isTargetHit = false;
		foreach (var monster in EnemyList)
		{
			if (!monster.Stats.IsDead && CheckAttackRange(MainPlayer, monster))
			{
				monster.OnAttacked(obj, Player.instance.GetCurrentDamage(obj.Stats.Damage));
				Player.instance.OnAttack();
				isTargetHit = true;
			}
		}

		return isTargetHit;
	}

	private void MainPlayer_OnDamaged(int damage, float ratio)
	{
		Player.instance.OnDamaged(damage, ratio);
	}

	private void MainPlayer_OnFinishTarget(Character obj)
	{
		if (!Player.instance.IsInBerserkerState)
			Player.instance.GainRage(100);
	}

	private void Instance_OnBerserkStateStarted()
	{
		MainPlayer.IgnoreTurnSpeed = false;
		MainPlayer.SetRageMode(true);
		MainPlayer.SetMoveType(AnimationState.Sprint);
		MainPlayer.Stats.MoveSpeed *= Player.moveSpeedBonusMultipliedInBerserk;
		MainPlayer.Stats.MoveSpeed += Player.moveSpeedBonusAddedInBerserk;
	}

	private void Instance_OnBerserkStateEnded()
	{
		MainPlayer.IgnoreTurnSpeed = true;
		MainPlayer.SetRageMode(false);
		MainPlayer.SetMoveType(AnimationState.Running);
		MainPlayer.Stats.MoveSpeed -= Player.moveSpeedBonusAddedInBerserk;
		MainPlayer.Stats.MoveSpeed /= Player.moveSpeedBonusMultipliedInBerserk;
	}

	private void MainPlayer_OnDie(Character obj)
	{
		Player.instance.OnDie();
	}

	private bool Enemy_OnAttack(Character obj)
	{
		if (CheckAttackRange(obj, MainPlayer))
		{
			MainPlayer.OnAttacked(obj, obj.Stats.Damage);
			return true;
		}

		return false;
	}

	private void Enemy_OnDie(Character obj)
	{
		EnemyList.Remove(obj);
		activeCharacters.Remove(obj);
		pooledCharacters.Enqueue(obj);
		obj.gameObject.SetActive(false);
		obj.Init();
	}

	#endregion
}
