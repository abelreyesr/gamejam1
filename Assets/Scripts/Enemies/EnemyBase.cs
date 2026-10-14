using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected float _maxHp;
    [SerializeField] protected float _speed;
    [SerializeField] protected float _acceleration;

    [Header("AI")]
    [SerializeField] protected bool _facePlayer;
    [SerializeField] protected float _preferredDistance = 3f;
    [SerializeField] protected float _distanceVariance = 1.5f;
    [SerializeField] protected float _minRepositionInterval = 0.8f;
    [SerializeField] protected float _maxRepositionInterval = 2f;
    [SerializeField] protected float _strafeAngleRange = 60f;
    [SerializeField] protected float _maxActionCooldown = 4f;
    [SerializeField] protected float _minActionCooldown = 0.5f;

    [Header("Bounds Check")]
    [SerializeField] protected LayerMask _boundsObstacleLayer;
    [SerializeField] protected float _boundsCheckMargin = 1f;

    protected float _nextRepositionTime;

    [Header("Attacks")]
    [SerializeField] protected EnemyAttack[] _attacks;

    [Header("Components")]
    [SerializeField] protected Rigidbody2D _rb;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected Collider2D _collider;
    [SerializeField] protected Transform _visualTransform;
    [SerializeField] protected EnemyAttackExecutor _attackExecutor;
    [SerializeField] protected SpriteFlashController _spriteFlash;

    protected Transform _kaijuTransform;
    protected Transform _playerTransform;
    protected Vector2 _targetPos;
    protected bool _moving;
    protected Vector2 _movementVelocity;
    protected Vector2 _knockbackVelocity;
    protected Vector2 _posOffset;

    protected float _hp;
    public float HealthPercent => _hp / _maxHp;
    protected bool _dead;

    protected float _nextActionTime;
    protected bool _movementOverridden;

    void Awake()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_collider == null) _collider = GetComponent<Collider2D>();
        if (_attackExecutor == null) _attackExecutor = GetComponent<EnemyAttackExecutor>();
        _kaijuTransform = FindAnyObjectByType<KaijuControllerBase>().transform; 
        _playerTransform = FindAnyObjectByType<PlayerMovement>().transform; 

        _hp = _maxHp;
        _posOffset = _visualTransform.position - transform.position;

        EnemyAwake();
    }

    void Update()
    {
        if (_dead) return;

        if (_facePlayer && !_moving) FaceDirection(GetDirectionToKaiju(transform.position).x);

        HandleMovement();
        HandleAI();

        EnemyUpdate();
    }

    protected virtual void HandleAI()
    {
        if (_movementOverridden) return;

        if (_attackExecutor.IsAttacking) { _moving = false; return; }

        if (Time.time >= _nextRepositionTime)
            PickNewRepositionTarget();

        if (Time.time >= _nextActionTime && _attacks.Length > 0)
        {
            float dist = DistanceToKaiju();
            var validAttacks = FilterAttacksByRange(_attacks, dist);

            if (validAttacks.Count > 0)
                _attackExecutor.Execute(PickWeightedAttack(validAttacks));
        }
    }

    private void PickNewRepositionTarget()
    {
        _nextRepositionTime = Time.time + Random.Range(_minRepositionInterval, _maxRepositionInterval);

        Vector2 toEnemy = (Vector2)transform.position - (Vector2)_kaijuTransform.position;
        float currentAngle = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg;

        float newAngle = currentAngle + Random.Range(-_strafeAngleRange, _strafeAngleRange);
        float newDistance = _preferredDistance + Random.Range(-_distanceVariance, _distanceVariance);

        Vector2 dir = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad));
        Vector2 targetPoint = (Vector2)_kaijuTransform.position + dir * newDistance;

        PerformMove(targetPoint);
    }

    private List<EnemyAttack> FilterAttacksByRange(EnemyAttack[] pool, float distance)
    {
        var result = new List<EnemyAttack>();
        foreach (var a in pool)
            if (distance >= a.minRange && distance <= a.maxRange)
                result.Add(a);
        return result;
    }


    private void OnEnable() => _attackExecutor.OnAttackFinished += HandleAttackFinished;
    private void OnDisable() => _attackExecutor.OnAttackFinished -= HandleAttackFinished;

    private void HandleAttackFinished()
    {
        _nextActionTime = Time.time + Random.Range(_minActionCooldown, _maxActionCooldown);
    }


    protected float DistanceToKaiju() => Vector2.Distance(transform.position, _kaijuTransform.position);
    protected Vector2 GetDirectionToKaiju(Vector2 from) => ((Vector2)_kaijuTransform.position - from).normalized;
    protected Vector2 GetDirectionToKPlayer(Vector2 from) => ((Vector2)_playerTransform.position - from).normalized;

    private void HandleMovement()
    {
        _animator.SetBool("isWalking", _moving);

        if (!_moving)
        {
            _movementVelocity = Vector2.MoveTowards(_movementVelocity, Vector2.zero, _acceleration * 2 * Time.deltaTime);
        }
        else
        {
            Vector2 curPos = transform.position;
            Vector2 dir = (_targetPos - curPos).normalized;
            Vector2 targetVel = dir * _speed;

            _movementVelocity.x = MoveAxisTowards(_movementVelocity.x, targetVel.x);
            _movementVelocity.y = MoveAxisTowards(_movementVelocity.y, targetVel.y);

            float distance = Vector2.Distance(curPos, _targetPos);
            if (distance < 0.3f) _moving = false;
        }

        _knockbackVelocity = Vector2.MoveTowards(_knockbackVelocity, Vector2.zero, 8f * Time.deltaTime);

        _rb.linearVelocity = _movementVelocity + _knockbackVelocity;
    }

    protected void ChaseKaiju(float stopDistance = 2f)
    {
        if (_dead || _attackExecutor.IsAttacking) return;
        if (DistanceToKaiju() <= stopDistance) { _moving = false; return; }
        PerformMove(_kaijuTransform.position);
    }

    protected void FleeFromKaiju(float minDistance)
    {
        if (_dead || _attackExecutor.IsAttacking) return;
        if (DistanceToKaiju() >= minDistance) { _moving = false; return; }

        Vector2 dir = ((Vector2)transform.position - (Vector2)_kaijuTransform.position).normalized;
        PerformMove(transform.position + (Vector3)dir * 2f);
    }

    private float MoveAxisTowards(float current, float target)
    {
        bool isReversing = current * target < 0f;
        float rate = isReversing ? _acceleration * 2f : _acceleration;

        return Mathf.MoveTowards(current, target, rate * Time.deltaTime);
    }

    protected void PerformMove(Vector3 worldPos)
    {
        _moving = true;
        _targetPos = worldPos;
        _targetPos -= _posOffset;

        Vector2 curPos = transform.position;
        Vector2 dir = (_targetPos - curPos).normalized;

        FaceDirection(dir.x);
    }


    protected EnemyAttack PickWeightedAttack(List<EnemyAttack> pool)
    {
        float total = 0f;
        foreach (var a in pool) total += a.weight;

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var a in pool)
        {
            cumulative += a.weight;
            if (roll <= cumulative) return a;
        }

        return pool[pool.Count - 1];
    }


    public void Damage(float damage, float knockback, Vector2 pos)
    {
        if(_dead) return;

        _spriteFlash.Flash(0.2f);

        _hp -= damage;

        if(knockback > 0f)
        {
            Vector2 knockbackDir = (pos - (Vector2)transform.position).normalized;
            ApplyKnockback(-knockbackDir, knockback);
        }

        if (_hp <= 0) 
        { 
            Die();
            return;
        }

        OnHit();
    }
    public void ApplyKnockback(Vector2 direction, float force)
    {
        _knockbackVelocity += direction.normalized * force;
    }

    public void Die()
    {
        _dead = true;
        GameManager.Instance.TriggerVictory();
        OnDeath();
    }

    private int _facing;
    private Tween _turnAround;
    protected void FaceDirection(float dir)
    {
        int faceDir = (int)Mathf.Sign(dir);
        if (faceDir == 0 || _facing == faceDir) return;

        _facing = faceDir;

        _turnAround = transform.DOScaleX(faceDir, 0.2f).SetEase(Ease.InExpo);
    }

    private void ShakeSprite(float duration)
    {
        _visualTransform.DOShakePosition(duration, 0.03f, 20);
    }

    protected void PostponeAI(float delay)
    {
        _nextRepositionTime = Time.time + delay;
        _nextActionTime = Time.time + delay;
    }

    protected Vector2 ClampToBounds(Vector2 from, Vector2 targetPoint)
    {
        Vector2 dir = (targetPoint - from);
        float distance = dir.magnitude;
        if (distance < 0.01f) return targetPoint;

        dir /= distance;

        RaycastHit2D hit = Physics2D.Raycast(from, dir, distance, _boundsObstacleLayer);
        if (hit.collider != null)
        {
            float safeDistance = Mathf.Max(0f, hit.distance - _boundsCheckMargin);
            return from + dir * safeDistance;
        }

        return targetPoint;
    }

    protected abstract void OnDeath();
    protected abstract void OnHit();
    protected virtual void EnemyAwake() { }
    protected virtual void EnemyUpdate() { }
}
