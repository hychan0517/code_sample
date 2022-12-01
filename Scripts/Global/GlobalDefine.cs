using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GlobalDefine
{
	static public class Define
	{
		public const int RANDOM_POOL_COUNT = 1024;
        public static WaitForEndOfFrame WAIT_FOR_END_OF_FRAME = new WaitForEndOfFrame();
        public static WaitForSeconds WAIT_FOR_SECONS_POINT_FIVE = new WaitForSeconds(0.5f);
        public static WaitForSeconds WAIT_FOR_SECONS_POINT_ONE = new WaitForSeconds(0.1f);
        public static WaitForSeconds SKILL_LIMIT_SECONDS = new WaitForSeconds(60);
        public static float DEFAULT_ATTACK_SPEED = 1;
        public static float MONSTER_HP_DURATION = 0.3f;
        public static readonly string MAP_RESOURCE_BASE = "Map_";
        public static readonly string MONSTER_LAYER = "Monster";
        /// <summary> 태그와 레이어 같은 이름으로 사용하는데 추후에 변경되면 분리해야함 </summary>
        public static readonly string LIVE_MONSTER_LAYER = "Monster";
        public static readonly string DEAD_MONSTER_LAYER = "DeadMonster";

        //Path====================================================================================================
        #region Path
        public static readonly string SKILL_PREFAB_PATH = "Prefabs/Skill/Skill_{0}";
        public static readonly string HIT_PREFAB_PATH = "Prefabs/Hit/Hit_{0}";
        public static readonly string MONSTER_PREFAB_PATH = "Prefabs/Monster/{0}";
		#endregion
		//====================================================================================================Path
		public static Color ColorHexToRGBA(string hexColor)
        {
            if (hexColor[0] != '#')
                hexColor = "#" + hexColor;
            if (hexColor.Length == 7)
                hexColor = hexColor + "FF";

            ColorUtility.TryParseHtmlString(hexColor, out Color c);
            return c;
        }
    }
	static public class Rand // 만분율 기준 0~9999까지 저장
	{
        static private int _index = 0;
        static private int[] _randomArr = new int[Define.RANDOM_POOL_COUNT];

		static Rand()
		{
			for (int i = 0; i < Define.RANDOM_POOL_COUNT; ++i)
			{
				_randomArr[i] = UnityEngine.Random.Range(0, 10000);
			}
		}

        static private int nIndex
		{
			get
			{
				int nTemp = _index++;

				if (_index >= Define.RANDOM_POOL_COUNT) { _index = 0; }

				return nTemp;
			}
		}

        static public int Random() { return _randomArr[nIndex]; }

        static public bool Percent(float a_nPercent) { return _randomArr[nIndex] <= (a_nPercent * 100); }
        static public bool Permile(float a_nPermile) { return _randomArr[nIndex] <= (a_nPermile * 10); }
        static public bool Permilad(int a_nPermilad) { return _randomArr[nIndex] <= a_nPermilad; }
        static public int Range(int a_nStart, int a_nEnd)
		{
			if (a_nStart > a_nEnd)
			{
				int nTemp = a_nStart;
				a_nStart = a_nEnd;
				a_nEnd = nTemp;
			}

			return (Random() % (a_nEnd - a_nStart)) + a_nStart;
		}
	}

	static public class AESEncrypt
	{
		private static SHA256Managed sha256Managed = new SHA256Managed();
		private static RijndaelManaged aes = new RijndaelManaged();
		private static readonly string privateKey = "R[=n#m!*h@~";

		static AESEncrypt()
		{
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
		}

        // 암호화
        static public byte[] AESEncrypt256(string strEncryptData)
		{
			byte[] encryptData = Encoding.UTF8.GetBytes(strEncryptData);
			var salt = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(privateKey.Length.ToString()));
			//var PBKDF2Key = new Rfc2898DeriveBytes(privateKey, salt, 65535);    //반복 65535
			var PBKDF2Key = new Rfc2898DeriveBytes(privateKey, salt, 5);    //반복 5
			var secretKey = PBKDF2Key.GetBytes(aes.KeySize / 8);
			var iv = PBKDF2Key.GetBytes(aes.BlockSize / 8);
			byte[] xBuff = null;
			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, aes.CreateEncryptor(secretKey, iv), CryptoStreamMode.Write))
				{
					cs.Write(encryptData, 0, encryptData.Length);
				}
				xBuff = ms.ToArray();
			}
			return xBuff;
		}

        // 복호화
        static public string AESDecrypt256(byte[] decryptData)
		{
			var salt = sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(privateKey.Length.ToString()));
			//var PBKDF2Key = new Rfc2898DeriveBytes(privateKey, salt, 65535);    //반복 65535
			var PBKDF2Key = new Rfc2898DeriveBytes(privateKey, salt, 5);    //반복 5
			var secretKey = PBKDF2Key.GetBytes(aes.KeySize / 8);
			var iv = PBKDF2Key.GetBytes(aes.BlockSize / 8);
			byte[] xBuff = null;
			using (var ms = new MemoryStream())
			{
				using (var cs = new CryptoStream(ms, aes.CreateDecryptor(secretKey, iv), CryptoStreamMode.Write))
				{
					cs.Write(decryptData, 0, decryptData.Length);
				}
				xBuff = ms.ToArray();
			}
			//return xBuff;
			return Encoding.UTF8.GetString(xBuff);
		}
	}
	static public class FileIO
	{
        static public List<Dictionary<string, string>> CSVRead(string strFileName)
		{
			List<Dictionary<string, string>> listData = new List<Dictionary<string, string>>();

			TextAsset resourceText = Resources.Load<TextAsset>(strFileName);
			if (resourceText == null)
			{
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
        static public string CreateDataFolder(string dirName)
        {
            string path = Path.Combine(Application.persistentDataPath, dirName);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }

        static public void DeleteAllFile(string DirFullPath)
        {
            string[] fileList = Directory.GetFiles(DirFullPath);
            for (int i = 0; i < fileList.Length; i++)
            {
                string str = fileList[i];
                string filePath = Path.Combine(DirFullPath, str);
                FileInfo fi = new FileInfo(filePath);
                try
                {
                    if (fi.Exists)
                    {
                        fi.Delete();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error Delete File : {0}", e));
                    break;
                }
            }
        }
        static public void DeleteFile(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            try
            {
                if (fi.Exists)
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error Delete File : {0}", e));
            }
        }

        static public IEnumerable<string> GetFiles(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return null;
            try
            {
                return Directory.GetFiles(dirPath).Select(file => Path.GetFileName(file));
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error FileSave : {0}", e));
            }
            return null;
        }

        static public string[] GetFilesFullPath(string dirPath)
        {
            if (string.IsNullOrWhiteSpace(dirPath))
                return null;
            try
            {
                return Directory.GetFiles(dirPath);
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error FileSave : {0}", e));
            }
            return null;
        }

        static public void FileWrite(string saveFilePath, byte[] bytes)
        {
            if (bytes.Length <= 0)
            {
                LogManager.Log(string.Format("byte == null"));
                return;
            }
            try
            {
                FileStream fs = new FileStream(saveFilePath, FileMode.Create, FileAccess.ReadWrite);
                fs.Lock(0, bytes.Length);
                fs.Write(bytes, 0, bytes.Length);
                fs.Unlock(0, bytes.Length);
                fs.Close();
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error FileSave : {0}", e));
            }
        }


        static public IEnumerator FileRead(string fileFullPath, Action<byte[]> endCallback)
        {
            if (string.IsNullOrWhiteSpace(fileFullPath))
            {
                Debug.LogError(string.Format("FileRead => fileFullPath == null"));
                endCallback?.Invoke(null);
                yield break;
            }

            if (IsAndroid())
            {
                byte[] bytes = null;
                UnityWebRequest www = null;
                bool isError = false;
                string strError = string.Empty;

                try
                {
                    string localPath = string.Format("file://{0}", fileFullPath);
                    www = UnityWebRequest.Get(localPath);
                    UnityWebRequestAsyncOperation request = www.SendWebRequest();
                    float _timeOut = 0;
                    while (!request.isDone || !www.isDone)
                    {
                        yield return Define.WAIT_FOR_SECONS_POINT_FIVE;
                        if (_timeOut > 10)
                        {
                            isError = true;
                            strError = "FileRead timeOut!";
                            break;
                        }
                        _timeOut += Time.deltaTime;
                    };

                    if (www == null)
                    {
                        isError = true;
                        strError = "FileRead www is null";
                    }
                    else if (www.result != UnityWebRequest.Result.Success)
                    {
                        isError = true;
                        strError = www.error;
                    }
                    else
                    {
                        bytes = www.downloadHandler.data;
                    }
                }
                finally
                {
                    www.Dispose();
                }


                if (isError == true)
                {
                    string str = string.Format("****************FileRead Error,  ErrorMsg : {0}****************", strError);
                    Debug.LogError(str);
                    endCallback?.Invoke(null);
                }
                else
                {
                    if (bytes != null && bytes.Length > 0)
                    {
                        endCallback?.Invoke(bytes);
                    }
                    else
                    {
                        Debug.LogError("****************File is null****************");
                        endCallback?.Invoke(null);
                        yield break;
                    }
                }
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(fileFullPath);
                float _timeOut = 0;
                while (bytes == null)
                {
                    yield return Define.WAIT_FOR_SECONS_POINT_FIVE;
                    if (_timeOut > 10)
                        yield break;
                    _timeOut += Time.deltaTime;
                }

                if (bytes != null)
                {
                    if (bytes.Length > 0)
                    {
                        endCallback?.Invoke(bytes);
                    }
                    else
                    {
                        Debug.LogError("****************File is null****************");
                        endCallback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError("****************File is null****************");
                    endCallback?.Invoke(null);
                }
            }
        }
        static public bool IsAndroid()
        {
            if (Application.platform == RuntimePlatform.Android &&
                Application.platform != RuntimePlatform.OSXEditor &&
                Application.platform != RuntimePlatform.WindowsEditor)
                return true;
            return false;
        }

        static public T ParserJsonToObject<T>(string strJson)
        {
            return JsonUtility.FromJson<T>(strJson);
        }
    }
}