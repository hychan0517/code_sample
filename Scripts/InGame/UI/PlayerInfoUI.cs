using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
	[SerializeField]
	private GameObject _content;
	[SerializeField]
	private GameObject _playerInfoSlotSample;

	public void InitUIObject()
	{
		if (_content == null)
			LogManager.LogError("Cannt find _content");

		if(_playerInfoSlotSample == null)
			LogManager.LogError("Cannt find _playerInfoSlotSample");
	}

	public void SetPlayer(Player player)
	{
		if (_content && _playerInfoSlotSample)
		{
			GameObject instantiate = Instantiate(_playerInfoSlotSample, transform);

			PlayerInfoSlot playerInfoSlot = instantiate.GetComponent<PlayerInfoSlot>();
			if (playerInfoSlot)
				playerInfoSlot.SetPlayer(player);
		}
	}
}
