using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private Camera _cam;

    private InputAction _move;
    private InputAction _basicAttack;
    private InputAction _qAttack;
    private InputAction _eAttack;
    private InputAction _pointerPosition;
    private InputAction _kaijuMove;

    private Vector2 _moveInput;
    public Vector2 MoveInput => _moveInput;

    public event Action<Vector3> OnBasicAttack;
    public event Action<Vector3> OnQAttack;
    public event Action<Vector3> OnEAttack;
    public event Action<Vector3> OnKaijuMoveOrder;

    private bool _kaijuMoveHeld;

    private void Awake()
    {
        var map = inputActions.FindActionMap("Gameplay");

        _move = map.FindAction("Move");
        _basicAttack = map.FindAction("AttackPress");
        _qAttack = map.FindAction("QAttack");
        _eAttack = map.FindAction("EAttack");
        _kaijuMove = map.FindAction("KaijuMove");
        _pointerPosition = map.FindAction("PointerPosition");

        if (_cam == null) _cam = Camera.main;
    }

    private void OnEnable()
    {
        inputActions.FindActionMap("Gameplay").Enable();

        _move.performed += OnMove;
        _move.canceled += OnMove;

        _basicAttack.started += ctx => OnBasicAttack?.Invoke(GetWorldPointerPos());
        _qAttack.started += ctx => OnQAttack?.Invoke(GetWorldPointerPos());
        _eAttack.started += ctx => OnEAttack?.Invoke(GetWorldPointerPos());

        _kaijuMove.started += ctx => _kaijuMoveHeld = true;
        _kaijuMove.canceled += ctx => _kaijuMoveHeld = false;
    }

    private void OnDisable()
    {
        _move.performed -= OnMove;
        _move.canceled -= OnMove;

        inputActions.FindActionMap("Gameplay").Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (GameManager.Instance.State is GameState.Defeat or GameState.Victory) DisableInput();
        if (!_kaijuMoveHeld) return;
        OnKaijuMoveOrder?.Invoke(GetWorldPointerPos());
    }

    private Vector3 GetWorldPointerPos() => _cam.ScreenToWorldPoint(_pointerPosition.ReadValue<Vector2>());

    private void DisableInput()
    {
        _moveInput = Vector2.zero;
        enabled = false;
    }
}
