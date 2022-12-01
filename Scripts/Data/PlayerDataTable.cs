public class PlayerDataTable
{
	public string name;
	public float maxHP;
	public float nowHP;
	/// <summary> 1초에 움직일 수 있는 크기 </summary>
	public float speed;
	/// <summary> 몬스터를 탐지할 수 있는 길이 (x,y,z 전부 계산) </summary>
	public float attackRange;
	public float attackDamage;
	/// <summary> 1초에 공격할 수 있는 횟수 </summary>
	public float attackSpeed;
}
