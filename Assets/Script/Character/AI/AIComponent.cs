using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AIComponent : MonoBehaviour
{
	[SerializeField]
	public bool Active = true;

	protected Character Character = null;

	private void Start()
	{
		Character = GetComponent<Character>();
	}

	private void Update()
	{
		if (Active)
			Think(Time.deltaTime);
	}

	public abstract void Think(float delta);
}
