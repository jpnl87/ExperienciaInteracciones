using UnityEngine;

public class CuboGiratorio : MonoBehaviour
{
    [Tooltip("Cuántos grados gira con cada pulsación")]
    public float gradosPorPulsacion = 45f;

    [Tooltip("Qué tan rápido gira, en grados por segundo")]
    public float velocidad = 180f;

    float anguloObjetivo = 0f;   // el ángulo al que queremos llegar

    void Start()
    {
        anguloObjetivo = transform.eulerAngles.y;   // empezamos con el que ya tiene
    }

    void Update()
    {
        Quaternion objetivo = Quaternion.Euler(0f, anguloObjetivo, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, objetivo, velocidad * Time.deltaTime);
    }

    // Lo llama el botón "Derecha".
    public void GirarDerecha()
    {
        anguloObjetivo += gradosPorPulsacion;
    }

    // Lo llama el botón "Izquierda".
    public void GirarIzquierda()
    {
        anguloObjetivo -= gradosPorPulsacion;
    }
}
