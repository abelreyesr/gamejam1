using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected AttackHitbox _hitbox;

    [Header("Params")]
    [SerializeField] protected float _damage;
    [SerializeField] protected float _knockback = 0f;
    [SerializeField] protected float _speed;
    [SerializeField] protected float _despawnTime;

    protected Vector2 _direction;
    protected Transform _target;

    private void OnEnable()
    {
        _hitbox.onHit += OnHit;
    }
    private void OnDisable()
    {
        _hitbox.onHit -= OnHit;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //damage player
        }
        else if (other.CompareTag("Kaiju"))
        {
            //damage kaiju
        }
    }

    protected void InitCommon(Vector2 direction, IDamageable owner, Transform target = null) 
    { 
        if(_hitbox == null) _hitbox = GetComponent<AttackHitbox>();
        _hitbox.Configure(_damage, _knockback, owner);

        // si se acumulan muchas cosas mejor crear un struct de proyectiledata y pasarle eso
        _direction = direction;
        _target = target;

        if (_despawnTime > 0) StartDespawnTimeout(_despawnTime);
    }

    protected virtual void OnHit(IDamageable hit) { }
    protected abstract void Detonate();

    protected virtual void Consume()
    {
        Destroy(gameObject);
    }


    protected void StartDespawnTimeout(float t) => Invoke(nameof(OnDespawnTimeout), t);
    protected void StopDespawnTimeout() => CancelInvoke(nameof(OnDespawnTimeout));

    protected virtual void OnDespawnTimeout() => Detonate();
}
