using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class StraightProjectile : ProjectileBase
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] SpriteRenderer _sprite;

    public void Init(Vector2 direction, IDamageable owner)
    {
        InitCommon(direction, owner);

        if(_rb == null) _rb = GetComponent<Rigidbody2D>();
        if(_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();

        if(Random.value > 0.5f) _sprite.flipX = true;
        
        _rb.linearVelocity = direction * _speed;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
        _rb.linearVelocity = _rb.linearVelocity.normalized * _speed;
    }

    public void SetDespawnTimeout(float time)
    {
        StopDespawnTimeout();
        StartDespawnTimeout(time);
    }

    protected override void Detonate()
    {
        Consume();
    }
}
