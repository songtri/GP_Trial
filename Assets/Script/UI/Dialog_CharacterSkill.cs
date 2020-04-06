using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog_CharacterSkill : UIDialog
{
	[SerializeField]
	List<GameObject> LeftClickAttackSelected = null;
	[SerializeField]
	List<GameObject> RightClickAttackSelected = null;

	private void Start()
	{
		for (int i = 0; i < LeftClickAttackSelected.Count; ++i)
		{
			LeftClickAttackSelected[i].SetActive(i == (int)Player.instance.GetAttack(0));
		}
		for (int i = 0; i < RightClickAttackSelected.Count; ++i)
		{
			RightClickAttackSelected[i].SetActive(i == (int)Player.instance.GetAttack(1));
		}
	}

	public void OnClickLeftSkill(int skill)
	{
		Player.instance.SetAttack(0, skill);
		for (int i = 0; i < LeftClickAttackSelected.Count; ++i)
		{
			LeftClickAttackSelected[i].SetActive(i == skill);
		}
	}

	public void OnClickRightSkill(int skill)
	{
		Player.instance.SetAttack(1, skill);
		for (int i = 0; i < RightClickAttackSelected.Count; ++i)
		{
			RightClickAttackSelected[i].SetActive(i == skill);
		}
	}
}
