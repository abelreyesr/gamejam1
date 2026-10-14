using UnityEngine;

public interface IDamageable
{
    void Damage(float damage, float knockback = 0f) => Damage(damage, knockback, Vector2.zero);
    void Damage(float damage, float knockback, Vector2 position);
    void Die();
}
