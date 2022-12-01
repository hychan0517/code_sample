using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    /// <summary> 대쉬보드의 Android Game ID </summary>
    [SerializeField] string _androidGameId;
    /// <summary> 대쉬보드의 IOS Game ID </summary>
    [SerializeField] string _iOSGameId;
    /// <summary> Android 기본 광고 </summary>
    string _androidAdUnitId = "Interstitial_Android";
    /// <summary> IOS 기본 광고 </summary>
    string _iOsAdUnitId = "Interstitial_iOS";
    ///// <summary> Android 보상형 광고 </summary>
    //[SerializeField] string _androidAdUnitId = "Rewarded_Android";
    ///// <summary> IOS 보상형 광고 </summary>
    //[SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    bool _testMode;
    private string _gameId;
    string _adUnitId;

    void Awake()
    {
        InitializeAds();
    }

    private void InitializeAds()
    {
#if DEBUG_MODE
        _testMode = true;
#else
        _testMode = false;
#endif

#if !UNITY_EDITOR

#if UNITY_ANDROID
        _gameId = _androidGameId;
        _adUnitId = _androidAdUnitId;
#elif UNITY_IOS
        _gameId = _iOSGameId;
        _adUnitId = _iOsAdUnitId;
#endif
        Advertisement.Initialize(_gameId, _testMode, this);

#endif
    }

    //Init====================================================================================================
    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
    //====================================================================================================Init

    //Ads=====================================================================================================
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // Show the loaded content in the Ad Unit:
    public void ShowAd()
    {
        // Note that if the ad content wasn't previously loaded, this method will fail
        Debug.Log("Showing Ad: " + _adUnitId);
        Advertisement.Show(_adUnitId, this);
    }

    // Implement Load Listener and Show Listener interface methods: 
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) { }
    //=====================================================================================================Ads
}
