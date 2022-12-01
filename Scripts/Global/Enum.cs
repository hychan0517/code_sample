namespace GlobalDefine
{
    public enum eLangaugeType
    {
        Korean,
        English,
    }

    public enum eConnectServerType
	{
        Dev,
        QA,
        Live,
	}

    public enum eSceneType
    {
        None,
        Main,
        LobbyLoading,
        Lobby,
        InGameLoading,
        InGame,
    }


	//Player=====================================================
	#region Player
	public enum ePlayerStateType
	{ 
        Idle,
        Move,
        BaseAttack,
        SkillAttack,
        UltimateAttack,
        Die,
    }

    [System.Serializable]
	public enum ePlayerSKillType
	{
        None,
        Projectile,
        Buff,
        Debuff,
        Active,
    }
    public enum eBaseAttackSkillType
    {
        None,
        Base,
        Fire,
        Ice,
        Light,
        Max,
	}
	#endregion
	//=====================================================Player


	//Monster====================================================
	#region Monster
	public enum eMonsterStateType
	{
        Idle,
        Move,
        Die,
        Attack,
    }

	#endregion
	//====================================================Monster
}