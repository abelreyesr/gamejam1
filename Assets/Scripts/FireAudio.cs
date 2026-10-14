using System.Collections;
using UnityEngine;

public class FireAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] _ambientClips;

    [SerializeField] private float _minCheckInterval = 4f;
    [SerializeField] private float _maxCheckInterval = 8f;

    [Range(0f, 1f)]
    [SerializeField] private float _baseChance = 0.5f;

    private static int _activeFireCount = 0;
    private Coroutine _ambientRoutine;

    private void OnEnable()
    {
        _activeFireCount++;
        _ambientRoutine = StartCoroutine(AmbientLoop());
    }

    private void OnDisable()
    {
        _activeFireCount--;
        if (_ambientRoutine != null) StopCoroutine(_ambientRoutine);
    }

    private IEnumerator AmbientLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minCheckInterval, _maxCheckInterval));

            float chance = _baseChance / Mathf.Sqrt(_activeFireCount);

            if (Random.value <= chance)
                AudioManager.Instance.PlaySFX(_ambientClips);
        }
    }
}
