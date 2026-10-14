using UnityEngine;

public class ShadowSprite : MonoBehaviour
{
    private Vector3 _defaultScale;
    private Vector3 _posOffset;
    private Transform _parentTransform;

    private void Awake()
    {
        _parentTransform = transform.parent;
        _posOffset = _parentTransform.position - transform.position;
        _defaultScale = transform.lossyScale;

        transform.SetParent(null);
    }

    void Update()
    {
        if (_parentTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.localScale = _defaultScale;
        transform.position = _parentTransform.position - _posOffset;
    }
}
