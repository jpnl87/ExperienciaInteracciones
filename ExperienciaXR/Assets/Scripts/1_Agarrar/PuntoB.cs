using UnityEngine;

public class PuntoB : MonoBehaviour
{
    [Tooltip("El objeto que hay que traer hasta aquí")]
    public GameObject cubo;

    [Tooltip("El letrero '¡Lo lograste!' (debe empezar apagado)")]
    public GameObject letrero;

    [Tooltip("Las partículas de chispas")]
    public ParticleSystem chispas;

    void OnCollisionEnter(Collision colision)
    {
        if (colision.gameObject == cubo)
        {
            letrero.SetActive(true);
            chispas.Play();
        }
    }

    void OnCollisionExit(Collision colision)
    {
        if (colision.gameObject == cubo)
        {
            letrero.SetActive(false);
        }
    }
}
