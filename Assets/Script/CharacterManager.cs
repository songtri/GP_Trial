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

	private Character mainPlayer = null;
	private List<Character> enemyList = new List<Character>();
	private List<Transform> enemySpawnPoints = new List<Transform>();
	private int currentEnemySpawnIndex = 0;

	public Character MainPlayer => mainPlayer;
	public List<Character> EnemyList => enemyList;

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
		mainPlayer = GetCharacter(CharacterPrefab, MainPlayer_OnAttack, Player.instance.OnEnterCombat, MainPlayer_OnDamaged, Player.instance.OnOutOfCombat, MainPlayer_OnFinishTarget, MainPlayer_OnDie);
		Player.instance.HP = mainPlayer.CurrentHP;
		Player.instance.OnBerserkStateStarted += Instance_OnBerserkStateStarted;
		Player.instance.OnBerserkStateEnded += Instance_OnBerserkStateEnded;

		return mainPlayer;
	}

	public Character GetCharacter(GameObject prefab, Func<Character, bool> onAttack, Action onEnterCombat, Action<int, float> onDamaged, Action onOutOfCombat, Action<Character> onFinishTarget, Action<Character> onDie)
	{
		Character character;
		if (pooledCharacters.Count > 0)
			character = pooledCharacters.Dequeue();
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

	public void CreateEnemy()
	{
		var enemy = GetCharacter(EnemyPrefab, Enemy_OnAttack, null, null, null, null, Enemy_OnDie);
		enemy.transform.parent = EnemyObjectRoot.transform;
		enemy.transform.position = enemySpawnPoints[currentEnemySpawnIndex++].position;
		if (currentEnemySpawnIndex >= enemySpawnPoints.Count)
			currentEnemySpawnIndex = 0;
		enemyList.Add(enemy);
	}

	private bool CheckAttackRange(Character from, Character to)
	{
		var distVec = to.transform.position - from.transform.position;
		if (distVec.sqrMagnitude < (from.Stats.AttackRange + to.Stats.Radius) && Vector3.Dot(distVec, mainPlayer.transform.forward) > 0)
		{
			//Debug.Log($"{mainPlayer.name} collides with {monster.name}");
			return true;
		}

		return false;
	}

	public bool CheckCollisionWithOtherCharacter(Character character, Vector3 movement)
	{
		var dest = character.transform.position + movement;
		foreach (var c in activeCharacters)
		{
			if (c == character)
				continue;

			var diff = dest - c.transform.position;
			if (diff.sqrMagnitude < character.Stats.Radius + c.Stats.Radius)
				return true;
		}

		return false;
	}

	public int GetEnemyCount()
	{
		return enemyList.Count;
	}

	#region Event Handlers

	private bool MainPlayer_OnAttack(Character obj)
	{
		foreach (var monster in enemyList)
		{
			if (!monster.Stats.IsDead && CheckAttackRange(mainPlayer, monster))
			{
				monster.OnAttacked(obj, Player.instance.GetCurrentDamage(obj.Stats.Damage));
				Player.instance.OnAttack();
			}
		}

		return false;
	}

	private void MainPlayer_OnDamaged(int damage, float ratio)
	{
		Player.instance.OnDamaged(damage, ratio);
	}

	private void MainPlayer_OnFinishTarget(Character obj)
	{
		//throw new NotImplementedException();
	}

	private void Instance_OnBerserkStateStarted()
	{
		mainPlayer.SetMoveType(AnimationState.Running);
	}

	private void Instance_OnBerserkStateEnded()
	{
		mainPlayer.SetMoveType(AnimationState.Walking);
	}

	private void MainPlayer_OnDie(Character obj)
	{
		Player.instance.OnDie();
	}

	private bool Enemy_OnAttack(Character obj)
	{
		if (CheckAttackRange(obj, mainPlayer))
		{
			mainPlayer.OnAttacked(obj, obj.Stats.Damage);
			return true;
		}

		return false;
	}

	private void Enemy_OnDie(Character obj)
	{
		enemyList.Remove(obj);
		activeCharacters.Remove(obj);
		pooledCharacters.Enqueue(obj);
	}

	#endregion
}
