using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private GameObject CharacterPrefab = null;
	[SerializeField]
	private GameObject EnemyPrefab = null;
	[SerializeField]
	private int MaxEnemy = 3;
	[SerializeField]
	private GameObject EnemySpawnPoints = null;
	[SerializeField]
	private Transform EnemyObjectRoot = null;

	[SerializeField]
	private Camera mainCamera = null;
	[SerializeField]
	private UICombat UICombat = null;

	[SerializeField]
	private Vector3 cameraPosToPlayer = new Vector3(0f, 2f, -2.4f);

	private Character mainPlayer = null;
	private List<Character> enemyList = new List<Character>();
	private List<Transform> enemySpawnPoints = new List<Transform>();
	private int currentEnemySpawnIndex = 0;

	private void Start()
	{
		GameObject go = Instantiate(CharacterPrefab);
		mainPlayer = go.GetComponent<Character>();
		mainPlayer.OnAttack += MainPlayer_OnAttack;
		mainPlayer.OnEnterCombat += Player.instance.OnEnterCombat;
		mainPlayer.OnDamaged += MainPlayer_OnDamaged;
		mainPlayer.OnOutOfCombat += Player.instance.OnOutOfCombat;
		mainPlayer.OnDie += MainPlayer_OnDie;
		Player.instance.HP = mainPlayer.CurrentHP;

		mainCamera.transform.parent = mainPlayer.transform;
		mainCamera.transform.localPosition = /*mainPlayer.transform.position + */cameraPosToPlayer;
		mainCamera.transform.forward = mainPlayer.transform.forward;
		mainCamera.transform.Rotate(7f, 0f, 0f);

		enemySpawnPoints.Clear();
		for (int i = 0; i < EnemySpawnPoints.transform.childCount; ++i)
		{
			var tr = EnemySpawnPoints.transform.GetChild(i);
			enemySpawnPoints.Add(tr);
		}

		WorldInfo.instance.MainPlayer = mainPlayer;
		WorldInfo.instance.EnemyList = enemyList;
	}

	bool CheckAttackRange(Vector3 from, Vector3 to)
	{
		var distVec = to - from;
		if (distVec.sqrMagnitude < 2.25 && Vector3.Dot(distVec, mainPlayer.transform.forward) > 0)
		{
			//Debug.Log($"{mainPlayer.name} collides with {monster.name}");
			return true;
		}

		return false;
	}

	private void Update()
	{
		Player.instance.Update(Time.deltaTime);
		UpdateCamera();
		CheckInput();

		if (enemyList.Count < MaxEnemy)
			CreateEnemy();
	}

	private void UpdateCamera()
	{
		//mainCamera.transform.position = mainPlayer.transform.position + cameraPosToPlayer;
		//mainCamera.transform.SetPositionAndRotation(mainPlayer.transform.position + new Vector3(0f, 2f, -2.5f), mainPlayer.transform.rotation);
	}

	private void CheckInput()
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			int forwardMove = 0;
			int sideMove = 0;
			if (Input.GetKey(KeyCode.W))
				forwardMove = 1;
			else if (Input.GetKeyUp(KeyCode.W))
				forwardMove = 0;
			if (Input.GetKey(KeyCode.A))
				sideMove = -1;
			else if (Input.GetKeyUp(KeyCode.A))
				sideMove = 0;
			if (Input.GetKey(KeyCode.S))
				forwardMove = -1;
			else if (Input.GetKeyUp(KeyCode.S))
				forwardMove = 0;
			if (Input.GetKey(KeyCode.D))
				sideMove = 1;
			else if (Input.GetKeyUp(KeyCode.D))
				sideMove = 0;

			Vector2 direction = new Vector2(forwardMove, sideMove);
			bool move = forwardMove != 0 || sideMove != 0;
			mainPlayer.Move(direction, move);

			if (Input.GetMouseButtonDown(0))
			{
				mainPlayer.Attack(Player.instance.GetAttack(0));
			}
			if (Input.GetMouseButtonDown(1))
				mainPlayer.Attack(Player.instance.GetAttack(1));

			float xAngle = Input.GetAxis("Mouse X");
			mainPlayer.Rotate(xAngle * 1.5f);
			//mainCamera.transform.Rotate(0f, xAngle * 1.5f, 0f);
		}
	}

	private void CreateEnemy()
	{
		// TODO: use object pooling for more performance
		GameObject go = Instantiate(EnemyPrefab, EnemyObjectRoot);
		var enemy = go.GetComponent<Character>();
		enemy.OnAttack += Enemy_OnAttack;
		enemy.OnDie += Enemy_OnDie;
		enemy.transform.position = enemySpawnPoints[currentEnemySpawnIndex++].position;
		if (currentEnemySpawnIndex >= enemySpawnPoints.Count)
			currentEnemySpawnIndex = 0;
		enemyList.Add(enemy);
	}

	#region Event Handlers

	private bool MainPlayer_OnAttack(Character obj)
	{
		foreach (var monster in enemyList)
		{
			if (!monster.IsDead && CheckAttackRange(mainPlayer.transform.position, monster.transform.position))
			{
				monster.OnAttacked(obj.Damage);
				Player.instance.OnAttack();
				return true;
			}
		}

		return false;
	}

	private void MainPlayer_OnDamaged(int damage, float ratio)
	{
		Player.instance.OnDamaged(damage, ratio);
	}

	private void MainPlayer_OnDie(Character obj)
	{
		Player.instance.OnDie();
	}

	private bool Enemy_OnAttack(Character obj)
	{
		if (CheckAttackRange(obj.transform.position, mainPlayer.transform.position))
		{
			mainPlayer.OnAttacked(obj.Damage);
			return true;
		}

		return false;
	}

	private void Enemy_OnDie(Character obj)
	{
		enemyList.Remove(obj);
	}

	#endregion
}
