using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBoxUI : MonoBehaviour
{
    public enum eMessageBoxType
    {
        None,
        DefaultTwoButton,
        DefaultOneButton,
    }

    public struct sMessageBox
    {
        public GameObject _rootObject;
        public Button _okButton;
        public Button _cancelButton;

        public void SettingOKCallback(UnityAction okCallback)
        {
            if (_okButton)
            {
                if (okCallback != null)
                    _okButton.onClick.AddListener(okCallback);
            }
        }

        public void SettingCancelCallback(UnityAction cancelCallback)
        {
            if (_cancelButton)
            {
                if (cancelCallback != null)
                    _cancelButton.onClick.AddListener(cancelCallback);
            }
        }
    }

    [Header("Main")]
    [SerializeField]
    /// <summary> 모든 메세지박스 공용으로 사용하는 BG Exit </summary>
    private Button _bgExitButton;
    [SerializeField]
    private LocalizedText _messageText;
    [SerializeField]
    private GameObject _defaultOneButton;
    [SerializeField]
    private GameObject _defaultTwoButton;

    [SerializeField]
    private Dictionary<eMessageBoxType, sMessageBox> _messageBoxDict = new Dictionary<eMessageBoxType, sMessageBox>();

	#region Init
	public void InitUIObject()
    {
        if (_bgExitButton == null)
            LogManager.LogError("Cannot Find _bgExitButton");
        else
            _bgExitButton.onClick.AddListener(OnClickExitButton);

        if(_messageText == null)
            LogManager.LogError("Cannot Find _messageText");

        if (_messageBoxDict.ContainsKey(eMessageBoxType.DefaultOneButton) == false)
        {
            if(_defaultOneButton == null)
                LogManager.LogError("Cannot Find _defaultOneButton");
            else
                InitDefaultOneButton(_defaultOneButton); 
        }

        if (_messageBoxDict.ContainsKey(eMessageBoxType.DefaultTwoButton) == false)
        {
            if(_defaultTwoButton == null)
                LogManager.LogError("Cannot Find _defaultOneButton");
            else
                InitDefaultTwoButton(_defaultTwoButton); 
        }

        gameObject.SetActive(false);
    }

    private void InitDefaultOneButton(GameObject root)
    {
        GameObject[] childs = root.GetChildsObject();
        sMessageBox temp = new sMessageBox();
        temp._rootObject = root;
        foreach (GameObject child in childs)
        {
            if (string.Equals(child.name, "OKButton"))
                temp._okButton = child.GetComponent<Button>();
        }

        if (_messageBoxDict.ContainsKey(eMessageBoxType.DefaultOneButton) == false)
            _messageBoxDict.Add(eMessageBoxType.DefaultOneButton, temp);

        root.SetActive(false);
    }

    private void InitDefaultTwoButton(GameObject root)
    {
        GameObject[] childs = root.GetChildsObject();
        var temp = new sMessageBox();
        temp._rootObject = root;
        foreach (GameObject child in childs)
        {
            if (string.Equals(child.name, "CancelButton"))
                temp._cancelButton = child.GetComponent<Button>();
            else if (string.Equals(child.name, "OKButton"))
                temp._okButton = child.GetComponent<Button>();
        }

        if (_messageBoxDict.ContainsKey(eMessageBoxType.DefaultTwoButton) == false)
            _messageBoxDict.Add(eMessageBoxType.DefaultTwoButton, temp);

        root.SetActive(false);
    }
	#endregion

	public void SettingMessageBox(eMessageBoxType type, UnityAction exitCallback, UnityAction okCallback, UnityAction cancelCallback, string message)
    {
        //!!주의!! 버튼에 기본적으로 사운드가 들어가있음 콜백으로 사운드 넣을경우 2번발생함
        try
        {
            ActiveOffAllBox();
            gameObject.SetActive(true);
            _messageBoxDict[type]._rootObject.SetActive(true);

            //모든 버튼에 기본적으로 닫는 콜백 추가
            _bgExitButton.onClick.AddListener(OnClickExitButton);
            _messageBoxDict[type].SettingOKCallback(OnClickExitButton);
            _messageBoxDict[type].SettingCancelCallback(OnClickExitButton);
            
            if (exitCallback != null)
            {
                //BG 닫기 버튼 누른 후 콜백 있다면 셋팅
                _bgExitButton.onClick.AddListener(exitCallback);
            }

            if (_messageBoxDict.ContainsKey(type))
            {
                _messageText.text = message;
                //버튼 누른 후 콜백 있다면 셋팅
                _messageBoxDict[type].SettingOKCallback(okCallback);
                _messageBoxDict[type].SettingCancelCallback(cancelCallback);
                App.Ins.PushBackButtonEvent(ActiveOffAllBox);
            }
            else
            {
                ActiveOffAllBox();
            }
        }
        catch (Exception e)
        {
            LogManager.LogError("Setting MessageBox Err");
            LogManager.LogError(e.ToString());
            ActiveOffAllBox();
        }
    }

    private void ActiveOffAllBox()
    {
        var ent = _messageBoxDict.GetEnumerator();
        while (ent.MoveNext())
        {
            if (_messageBoxDict[ent.Current.Key]._rootObject)
                _messageBoxDict[ent.Current.Key]._rootObject.SetActive(false);
        }

        gameObject.SetActive(false);
        //disable이벤트로 모든 콜백 해제함
    }

    public void OnClickExitButtonToBackButton()
    {
        if (gameObject.activeSelf)
        {
            if (_bgExitButton != null)
                _bgExitButton.onClick.Invoke();
            else
                ActiveOffAllBox();
        }
    }

    private void OnClickExitButton()
    {
        App.Ins.ActiveBackButtonEvent();
    }

    private void OnDestroy()
    {
        RemoveAllListeners();
    }

    private void OnDisable()
    {
        RemoveAllListeners();
    }

    private void RemoveAllListeners()
    {
        if (_bgExitButton)
            _bgExitButton.onClick.RemoveAllListeners();

        var ent = _messageBoxDict.GetEnumerator();
        while (ent.MoveNext())
        {
            if (_messageBoxDict[ent.Current.Key]._okButton)
                _messageBoxDict[ent.Current.Key]._okButton.onClick.RemoveAllListeners();
            if (_messageBoxDict[ent.Current.Key]._cancelButton)
                _messageBoxDict[ent.Current.Key]._cancelButton.onClick.RemoveAllListeners();
        }
    }
}
