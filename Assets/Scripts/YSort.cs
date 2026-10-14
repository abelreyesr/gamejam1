using UnityEngine;

public class YSort : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Transform sortPoint;

    private void Awake()
    {
        sortPoint = transform;
    }

    private void LateUpdate()
    {
        spriteRenderer.sortingOrder =
            Mathf.RoundToInt(-sortPoint.position.y * 100);
    }
}
