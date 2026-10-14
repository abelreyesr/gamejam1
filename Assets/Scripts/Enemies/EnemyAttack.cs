using UnityEngine;

[System.Serializable]
public struct EnemyAttack
{
    public string animatorTrigger;
    public float weight;
    public float minRange;
    public float maxRange;

    [Header("Only Melee")]
    public int hitboxIndex;
    public float damage;
    public float knockback;
}