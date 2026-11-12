using TMPro;
using UnityEngine;

public class JuegoFeria : MonoBehaviour
{
    [Header("El jugador y el hotspot")]
    [Tooltip("La cabeza del jugador: arrastra CenterEyeAnchor")]
    public Transform cabeza;

    [Tooltip("El punto donde hay que pararse: arrastra el hotspot de la estación")]
    public Transform hotspot;

    [Tooltip("A cuántos metros del hotspot ya cuenta como 'parado encima'")]
    public float radio = 0.6f;

    [Header("El juego")]
    public PelotaFeria pelota;

    [Tooltip("Los 6 cubos de la pirámide")]
    public Transform[] cubos;

    public int intentosMaximos = 4;

    [Tooltip("Si un cubo se movió más que esto (en metros), cuenta como tumbado")]
    public float distanciaTumbado = 0.15f;

    [Tooltip("Si un cubo quedó inclinado más que esto (en grados), cuenta como tumbado")]
    public float anguloTumbado = 45f;

    [Header("El panel")]
    [Tooltip("El texto del panel")]
    public TMP_Text texto;

    [Tooltip("El botón 'Jugar otra vez' (empieza apagado)")]
    public GameObject botonOtraVez;

    Vector3[] posicionesIniciales;      // dónde estaba cada cubo
    Quaternion[] rotacionesIniciales;   // cómo estaba girado cada cubo
    int intentosUsados = 0;
    bool jugando = false;
    bool terminado = false;

    void Start()
    {
        // Anotamos dónde está cada cubo antes de que nadie los toque.
        posicionesIniciales = new Vector3[cubos.Length];
        rotacionesIniciales = new Quaternion[cubos.Length];
        for (int i = 0; i < cubos.Length; i++)
        {
            posicionesIniciales[i] = cubos[i].position;
            rotacionesIniciales[i] = cubos[i].rotation;
        }

        pelota.gameObject.SetActive(false);
        botonOtraVez.SetActive(false);
        texto.text = "Párate en el círculo para jugar";
    }

    void Update()
    {
        if (jugando || terminado)
        {
            return;   // ya hay un juego en curso o esperando "Jugar otra vez"
        }

        // Medimos la distancia solo en el piso: la altura de la cabeza no importa.
        Vector3 pies = cabeza.position;
        pies.y = hotspot.position.y;

        if (Vector3.Distance(pies, hotspot.position) < radio)
        {
            Empezar();
        }
    }

    void Empezar()
    {
        jugando = true;
        intentosUsados = 0;
        MostrarIntentos();
        pelota.Aparecer();
    }

    // Lo llama PelotaFeria cuando termina un lanzamiento.
    public void LanzamientoTerminado()
    {
        intentosUsados = intentosUsados + 1;

        if (TodosTumbados())
        {
            Terminar("¡Ganaste!\nTumbaste todos los cubos");
        }
        else if (intentosUsados >= intentosMaximos)
        {
            Terminar("Perdiste\nQuedaron cubos en pie");
        }
        else
        {
            MostrarIntentos();
            pelota.Aparecer();
        }
    }

    bool TodosTumbados()
    {
        for (int i = 0; i < cubos.Length; i++)
        {
            bool seMovio = Vector3.Distance(cubos[i].position, posicionesIniciales[i]) > distanciaTumbado;
            bool seInclino = Quaternion.Angle(cubos[i].rotation, rotacionesIniciales[i]) > anguloTumbado;

            if (!(seMovio || seInclino))
            {
                return false;   // con uno solo que siga en pie en su sitio, ya no están todos
            }
        }
        return true;
    }

    void Terminar(string mensaje)
    {
        jugando = false;
        terminado = true;
        pelota.gameObject.SetActive(false);
        texto.text = mensaje;
        botonOtraVez.SetActive(true);
    }

    void MostrarIntentos()
    {
        texto.text = "Intentos: " + (intentosMaximos - intentosUsados) + " de " + intentosMaximos;
    }

    // Lo llama el botón "Jugar otra vez".
    public void JugarOtraVez()
    {
        for (int i = 0; i < cubos.Length; i++)
        {
            cubos[i].position = posicionesIniciales[i];
            cubos[i].rotation = rotacionesIniciales[i];

            // Le quitamos el impulso que traía; si no, seguiría rodando.
            Rigidbody cuerpo = cubos[i].GetComponent<Rigidbody>();
            cuerpo.linearVelocity = Vector3.zero;
            cuerpo.angularVelocity = Vector3.zero;
        }

        terminado = false;
        botonOtraVez.SetActive(false);
        texto.text = "Párate en el círculo para jugar";
    }
}
