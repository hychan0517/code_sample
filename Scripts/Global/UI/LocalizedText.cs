using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(LocalizeStringEvent))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
	private LocalizeStringEvent _localizeStringEvent;
	public string text 
	{
		set 
		{
			//Awake나 Start에서 호출하면 비활성화되어있는 오브젝트에서 GetComponent할 수 없으니 set호출할때 확인하여 캐싱
			if (_localizeStringEvent == null)
				_localizeStringEvent = GetComponent<LocalizeStringEvent>();

			if(_localizeStringEvent)
				_localizeStringEvent.SetEntry(value);
		} 
	}
}
