using UnityEngine;

public class SoundCaller : MonoBehaviour
{
    [SerializeField] private AudioClip[] _audio;
    void PlayAudio(int index)
    {
        AudioManager.Instance.PlaySFX(_audio[index], randomizePitch: false);
    }

    void StopMusic()
    {
        AudioManager.Instance.StopMusic();
    }
}
