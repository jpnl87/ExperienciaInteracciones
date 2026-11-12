using UnityEngine;

public class ActivarSonido : MonoBehaviour
{
    [Tooltip("El Audio Source que debe sonar (desmarca su Play On Awake)")]
    public AudioSource parlante;

    // Lo llama el botón "Sonido".
    public void Sonar()
    {
        parlante.Play();
    }
}
