using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog_QuestLog : UIDialog
{
	[SerializeField]
	private GameObject NoQuestNotice = null;

	private void Start()
	{
		NoQuestNotice.SetActive(false);
	}
}
