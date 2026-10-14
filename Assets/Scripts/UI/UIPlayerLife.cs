using UnityEngine;
using UnityEngine.UI;

public class UIPlayerLife : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetActive(bool active)
    {
        _animator.SetBool("active", active);
    }
}
