using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AIComponent : MonoBehaviour
{
	[SerializeField]
	public bool Active = true;

	protected Character Character = null;

	protected virtual void Start()
	{
		Character = GetComponent<Character>();
	}

	private void Update()
	{
		if (Active)
			Think(Time.deltaTime);
	}

	public abstract void Think(float delta);

	protected bool IsInRange(Vector3 from, Vector3 to, float range, float angle)
	{
		Vector3 diff = to - from;
		diff.y = 0f;
		float angleBetween = Vector3.Angle(Character.transform.forward, diff);
		if (diff.sqrMagnitude < range * range && angleBetween < angle / 2f)
			return true;
		else
			return false;
	}
}
