using System.Collections.Generic;
using System;
using UnityEngine;

public class EnemyAttackExecutor : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AttackHitbox[] _hitboxes;

    public IDamageable owner;
    private EnemyAttack _currentAttack;
    public bool IsAttacking { get; private set; }

    public event Action OnAttackFinished;

    public void Awake()
    {
        owner = GetComponent<IDamageable>();
    }

    public void Execute(EnemyAttack attack)
    {
        _currentAttack = attack;
        IsAttacking = true;
        _animator.SetTrigger(attack.animatorTrigger);
    }

    public void ActivateHitbox(int index)
    {
        _hitboxes[index].Configure(_currentAttack.damage, _currentAttack.knockback, owner);
        _hitboxes[index].SetActive(true);
    }

    public void DeactivateHitbox(int index) => _hitboxes[index].SetActive(false);

    public void OnAttackAnimEnd()
    {
        IsAttacking = false;
        foreach (var hb in _hitboxes) hb.SetActive(false);
        OnAttackFinished?.Invoke();
    }
}