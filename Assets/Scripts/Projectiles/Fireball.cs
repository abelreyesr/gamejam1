using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class Fireball : ProjectileBase
{
    [Range(0f, 1f)]
    [SerializeField] private float _randomSpeedMod;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _visualTransform;

    [Header("Fire spread")]
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private float _minFireSpreadTime = 0.2f;
    [SerializeField] private float _maxFireSpreadTime = 0.6f;
    private float _nextFireSpreadTime;

    private IDamageable _owner;

    private void Update()
    {
        if(Time.time > _nextFireSpreadTime)
        {
            _nextFireSpreadTime = Time.time + Random.Range(_minFireSpreadTime, _maxFireSpreadTime);
            SpreadFire();
        }
    }

    private void Awake()
    {
        if(_rb == null) _rb = GetComponent<Rigidbody2D>();

        _visualTransform.DOShakePosition(0.5f, 0.1f, 10, 90f, false, false).SetLoops(-1, LoopType.Restart);
        _nextFireSpreadTime = Time.time + Random.Range(_minFireSpreadTime, _maxFireSpreadTime);
    }

    public void Init(Vector2 direction, IDamageable owner)
    {
        InitCommon(direction, owner);
        _owner = owner;

        // random speed variation
        _speed *= 1 + Random.Range(-_randomSpeedMod, _randomSpeedMod);

        _rb.linearVelocity = direction * _speed;
    }

    protected override void Detonate()
    {
        _visualTransform.DOKill();
        Consume();
    }

    private void SpreadFire()
    {
        Fire fire = Instantiate(_firePrefab, transform.position, Quaternion.identity).GetComponent<Fire>();

        fire.Init(Vector2.zero, _owner);
    }
}
