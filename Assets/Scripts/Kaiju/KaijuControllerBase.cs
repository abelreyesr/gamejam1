using System.Reflection.Metadata.Ecma335;
using UnityEngine;

public enum KaijuState
{
    Obedient,
    TooFar,
    Stunned,
    Enraged,
}

public abstract class KaijuControllerBase : MonoBehaviour, IKaijuController
{
    [Header("Stats")]
    [SerializeField] protected float _maxHp = 100f;
    protected float _hp;
    public float HealthPercent => _hp / _maxHp;

    [Header("Obedience")]
    [SerializeField] protected Transform _trainer;
    [SerializeField] protected float _maxObedienceRange = 5.5f;

    [Header("Components")]
    [SerializeField] protected Rigidbody2D _rb;
    [SerializeField] protected Transform _visualTransform;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected SpriteFlashController _spriteFlash;
    [SerializeField] protected Transform _border;

    [Header("Stamina")]
    [SerializeField] protected float _maxStamina = 5f;
    [SerializeField] protected float _staminaRegenRate = 0.5f;
    protected float _stamina;
    public float StaminaPercent => _stamina / _maxStamina;
    public float CurrentStamina => _stamina;

    protected KaijuState _state = KaijuState.Obedient;
    public KaijuState State => _state;
    protected Vector2 _posOffset;

    public bool ActionLocked { get; private set; }
    protected void LockAction() => ActionLocked = true;
    protected void UnlockAction() => ActionLocked = false;
    public bool CanStartAction() => CanObey() && !ActionLocked;

    private void Awake()
    {
        if (_trainer == null) _trainer = FindAnyObjectByType<PlayerMovement>().transform;
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_animator == null) _animator = GetComponent<Animator>();

        _posOffset = _visualTransform.position - transform.position;

        _hp = _maxHp;
        _stamina = _maxStamina;

        ChildAwake();
    }

    private void Update()
    {
        HandleDistance();
        HandleStaminaRegen();
        ChildUpdate();
    }

    private void HandleStaminaRegen()
    {
        _stamina = Mathf.Min(_maxStamina, _stamina + _staminaRegenRate * Time.deltaTime);
    }

    private void HandleDistance()
    {
        if (_state is KaijuState.Enraged or KaijuState.Stunned) return;

        float distance = Vector2.Distance(_trainer.position, transform.position);

        _state = distance > _maxObedienceRange ?
            KaijuState.TooFar :
            KaijuState.Obedient;

        // radius border
        Vector2 dir = DirectionToPlayer();

        _border.position = transform.position + (Vector3)dir * _maxObedienceRange;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        _border.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected bool TryConsumeStamina(float amount)
    {
        if (_stamina < amount) return false;
        _stamina -= amount;
        return true;
    }

    protected Vector2 DirectionToPlayer() => (_trainer.position - transform.position).normalized;

    protected virtual void ChildUpdate() { }
    protected virtual void ChildAwake() { }

    public void Move(Vector3 worldPos)
    {
        PerformMove(worldPos);
    }


    public void BasicAttack(Vector2 direction)
    {
        PerformBasicAttack(direction);
    }

    public void EAttack(Vector2 direction)
    {
        PerformEAttack(direction);
    }

    public void QAttack(Vector2 direction)
    {
        PerformQAttack(direction);
    }

    protected abstract void PerformMove(Vector3 worldPos);
    protected abstract void PerformBasicAttack(Vector2 direction);
    protected abstract void PerformQAttack(Vector2 direction);
    protected abstract void PerformEAttack(Vector2 direction);


    protected virtual bool CanObey()
    {
        if (_state is KaijuState.Stunned or KaijuState.Enraged or KaijuState.TooFar)
            return false;

        return true;
    }
}
