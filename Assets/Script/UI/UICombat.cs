using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICombat : MonoBehaviour
{
	[SerializeField]
	private Text RageValue = null;
	[SerializeField]
	private Slider PlayerRage = null;
	[SerializeField]
	private Text PlayerHP = null;
	[SerializeField]
	private UIDialog DialogCharacterSkill = null;
	[SerializeField]
	private UIDialog DialogQuestLog = null;
	[SerializeField]
	private GameObject CharacterDialogOpened = null;
	[SerializeField]
	private GameObject QuestDialogOpened = null;

	private const string RageFormat = "Rage: {0}";
	private const string HpFormat = "HP: {0}";

	private void Start()
	{
		RageValue.text = string.Format(RageFormat, Player.instance.Rage);
		PlayerRage.value = Player.instance.Rage;
		PlayerRage.maxValue = Player.instance.MaxRage;
		PlayerHP.text = string.Format(HpFormat, Player.instance.HP);
		DialogCharacterSkill.gameObject.SetActive(false);
		DialogQuestLog.gameObject.SetActive(false);
		CharacterDialogOpened.SetActive(false);
		QuestDialogOpened.SetActive(false);
	}

	private void Update()
	{
		RageValue.text = string.Format(RageFormat, Player.instance.Rage);
		PlayerRage.value = Player.instance.Rage;
		PlayerHP.text = string.Format(HpFormat, Player.instance.HP);
	}

	public void OnClickCharacter()
	{
		DialogCharacterSkill.gameObject.SetActive(!DialogCharacterSkill.gameObject.activeInHierarchy);
		CharacterDialogOpened.SetActive(DialogCharacterSkill.gameObject.activeInHierarchy);
	}

	public void OnClickQuest()
	{
		DialogQuestLog.gameObject.SetActive(!DialogQuestLog.gameObject.activeInHierarchy);
		QuestDialogOpened.SetActive(DialogQuestLog.gameObject.activeInHierarchy);
	}
}
