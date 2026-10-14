using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;

    [Header("SFX pool")]
    [SerializeField] private int initialPoolSize = 3;
    [SerializeField] private float defaultPitchVariation = 0.05f;

    [Header("SFX by name")]
    [SerializeField] private List<SFXEntry> namedSFX = new List<SFXEntry>();

    [Header("Music")]
    [SerializeField] private float musicCrossfadeDuration = 1.5f;

    private readonly Queue<AudioSource> availableSources = new Queue<AudioSource>();
    private readonly List<AudioSource> allPooledSources = new List<AudioSource>();
    private Dictionary<string, SFXEntry> namedSFXLookup;

    private AudioSource musicSourceA;
    private AudioSource musicSourceB;
    private AudioSource activeMusicSource;
    private Coroutine musicFadeRoutine;

    [System.Serializable]
    public class SFXEntry
    {
        public string name;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildNamedLookup();
        InitializePool();
        InitializeMusicSources();
    }

    private void BuildNamedLookup()
    {
        namedSFXLookup = new Dictionary<string, SFXEntry>();
        foreach (var entry in namedSFX)
        {
            if (!string.IsNullOrEmpty(entry.name) && !namedSFXLookup.ContainsKey(entry.name))
                namedSFXLookup.Add(entry.name, entry);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            AudioSource src = CreatePooledSource();
            availableSources.Enqueue(src);
        }
    }

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("PooledSFXSource");
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = sfxMixerGroup;
        allPooledSources.Add(src);
        return src;
    }

    private void InitializeMusicSources()
    {
        GameObject goA = new GameObject("MusicSourceA");
        goA.transform.SetParent(transform);
        musicSourceA = goA.AddComponent<AudioSource>();
        musicSourceA.loop = true;
        musicSourceA.playOnAwake = false;
        musicSourceA.outputAudioMixerGroup = musicMixerGroup;

        GameObject goB = new GameObject("MusicSourceB");
        goB.transform.SetParent(transform);
        musicSourceB = goB.AddComponent<AudioSource>();
        musicSourceB.loop = true;
        musicSourceB.playOnAwake = false;
        musicSourceB.outputAudioMixerGroup = musicMixerGroup;

        activeMusicSource = musicSourceA;
    }


    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, bool randomizePitch = true)
    {
        if (clip == null) return;

        AudioSource src = GetAvailableSource();
        src.clip = clip;
        src.volume = volume;
        src.pitch = randomizePitch ? pitch + Random.Range(-defaultPitchVariation, defaultPitchVariation) : pitch;
        src.Play();
        StartCoroutine(ReturnToPoolWhenDone(src, clip.length / Mathf.Max(0.01f, src.pitch)));
    }

    public void PlaySFX(AudioClip[] clips, float volume = 1f, float pitch = 1f, bool randomizePitch = true)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip chosen = clips[Random.Range(0, clips.Length)];
        PlaySFX(chosen, volume, pitch, randomizePitch);
    }

    public void PlaySFXByName(string sfxName)
    {
        if (namedSFXLookup == null) BuildNamedLookup();

        if (namedSFXLookup.TryGetValue(sfxName, out SFXEntry entry))
        {
            PlaySFX(entry.clips, entry.volume);
        }
        else
        {
            Debug.LogWarning($"AudioManager: {sfxName} not found");
        }
    }

    private AudioSource GetAvailableSource()
    {
        if (availableSources.Count > 0)
            return availableSources.Dequeue();

        return CreatePooledSource();
    }

    private IEnumerator ReturnToPoolWhenDone(AudioSource src, float delay)
    {
        yield return new WaitForSeconds(delay);
        src.Stop();
        src.clip = null;
        availableSources.Enqueue(src);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float targetVolume = 1f)
    {
        if (clip == null) return;
        if (activeMusicSource.clip == clip && activeMusicSource.isPlaying) return; // ya sonando

        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(CrossfadeMusic(clip, loop, targetVolume));
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
        musicFadeRoutine = StartCoroutine(FadeOutAndStop(activeMusicSource,
            fadeDuration >= 0f ? fadeDuration : musicCrossfadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop, float targetVolume)
    {
        AudioSource incoming = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;
        AudioSource outgoing = activeMusicSource;

        incoming.clip = newClip;
        incoming.loop = loop;
        incoming.volume = 0f;
        incoming.Play();

        float t = 0f;
        float startOutVolume = outgoing.volume;
        while (t < musicCrossfadeDuration)
        {
            t += Time.deltaTime;
            float lerp = t / musicCrossfadeDuration;
            incoming.volume = Mathf.Lerp(0f, targetVolume, lerp);
            outgoing.volume = Mathf.Lerp(startOutVolume, 0f, lerp);
            yield return null;
        }

        incoming.volume = targetVolume;
        outgoing.Stop();
        outgoing.clip = null;

        activeMusicSource = incoming;
        musicFadeRoutine = null;
    }

    private IEnumerator FadeOutAndStop(AudioSource src, float duration)
    {
        float startVolume = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        src.Stop();
        src.volume = startVolume;
    }
}
