using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class LightningOrb : ProjectileBase
{
    [SerializeField] private float _turnSpeed;
    [Range(0f, 1f)]
    [SerializeField] private float _randomSpeedMod;

    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Transform _visualTransform;

    private float _speedBoost = 2f;

    private void Awake()
    {
        if(_rb == null) _rb = GetComponent<Rigidbody2D>();

        _visualTransform.DOShakePosition(0.5f, 0.1f, 10, 90f, false, false).SetLoops(-1, LoopType.Restart);
    }

    public void Init(Vector2 direction, IDamageable owner, Transform target)
    {
        InitCommon(direction, owner, target);

        // random speed variation
        _speed *= 1 + Random.Range(-_randomSpeedMod, _randomSpeedMod);
        _turnSpeed *= 1 + Random.Range(-_randomSpeedMod, _randomSpeedMod);

        _rb.linearVelocity = direction * _speed * _speedBoost;
    }

    void FixedUpdate()
    {
        RotateTowardsTarget();

        //initial speed boost
        if (_speedBoost > 1f) _speedBoost = Mathf.MoveTowards(_speedBoost, 1f, 1f * Time.fixedDeltaTime);
    }

    private void RotateTowardsTarget()
    {
        Vector3 curDir = _rb.linearVelocity.normalized;
        Vector3 targetDir = (_target.position - transform.position).normalized;
        float curAngle = Mathf.Atan2(curDir.y, curDir.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        float newAngle = Mathf.MoveTowardsAngle(curAngle, targetAngle, _turnSpeed * Time.fixedDeltaTime);

        Vector2 newDir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));

        _rb.linearVelocity = newDir * _speed * _speedBoost;
    }

    protected override void Detonate()
    {
        _visualTransform.DOKill();
        Consume();
    }
}
