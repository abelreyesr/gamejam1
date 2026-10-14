using System.Collections;
using UnityEngine;

public class SpriteFlashController : MonoBehaviour
{
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private Material _flashMaterial;
    private Coroutine _flashCoroutine;

    private static readonly int FlashAmountProp = Shader.PropertyToID("_FlashAmount");

    void Start()
    {
        if(_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        _flashMaterial = _spriteRenderer.material;
        _flashMaterial.color = _color;
    }

    public void Flash(float duration)
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(FlashRoutine(duration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float flashAmount = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            _flashMaterial.SetFloat(FlashAmountProp, flashAmount);

            yield return null;
        }

        _flashMaterial.SetFloat(FlashAmountProp, 0f);
        _flashCoroutine = null;
    }

}
