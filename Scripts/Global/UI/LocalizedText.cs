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
			//Awake�� Start���� ȣ���ϸ� ��Ȱ��ȭ�Ǿ��ִ� ������Ʈ���� GetComponent�� �� ������ setȣ���Ҷ� Ȯ���Ͽ� ĳ��
			if (_localizeStringEvent == null)
				_localizeStringEvent = GetComponent<LocalizeStringEvent>();

			if(_localizeStringEvent)
				_localizeStringEvent.SetEntry(value);
		} 
	}
}
