using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListUI : MonoBehaviour
{
    public struct RoomData
	{
        public string roomName;
        public bool isLock;
        public int joinedUserCount;
	}

    [Header("MapList")]
    //TODO : 맵리스트 데이터로 초기화하기
    [SerializeField]
    private GameObject _mapContent;
    [SerializeField]
    private MapSlot _mapSlot_1;
    [SerializeField]
    private MapSlot _mapSlot_2;

    [Header("Scroll")]
    [SerializeField]
    private GameObject _selectDescriptionText;
    [SerializeField]
    private ScrollRect _scrollView;
    [SerializeField]
    private RectTransform _scrollViewRect;
    [SerializeField]
    private RectTransform _roomScrollContent;
    [SerializeField]
    private GameObject _slotSample;

    [Header("Action")]
    [SerializeField]
    private TMP_InputField _searchInputField;
    [SerializeField]
    private Button _fastRoomButton;
    [SerializeField]
    private Button _createRoomButton;

    private List<RoomData> _dataList = new List<RoomData>();
    private List<RoomSlot> _slotList = new List<RoomSlot>();
    private Vector2 _slotSize;

    [Header("Scroll Option")]
    [SerializeField]
    private float _leftPadding = 0;
    [SerializeField]
    private float _topPadding = 0;
    [SerializeField]
    private float _verSpacing = 0;
    [SerializeField]
    private float _horSpacing = 0;
    [SerializeField]
    private int _testDataCount = 50;
    private int _lastDataCount = 50;
    [SerializeField]
    private Vector2 _roomSlotSize;
    private Vector2 _lastSlotSize;
    
    private int _totalShowCount;  // 화면에 보여질 슬롯
    private int _slotIndex = 0;
    private int _dataIndex = 0;

    private int _horShowCount;
    private int _verShowCount;

    private int _selectedMapID;

    public void InitUIObject()
	{
        //MapList
        //if (_mapContent)
        //    _mapContent.AddComponent<UIScreenScaler>().InitScaler(UIScreenScaler.eUIType.Layout);
        //else
        //    LogManager.LogError("Cannot Find _mapContent");

        if (_mapSlot_1)
            _mapSlot_1.InitUIObject(SettingMapData);
        else
            LogManager.LogError("Cannot Find _mapSlot_1");

        if (_mapSlot_2)
            _mapSlot_2.InitUIObject(SettingMapData);
        else
            LogManager.LogError("Cannot Find _mapSlot_2");

        if (_searchInputField == null)
            LogManager.LogError("Cannot Find _searchInputField");

        //Scroll
        if (_selectDescriptionText)
            _selectDescriptionText.SetActive(false);
        else
            LogManager.LogError("Cannot Find _selectDescriptionText");

        if (_scrollView)
            _scrollView.onValueChanged.AddListener((_x) => { OnValueChangedScroll(); });
        else
            LogManager.LogError("Cannot Find _scrollView");

        if (_scrollViewRect == null)
            LogManager.LogError("Cannot Find _mapContent");

        if (_roomScrollContent == null)
            LogManager.LogError("Cannot Find _roomScrollContent");

        if (_slotSample)
        {
            _slotSample.SetActive(false);
            _slotSample.GetComponent<UIScreenScaler>().InitScaler(UIScreenScaler.eUIType.Base);
            Destroy(_slotSample.GetComponent<UIScreenScaler>());
        }
        else
        {
            LogManager.LogError("Cannot Find _slotSample");
        }

        //Action
        if (_fastRoomButton)
            _fastRoomButton.onClick.AddListener(OnClickFastRoomButton);
        else
            LogManager.LogError("Cannot Find _fastRoomButton");

        if (_createRoomButton)
            _createRoomButton.onClick.AddListener(OnClickCreateRoomButton);
        else
            LogManager.LogError("Cannot Find _createRoomButton");
    }


	//UI Func========================================================================================
	#region UI Func
    public void SettingRoomListUIAndResetData()
	{
        _dataList.Clear();
        _slotList.Clear();
        UnityUtility.DestroyChildObject(_roomScrollContent.gameObject);
        Test();
        SettingScroll();
    }

    public void SettingRoomListUIAndResize()
	{
        _slotList.Clear();
        UnityUtility.DestroyChildObject(_roomScrollContent.gameObject);
        SettingScroll();
    }

    public void SettingRoomListUI()
	{
        SettingSelectState(false);
        _dataList.Clear();
        SettingMapList();
        SettingScroll();
    }

    private void SettingMapData(int selectMapID)
	{
        SettingSelectState(true);
        if (_fastRoomButton)
            _fastRoomButton.interactable = true;

        _selectedMapID = selectMapID;

        if (_selectedMapID == 1)
		{
            //TODO : 리스트관리
            if (_mapSlot_1)
                _mapSlot_1.SetSelectState(true);
            if (_mapSlot_2)
                _mapSlot_2.SetSelectState(false);
        }
        else
		{
            if (_mapSlot_1)
                _mapSlot_1.SetSelectState(false);
            if (_mapSlot_2)
                _mapSlot_2.SetSelectState(true);
        }

#if DEBUG_MODE
        Test();
#else
        //TODO : 서버 데이터 셋팅
#endif
        SettingScroll();
    }

    public void SettingMapList()
	{
        if (_mapSlot_1)
            _mapSlot_1.SettingMapSlot();
        if (_mapSlot_2)
            _mapSlot_2.SettingMapSlot();
    }

    private void Test()
	{
        _dataList.Clear();
        for(int i = 0; i < _testDataCount; ++i)
		{
            _dataList.Add(new RoomData() { isLock = UnityEngine.Random.Range(0, 5) == 0 ? true : false,
                                            joinedUserCount = UnityEngine.Random.Range(0,4),
                                            roomName = i.ToString() });
        }
        _lastDataCount = _testDataCount;
    }

    /// <summary> 빠른참여버튼, 맵을 선택해주세요 문구 상태 초기화 </summary>
    /// <param name="isSelect"> true = 맵 선택, true = 맵 선택 안함 </param>
    private void SettingSelectState(bool isSelect)
	{
        if (_selectDescriptionText)
            _selectDescriptionText.SetActive(!isSelect);
        if (_fastRoomButton)
            _fastRoomButton.interactable = isSelect;
    }
	#endregion
	//========================================================================================UI Func


	//Scroll=========================================================================================
	#region Scroll
	private void SettingScroll()
    {
        CreateSlot();
        ResetSlot();
        ResetScroll();
        SettingSlotDefault();
    }

    private void CreateSlot()
    {
        if (_slotList.Count == 0 && _slotSample)
        {
            // 슬롯 크기
            Rect realSlotSize = _slotSample.GetComponent<RectTransform>().rect;

            _roomSlotSize = new Vector2(realSlotSize.width, realSlotSize.height);
            _lastSlotSize = _roomSlotSize;

            // 비율 적용된 슬롯 사이즈
            _slotSize = new Vector2(realSlotSize.width + _horSpacing, realSlotSize.height + _verSpacing);

            _horShowCount = Mathf.Clamp(Convert.ToInt32(Math.Truncate((_scrollViewRect.rect.width + _horSpacing - _leftPadding) / _slotSize.x)), 1, int.MaxValue);

            // 화면에 보여질 슬롯 수
            _verShowCount = Convert.ToInt32(Math.Truncate(_scrollViewRect.rect.height / _slotSize.y)) + 2;

            _totalShowCount = _verShowCount * _horShowCount;

            // 기본 슬롯 생성,배치
            for (int i = 0; i < _totalShowCount; i++)
            {
                GameObject slot = Instantiate(_slotSample, _roomScrollContent.transform);
                _slotList.Add(slot.GetComponent<RoomSlot>());
            }
        }
    }

    private void ResetSlot()
    {
        foreach (var i in _slotList)
        {
            i.gameObject.SetActive(false);
        }
    }

    private void ResetScroll()
    {
        _scrollView.velocity = Vector2.zero;
        _roomScrollContent.anchoredPosition = Vector2.zero;
        int lastLine = 0;
        if (_dataList.Count % _horShowCount != 0)
            lastLine = 1;
        _roomScrollContent.sizeDelta = new Vector2(_roomScrollContent.sizeDelta.x, ((_dataList.Count / _horShowCount) + lastLine) * _slotSize.y + _topPadding);
        _slotIndex = 0;
        _dataIndex = 0;
    }

    private void SettingSlotDefault()
    {
        for (int i = 0; i < _dataList.Count; i++)
        {
            if (i >= _totalShowCount)
                break;

            SettingSlot(i, i);
        }
    }

    private void OnValueChangedScroll()
    {
        if (_verShowCount != 0 && _horShowCount != 0)
        {
            if (_roomScrollContent.anchoredPosition.y - _topPadding > ((_dataIndex / _horShowCount) + 1) * _slotSize.y)
                DownScroll();
            else if (_roomScrollContent.anchoredPosition.y - _topPadding < (_dataIndex / _horShowCount) * _slotSize.y)
                UpScroll();
        }
    }

    private void DownScroll()
    {
        if (_dataIndex + _totalShowCount < _dataList.Count)
        {
            for (int i = 0; i < _horShowCount; ++i)
            {
                SettingSlot(_slotIndex, _dataIndex + _totalShowCount);
                SetNextSlotIndex();
            }
            OnValueChangedScroll();
        }
    }

    private void UpScroll()
    {
        if (_dataIndex > 0)
        {
            for (int i = 0; i < _horShowCount; ++i)
            {
                SetPreviousSlotIndex();
                SettingSlot(_slotIndex, _dataIndex);
            }
            OnValueChangedScroll();
        }
    }

    private void SetNextSlotIndex()
    {
        //최상단 슬롯의 인덱스 갱신
        _dataIndex++;
        if (_slotIndex == _slotList.Count - 1)
            _slotIndex = 0;
        else
            ++_slotIndex;
    }

    private void SetPreviousSlotIndex()
    {
        //최상단 슬롯의 인덱스 갱신
        --_dataIndex;
        if (_slotIndex == 0)
            _slotIndex = _slotList.Count - 1;
        else
            _slotIndex = _slotIndex - 1;
    }

    private void SettingSlot(int slotIndex, int dataIndex)
    {
        if (slotIndex >= 0 && dataIndex >= 0 && slotIndex < _slotList.Count && dataIndex < _dataList.Count)
        {
            int horIndex = dataIndex % _horShowCount;
            int verIndex = dataIndex / _horShowCount;
            _slotList[slotIndex].transform.localPosition = new Vector3(horIndex * _slotSize.x + _leftPadding,
                                                                    -verIndex * _slotSize.y - _topPadding,
                                                                    _slotList[slotIndex].transform.localPosition.z);

            _slotList[slotIndex].SettingSlot(_dataList[dataIndex]);
        }
    }
    #endregion
    //=========================================================================================Scroll


    //UI Event=======================================================================================
    #region UI Event
    private void OnClickCreateRoomButton()
    {
        AudioManager.Ins.PlayClick();
    }

    private void OnClickFastRoomButton()
    {
        AudioManager.Ins.PlayClick();
        NetworkManager_Lobby.Ins.JoinRandomOrCreateRoom(_selectedMapID);
    }
	#endregion
	//=======================================================================================UI Event


	private void OnDestroy()
    {
        if (_scrollView)
            _scrollView.onValueChanged.RemoveAllListeners();
        if (_fastRoomButton)
            _fastRoomButton.onClick.RemoveAllListeners();
        if (_createRoomButton)
            _createRoomButton.onClick.RemoveAllListeners();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && _slotList.Count > 0)
        {
            if (_lastSlotSize != _roomSlotSize)
            {
                //슬롯 사이즈 변경
                _slotSample.GetComponent<RectTransform>().sizeDelta = new Vector2(_roomSlotSize.x, _roomSlotSize.y);
                SettingRoomListUIAndResize();
            }
            else if (_lastDataCount == _testDataCount)
            {
                //스크롤 옵션 변경
                SettingRoomListUIAndResize();
            }
            else
            {
                //데이터 카운트 변경
                SettingRoomListUIAndResetData();
            }
        }
	}
#endif
}
