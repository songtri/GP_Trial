using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField]
	private GameObject CharacterPrefab = null;

	[SerializeField]
	private Camera mainCamera = null;

	private Character mainPlayer = null;

	private void Start()
	{
		GameObject go = Instantiate(CharacterPrefab);
		mainPlayer = go.GetComponent<Character>();
		mainCamera.transform.parent = mainPlayer.transform;
		mainCamera.transform.localScale = Vector3.one;
		mainCamera.transform.localPosition = new Vector3(0, 0.1f, -0.2f);
		mainCamera.transform.forward = mainPlayer.transform.forward;
	}

	private void Update()
	{
		UpdateCamera();
		CheckInput();
	}

	private void UpdateCamera()
	{
		//mainCamera.transform.position = mainPlayer.transform.position;
		//mainCamera.transform.SetPositionAndRotation(mainPlayer.transform.position + new Vector3(0f, 0.1f, -0.15f), mainPlayer.transform.rotation);
	}

	private void CheckInput()
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
			mainPlayer.Attack();
		}

		float xAngle = Input.GetAxis("Mouse X");
		mainPlayer.Rotate(xAngle);
	}
}
