using GlobalDefine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _buttonPanel;
    [SerializeField]
    private GameObject _startPanel;
    [SerializeField]
    private Button _guestLoginButton;
    [SerializeField]
    private Button _kakaoLoginButton;
    [SerializeField]
    private Button _startButton;
    [SerializeField]
    private Button _logoutButton;
    [SerializeField]
    private TextMeshProUGUI _nicknameText;

    [SerializeField]
    private NicknameUI _nicknameUI;

    private string _accessToken;

    public void InitUIObject()
	{
        if (_buttonPanel == null)
            LogManager.LogError("Cannot Find _buttonPanel");

        if (_startPanel == null)
            LogManager.LogError("Cannot Find _startPanel");

        if (_guestLoginButton)
            _guestLoginButton.onClick.AddListener(OnClickGuestLoginButton);
        else
            LogManager.LogError("Cannot Find _guestLoginButton");

        if (_kakaoLoginButton)
            _kakaoLoginButton.onClick.AddListener(OnClickKakaoLoginButton);
        else
            LogManager.LogError("Cannot Find _kakaoLoginButton");

        if (_startButton)
            _startButton.onClick.AddListener(OnClickStartButton);
        else
            LogManager.LogError("Cannot Find _startButton");

        if (_logoutButton)
            _logoutButton.onClick.AddListener(OnClickLogoutButton);
        else
            LogManager.LogError("Cannot Find _logoutButton");

        ResetUI();
    }

	public void SettingLoginUI()
	{
        ResetUI();

        if (IsLogined())
            SettingStartPanel();
        else
            SettingLoginPanel();
	}

    private void ResetUI()
	{
        SetButtonInteractable(false);

        if (_buttonPanel)
            _buttonPanel.SetActive(false);
        if (_startPanel)
            _startPanel.SetActive(false);

        _accessToken = string.Empty;
    }

    private void SettingStartPanel()
	{
        if (_startPanel)
            _startPanel.gameObject.SetActive(true);
        if (_startButton)
            _startButton.interactable = true;
        if (_logoutButton)
            _logoutButton.interactable = true;
        if (_nicknameText)
            _nicknameText.text = App.Ins.userDataTable.nickName;
    }

    private void SettingLoginPanel()
	{
        if (_buttonPanel)
            _buttonPanel.gameObject.SetActive(true);
        if (_guestLoginButton)
            _guestLoginButton.interactable = true;
        if (_kakaoLoginButton)
            _kakaoLoginButton.interactable = true;
    }

    private bool IsLogined()
	{
        return !string.IsNullOrWhiteSpace(App.Ins.userDataTable.nickName);
    }

    // 카카로 로그인 응답
    private void ResultKakaoLogin(bool isSuccess)
    {
        if (isSuccess == false)
        {
            LogManager.LogError("KakaoLogin Fail");
            SetButtonInteractable(true);
            App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "error_login_fail_kakao_login");
            return;
        }
        string token = KakaoManager.Instance.GetToken();
        if (string.IsNullOrWhiteSpace(token) == true)
        {
            LogManager.LogError("Kakao token is empty");
            SetButtonInteractable(true);
            App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "error_login_fail_kakao_login");
            return;
        }

        string userID = KakaoManager.Instance.GetUserID();
        if (string.IsNullOrWhiteSpace(userID) == true)
        {
            LogManager.LogError("Kakao userID is empty");
            SetButtonInteractable(true);
            App.Ins.globalUI.messageBoxUI.SettingMessageBox(MessageBoxUI.eMessageBoxType.DefaultOneButton, null, null, null, "error_login_fail_kakao_login");
            return;
        }

        string jsonStr = App.Ins.playerPrefsManager.GetUserData(userID);
        if (string.IsNullOrEmpty(jsonStr) == false)
		{
            //유저정보 있음
            UserDataTable temp = JsonUtility.FromJson<UserDataTable>(jsonStr);
            if (temp != null)
            {
                LogManager.Log("Find UserData");
                App.Ins.userDataTable.SetLoadedUserData(temp);
                SettingLoginUI();
            }
            else
            {
                //파싱싱패
                LogManager.LogError("ResultKakaoLogin UserData Parsing Failed");
                ParserTokens(token);
                CreateNickname(_accessToken, userID, true);
            }
		}
        else
		{
            //유저정보 없음
            LogManager.Log("Cannot Find UserData");
            ParserTokens(token);
            CreateNickname(_accessToken, userID, true);
        }
    }

    /// <summary> 문자열로된 토큰 파싱 및 AccessToken값 취합 </summary>
    private void ParserTokens(string tokens)
    {
        string[] token = tokens.Split(',');
        if (token.Length != 0)
        {
            for (int i = 0; i < token.Length; i++)
            {
                string[] str = token[i].Split(':');
                if (str.Length == 2)
                {
                    string key = str[0];
                    string value = str[1];
                    SetToken(key, value);
                }
            }
        }
    }

    /// <summary> AccessToken값 캐싱 </summary>
    private void SetToken(string key, string value)
    {
        //카카오 SDK 업데이트하면서 Refresh Token은 사라짐
        //형식은 유지했지만 이외 key값은 항상 빈 문자열로 들어옴
        LogManager.Log("key : " + key + ",  value : " + value);
        if (string.Equals(key, "AccessTokenToken"))
        {
            _accessToken = value;
        }
    }

    private void SetButtonInteractable(bool isInteractable)
	{
        if (_guestLoginButton)
            _guestLoginButton.interactable = isInteractable;

        if (_kakaoLoginButton)
            _kakaoLoginButton.interactable = isInteractable;

        if (_startButton)
            _startButton.interactable = isInteractable;

        if (_logoutButton)
            _logoutButton.interactable = isInteractable;
    }

    private void CreateNickname(string accessToken, string userID, bool isKakao)
	{
        App.Ins.userDataTable.accessToken = accessToken;
        App.Ins.userDataTable.userID = userID;
        if(GetNicknameUI())
            _nicknameUI.SettingNicknameUI(OnCompleteCreateNickname, isKakao);
    }

    private NicknameUI GetNicknameUI()
    {
        if (_nicknameUI == null)
        {
            UnityUtility.InstantiateUI(ref _nicknameUI, ResourcesPath.LOBBY_NICKNAME_UI, App_Lobby.Ins.mainCanvas.transform, 5);
        }

        return _nicknameUI;
    }


    //Custom Events==============================================================================
    #region Custom Events
    private void OnCompleteCreateNickname(string nickname, bool isKakao)
    {
        App.Ins.userDataTable.nickName = nickname;
        App.Ins.playerPrefsManager.SetLoginedUserData();
        if (isKakao)
            App.Ins.playerPrefsManager.SetNowUserData();

        SettingLoginUI();
    }
    #endregion
    //==============================================================================Custom Events


    //UI Events==================================================================================
    #region UI Events
    private void OnClickGuestLoginButton()
	{
        AudioManager.Ins.PlayClick();
        SetButtonInteractable(false);
        CreateNickname(string.Empty, App.Ins.playerPrefsManager.GetUserIndex().ToString(), false);
    }

    private void OnClickKakaoLoginButton()
    {
        AudioManager.Ins.PlayClick();
#if !UNITY_EDITOR
        SetButtonInteractable(false);
        App.Ins.userDataTable.accessToken = string.Empty;
        App.Ins.userDataTable.userID = string.Empty;
        KakaoManager.Instance.KakaoLoginStart(ResultKakaoLogin);
#endif
    }

    private void OnClickStartButton()
	{
        AudioManager.Ins.PlayClick();
        App.Ins.userDataTable._isLoginComplete = true;
        SetButtonInteractable(false);
        App_Lobby.Ins.lobbyUI.SettingMainUI();
        SendBirdManager.GetInstance().ConnectSendBird();
	}

    private void OnClickLogoutButton()
	{
        AudioManager.Ins.PlayClick();
        App.Ins.Logout();
        SettingLoginUI();
	}
#endregion
	//==================================================================================UI Events


	private void OnDestroy()
	{
        if (_guestLoginButton)
            _guestLoginButton.onClick.RemoveAllListeners();
        if (_kakaoLoginButton)
            _kakaoLoginButton.onClick.RemoveAllListeners();
        if (_logoutButton)
            _logoutButton.onClick.RemoveAllListeners();
        if (_startButton)
            _startButton.onClick.RemoveAllListeners();
    }
}
