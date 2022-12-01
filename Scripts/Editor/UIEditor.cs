using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIEditor : EditorWindow
{
#if UNITY_EDITOR
	public struct sLinkData
	{
		public string VariableName;
		public List<string> ParentPath;

		public string GetParentPath()
		{
			StringBuilder sb = new StringBuilder();
			foreach(string i in ParentPath)
			{
				sb.Append(string.Format("\"{0}\"", i));
				sb.Append(",");
			}

			if (sb.Length > 0)
				sb.Remove(sb.Length - 1, 1);
			return sb.ToString();
		}
	}

	public struct sTextData
	{
		public GameObject GameObject;
		public bool IsAuto;
		public float FontSize;
		public float MinSize;
		public float MaxSize;
		public Font Font;
		public string Text;
		public Color Color;
		public bool IsRay;
		public TextAnchor Anchor;
		public float LineSpacing;
		public bool IsOverFlow;
	}

	[MenuItem("Tools/UIAnchorscaling %q")]
	public static void ProcessUIAnchorScaler()
	{
		Object[] objects = Selection.objects;
		foreach(var i in objects)
		{
			if(i is GameObject)
			{
				GameObject gameObject = i as GameObject;
				if(gameObject)
				{
					UIAnchorScaler scaler = gameObject.AddComponent<UIAnchorScaler>();
					scaler.ProcessUIAnchorScaler();
				}
			}
		}
	}

	[MenuItem("Tools/TextToTextMeshPro")]
	public static void TextToTextMeshPro()
	{
		Object[] objects = Selection.objects;
		if (objects.Length > 0)
		{
			if (objects[0] is GameObject)
			{
				CheckPopup.OpenPopup(objects[0], ProcessTextToTextMeshPro);
			}
		}
	}

	public static void ProcessTextToTextMeshPro(Object root)
	{
		try
		{
			if (root is GameObject)
			{
				GameObject gameObject = root as GameObject;
				Debug.Log(string.Format("ProcessTextToTextMeshPro Root : {0}", gameObject.name));
				List<Text> texts = gameObject.GetComponentsInChildren<Text>(true).ToList();
				Debug.Log(string.Format("ProcessTextToTextMeshPro Count : {0}", texts.Count));
				List<sTextData> copyTextList = new List<sTextData>();

				for (int i = 0; i < texts.Count; ++i)
				{
					if (texts[i].transform.parent != null)
					{
						InputField[] inputs = texts[i].transform.parent.GetComponentsInParent<InputField>(true);
						if (inputs.Length > 0)
						{
							Debug.LogWarning(string.Format("ProcessTextToTextMeshPro Pass InputField : {0}", texts[i].gameObject.name));
							texts.Remove(texts[i]);
							--i;
							continue;
						}
					}
					sTextData tempData = new sTextData();
					tempData.GameObject = texts[i].gameObject;
					tempData.Color = texts[i].color;
					tempData.FontSize = texts[i].fontSize;
					tempData.Font = texts[i].font;
					tempData.Anchor = texts[i].alignment;
					tempData.IsAuto = texts[i].resizeTextForBestFit;
					tempData.IsRay = texts[i].raycastTarget;
					tempData.LineSpacing = texts[i].lineSpacing;
					tempData.Text = texts[i].text;
					tempData.MaxSize = texts[i].resizeTextMaxSize;
					tempData.MinSize = texts[i].resizeTextMinSize;
					if (texts[i].horizontalOverflow == HorizontalWrapMode.Overflow || texts[i].verticalOverflow == VerticalWrapMode.Overflow)
						tempData.IsOverFlow = true;
					else
						tempData.IsOverFlow = false;
					copyTextList.Add(tempData);
				}

				for (int i = 0; i < texts.Count; ++i)
				{
					DestroyImmediate(texts[i]);
				}

				for (int i = 0; i < copyTextList.Count; ++i)
				{
					copyTextList[i].GameObject.AddComponent<TextMeshProUGUI>();
					TextMeshProUGUI textMeshProUGUI = copyTextList[i].GameObject.GetComponent<TextMeshProUGUI>();
					textMeshProUGUI.enableAutoSizing = copyTextList[i].IsAuto;
					textMeshProUGUI.fontSize = copyTextList[i].FontSize;
					textMeshProUGUI.fontSizeMin = copyTextList[i].MinSize;
					textMeshProUGUI.fontSizeMax = copyTextList[i].MaxSize;
					textMeshProUGUI.text = copyTextList[i].Text;
					textMeshProUGUI.color = copyTextList[i].Color;
					textMeshProUGUI.raycastTarget = copyTextList[i].IsRay;
					textMeshProUGUI.lineSpacing = copyTextList[i].LineSpacing;
					if (copyTextList[i].IsOverFlow)
					{
						textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
					}
					else
					{
						textMeshProUGUI.overflowMode = TextOverflowModes.Truncate;
					}

					textMeshProUGUI.enableWordWrapping = true;

					switch (copyTextList[i].Anchor)
					{
						case TextAnchor.MiddleLeft:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Middle;
							break;
						case TextAnchor.MiddleRight:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Right;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Middle;
							break;
						case TextAnchor.MiddleCenter:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Center;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Middle;
							break;
						case TextAnchor.LowerLeft:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Bottom;
							break;
						case TextAnchor.LowerRight:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Right;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Bottom;
							break;
						case TextAnchor.LowerCenter:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Center;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Bottom;
							break;
						case TextAnchor.UpperLeft:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Left;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Top;
							break;
						case TextAnchor.UpperRight:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Right;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Top;
							break;
						case TextAnchor.UpperCenter:
							textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Center;
							textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Top;
							break;
						default:
							Debug.LogError(string.Format("ProcessTextToTextMeshPro Error NotFound Anchor Type : {0}", copyTextList[i].GameObject.name));
							break;
					}
				}

				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError("ProcessTextToTextMeshPro Error NotFound MonoBehaviour");
			}
		}
		catch(System.Exception e)
		{
			LogManager.LogError("ProcessTextToTextMeshPro Error");
			LogManager.LogError(e.ToString());
		}
	}

	[MenuItem("Tools/CreateUILink")]
	public static void ProcessUILink()
	{
		//사용방법
		//1. 최상단 오브젝트와 같은이름의 스크립트 만들어서 AddComponent
		//2. 최상단 오브젝트를 포함하여 변수로 만들 오브젝트 선택(중복선택가능, 순서상관X, 최상단 꼭 포함해야함!)
		//3. Tools/UILink클릭 후 Refresh까지 대기

		try
		{
			Object[] objects = Selection.objects;
			List<string> cacheNameList = new List<string>();
			List<sLinkData> buttonList = new List<sLinkData>();
			List<sLinkData> toggleList = new List<sLinkData>();
			List<sLinkData> textList = new List<sLinkData>();
			List<sLinkData> InputFieldList = new List<sLinkData>();
			List<sLinkData> imageList = new List<sLinkData>();
			List<sLinkData> objectList = new List<sLinkData>();
			StringBuilder completeStr = new StringBuilder();

			if (objects.Length > 0)
			{
				//루트오브젝트 찾기
				GameObject root = null;
				int minParentCount = int.MaxValue;
				foreach (Object i in objects)
				{
					if (i is GameObject)
					{
						GameObject gameObject = i as GameObject;
						int parentCount = gameObject.GetComponentsInParent<Transform>(true).Length;
						if (parentCount < minParentCount)
						{
							root = gameObject;
							minParentCount = parentCount;
						}
					}
				}
				Debug.Log(string.Format("ProcessUILink Root : {0}", root.name));
				string rootName = root.name.Replace(" ", "");


				//UI컴포넌트 위치찾기
				string[] res = Directory.GetFiles(Application.dataPath, string.Format("{0}.cs", rootName), SearchOption.AllDirectories);
				if(res.Length == 0)
				{
					//최상단 cs파일 찾을 수 없음
					Debug.LogError(string.Format("ProcessUILink Cannot Find Root Script : {0}", rootName));
					return;
				}
				else if(res.Length > 1)
				{
					//cs파일 이름 중복
					Debug.LogError(string.Format("ProcessUILink Find Root Script more than 2 : {0}", rootName));
					return;
				}
				

				//Using
				completeStr.Append("using System;\n");
				completeStr.Append("using TMPro;\n");
				completeStr.Append("using UnityEngine;\n");
				completeStr.Append("using UnityEngine.UI;\n\n");

				completeStr.Append(string.Format("public class {0} : MonoBehaviour\n", rootName));
				completeStr.Append("{\n");


				//변수선언
				foreach (Object i in objects)
				{
					if (i is GameObject)
					{
						if (i.name == root.name)
						{
							//루트 오브젝트
							continue;
						}

						//root 오브젝트 하위에있는 오브젝트인지 확인
						GameObject gameObject = i as GameObject;
						Transform parent = gameObject.transform;
						bool error = true;
						while (parent)
						{
							if (parent.name == root.name)
							{
								error = false;
								break;
							}
							else
							{
								parent = parent.parent;
							}
						}

						if (error)
						{
							//root 오브젝트 하위에 없음
							Debug.LogWarning(string.Format("ProcessUILink Error. Object is not child : {0}", i.name));
						}
						else
						{
							//root 오브젝트 하위에 있음
							string variableName = gameObject.name.Replace(" ", "");
							if (variableName.Length > 0)
							{
								variableName = string.Format("{0}{1}", char.ToLower(variableName[0]), variableName.Substring(1));
							}
							else
							{
								Debug.LogError(string.Format("variableName Error : {0}", gameObject.name));
								continue;
							}
							int overlapCount = 0;
							while (cacheNameList.Contains(variableName))
							{
								variableName = variableName.Insert(variableName.Length, string.Format("_{0}", ++overlapCount));
							}
							cacheNameList.Add(variableName);

							if (gameObject.GetComponent<Button>())
							{
								buttonList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private Button _{0};\n", variableName));
							}
							else if (gameObject.GetComponent<Toggle>())
							{
								toggleList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private Toggle _{0};\n", variableName));
							}
							else if (gameObject.GetComponent<TextMeshProUGUI>())
							{
								textList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private TextMeshProUGUI _{0};\n", variableName));
							}
							else if (gameObject.GetComponent<TMP_InputField>())
							{
								InputFieldList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private TMP_InputField _{0};\n", variableName));
							}
							else if (gameObject.GetComponent<Image>())
							{
								imageList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private Image _{0};\n", variableName));
							}
							else
							{
								objectList.Add(new sLinkData() { VariableName = variableName, ParentPath = GetParentPath(gameObject, root.name) });
								completeStr.Append("[SerializeField]\n");
								completeStr.Append(string.Format("private GameObject _{0};\n", variableName));
							}
						}
					}
				}


				//초기화
				completeStr.Append("\n\n");
				completeStr.Append("public void InitUIObject()\n");
				completeStr.Append("{");
				foreach (sLinkData i in buttonList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
					completeStr.Append("else\n");
					completeStr.Append(string.Format("_{0}.onClick.AddListener(OnClick{1});\n", i.VariableName, string.Format("{0}{1}", char.ToUpper(i.VariableName[0]), i.VariableName.Substring(1))));
				}

				foreach (sLinkData i in toggleList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
					completeStr.Append("else\n");
					completeStr.Append(string.Format("_{0}.onValueChanged.AddListener(OnValueChanged{1});\n", i.VariableName, string.Format("{0}{1}", char.ToUpper(i.VariableName[0]), i.VariableName.Substring(1))));
				}

				foreach (sLinkData i in textList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
				}

				foreach (sLinkData i in InputFieldList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
				}

				foreach (sLinkData i in imageList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
				}

				foreach (sLinkData i in objectList)
				{
					completeStr.Append(string.Format("\nif (_{0} == null)\n", i.VariableName));
					completeStr.Append(string.Format("LogManager.LogError(\"Cannot Find _{0}\");\n", i.VariableName));
				}
				completeStr.Append("}\n\n\n");

				//Setting 함수
				completeStr.Append("//UI Func====================================================================================\n");
				completeStr.Append("#region UI Fun\n");
				completeStr.Append(string.Format("public void Setting{0}()", rootName));
				completeStr.Append("\n{\n}\n");
				completeStr.Append("#endregion\n");
				completeStr.Append("//====================================================================================UI Func\n\n\n");

				//이벤트 콜백
				completeStr.Append("//UI Events==================================================================================\n");
				completeStr.Append("#region UI Events");
				foreach (sLinkData i in buttonList)
				{
					completeStr.Append("\n");
					completeStr.Append(string.Format("private void OnClick{0}()\n", string.Format("{0}{1}", char.ToUpper(i.VariableName[0]), i.VariableName.Substring(1))));
					completeStr.Append("{\nAudioManager.Ins.PlayClick();\n}\n");
				}
				foreach (sLinkData i in toggleList)
				{
					completeStr.Append("\n");
					completeStr.Append(string.Format("private void OnValueChanged{0}(bool isActive)\n", string.Format("{0}{1}", char.ToUpper(i.VariableName[0]), i.VariableName.Substring(1))));
					completeStr.Append("{\n}\n");
				}
				if (buttonList.Count == 0 && toggleList.Count == 0)
					completeStr.Append("\n");
				completeStr.Append("#endregion\n");
				completeStr.Append("//==================================================================================UI Events\n\n");
				completeStr.Append("\n");
				completeStr.Append("private void OnDestroy()\n{\n");
				foreach (sLinkData i in buttonList)
				{
					completeStr.Append(string.Format("if(_{0}) _{0}.onClick.RemoveAllListeners();\n", i.VariableName));
				}
				foreach (sLinkData i in toggleList)
				{
					completeStr.Append(string.Format("if(_{0}) _{0}.onValueChanged.RemoveAllListeners();\n", i.VariableName));
				}
				completeStr.Append("}\n");


				//Link
				completeStr.Append("private void OnValidate()\n{\nLink();\n}\n");
				completeStr.Append("public void Link()\n{\n");
				foreach (sLinkData i in buttonList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1}).GetComponent<Button>();\n", i.VariableName, i.GetParentPath()));
				}
				foreach (sLinkData i in toggleList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1}).GetComponent<Toggle>();\n", i.VariableName, i.GetParentPath()));
				}
				foreach (sLinkData i in textList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1}).GetComponent<TextMeshProUGUI>();\n", i.VariableName, i.GetParentPath()));
				}
				foreach (sLinkData i in InputFieldList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1}).GetComponent<TMP_InputField>();\n", i.VariableName, i.GetParentPath()));
				}
				foreach (sLinkData i in imageList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1}).GetComponent<Image>();\n", i.VariableName, i.GetParentPath()));
				}
				foreach (sLinkData i in objectList)
				{
					completeStr.Append(string.Format("_{0} = gameObject.GetChildObjectOptimize({1});\n", i.VariableName, i.GetParentPath()));
				}

				//Replace
				completeStr.Append(string.Format("string[] res = System.IO.Directory.GetFiles(Application.dataPath, \"{0}.cs\", System.IO.SearchOption.AllDirectories);\n", rootName));
				completeStr.Append("if (res.Length == 0)\n{\n");
				completeStr.Append(string.Format("Debug.LogError(\"ProcessUILink Cannot Find Root Script : {0}\");\n", rootName));
				completeStr.Append("return;\n}\n");
				completeStr.Append("else if (res.Length > 1)\n{\n");
				completeStr.Append(string.Format("Debug.LogError(\"ProcessUILink Find Root Script more than 2 : {0}\");\n", rootName));
				completeStr.Append("return;\n}\n");
				completeStr.Append("else\n{\n");
				completeStr.Append("System.IO.FileInfo fileInfo = new System.IO.FileInfo(res[0]);\n");
				completeStr.Append("if (fileInfo.Exists)\n{\n");
				completeStr.Append("string allText = System.IO.File.ReadAllText(fileInfo.FullName);\n");
				completeStr.Append("int startIndex = allText.IndexOf(\"private void OnValidate()\");\n");
				completeStr.Append("int endIndex = allText.LastIndexOf('}');\n");
				completeStr.Append("System.IO.File.WriteAllText(fileInfo.FullName, allText.Remove(startIndex, endIndex -1 -startIndex));\n");
				completeStr.Append("#if UNITY_EDITOR\n");
				completeStr.Append("UnityEditor.AssetDatabase.Refresh();\n#endif\n}\n");
				completeStr.Append("else\n{\n");
				completeStr.Append("Debug.LogError(string.Format(\"ProcessUILink Cannot Find Root Script: {0}\", res[0]));\n}\n");

				completeStr.Append("}\n");
				completeStr.Append("}\n");
				completeStr.Append("}\n");
				//파일 생성
				FileInfo fileInfo = new FileInfo(res[0]);
				if (fileInfo.Exists)
				{
					File.WriteAllText(fileInfo.FullName, completeStr.ToString());
					AssetDatabase.Refresh();
				}
				else
				{
					Debug.LogError(string.Format("ProcessUILink Cannot Find Root Script : {0}", res[0]));
				}
			}
		}
		catch(System.Exception e)
		{
			Debug.LogError("ProcessUILink Error");
			Debug.LogError(e.ToString());
		}
	}

	private static List<string> GetParentPath(GameObject childObject, string rootName)
	{
		List<string> temp = new List<string>();
		Transform parent = childObject.transform;
		while(parent != null && parent.gameObject.name != rootName)
		{
			temp.Add(parent.gameObject.name);
			parent = parent.parent;
		}

		temp.Reverse();
		return temp;
	}
#endif
}

public class CheckPopup : EditorWindow
{
#if UNITY_EDITOR
	private Vector2 _scrollPosition;
	private bool _isInit;
	private static Object _root;
	private static UnityEngine.Events.UnityAction<Object> _callback;

	static public void OpenPopup(Object root, UnityEngine.Events.UnityAction<Object> callback)
	{
		GetWindow<CheckPopup>(true, "TestMeshProUGUI Changer");
		_root = root;
		_callback = callback;
	}

	internal void OnGUI()
	{
		if (_isInit == false)
		{
			_isInit = true;
			position = new Rect(Screen.width * 2, Screen.height * 2, 520, 180);
		}
		_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
		EditorGUILayout.Space(10);
		var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		centeredStyle.alignment = TextAnchor.UpperCenter;
		GUILayout.Label(new GUIContent(string.Format("[{0}]\n\nText -> TextMshProUGUI로 변경하시겠습니까?\n변경된 내용은 되돌릴 수 없습니다.\n\n!!백업 후 진행 권장!!", _root.name)), centeredStyle);
		EditorGUILayout.EndScrollView();

		GUILayout.BeginHorizontal();
		EditorGUILayout.Space(5);
		if (GUILayout.Button("변경"))
		{
			_callback?.Invoke(_root);
			Close();
		}
		if (GUILayout.Button("취소"))
		{
			Close();
		}
		EditorGUILayout.Space(5);
		GUILayout.EndHorizontal();

		EditorGUILayout.Space(30);
	}
#endif
}