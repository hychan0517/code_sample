using GlobalDefine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityUtility : MonoBehaviour
{
    /// <summary> 자식 오브젝트 전부 Destroy</summary>
    static public void DestroyChildObject(GameObject parent)
    {
        GameObject[] childs = parent.GetChildsObject();
        foreach (GameObject i in childs)
        {
            Destroy(i);
        }
    }

    static public List<Dictionary<string, string>> CSVRead(string strFileName)
    {
        try
        {
            List<Dictionary<string, string>> listData = new List<Dictionary<string, string>>();

            TextAsset resourceText = Resources.Load<TextAsset>(strFileName);
            if (resourceText == null)
            {
                LogManager.LogError(string.Format("$$$$$(error)CSV : file not found ({0})$$$$$", strFileName));

                var loadText = Resources.Load<TextAsset>(strFileName);
                if (loadText == null)
                {
                    LogManager.LogError(string.Format("$$$$$(error)CSV : 2 - file not found ({0})$$$$$", strFileName));
                }

                return listData;
            }

            // 한 라인 씩 저장
            string[] strLines = resourceText.text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None);
            if (strLines.Length <= 1)
                return listData;

            // 키 값 정보
            string[] strKeyTable = strLines[0].Split(',');

            for (int i = 1; i < strLines.Length; ++i)
            {
                if (strLines[i] == "" || strLines[i] == " ")
                    continue;

                string[] strElementTable = strLines[i].Split(',');

                Dictionary<string, string> element = new Dictionary<string, string>();

                for (int k = 0; k < strElementTable.Length; ++k)
                {
                    element.Add(strKeyTable[k], strElementTable[k]);
                }

                listData.Add(element);
            }

            return listData;
        }
        catch (Exception e)
        {
            Debug.Log("$$$$$(error) CSVRead Exception!: " + e.Message);
        }

        return null;
    }

    public static Vector2 GetRatioOfScreenResolution()
    {
        Vector2 _rateioforScreenResolution = new Vector2();
        const float width = 2560f;
        const float heigh = 1600f;
        _rateioforScreenResolution.x = 1.0f;
        _rateioforScreenResolution.y = 1.0f;

        RectTransform canvas = App.Ins.globalUI.canvasRectTransform;
        if (canvas != null)
        {
            _rateioforScreenResolution.x = canvas.sizeDelta.x / width;
            _rateioforScreenResolution.y = canvas.sizeDelta.y / heigh;
        }
        return _rateioforScreenResolution;
    }

    /// <summary> 런타임에서 첫 활성화 때 생성 후 캐싱하는 UI의 생성 </summary>
    /// <param name="variable"> 생성되는 UI 캐싱할 변수 </param>
    /// <param name="path"> Resources 경로 </param>
    /// <param name="parent"> 생성되는 UI 부모 객체 </param>
    /// <param name="siblingIndex"> 생성되는 UI Depth </param>
    public static void InstantiateUI<T>(ref T variable, string path, Transform parent, int siblingIndex) where T : MonoBehaviour, IInstantiatable
    {
        try
        {
            GameObject load = Resources.Load<GameObject>(path);
            if (load)
            {
                T component = load.GetComponent<T>();
                if (component)
                {
                    GameObject instantiate = Instantiate(load, parent);

                    if (parent.childCount < siblingIndex)
                        instantiate.transform.SetAsLastSibling();
                    else
                        instantiate.transform.SetSiblingIndex(siblingIndex);

                    variable = instantiate.GetComponent<T>();
                    variable.Init();
                }
                else
                {
                    LogManager.LogError(string.Format("InstantiateUI Type Error Variable : {0}", variable));
                }
            }
            else
            {
                LogManager.LogError(string.Format("InstantiateUI Path Error Path : {0}", path));
            }
        }
        catch(Exception e)
		{
            LogManager.LogError(string.Format("InstantiateUI Exception Type : {0}", typeof(T)));
            LogManager.LogError(e.ToString());
        }
    }
}
