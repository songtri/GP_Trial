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
	private GameObject FinalBlowGroup = null;
	[SerializeField]
	private Image VignetteImage = null;
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
		RageValue.text = string.Format(RageFormat, Player.instance.RageInt);
		PlayerRage.value = Player.instance.Rage;
		PlayerRage.maxValue = Player.instance.MaxRage;
		PlayerHP.text = string.Format(HpFormat, Player.instance.HP);
		ShowFinalBlowNotice(false);
		DialogCharacterSkill.gameObject.SetActive(false);
		DialogQuestLog.gameObject.SetActive(false);
		CharacterDialogOpened.SetActive(false);
		QuestDialogOpened.SetActive(false);
	}

	private int vignetteAlphaAdjustment = 1;
	private void Update()
	{
		//Debug.Log("UICombat.Update: " + Time.deltaTime);
		RageValue.text = string.Format(RageFormat, Player.instance.RageInt);
		PlayerRage.value = Player.instance.Rage;
		PlayerHP.text = string.Format(HpFormat, Player.instance.HP);

		if (VignetteImage.gameObject.activeInHierarchy)
		{
			Color32 oldColor = VignetteImage.color;
			int alpha = oldColor.a;
			if (alpha >= 200)
				vignetteAlphaAdjustment = -1;
			else if (alpha <= 50)
				vignetteAlphaAdjustment = 1;
			VignetteImage.color = new Color32(oldColor.r, oldColor.g, oldColor.b, (byte)(alpha + vignetteAlphaAdjustment));
		}
	}

	public void OnClickCharacter()
	{
		DialogCharacterSkill.gameObject.SetActive(!DialogCharacterSkill.gameObject.activeInHierarchy);
		CharacterDialogOpened.SetActive(DialogCharacterSkill.gameObject.activeInHierarchy);
	}

	public void OnClickQuestLog()
	{
		DialogQuestLog.gameObject.SetActive(!DialogQuestLog.gameObject.activeInHierarchy);
		QuestDialogOpened.SetActive(DialogQuestLog.gameObject.activeInHierarchy);
	}

	public void ShowFinalBlowNotice(bool show)
	{
		FinalBlowGroup.SetActive(show);
		Color32 oldColor = VignetteImage.color;
		VignetteImage.color = new Color32(oldColor.r, oldColor.g, oldColor.b, 130);
	}

	public void ShowVignetteImage(bool show)
	{
		VignetteImage.gameObject.SetActive(show);
	}
}
