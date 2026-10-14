using UnityEngine;

public class BorderRadiusScript : MonoBehaviour
{
    [SerializeField] private float _minShowDistance = 2f;
    private SpriteRenderer _sprite;
    private Transform _player;
    private float _defaultAlpha;

    void Start()
    {
        _player = FindAnyObjectByType<PlayerMovement>().transform;
        _sprite = GetComponent<SpriteRenderer>();

        _defaultAlpha = _sprite.color.a;

        transform.SetParent(null);

        SetAlpha(0f);
    }

    void Update()
    {
        float dist = Vector2.Distance(_player.position, transform.position);

        float alpha = Mathf.InverseLerp( _minShowDistance, _minShowDistance / 3f, dist);

        SetAlpha(alpha * _defaultAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color color = _sprite.color;
        color.a = alpha;
        _sprite.color = color;
    }
}
