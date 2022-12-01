using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoadManager : MonoBehaviour
{
	#region Singleton
	private static SceneLoadManager _sceneLoader;
	public static SceneLoadManager GetInstance()
	{
		if (_sceneLoader == null)
		{
			_sceneLoader = (SceneLoadManager)FindObjectOfType(typeof(SceneLoadManager));
			if (!_sceneLoader)
			{
				GameObject temp = new GameObject();
				temp.name = "SceneLoadManager";
				temp.AddComponent<SceneLoadManager>();
				_sceneLoader = temp.GetComponent<SceneLoadManager>();
			}
		}
		return _sceneLoader;
	}
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
	#endregion
	private string loadSceneName;
	public void LoadScene(string sceneName)
	{
		LoadLoadingScene(sceneName);
	}
	private void LoadLoadingScene(string sceneName)
	{
		gameObject.SetActive(true);
		SceneManager.LoadScene("Loading");
		SceneManager.sceneLoaded += LoadSceneEnd;

		loadSceneName = sceneName;

		StartCoroutine(Load(sceneName));

	}

	private IEnumerator Load(string sceneName)
	{
		yield return StartCoroutine(Fade(true));

		AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

		op.allowSceneActivation = false;

		float timer = 0.0f;

		while (!op.isDone)
		{
			yield return null;

			timer += Time.unscaledDeltaTime;

			if (op.progress >= 0.9f)
			{
				op.allowSceneActivation = true;
				yield break;
			}
		}
	}



	private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (scene.name == loadSceneName)
		{
			StartCoroutine(Fade(false));

			SceneManager.sceneLoaded -= LoadSceneEnd;
		}
	}


	private IEnumerator Fade(bool isFadeIn)
	{
		float timer = 0f;

		while (timer <= 1f)
		{
			yield return null;

			timer += Time.unscaledDeltaTime * 2f;
		}

		if (!isFadeIn)
		{
			gameObject.SetActive(false);
		}
	}
}
