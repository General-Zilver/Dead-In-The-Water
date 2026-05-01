using UnityEngine;

public enum EnemyType
{
    Shark,
    SawShark,
    SeaAngler,
    SwordFish
}

public class EnemyIdentity : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.Shark;
}
