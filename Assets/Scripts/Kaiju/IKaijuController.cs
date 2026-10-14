using UnityEngine;

public interface IKaijuController
{
    KaijuState State { get; }
    void Move(Vector3 worldPos);
    void BasicAttack(Vector2 direction);
    void QAttack(Vector2 direction);
    void EAttack(Vector2 direction);
}