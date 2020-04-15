using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private Camera mainCamera = null;
	[SerializeField]
	private UICombat UICombat = null;
	[SerializeField]
	private CharacterManager characterManager = null;

	[SerializeField]
	private int MaxEnemy = 3;
	[SerializeField]
	private Vector3 cameraPosToPlayer = new Vector3(0f, 2f, -2.4f);

	private Character mainPlayer = null;

	private void Start()
	{
		mainPlayer = characterManager.CreateMainPlayer();

		mainCamera.transform.parent = mainPlayer.transform;
		mainCamera.transform.localPosition = /*mainPlayer.transform.position + */cameraPosToPlayer;
		mainCamera.transform.forward = mainPlayer.transform.forward;
		mainCamera.transform.Rotate(7f, 0f, 0f);
	}

	private void Update()
	{
		//Debug.Log("GameController.Update: " + Time.deltaTime);
		Player.instance.Update(Time.deltaTime);
		UpdateCamera();
		CheckInput();

		if (characterManager.GetEnemyCount() < MaxEnemy)
			characterManager.CreateEnemy();
	}

	private void UpdateCamera()
	{
		//mainCamera.transform.position = mainPlayer.transform.position + cameraPosToPlayer;
		//mainCamera.transform.SetPositionAndRotation(mainPlayer.transform.position + new Vector3(0f, 2f, -2.5f), mainPlayer.transform.rotation);
	}

	private void CheckInput()
	{
		if (!EventSystem.current.IsPointerOverGameObject() && !Player.instance.IsInBerserkerState)
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
		}
	}

}
