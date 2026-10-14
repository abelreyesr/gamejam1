using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserScript : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private float _damage;
    [SerializeField] private float _knockback;

    [SerializeField] private Color _telegraphColor;
    [SerializeField] private Color _laserColor;

    [Header("Sfx")]
    [SerializeField] private AudioClip _starSfx;
    [SerializeField] private AudioClip _shootSfx;

    private IDamageable _owner;
    private List<IDamageable> _hitDamageables = new();

    public void Init(Vector2 direction, IDamageable owner)
    {
        _owner = owner;

        StartCoroutine(LaserCoroutine(direction));
    }

    private IEnumerator LaserCoroutine(Vector2 direction)
    {
        _hitDamageables.Clear();
        Color invisible = new Color(0f, 0f, 0f, 0f);
        SetLineColor(invisible);
        _line.SetPosition(0, transform.position);
        _line.SetPosition(1, transform.position + (Vector3)direction * 100f);
        AudioManager.Instance.PlaySFX(_starSfx, 0.7f);

        yield return FadeToColor(_telegraphColor, 0.8f);
        yield return new WaitForSeconds(0.2f);

        yield return FadeToColor(_laserColor, 0.05f);

        AudioManager.Instance.PlaySFX(_shootSfx, 0.55f);
        yield return DamageDuringLaser(direction, 0.2f);

        yield return FadeToColor(invisible, 0.3f);
        Destroy(gameObject, 1f);
    }

    private IEnumerator FadeToColor(Color targetColor, float duration)
    {
        Color startColor = _line.startColor;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentage = elapsed / duration;

            Color currentColor = Color.Lerp(startColor, targetColor, percentage);
            SetLineColor(currentColor);

            yield return null;
        }

        SetLineColor(targetColor);
    }

    private void SetLineColor(Color color)
    {
        _line.startColor = color;
        _line.endColor = color;
    }

    private IEnumerator DamageDuringLaser(Vector2 direction, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ShootLaser(direction);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ShootLaser(Vector2 direction)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.05f, direction, 100f, LayerMask.GetMask("Hurtbox"));

        foreach (RaycastHit2D hit in hits)
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                if (damageable == _owner) continue;

                if (_hitDamageables.Contains(damageable)) continue;

                damageable.Damage(_damage, _knockback, hit.point);
                _hitDamageables.Add(damageable);
            }
        }
    }
}
