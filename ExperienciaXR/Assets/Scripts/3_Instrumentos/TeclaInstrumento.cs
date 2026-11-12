using UnityEngine;

public class TeclaInstrumento : MonoBehaviour
{
    [Tooltip("El Audio Source con la nota de esta tecla (desmarca Play On Awake)")]
    public AudioSource nota;

    [Tooltip("Cuántos metros se hunde la tecla al golpearla")]
    public float hundimiento = 0.01f;

    [Tooltip("Segundos mínimos entre dos golpes: evita que un golpe suene dos veces")]
    public float tiempoEntreGolpes = 0.12f;

    Vector3 posicionInicial;   // dónde está la tecla cuando nadie la toca
    float ultimoGolpe = -10f;  // cuándo fue el último golpe (-10 = hace mucho)

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void OnTriggerEnter(Collider otro)
    {
        if (otro.CompareTag("Baqueta"))
        {
            Golpear();
        }
    }

    // Es pública para poder probarla sin gafas (clic derecho en el componente > Probar: golpear).
    [ContextMenu("Probar: golpear")]
    public void Golpear()
    {
        // Si el último golpe fue hace muy poco, no hacemos nada.
        if (Time.time - ultimoGolpe < tiempoEntreGolpes)
        {
            return;
        }
        ultimoGolpe = Time.time;

        nota.Play();

        transform.localPosition = posicionInicial + Vector3.down * hundimiento;
        Invoke("VolverArriba", 0.1f);
    }

    void VolverArriba()
    {
        transform.localPosition = posicionInicial;
    }
}
