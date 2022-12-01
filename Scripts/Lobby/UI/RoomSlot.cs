using TMPro;
using UnityEngine;

public class RoomSlot : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _titleText;
	[SerializeField]
	private GameObject _lockIcon;
	[SerializeField]
	private TextMeshProUGUI _userCountText;

	public void SettingSlot(RoomListUI.RoomData data)
	{
		gameObject.SetActive(true);
		if (_titleText)
			_titleText.text = data.roomName;
		if(_lockIcon)
			_lockIcon.SetActive(data.isLock);
		if (_userCountText)
			_userCountText.text = string.Format("({0}/{1})", data.joinedUserCount, 3);
	}
}
