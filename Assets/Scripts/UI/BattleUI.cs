using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Kaiju")]
    [SerializeField] private Image _kaijuHealthBar;
    [SerializeField] private Image _kaijuStaminaBar;
    [SerializeField] private KaijuControllerBase _kaijuController;
    private Color _staminaColor;

    [Header("Enemy")]
    [SerializeField] private Image _enemyHealthBar;
    [SerializeField] private EnemyBase _enemyController;

    [Header("Player lifes")]
    [SerializeField] private PlayerMovement _player;
    [SerializeField] private GameObject _playerLife;
    private List<UIPlayerLife> _playerLifes = new List<UIPlayerLife>();

    [Header("EndScreen")]
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private GameObject _victoryScreen;

    private void OnEnable()
    {
        GameManager.Instance.OnVictorySequenceStarted += ShowVictoryScreen;
        GameManager.Instance.OnDefeatSequenceStarted += ShowDeathScreen;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnVictorySequenceStarted -= ShowVictoryScreen;
        GameManager.Instance.OnDefeatSequenceStarted -= ShowDeathScreen;
    }

    private void Awake()
    {
        if(_kaijuController == null) _kaijuController = FindAnyObjectByType<KaijuController>();
        if(_enemyController == null) _enemyController = FindAnyObjectByType<EnemyBase>();
        if(_player == null) _player = FindAnyObjectByType<PlayerMovement>();

        _staminaColor = _kaijuStaminaBar.color;

        InitPlayerLifes();
    }

    private void Update()
    {
        UpdateHealthBars();
        UpdateStaminaBar();
    }

    private void UpdateHealthBars()
    {
        _kaijuHealthBar.fillAmount = _kaijuController.HealthPercent;
        _enemyHealthBar.fillAmount = _enemyController.HealthPercent;

        for(int i = 0; i < _playerLifes.Count; i++)
        {
            bool active = i < _player.Hp;
            
            _playerLifes[i].SetActive(active);
        }
    }

    private void UpdateStaminaBar()
    {
        float fill = _kaijuController.StaminaPercent;
        _kaijuStaminaBar.fillAmount = fill;

        if (fill < 0.2f) _kaijuStaminaBar.color = _staminaColor * 0.25f;
        else if (fill < 0.4f) _kaijuStaminaBar.color = _staminaColor * 0.5f;
        else _kaijuStaminaBar.color = _staminaColor;
    }

    private void InitPlayerLifes()
    {
        _playerLifes.Add(_playerLife.GetComponent<UIPlayerLife>());

        for (int i = 0; i < _player.MaxHp-1; i++) 
        {
            UIPlayerLife life = Instantiate(_playerLife, _playerLife.transform.position, Quaternion.identity, _playerLife.transform.parent).GetComponent<UIPlayerLife>();
            _playerLifes.Add(life);
        }
    }

    private void ShowDeathScreen()
    { 
        _deathScreen.SetActive(true);
    }

    private void ShowVictoryScreen()
    {
        _victoryScreen.SetActive(true);
    }

    public void RestartGame() => GameManager.Instance.StartNewGame();

    public void MainMenu() => GameManager.Instance.ReturnToMainMenu();
}
