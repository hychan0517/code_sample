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
    private string _localPath;
    private Dictionary<string, IResourceLocation> _resourceLocator = new Dictionary<string, IResourceLocation>();
    private Dictionary<string, HashSet<string>> _awsResourceDataDict = new Dictionary<string, HashSet<string>>();
    private List<string> _downloadedMapList = new List<string>();

    public void Init()
	{
        CheckDirectory();
        SetCatalog();
    }

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

	private void SetCatalog()
    {
        string path = string.Format("{0}/{1}", BASE_URL, "catalog_heim.json");

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

                        if (_awsResourceDataDict.ContainsKey(loc.ToString()) == false)
                        {
                            _awsResourceDataDict.Add(loc.ToString(), tempHash);
                            LogManager.NetworkLog(string.Format("Locate : {0}", loc.ToString()));
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
                            CacheCheck();
                            Addressables.Release(handle);
                        };
                        return;
                    }
                }
            }
        }
        CacheCheck();
    }

    private void CacheCheck()
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
