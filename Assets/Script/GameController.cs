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
		Player.instance.OnBerserkStateStarted += Instance_OnBerserkStateStarted;
		Player.instance.OnBerserkStateEnded += Instance_OnBerserkStateEnded;

		mainCamera.transform.parent = mainPlayer.transform;
		mainCamera.transform.localPosition = /*mainPlayer.transform.position + */cameraPosToPlayer;
		mainCamera.transform.forward = mainPlayer.transform.forward;
		mainCamera.transform.Rotate(8f, 0f, 0f);
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
		//if (Player.instance.IsInBerserkerState)
		//{
		//	mainCamera.transform.parent = null;
		//}
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
				mainPlayer.FinishingBlow(1);
				return;
			}
			else if (Input.GetKeyUp(KeyCode.E))
			{
				mainPlayer.FinishingBlow(2);
				return;
			}
		}

		if (!EventSystem.current.IsPointerOverGameObject() && !Player.instance.IsInBerserkerState)
		{
			Vector2 direction = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
			bool move = direction.x != 0 || direction.y != 0;
			mainPlayer.Move(direction, move);

			if (Input.GetMouseButtonDown(0))
				mainPlayer.Attack(Player.instance.GetAttack(0));
			if (Input.GetMouseButtonDown(1))
				mainPlayer.Attack(Player.instance.GetAttack(1));

			float xAngle = Input.GetAxis("Mouse X");
			mainPlayer.Rotate(xAngle * 1.5f);
		}

		//if (Player.instance.IsInBerserkerState)
		//{
		//	float xAngle = Input.GetAxis("Mouse X");
		//	mainCamera.transform.Rotate(0f, xAngle, 0f);
		//}
	}

	private void MainPlayer_OnFinishTarget(Character obj)
	{
		if (!Player.instance.IsInBerserkerState)
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

	private void Instance_OnBerserkStateStarted()
	{
		UICombat.ShowVignetteImage(true);
	}

	private void Instance_OnBerserkStateEnded()
	{
		UICombat.ShowVignetteImage(false);
	}
}
