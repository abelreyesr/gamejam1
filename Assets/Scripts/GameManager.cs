using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private GameState _state = GameState.MainMenu;
    public GameState State => _state;

    [Header("Scenes")]
    [SerializeField] private string _mainMenuScene = "MainMenu";
    [SerializeField] private string _gameplayScene = "Gameplay";

    [Header("Slowmo Settings")]
    [SerializeField] private float _slowMoRampDuration = 0.4f;
    [SerializeField] private float _slowMoHoldDuration = 1.2f;
    [SerializeField] private float _slowMoRecoverDuration = 0.6f;

    [Header("Refs")]
    [SerializeField] private ParticleSystem _victoryFx;
    [SerializeField] private ParticleSystem _defeatFx;
    [SerializeField] private AudioClip _victoryStingerSfx;
    [SerializeField] private AudioClip _defeatStingerSfx;
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _battleMusic;

    public event System.Action<GameState> OnStateChanged;
    public event System.Action OnVictorySequenceStarted;
    public event System.Action OnDefeatSequenceStarted;

    private Coroutine _sequenceCoroutine;

    private InputHandler _inputHandler;

    public bool HasSeenTutorial = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == _gameplayScene)
        {
            _state = GameState.Playing;
            AudioManager.Instance.PlayMusic(_battleMusic);
        }
        else AudioManager.Instance.PlayMusic(_menuMusic);
    }

    public void StartNewGame()
    {
        Debug.LogError("StartNewGame llamado desde:\n" + System.Environment.StackTrace);

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        SceneManager.LoadScene(_gameplayScene);
        AudioManager.Instance.PlayMusic(_battleMusic);
    }

    public void ReturnToMainMenu()
    {
        Debug.LogError("ReturnToMainMenu llamado desde:\n" + System.Environment.StackTrace);

        Time.timeScale = 1f;
        SetState(GameState.MainMenu);
        SceneManager.LoadScene(_mainMenuScene);
        AudioManager.Instance.PlayMusic(_menuMusic);
    }

    public void RestartCurrentGame()
    {
        Debug.LogError("RestartCurrentGame llamado desde:\n" + System.Environment.StackTrace);

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        if (_state == GameState.Playing)
        {
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }
        else if (_state == GameState.Paused)
        {
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }

    public void TriggerDefeat()
    {
        if (_state == GameState.Defeat || _state == GameState.Victory) return;

        SetState(GameState.Defeat);
        
        if (_defeatFx != null) _defeatFx.Play();
        if (_defeatStingerSfx != null) AudioManager.Instance.PlaySFX(_defeatStingerSfx);

        if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
        _sequenceCoroutine = StartCoroutine(SlowMoSequence(0.2f));
    }

    public void TriggerVictory()
    {
        if (_state == GameState.Defeat || _state == GameState.Victory) return;

        SetState(GameState.Victory);
        
        if (_victoryFx != null) _victoryFx.Play();
        if (_victoryStingerSfx != null) AudioManager.Instance.PlaySFX(_victoryStingerSfx);

        if (_sequenceCoroutine != null) StopCoroutine(_sequenceCoroutine);
        _sequenceCoroutine = StartCoroutine(SlowMoSequence());
    }

    private IEnumerator SlowMoSequence(float holdOverride = 0f)
    {
        Time.timeScale = 0.1f;

        float hold = holdOverride > 0f ? holdOverride : _slowMoHoldDuration;
        yield return WaitRealSeconds(_slowMoHoldDuration);

        float t = 0f;
        while (t < _slowMoRecoverDuration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = t / _slowMoRecoverDuration;
            Time.timeScale = Mathf.Lerp(0f, 1, lerp);
            yield return null;
        }
        Time.timeScale = 1;

        yield return new WaitForSeconds(2f);

        switch (_state)
        {
            case GameState.Victory:
                AudioManager.Instance.StopMusic();
                OnVictorySequenceStarted?.Invoke(); break;
            case GameState.Defeat: OnDefeatSequenceStarted?.Invoke(); break;
            default: break;
        }
    }

    private void ShowVictoryScreen()
    {
    }

    private void ShowDeathScreen()
    {
    }

    private IEnumerator WaitRealSeconds(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void SetState(GameState newState)
    {
        _state = newState;
        OnStateChanged?.Invoke(_state);
    }
}