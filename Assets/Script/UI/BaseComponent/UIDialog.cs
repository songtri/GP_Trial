using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDialog : MonoBehaviour
{
	[SerializeField]
	private Text Title = null;

	public virtual void OnClickClose()
	{
		gameObject.SetActive(false);
	}
}
