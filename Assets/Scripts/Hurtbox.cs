using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Hurtbox : MonoBehaviour
{
    public IDamageable Owner { get; private set; }

    private void Awake()
    {
        Owner = GetComponentInParent<IDamageable>();

        if (Owner == null)
            Debug.LogWarning($"{name} no IDamageable found on hurtbox", this);
    }
}