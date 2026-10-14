using UnityEngine;

public class FeatherProjectile : ProjectileBase
{
    [Range(0f, 1f)]
    [SerializeField] private float _randomSpeedMod;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _visualTransform;

    private void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, IDamageable owner)
    {
        InitCommon(direction, owner);

        // random speed variation
        _speed *= 1 + Random.Range(-_randomSpeedMod, _randomSpeedMod);

        _rb.linearVelocity = direction * _speed;
        transform.right = direction;
    }

    void FixedUpdate()
    {

    }

    protected override void Detonate()
    {
        Consume();
    }
}
