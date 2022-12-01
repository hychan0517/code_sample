using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BadWordDataManager
{
	private int _localVersion;
	private List<string> _listData = new List<string>();

	List<Tuple<int, char>> stringDictOrg = new List<Tuple<int, char>>();
	List<Tuple<int, char>> stringDictKor = new List<Tuple<int, char>>();
	List<Tuple<int, char>> stringDictEng = new List<Tuple<int, char>>();

	private const string BAD_WORD_REPAIRE = "**";

	public bool IsBadword(string str)
	{
		string repairWord = GetRepairWord(str);
		return !string.Equals(str, repairWord);
	}

	public string GetRepairWord(string str)
	{
		try
		{
			string originStr = str.ToUpper();

			stringDictOrg.Clear();
			stringDictKor.Clear();
			stringDictEng.Clear();

			//인덱스를 키값으로 가지는 정규식 문자열
			for (int i = 0; i < originStr.Length; ++i)
			{
				if (char.IsWhiteSpace(originStr[i]) == false)
					stringDictOrg.Add(new Tuple<int, char>(i, originStr[i]));

				if ((0xAC00 <= originStr[i] && originStr[i] <= 0xD7A3))
					stringDictKor.Add(new Tuple<int, char>(i, originStr[i]));
				else if ((0x61 <= originStr[i] && originStr[i] <= 0x7A) || (0x41 <= originStr[i] && originStr[i] <= 0x5A))
					stringDictEng.Add(new Tuple<int, char>(i, originStr[i]));
			}

			//정규식 문자열
			//위 계산과 결과는같음
			string madeStrOrg = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "").ToUpper();
			string madeStrKor = System.Text.RegularExpressions.Regex.Replace(str, @"[^가-힣]+", "");
			string madeStrEng = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-zA-Z]+", "").ToUpper();

			//문자열 처리과정 없이 비속어있는지 1차 확인
			string badwordOrg = _listData.Find((x) =>
			{
				if (madeStrOrg.Contains(x) == true)
					return true;
				return false;
			});

			string badwordKor = _listData.Find((x) =>
			{
				if (madeStrKor.Contains(x) == true)
					return true;
				return false;
			});

			string badwordEng = _listData.Find((x) =>
			{
				if (madeStrEng.Contains(x) == true)
					return true;
				return false;
			});

			//대소문자 원본을 살리기위해, 비속어외 문자를 살리기위해 인덱스로 비속어 제외한 문자열 가공
			if (string.IsNullOrWhiteSpace(badwordOrg) == false)
			{
				//원본 비속어
				int index = IndexOfKorean(madeStrOrg, badwordOrg);
				//원본문자열 욕설 시작 인덱스
				int startIndex = stringDictOrg[index].Item1;
				//원본문자열 숫자,특수문자포함 종료 인덱스
				int endIndex = stringDictOrg[index + badwordOrg.Length - 1].Item1;

				//숫자,특수문자포함 비속어 제거
				str = str.Remove(startIndex, endIndex - startIndex + 1);
				//비속어 위치에 변환된 문자열 추가
				str = str.Insert(startIndex, BAD_WORD_REPAIRE);

				//비속어 없을때까지 반복
				return GetRepairWord(str);
			}
			else if (string.IsNullOrWhiteSpace(badwordKor) == false)
			{
				//한글 비속어
				int index = madeStrKor.IndexOf(badwordKor);
				//원본문자열 욕설 시작 인덱스
				int startIndex = stringDictKor[index].Item1;
				//원본문자열 숫자,특수문자포함 종료 인덱스
				int endIndex = stringDictKor[index + badwordKor.Length - 1].Item1;

				//숫자,특수문자포함 비속어 제거
				str = str.Remove(startIndex, endIndex - startIndex + 1);
				//비속어 위치에 변환된 문자열 추가
				str = str.Insert(startIndex, BAD_WORD_REPAIRE);

				//비속어 없을때까지 반복
				return GetRepairWord(str);
			}
			else if (string.IsNullOrWhiteSpace(badwordEng) == false)
			{
				//영어 비속어
				int index = madeStrEng.IndexOf(badwordEng);
				//원본문자열 욕설 시작 인덱스
				int startIndex = stringDictEng[index].Item1;
				//원본문자열 숫자,특수문자포함 종료 인덱스
				int endIndex = stringDictEng[index + badwordEng.Length - 1].Item1;

				//숫자,특수문자포함 비속어 제거
				str = str.Remove(startIndex, endIndex - startIndex + 1);
				//비속어 위치에 변환된 문자열 추가
				str = str.Insert(startIndex, BAD_WORD_REPAIRE);

				//비속어 없을때까지 반복
				return GetRepairWord(str);
			}
			else
			{
				return str;
			}
		}
		catch (Exception e)
		{
			LogManager.LogError("GetRepairWord Error");
			LogManager.LogError(e.ToString());
			return string.Empty;
		}
	}

	private int IndexOfKorean(string origin, string badWord)
	{
		//자음 IndexOf잘안됨.. 첫번째 문자만 비교하는느낌.. 어쩔수없지 탐색으로 구현
		for(int i = 0; i <= origin.Length - badWord.Length; ++i)
		{
			string cutStr = origin.Substring(i, badWord.Length);
			if (string.Equals(cutStr, badWord))
				return i;
		}
		return -1;
	}

	//Local============================================================================
	#region Local
	public void LoadLocalData()
	{
		CheckDirectory();
		LoadLocalVersion();
		//버전 로드 이후 로컬에 저장된 Data 캐싱하여 비정상으로 종료되면 버전정보 초기화하여 서버에서 다시 받을 수 있도록함
		LoadBadWordToLocalDataJson();
	}

	private string GetDirectoryPath()
	{
		return string.Format("{0}/_YafitCycleSystem", Application.persistentDataPath);
	}

	private void CheckDirectory()
	{
		DirectoryInfo directory = new DirectoryInfo(GetDirectoryPath());
		if (directory.Exists == false)
		{
			LogManager.DebugLog("Create SYSTEM Directory");
			directory.Create();
		}
	}

	private void DeleteVersionFile()
	{
		File.Delete(GetVersionFilePath());
	}

	private void DeleteBadWordDataFile()
	{
		File.Delete(GetBadWordDataPath());
	}

	private string GetVersionFilePath()
	{
		return string.Format("{0}/BadWordVersion_{1}.txt", GetDirectoryPath(), App.Ins.GetConnectServerType());
	}

	private string GetBadWordDataPath()
	{
		return string.Format("{0}/Badword.csv", GetDirectoryPath());
	}


	private void LoadLocalVersion()
	{
		string filePath = GetVersionFilePath();
		FileInfo fileInfo = new FileInfo(filePath);
		if (fileInfo.Exists)
		{
			try
			{
				string versionStr = File.ReadAllText(filePath);
				string[] versionStrArr = versionStr.Split('=');
				if (versionStrArr.Length == 2)
				{
					if (int.TryParse(versionStrArr[1], out _localVersion) == false)
					{
						//데이터 오류
						LogManager.LogError("BadWord LoadLocalVersion Data Err");
						_localVersion = 0;
					}
				}
			}
			catch (Exception e)
			{
				//시스템 오류
				LogManager.LogError("BadWord LoadLocalVersion Err");
				LogManager.LogError(e.ToString());
				_localVersion = 0;
			}
		}
		else
		{
			//파일 없음
			_localVersion = 0;
			LogManager.LogError("Cannot Find BadWord Data Version File");
		}
	}

	public void LoadBadWordToLocalDataJson()
	{
		CheckDirectory();

		LogManager.Log("LoadBadWordToLocalData");
		string filePath = GetBadWordDataPath();
		_listData.Clear();
		if (File.Exists(filePath))
		{
			string jsonStr = File.ReadAllText(filePath);
			if (string.IsNullOrWhiteSpace(jsonStr) == false)
			{
				try
				{
					if (TryParserBadWordData(jsonStr) == false)
					{
						//파싱에러
						_localVersion = 0;
						LogManager.LogError("SetS3BadWordData Data Parse error");
						ReadTextCSV();
					}
				}
				catch (Exception e)
				{
					//파싱에러
					_localVersion = 0;
					LogManager.LogError("SetS3BadWordData Data Err");
					LogManager.LogError(e.ToString());
				}
			}
			else
			{
				//파일 없음
				_localVersion = 0;
				LogManager.LogError("LoadBadWordToLocalData jsonStr == empty");
			}
		}
		else
		{
			//파일 없음
			_localVersion = 0;
			LogManager.LogError("LoadBadWordToLocalData Err Cannot Find File");
		}
		//불러올 때 문제 발생시 서버에서 다시 받아오기위해 로컬 버전정보 초기화
	}

	#endregion
	//============================================================================Local

	//Server===========================================================================
	#region Server
	public bool CheckVersion(int serverVersion)
	{
		//서버의 데이터 버전과 로컬버전 비교할 때 서버에서 받은 최신버전은 캐싱
		//로컬 버전과 서버 버전이 달라서 데이터를 다시 저장할 때 캐싱된 서버 버전을 로컬에 기록함
		return _localVersion == serverVersion ? true : false;
	}


	/// <summary> 서버에서 받은 최신 데이터 저장 </summary>
	public void SetS3BadWordData(string dataStr)
	{
		_listData.Clear();
		if (string.IsNullOrWhiteSpace(dataStr) == false)
		{
			try
			{
				if (TryParserBadWordData(dataStr))
				{
					// 로컬 파일에 비속어 파일을 기록한다
					if (SaveS3BadWordData(dataStr))
					{
						//비속어 저정 성공
						//로컬 파일에 비속어 버전 파일을 기록한다
						SaveBadWordVersion(0);
					}
					else
					{
						//파일 저장실패시 버전 파일 초기화
						SaveBadWordVersion(0);
						ReadTextCSV();
					}
				}
				else
				{
					//데이터 파싱에 실패하면 버전초기화, 클라이언트 데이터로드
					SaveBadWordVersion(0);
					ReadTextCSV();
				}
			}
			catch (Exception e)
			{
				SaveBadWordVersion(0);
				ReadTextCSV();
				LogManager.LogError("SetS3BadWordData Data Err");
				LogManager.LogError(e.ToString());
			}
		}
		else
		{
			SaveBadWordVersion(0);
			ReadTextCSV();
		}
	}
	#endregion
	//===========================================================================Server

	private bool TryParserBadWordData(string dataStr)
	{
		try
		{
			_listData.Clear();
			if (string.IsNullOrWhiteSpace(dataStr) == false)
			{
				string[] strLines = dataStr.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None);
				if (strLines.Length <= 0)
				{
					return false;
				}

				for (int i = 0; i < strLines.Length; i++)
				{
					if (strLines[i] == "" || strLines[i] == " ")
						continue;
					_listData.Add(strLines[i].ToUpper());
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
	}

	// 로컬 파일에 비디오 버전 파일을 기록한다
	public void SaveBadWordVersion(int version)
	{
		CheckDirectory();

		string filePath = GetVersionFilePath();

		FileInfo fi = new FileInfo(GetVersionFilePath());
		if (fi.Exists)
			DeleteVersionFile();

		try
		{
			FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			StreamWriter sw = new StreamWriter(file);
			sw.WriteLine("Version={0}", version);
			sw.Close();
			file.Close();
		}
		catch (Exception e)
		{
			LogManager.LogError("save Badword version Error : " + e);
		}
	}


	private bool SaveS3BadWordData(string dataStr)
	{
		CheckDirectory();

		string filePath = GetBadWordDataPath();

		try
		{
			FileInfo fileInfo = new FileInfo(filePath);
			if (fileInfo.Exists)
				DeleteBadWordDataFile();


			FileStream create = fileInfo.Create();
			create.Close();

			if (string.IsNullOrEmpty(dataStr) == false)
			{
				FileStream fs = fileInfo.OpenWrite();
				StreamWriter sw = new StreamWriter(fs);
				sw.WriteLine(dataStr);
				sw.Dispose();
				fs.Dispose();
				return true;
			}
			else
			{
				LogManager.DebugLog("save BadWord data Error");
				return false;
			}
		}
		catch (Exception e)
		{
			LogManager.LogError("save BadWord data Error : " + e.ToString());
			return false;
		}

	}

	/// <summary> 서버에서 받는 로직 실패시 로컬데이터 로드 </summary>
	public void ReadTextCSV()
	{
		LogManager.StartLog("Read Client BadWord Table");
		CSVRead("Data/Badword");
		LogManager.EndLog("Read Client BadWord Table");
	}

	private void CSVRead(string strFileName)
	{
		if (_listData == null)
			_listData = new List<string>();

		TextAsset resourceText = Resources.Load<TextAsset>(strFileName);
		if (resourceText == null)
		{
			LogManager.LogError(string.Format("$$$$$(error)CSV : file not found({0})", strFileName));
			return;
		}

		// 한 라인 씩 저장
		string[] strLines = resourceText.text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None);
		if (strLines.Length <= 0)
			return;

		_listData.Clear();

		for (int i = 0; i < strLines.Length; i++)
		{
			if (strLines[i] == "" || strLines[i] == " ")
				continue;
			_listData.Add(strLines[i].ToUpper());
		}
	}
}
