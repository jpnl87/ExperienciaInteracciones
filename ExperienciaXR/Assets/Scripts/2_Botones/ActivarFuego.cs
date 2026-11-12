using UnityEngine;

public class ActivarFuego : MonoBehaviour
{
    [Tooltip("Las partículas de fuego (desmarca su Play On Awake)")]
    public ParticleSystem fuego;

    // Lo llama el botón "Fuego".
    public void EncenderOApagar()
    {
        if (fuego.isEmitting)
        {
            fuego.Stop();
        }
        else
        {
            fuego.Play();
        }
    }
}
