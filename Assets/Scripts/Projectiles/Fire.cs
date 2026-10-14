using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class Fire : ProjectileBase
{
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] SpriteRenderer _sprite;

    public void Init(Vector2 direction, IDamageable owner)
    {
        InitCommon(direction, owner);

        if(_rb == null) _rb = GetComponent<Rigidbody2D>();
        if(_sprite == null) _sprite = GetComponentInChildren<SpriteRenderer>();

        if(Random.value > 0.5f) _sprite.flipX = true;

        transform.localScale *= Random.Range(0.9f, 1.1f);
        
        Vector2 randSpeed = Random.onUnitCircle * Random.Range(0f, _speed);
        _rb.linearVelocity = randSpeed;
    }

    protected override void Detonate()
    {
        Consume();
    }
}
