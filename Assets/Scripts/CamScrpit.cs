using UnityEngine;

public class CamScrpit : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _kaiju;
    [SerializeField] private Transform _boss;

    void Awake()
    {
        if(_player == null) _player = FindAnyObjectByType<PlayerMovement>().transform;
        if(_kaiju == null) _kaiju = FindAnyObjectByType<KaijuController>().transform;
    }

    void Update()
    {
        Vector2 pos = Vector2.Lerp(_player.position, _kaiju.position, 0.5f);
        
        if(_boss  != null)
            pos = Vector2.Lerp(pos, _boss.position, 0.5f);

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}
