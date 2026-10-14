using UnityEngine;

public class AnimationParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] _particles;

    public void PlayParticles(int index)
    {
        if (_particles[index] != null)
        {
            _particles[index].Play();
        }
    }
}
