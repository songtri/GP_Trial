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
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		mainPlayer = characterManager.CreateMainPlayer();
		mainPlayer.OnFinishTarget += MainPlayer_OnFinishTarget;

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
		if (Input.GetKeyUp(KeyCode.C))
		{
			UICombat.OnClickCharacter();
			return;
		}
		else if (Input.GetKeyUp(KeyCode.L))
		{
			UICombat.OnClickQuestLog();
			return;
		}

		if (Player.instance.CanUseFinalBlow)
		{
			if (Input.GetKeyUp(KeyCode.Q))
			{
				mainPlayer.FinishMove(1);
				return;
			}
			else if (Input.GetKeyUp(KeyCode.E))
			{
				mainPlayer.FinishMove(2);
				return;
			}
		}

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

		if (!EventSystem.current.IsPointerOverGameObject() && !Player.instance.IsInBerserkerState)
		{
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

	private void MainPlayer_OnFinishTarget(Character obj)
	{
		StartCoroutine(ShowFinalBlowUI());
	}

	private IEnumerator ShowFinalBlowUI()
	{
		Player.instance.CanUseFinalBlow = true;
		UICombat.ShowFinalBlowNotice(true);

		yield return new WaitForSeconds(1f);

		Player.instance.CanUseFinalBlow = false;
		UICombat.ShowFinalBlowNotice(false);
	}
}
