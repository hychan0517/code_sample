using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillUI_Slot : MonoBehaviour
{
    private Image _skillImage;
    private Button _slotButton;
    private int _skillID;
    public void InitUIObject()
	{
		_slotButton = GetComponent<Button>();
		_skillImage = gameObject.GetChildObject("Image").GetComponent<Image>();
		_slotButton.onClick.AddListener(OnClickSlotButton);
		
	}

	public void SettingSkill(int skillID)
	{
		_skillID = skillID;
		LogManager.Log(string.Format("Setting Skill : {0}", _skillID));
	}

	private void OnClickSlotButton()
	{
		if(_skillID > 0)
		{
			LogManager.Log(string.Format("Active Skill : {0}", _skillID));
			App_InGame.Ins.ActivePlayerSkill(_skillID);
			gameObject.SetActive(false);
		}
	}

	private void OnDestroy()
	{
		if (_slotButton)
			_slotButton.onClick.RemoveAllListeners();
	}
}
