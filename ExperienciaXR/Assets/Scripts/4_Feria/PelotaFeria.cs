using UnityEngine;

public class PelotaFeria : MonoBehaviour
{
    [Tooltip("El árbitro: arrastra Estacion4_Feria")]
    public JuegoFeria juego;

    [Tooltip("Dónde aparece la pelota (un objeto vacío sobre el soporte)")]
    public Transform puntoDeLaPelota;

    [Tooltip("El piso: si la pelota lo toca, el lanzamiento ya cuenta")]
    public GameObject piso;

    [Tooltip("A cuántos metros del soporte ya cuenta como lanzada")]
    public float distanciaLanzada = 1f;

    [Tooltip("Segundos de espera antes de revisar los cubos")]
    public float segundosDeEspera = 3f;

    Rigidbody cuerpo;
    bool yaContada = false;   // ¿este lanzamiento ya se contó?

    // Lo llama JuegoFeria cuando empieza el juego y entre un intento y otro.
    public void Aparecer()
    {
        gameObject.SetActive(true);
        cuerpo = GetComponent<Rigidbody>();

        transform.position = puntoDeLaPelota.position;
        transform.rotation = puntoDeLaPelota.rotation;

        if (cuerpo.isKinematic == false)
        {
            cuerpo.linearVelocity = Vector3.zero;
            cuerpo.angularVelocity = Vector3.zero;
        }

        yaContada = false;
    }

    void Update()
    {
        // Solo cuenta si ya la soltaste (no es kinematic) y está lejos del soporte.
        if (yaContada == false &&
            cuerpo.isKinematic == false &&
            Vector3.Distance(transform.position, puntoDeLaPelota.position) > distanciaLanzada)
        {
            ContarLanzamiento();
        }
    }

    void OnCollisionEnter(Collision colision)
    {
        if (yaContada == false && colision.gameObject == piso)
        {
            ContarLanzamiento();
        }
    }

    void ContarLanzamiento()
    {
        yaContada = true;
        Invoke("AvisarAlJuego", segundosDeEspera);
    }

    void AvisarAlJuego()
    {
        juego.LanzamientoTerminado();
    }
}
