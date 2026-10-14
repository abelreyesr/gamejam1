using System;
using UnityEngine;
public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    [SerializeField] private bool _hitEnemies;
    private float _damage;
    private float _knockback;
    public IDamageable Owner { get; private set; }

    public Action<IDamageable> onHit;

    public void Configure(float damage, float knockback, IDamageable owner)
    {
        _damage = damage;
        _knockback = knockback;
        Owner = owner;
    }

    public void SetActive(bool active) => _collider.enabled = active;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !_hitEnemies) return;

        if (!other.TryGetComponent<Hurtbox>(out var hurtbox)) return;
        if (hurtbox.Owner == Owner) return;

        hurtbox.Owner.Damage(_damage, _knockback, transform.position);
        onHit?.Invoke(hurtbox.Owner);
    }
}