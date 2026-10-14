using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _tutorial;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _musicSlider;

    private const string MasterKey = "MasterVolume";
    private const string SfxKey = "SFXVolume";
    private const string MusicKey = "MusicVolume";

    private void Start()
    {
        float master = PlayerPrefs.GetFloat(MasterKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);

        if (_masterSlider != null) _masterSlider.value = master;
        if (_sfxSlider != null) _sfxSlider.value = sfx;
        if (_musicSlider != null) _musicSlider.value = music;

        SetMasterVolume(master);
        SetSFXVolume(sfx);
        SetMusicVolume(music);
    }

    public void SetMasterVolume(float value)
    {
        _mixer.SetFloat(MasterKey, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(MasterKey, value);
    }

    public void SetSFXVolume(float value)
    {
        _mixer.SetFloat(SfxKey, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(SfxKey, value);
    }

    public void SetMusicVolume(float value)
    {
        _mixer.SetFloat(MusicKey, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
        PlayerPrefs.SetFloat(MusicKey, value);
    }

    public void ToggleFullscreen()
    {
        bool goingFullscreen = !Screen.fullScreen;

        if (goingFullscreen)
        {
            Resolution native = Screen.currentResolution;
            Screen.SetResolution(native.width, native.height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool _tutorialOpened = false;
    public void PlayGame()
    {
        if (!GameManager.Instance.HasSeenTutorial)
        {
            ShowTutorial();
        }
        else if(!_tutorialOpened)
        {
            GameManager.Instance.StartNewGame();
        }
    }

    public void ShowTutorial()
    {
        _tutorialOpened = true;
        _tutorial.SetActive(true);
        GameManager.Instance.HasSeenTutorial = true;
    }

    public void CloseTutorial()
    {
        _tutorialOpened = false;
        _tutorial.SetActive(false);
    }
}