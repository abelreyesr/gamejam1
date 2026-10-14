using System;
using UnityEngine;

public class PlayerOrderIssuer : MonoBehaviour
{
    [SerializeField] private InputHandler _input;
    [SerializeField] private KaijuControllerBase _kaijuBehaviour;
    [SerializeField] private ParticleSystem _actionParticles;
    private IKaijuController _kaiju;

    private const float BufferWindow = 0.5f;
    private Action _bufferedOrder;
    private float _bufferedOrderTime;

    private bool _stunned = false;
    private bool CanIssueOrders => !_stunned && _kaijuBehaviour.CanStartAction();

    void Awake()
    {
        if (_input == null) _input = GetComponent<InputHandler>();
        _kaiju = _kaijuBehaviour as IKaijuController;
        if (_kaiju == null) _kaiju = FindAnyObjectByType<KaijuControllerBase>();
    }

    private void Update() => HandleBuffer();

    private void HandleBuffer()
    {
        if (_bufferedOrder == null) return;
        if (Time.unscaledTime - _bufferedOrderTime > BufferWindow) { _bufferedOrder = null; return; }
        if (!CanIssueOrders) return;

        var order = _bufferedOrder;
        _bufferedOrder = null;
        _actionParticles.Play();
        order.Invoke();
    }

    private void TryStartAction(Action order)
    {
        if (CanIssueOrders)
        {
            _actionParticles.Play();
            order.Invoke();
        }
        else
        {
            _bufferedOrder = order;
            _bufferedOrderTime = Time.unscaledTime;
        }
    }

    private void OnEnable()
    {
        _input.OnBasicAttack += HandleBasicAttack;
        _input.OnQAttack += HandleQAttack;
        _input.OnEAttack += HandleEAttack;
        _input.OnKaijuMoveOrder += HandleMoveOrder;
    }

    private void OnDisable()
    {
        _input.OnBasicAttack -= HandleBasicAttack;
        _input.OnQAttack -= HandleQAttack;
        _input.OnEAttack -= HandleEAttack;
        _input.OnKaijuMoveOrder -= HandleMoveOrder;
    }

    private void HandleBasicAttack(Vector3 worldPos) => TryStartAction(() => _kaiju.BasicAttack(GetAimDirection(worldPos)));
    private void HandleQAttack(Vector3 worldPos) => TryStartAction(() => _kaiju.QAttack(GetAimDirection(worldPos)));
    private void HandleEAttack(Vector3 worldPos) => TryStartAction(() => _kaiju.EAttack(GetAimDirection(worldPos)));
    private void HandleMoveOrder(Vector3 worldPos)
    {
        if (CanIssueOrders) _kaiju.Move(worldPos);
    }

    private Vector2 GetAimDirection(Vector3 worldPos)
    {
        Vector2 kaijuPos = _kaijuBehaviour.transform.position;
        return ((Vector2)worldPos - kaijuPos).normalized;
    }

}
