using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class KaijuController : KaijuControllerBase, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float _speed = 4f;
    [SerializeField] private float _acceleration = 2f;

    [Header("Basic Attack")]
    [SerializeField] private Transform _slashTransform;
    [SerializeField] private AttackHitbox _slashHitbox;
    [SerializeField] private float _basicAttackForce = 2f;
    [SerializeField] private float _basicAttackDamage = 1f;

    [Header("Down Attack")]
    [SerializeField] private GameObject _firePrefab;
    [SerializeField] private float _fireSpreadAngle;
    [SerializeField] private float _fireSpreadTime;
    [SerializeField] private float _randomFlameChance;
    private float _nextSpreadTime;
    private bool _shootingFireRing;

    [Header("Right Attack")]
    [SerializeField] private GameObject _fireballPrefab;
    [SerializeField] private float _fireballAngleError;
    [SerializeField] private Transform _mouthPos;

    [Header("Stamina Costs")]
    [SerializeField] private float _downAttackStaminaCost = 2f;
    [SerializeField] private float _rightAttackStaminaCost = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] _hurtSfx;
    [SerializeField] private AudioClip[] _stepSfx;
    [SerializeField] private AudioClip[] _roar1Sfx;
    [SerializeField] private AudioClip[] _roar2Sfx;
    [SerializeField] private AudioClip _swipeSfx;
    [SerializeField] private AudioClip[] _fireballSfx;

    private bool _moving = false;
    private Vector2 _targetPos;
    private Vector2 _movementVelocity;
    private Vector2 _knockbackVelocity;

    private Vector2 _pendingAttackDirection;

    private bool _dead;

    protected override void ChildAwake()
    {
        _slashHitbox.Configure(_basicAttackDamage, _basicAttackForce * 1.2f, this);
    }

    protected override void ChildUpdate()
    {
        if (_shootingFireRing && Time.time > _nextSpreadTime)
        {
            _nextSpreadTime = Time.time + _fireSpreadTime;

            ShootFireRing();
        }

        _animator.SetBool("isWalking", _moving);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!_moving)
        {
            _movementVelocity = Vector2.MoveTowards(_movementVelocity, Vector2.zero, _acceleration * 2 * Time.fixedDeltaTime);
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

        _knockbackVelocity = Vector2.MoveTowards(_knockbackVelocity, Vector2.zero, 8f * Time.fixedDeltaTime);

        _rb.linearVelocity = _movementVelocity + _knockbackVelocity;

    }

    private float MoveAxisTowards(float current, float target)
    {

        bool isReversing = current * target < 0f;
        float rate = isReversing ? _acceleration * 2f : _acceleration;

        return Mathf.MoveTowards(current, target, rate * Time.fixedDeltaTime);
    }

    protected override void PerformMove(Vector3 worldPos)
    {
        _moving = true;
        _targetPos = worldPos;
        _targetPos -= _posOffset;

        Vector2 curPos = transform.position;
        Vector2 dir = (_targetPos - curPos).normalized;

        FaceDirection(dir.x);
    }

    protected override void PerformBasicAttack(Vector2 direction)
    {
        LockAction();
        _moving = false;
        FaceDirection(direction.x);

        // rotate slash and hitbox towards attack direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (_facing < 0)
            angle = 180f - angle;

        _slashTransform.localRotation = Quaternion.Euler(0f, 0f, angle);

        _pendingAttackDirection = direction;

        _animator.Play("attack_claw");
    }

    private void ApplyClawSlash(int i)
    {
        bool active = i == 1;

        _slashHitbox.SetActive(active);

        if (active) ApplyKnockback(_pendingAttackDirection, _basicAttackForce);
    }


    protected override void PerformQAttack(Vector2 direction)
    {
        if (!TryConsumeStamina(_downAttackStaminaCost)) return;

        LockAction();
        _moving = false;

        _animator.Play("attack_down");
    }

    private void ToggleFireRing(int state)
    {
        bool active = state == 1 ? true : false;
        _shootingFireRing = active;
    }

    int n = 0;
    private void ShootFireRing()
    {
        n++;
        n %= 5;

        float angleStep = _fireSpreadAngle;

        float offset = Mathf.Lerp(0f, angleStep, n / 5f);

        int nFlames = Mathf.RoundToInt(360f / angleStep);

        PlayFireballSfx();

        for (int i = 0; i < nFlames; i++)
        {
            float angle = angleStep * i + offset;

            StraightProjectile fire = Instantiate(_firePrefab, transform.position, Quaternion.identity).GetComponent<StraightProjectile>();

            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            fire.Init(direction, this);

            // % to shoot random flame
            if (Random.value < _randomFlameChance)
            {
                print("fuego extra");
                fire = Instantiate(_firePrefab, transform.position, Quaternion.identity).GetComponent<StraightProjectile>();
                fire.Init(direction, this);

                fire.SetSpeed(Random.Range(1f, 3f));
                fire.SetDespawnTimeout(20f);
            }
        }
    }


    protected override void PerformEAttack(Vector2 direction)
    {
        if (!TryConsumeStamina(_rightAttackStaminaCost)) return;

        LockAction();
        _moving = false;
        FaceDirection(direction.x);

        _pendingAttackDirection = direction;

        _animator.Play("attack_right");
    }
    private void ShootFireball()
    {
        float angleErrror = Random.Range(-_fireballAngleError, _fireballAngleError); // arrrg camarada
        Vector2 dir = Quaternion.Euler(0f, 0f, angleErrror) * _pendingAttackDirection;

        GameObject proj = Instantiate(_fireballPrefab, _mouthPos.position, Quaternion.identity);
        proj.transform.right = dir;

        Fireball fireball = proj.GetComponent<Fireball>();
        fireball.Init(dir, this);

        PlayFireballSfx();
    }


    private void EndAction()
    {
        UnlockAction();
    }

    private int _facing = 1;
    private void FaceDirection(float dir)
    {
        int faceDir = (int)Mathf.Sign(dir);
        if (faceDir == 0 || _facing == faceDir) return;

        _facing = faceDir;

        transform.DOKill();
        transform.DOScaleX(faceDir, 0.2f).SetEase(Ease.InExpo);
    }

    private void ShakeSprite(float duration)
    {
        _visualTransform.DOShakePosition(duration, 0.03f, 20);
    }

    private void ShakeSprite2(float duration)
    {
        _visualTransform.DOShakePosition(duration, 0.1f, 20);
    }

    public void Damage(float damage, float knockback, Vector2 pos)
    {
        if (_dead) return;

        _spriteFlash.Flash(0.2f);
        AudioManager.Instance.PlaySFX(_hurtSfx, 0.9f);

        _hp -= damage;

        if (knockback > 0f)
        {
            Vector2 knockbackDir = (pos - (Vector2)transform.position).normalized;
            ApplyKnockback(-knockbackDir, knockback);
        }


        if (_hp <= 0) Die();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        _knockbackVelocity += direction.normalized * force;
    }

    public void Die()
    {
        _dead = true;
        GameManager.Instance.TriggerDefeat();
    }

    // AUDIO
    public void PlayStepSfx() => AudioManager.Instance.PlaySFX(_stepSfx, 0.3f);
    public void PlayRoar1Sfx() => AudioManager.Instance.PlaySFX(_roar1Sfx, 0.7f);
    public void PlayRoar2Sfx() => AudioManager.Instance.PlaySFX(_roar2Sfx);
    public void PlaySwipeSfx() => AudioManager.Instance.PlaySFX(_swipeSfx);
    public void PlayFireballSfx() => AudioManager.Instance.PlaySFX(_fireballSfx);
}
