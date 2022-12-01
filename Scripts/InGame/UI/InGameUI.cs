using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InGameUI : MonoBehaviour
{
	[Header("Canvas")]
	[SerializeField]
	private Canvas _worldCanvas;
	[SerializeField]
	private Canvas _cameraCanvas;
	[SerializeField]
	private Canvas _inGameOverlayCanvas;

	[Header("UI")]
	[SerializeField]
	private PlayerControllerUI _playerControllerUI;
	[SerializeField]
	private PlayerSkillUI _playerSkillUI;
	[SerializeField]
	private PlayerInfoUI _playerInfoUI;
	[SerializeField]
	private PlayerInfoSlot _myInfoSlot;
	[SerializeField]
	private Image _baseSkillAnimationImage;
	[SerializeField]
	private GameObject _loadingPanel;
	public GameResultUI _gameResultUI;

	private Tween _baseSkillAnimationTween;

	public void InitUIObject()
	{
		if (_cameraCanvas == null)
			LogManager.LogError("Cannot find _cameraCanvas");

		if (_worldCanvas == null)
			LogManager.LogError("Cannot find _worldCanvas");

		if (_inGameOverlayCanvas == null)
			LogManager.LogError("Cannot find _overlayCanvas");

		if (_playerControllerUI)
			_playerControllerUI.InitUIObject();
		else
			LogManager.LogError("Cannot find _playerControllerUI");

		if (_playerSkillUI)
			_playerSkillUI.InitUIObject();
		else
			LogManager.LogError("Cannot find _playerSkillUI");

		if (_baseSkillAnimationImage == null)
			LogManager.LogError("Cannot find _baseSkillAnimationImage");

		if (_playerInfoUI)
			_playerInfoUI.InitUIObject();
		else
			LogManager.LogError("Cannot find _playerInfoUI");

		if (_myInfoSlot)
			_myInfoSlot.InitUIObject();
		else
			LogManager.LogError("Cannot find _myInfoSlot");

		if (_loadingPanel)
			_loadingPanel.SetActive(true);
		else
			LogManager.LogError("Cannot find _loadingPanel");

		if (_gameResultUI)
			_gameResultUI.InitUIObject();
		else
			LogManager.LogError("Cannot find _gameResultUI");

		App_InGame.Ins.GetEventManager().AddGameStartEvent(SetActiveOffLoadingPanel);
	}

	private void SetActiveOffLoadingPanel()
	{
		if (_loadingPanel)
			_loadingPanel.SetActive(false);
	}

	public PlayerControllerUI GetPlayerControllerUI()
	{
		return _playerControllerUI;
	}

	public PlayerInfoSlot GetMyInfoSlot()
	{
		return _myInfoSlot;
	}

	public PlayerInfoUI GetPlayerInfoUI()
	{
		return _playerInfoUI;
	}

	public Canvas GetCanvas()
	{
		return _inGameOverlayCanvas;
	}

	public void OnStartBaseSkillAnimation()
	{
		KillBaseSkillAnimation();
		//TODO : 스킬시간, 스킬타입과 동기화 맞춰줘야함
		_baseSkillAnimationTween = _baseSkillAnimationImage.DOColor(new Color(_baseSkillAnimationImage.color.r, _baseSkillAnimationImage.color.g, _baseSkillAnimationImage.color.b, 0.2f), 1f).OnComplete(KillBaseSkillAnimation);
	}
	private void KillBaseSkillAnimation()
	{
		if (_baseSkillAnimationTween != null && _baseSkillAnimationTween.active)
			_baseSkillAnimationTween.Kill();
		_baseSkillAnimationTween = null;
		_baseSkillAnimationImage.color = new Color(_baseSkillAnimationImage.color.r, _baseSkillAnimationImage.color.g, _baseSkillAnimationImage.color.b, 0.0f);
	}

	//Button===================================================================================================================
	#region Button
	#endregion
	//=================================================================================================================Button
}
