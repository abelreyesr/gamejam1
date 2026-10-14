using UnityEngine;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _maxHp = 3f;
    [SerializeField] private float _iFrames = 2f;
    private float _hp;
    private bool _dead = false;

    public float MaxHp => _maxHp;
    public float Hp => _hp;


    [Header("Components")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private InputHandler _input;
    [SerializeField] private Animator _animator;
    [SerializeField] protected SpriteFlashController _spriteFlash;

    [Header("Audio")]
    [SerializeField] private AudioClip _ouchSfx;

    private Vector2 _movementVelocity;
    private Vector2 _knockbackVelocity;
    private float _moveLockedUntil;

    private bool MovementLocked => Time.time < _moveLockedUntil;

    void Awake()
    {
        if (_input == null) _input = GetComponent<InputHandler>();
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponent<Animator>();

        _hp = _maxHp;
    }

    private void Update()
    {
        bool hasInput = _input.MoveInput.sqrMagnitude > 0.1f;
        _animator.SetBool("hasInput", hasInput);

        if (hasInput && !MovementLocked)
        {
            float sign = transform.localScale.x;
            if (_input.MoveInput.x > 0) sign = 1;
            else if (_input.MoveInput.x < 0) sign = -1;

                transform.localScale = new Vector3(sign, 1, 1);
        }
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (MovementLocked)
        {
            _movementVelocity = Vector2.MoveTowards(_movementVelocity, Vector2.zero, _acceleration * Time.fixedDeltaTime);
        }
        else
        {
            Vector2 moveDir = _input.MoveInput;
            Vector2 targetVel = moveDir * _speed;

            // separate axis for more snappy movement
            _movementVelocity.x = MoveAxisTowards(_movementVelocity.x, targetVel.x);
            _movementVelocity.y = MoveAxisTowards(_movementVelocity.y, targetVel.y);
        }

        _knockbackVelocity = Vector2.MoveTowards(_knockbackVelocity, Vector2.zero, 8f * Time.fixedDeltaTime);

        _rb.linearVelocity = _movementVelocity + _knockbackVelocity;
    }

    private float MoveAxisTowards(float current, float target)
    {
        // quick turns for snappier movement
        bool isReversing = current * target < 0f;
        float rate = isReversing ? _acceleration * 2f : _acceleration;

        return Mathf.MoveTowards(current, target, rate * Time.fixedDeltaTime);
    }

    private float _damagedTime;
    public void Damage(float damage, float knockback, Vector2 pos)
    {
        if(_dead ) return;

        if (Time.time < _damagedTime + _iFrames) return;
        _damagedTime = Time.time;

        _spriteFlash.Flash(0.6f);
        _animator.SetTrigger("hurt");
        AudioManager.Instance.PlaySFX(_ouchSfx, 2f);

        _hp--;

        _moveLockedUntil = Time.time + 0.5f;

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
        _animator.SetTrigger("dead");
        GameManager.Instance.TriggerDefeat();
    }
}

