using GlobalDefine;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class AddressableManager
{
	private const string BASE_URL = "https://heimdefence.s3.ap-northeast-2.amazonaws.com";
    private const string CATALOG_NAME = "catalog_heim.json";
    private string _localPath;
    private Dictionary<string, IResourceLocation> _resourceLocator = new Dictionary<string, IResourceLocation>();
    private List<string> _downloadedMapList = new List<string>();

    public void Init()
	{
        CheckDirectory();
        SetCatalog();
    }

    /// <summary> 리소스 로컬 경로 Directory체크(없을경우 생성) </summary>
	private void CheckDirectory()
	{
        _localPath = string.Format("{0}/map_resource_folder", Application.persistentDataPath);

        DirectoryInfo directory = new DirectoryInfo(_localPath);

        if (!directory.Exists)
        {
            LogManager.Log("Create AdressableManager Directory");
            directory.Create();
        }

        Caching.currentCacheForWriting = Caching.AddCache(_localPath);
    }

    /// <summary> 빌드된 catalog 데이터 캐싱 </summary>
	private void SetCatalog()
    {
        string path = string.Format("{0}/{1}", BASE_URL, CATALOG_NAME);

        var temp = Addressables.LoadContentCatalogAsync(path, true);

        temp.Completed += (handle) =>
        {
            foreach (var loc in handle.Result.Keys)
            {
                if (handle.Result.Locate(loc.ToString(), typeof(object), out var locations))
                {
					if (loc.ToString().Contains(Define.MAP_RESOURCE_BASE))
					{
						HashSet<string> tempHash = new HashSet<string>();

						for (int i = 0; i < locations.Count; i++)
						{
							tempHash.Add(locations[i].PrimaryKey);
                            if (_resourceLocator.ContainsKey(locations[i].PrimaryKey) == false)
                            {
                                _resourceLocator.Add(locations[i].PrimaryKey, locations[i]);
                                LogManager.NetworkLog(string.Format("PrimaryKey : {0}", locations[i].PrimaryKey));
                            }
						}
					}
				}
            }
            CheckBuiltinData();
        };
    }

    private void CheckBuiltinData()
    {
        foreach (var val in _resourceLocator.Values)
        {
            if (val.HasDependencies)
            {
                foreach (var dep in val.Dependencies)
                {
                    if (dep.PrimaryKey.Contains("unitybuiltinshaders"))
                    {
                        Addressables.DownloadDependenciesAsync(dep.PrimaryKey).Completed += (handle) =>
                        {
                            CheckCache();
                            Addressables.Release(handle);
                        };
                        return;
                    }
                }
            }
        }
        CheckCache();
    }

    /// <summary> 현재 버전의 데이터와 로컬 데이터 비교 </summary>
    private void CheckCache()
    {
        HashSet<string> hashName = new HashSet<string>();
        HashSet<string> bundleName = new HashSet<string>();

        foreach (var val in _resourceLocator.Values)
        {
            if (val.HasDependencies)
            {
                foreach (var dep in val.Dependencies)
                {
                    if (dep.Data is AssetBundleRequestOptions)
                    {
                        hashName.Add((dep.Data as AssetBundleRequestOptions).Hash);
                        bundleName.Add((dep.Data as AssetBundleRequestOptions).BundleName);
                    }
                }
            }
        }

        var dirInfo = new DirectoryInfo(_localPath);
        if (dirInfo.Exists)
        {
            foreach (var dir in dirInfo.GetDirectories())
            {
                if (bundleName.Contains(dir.Name) == false) // Bundle 이름이 바뀐 경우
                {
                    LogManager.Log(string.Format("파일 삭제 경로 : {0}", dir.FullName));
                    Directory.Delete(dir.FullName, true);
                    continue;
                }

                foreach (var _dir in dir.GetDirectories()) // Hash 이름이 바뀐 경우
                {
                    if (hashName.Contains(_dir.Name) == false)
                    {
                        LogManager.Log(string.Format("파일 삭제 경로 : {0}",dir.FullName));
                        Directory.Delete(_dir.FullName, true);
                    }
                }
            }
        }

        CheckDownloadedMap();
    }

    /// <summary> 다운로드 완료된 맵 리스트 캐싱 </summary>
    private void CheckDownloadedMap()
	{
        foreach (var key in _resourceLocator.Keys)
        {
            Addressables.GetDownloadSizeAsync(key).Completed += (handle) =>
            {
                if (handle.Result == 0f)
                {
                    _downloadedMapList.Add(key);
                    LogManager.Log(string.Format("Downloaded Map : {0}", key));
                }
                Addressables.Release(handle);
            };
        }
    }
}
