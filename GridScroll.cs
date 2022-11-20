using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridScroll : MonoBehaviour
{
    [Header("Scroll")]
    [SerializeField]
    private ScrollRect _scrollView;
    [SerializeField]
    private RectTransform _scrollViewRect;
    [SerializeField]
    private RectTransform _roomScrollContent;
    [SerializeField]
    private GameObject _slotSample;

    private List<string> _dataList = new List<string>();
    private List<GameObject> _slotList = new List<GameObject>();
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

    private void InitUIObject()
    {
        if (_scrollView)
            _scrollView.onValueChanged.AddListener((_x) => { OnValueChangedScroll(); });
        else
            Debug.LogError("Cannot Find _scrollView");

        if (_scrollViewRect == null)
            Debug.LogError("Cannot Find _mapContent");

        if (_roomScrollContent == null)
            Debug.LogError("Cannot Find _roomScrollContent");

        if (_slotSample)
            _slotSample.SetActive(false);
        else
            Debug.LogError("Cannot Find _slotSample");
    }

    private void Awake()
    {
        InitUIObject();
    }

    private void Start()
    {
        TestData();
        SettingScroll();
    }


    //UI Func========================================================================================
    #region UI Func
    /// <summary> 데이터 카운트 변경 </summary>
    private void SettingScrollUIAndResetData()
    {
        _dataList.Clear();
        _slotList.Clear();
        DestroyChildObject(_roomScrollContent.gameObject);
        TestData();
        SettingScroll();
    }

    /// <summary> 스크롤 옵션 변경 </summary>
    public void SettingScrollUIAndResize()
    {
        _slotList.Clear();
        DestroyChildObject(_roomScrollContent.gameObject);
        SettingScroll();
    }

    private void TestData()
    {
        _dataList.Clear();
        for (int i = 0; i < _testDataCount; ++i)
        {
            _dataList.Add(i.ToString());
        }
        _lastDataCount = _testDataCount;
    }

    private void DestroyChildObject(GameObject parent)
    {
        GameObject[] childs = new GameObject[parent.transform.childCount];
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            childs[i] = parent.transform.GetChild(i).gameObject;
        }

        foreach (GameObject i in childs)
        {
            Destroy(i);
        }
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
                _slotList.Add(slot);
            }
        }
    }

    private void ResetSlot()
    {
        foreach (var i in _slotList)
        {
            if(i)
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

    /// <summary> 최상단 정보로 초기화 </summary>
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
            //슬롯 위치 조정
            int horIndex = dataIndex % _horShowCount;
            int verIndex = dataIndex / _horShowCount;
            _slotList[slotIndex].transform.localPosition = new Vector3(horIndex * _slotSize.x + _leftPadding,
                                                                    -verIndex * _slotSize.y - _topPadding,
                                                                    _slotList[slotIndex].transform.localPosition.z);

            //TESTCODE
            _slotList[slotIndex].SetActive(true);
            _slotList[slotIndex].GetComponentInChildren<Text>(true).text = dataIndex.ToString();
            _slotList[slotIndex].name = dataIndex.ToString();
        }
    }
    #endregion
    //=========================================================================================Scroll


    private void OnDestroy()
    {
        if (_scrollView)
            _scrollView.onValueChanged.RemoveAllListeners();
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
                SettingScrollUIAndResize();
            }
            else if (_lastDataCount == _testDataCount)
            {
                SettingScrollUIAndResize();
            }
            else
            {
                SettingScrollUIAndResetData();
            }
        }
    }
#endif
}
