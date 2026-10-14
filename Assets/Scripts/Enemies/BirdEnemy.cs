using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BirdEnemy : EnemyBase
{
    [Header("Lightning Orbs")]
    [SerializeField] private Transform _beakPoint;
    [SerializeField] private GameObject _orbPrefab;
    [SerializeField] private AudioClip _cawSfx;

    [Header("Dash Attack")]
    [SerializeField] private Transform _slashTransform;
    [SerializeField] private float _dashSpeed;
    private bool _dashing;
    private Vector2 _dashDirection;

    [Header("Feather Attack")]
    [SerializeField] private GameObject _featherPrefab;
    [SerializeField] private float _featherArc = 45f;
    [SerializeField] private int _feathersPerArc = 3;
    [SerializeField] private AudioClip[] _hardFlapSfx;

    [Header("Laser Attack")]
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private Transform _eyePoint;
    [SerializeField] private float _flyDistance = 6f;
    [SerializeField] private float _laserMinTime = 0.3f;
    [SerializeField] private float _laserMaxTime = 1.5f;
    [SerializeField] private float _laserAngleError = 15f;
    private bool _laserActive = false;
    [SerializeField] private AudioClip _chirpSfx;

    [Header("SFX")]
    [SerializeField] private AudioClip[] _flapSfx;
    [SerializeField] private AudioClip _dashSfx;
    [SerializeField] private AudioClip _tauntSfx;
    [SerializeField] private AudioClip[] _dmgSfx;

    protected override void EnemyAwake() { } 
    protected override void EnemyUpdate() 
    {
        if (_dashing) _rb.linearVelocity = _dashDirection * _dashSpeed;

        if (_laserActive && !_moving) EndLaserAttack();
    }

    protected override void OnHit() 
    {
        AudioManager.Instance.PlaySFX(_dmgSfx, 2f);
    }
    protected override void OnDeath()
    {
        _laserActive = false;
        _animator.SetTrigger("die");
    }

    private void ShootOrb()
    {
        // spawn projectile
        GameObject proj = Instantiate(_orbPrefab, _beakPoint.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX(_cawSfx, 0.4f);

        // choose target
        Vector2 dir = Random.insideUnitCircle.normalized;
        Transform target = Random.value > 0.5f ? _kaijuTransform : _playerTransform;

        LightningOrb projScript = proj.GetComponent<LightningOrb>();
        projScript.Init(dir, _attackExecutor.owner, target);
    }

    int n = 0;
    private void ShootFeathers()
    {
        Vector2 dir = Vector2.Lerp(GetDirectionToKaiju(transform.position), GetDirectionToKPlayer(transform.position), 0.5f).normalized;

        n++;
        n %= 2;
        int count = _feathersPerArc + n;

        for(int i = 0; i < count; i++)
        {
            float angle = -_featherArc / 2f + _featherArc * i / (count -1);
            Vector2 shotDir = Quaternion.Euler(0, 0, angle) * dir;

            GameObject proj = Instantiate(_featherPrefab, transform.position, Quaternion.identity);

            FeatherProjectile projScript = proj.GetComponent<FeatherProjectile>();
            projScript.Init(shotDir, _attackExecutor.owner);
        }
    }

    private void DashAttack()
    {
        Vector2 dir = GetDirectionToKaiju(transform.position);     

        _collider.enabled = false;
        _dashDirection = dir;
        _dashing = true;

        _facePlayer = false;

        if (transform.localScale.x < 0) transform.right = -dir;
        else transform.right = dir;
    }

    private void EndDashAttack() 
    {
        _collider.enabled = true;
        _dashing = false;
        _dashDirection = Vector2.zero;

        transform.rotation = Quaternion.identity;
        _facePlayer = true;
    }

    private Coroutine _laserCoroutine;
    private float _defaultSpeed;
    private void ActivateLaserAttack()
    {
        _laserActive = true;
        _movementOverridden = true;
        _defaultSpeed = _speed;
        _speed = 0.5f * _defaultSpeed;

        Vector2 toKaiju = GetDirectionToKaiju(transform.position);
        Vector2 perpendicular = Random.value > 0.5f
            ? new Vector2(-toKaiju.y, toKaiju.x)
            : new Vector2(toKaiju.y, -toKaiju.x);

        // para que el pajaro de mierda no se quede atascado intentando salir del mapa
        Vector2 visualFrom = (Vector2)transform.position + _posOffset;
        Vector2 rawTarget = visualFrom + perpendicular * _flyDistance;
        Vector2 clampedVisualTarget = ClampToBounds(visualFrom, rawTarget);

        PerformMove(clampedVisualTarget);

        _laserCoroutine = StartCoroutine(LaserRoutine());
    }

    private IEnumerator LaserRoutine()
    {
        while (_laserActive)
        {
            yield return new WaitForSeconds(Random.Range(_laserMinTime, _laserMaxTime));
            if (!_laserActive) yield break;

            LaserScript laser = Instantiate(_laserPrefab, _eyePoint.position, Quaternion.identity).GetComponent<LaserScript>();

            Vector2 dir = GetDirectionToKPlayer(_eyePoint.position);

            float angle = Random.Range(-_laserAngleError, _laserAngleError);
            dir = Quaternion.Euler(0f, 0f, angle) * dir;

            laser.Init(dir, this);
        }
    }

    private void EndLaserAttack()
    {
        _laserActive = false;
        _movementOverridden = false;
        _moving = false;
        _speed = _defaultSpeed;

        if (_laserCoroutine != null)
        {
            StopCoroutine(_laserCoroutine);
            _laserCoroutine = null;
        }

        PostponeAI(_minActionCooldown);
    }

    // SFX
    public void PlayHardFlapSFX() => AudioManager.Instance.PlaySFX(_hardFlapSfx, 0.8f);
    public void PlayFlapSFX() => AudioManager.Instance.PlaySFX(_flapSfx, 0.3f, 1.8f);
    public void PlayChirpSFX() => AudioManager.Instance.PlaySFX(_chirpSfx);
    public void PlayDashSFX() => AudioManager.Instance.PlaySFX(_dashSfx);
    public void PlayTauntSFX() => AudioManager.Instance.PlaySFX(_tauntSfx);
}
