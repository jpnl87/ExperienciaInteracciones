//
using TMPro;
using UnityEngine;

public class DialogoRobot : MonoBehaviour
{
    
    public GameObject canvas;          // el Canvas que se muestra y se esconde
    public TMP_Text textoNarracion;    // el texto que se escribe letra por letra

    
    public AudioClip narracion;        // el audio que dice el mismo texto

    
    public float distanciaParaHablar = 2.5f;   // en metros: más cerca que esto, el robot habla

    private AudioSource altavoz;       // el componente que reproduce el audio
    private Transform jugador;         // los ojos del jugador (la cámara de las gafas)
    private float tiempoPorLetra;      // cuántos segundos tarda en aparecer cada letra
    private float tiempoTranscurrido;  // cuánto lleva hablando
    private bool hablando;             // ¿está hablando ahora?

    void Start()
    {
        altavoz = GetComponent<AudioSource>();
        altavoz.clip = narracion;

        // La cámara principal son los ojos del jugador. En el rig de Meta es CenterEyeAnchor.
        if (Camera.main != null) jugador = Camera.main.transform;

        // Si hay audio, las letras van al ritmo de la voz. Si todavía no hay audio, 20 letras por segundo.
        if (narracion != null) tiempoPorLetra = narracion.length / textoNarracion.text.Length;
        else tiempoPorLetra = 0.05f;

        canvas.SetActive(false);
    }

    void Update()
    {
        if (jugador == null) return;

        // Distancia en el piso (sin contar la altura): así da igual si el jugador es alto o bajo.
        Vector3 diferencia = jugador.position - transform.position;
        diferencia.y = 0f;
        bool cerca = diferencia.magnitude < distanciaParaHablar;

        if (cerca && !hablando) EmpezarAHablar();
        if (!cerca && hablando) DejarDeHablar();

        if (hablando)
        {
            // Escribir el texto letra por letra.
            tiempoTranscurrido += Time.deltaTime;
            textoNarracion.maxVisibleCharacters = (int)(tiempoTranscurrido / tiempoPorLetra);

            // Girar el globo para que mire al jugador (si no, se leería al revés desde atrás).
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - jugador.position);
        }
    }

    void EmpezarAHablar()
    {
        hablando = true;
        tiempoTranscurrido = 0f;
        textoNarracion.maxVisibleCharacters = 0;
        canvas.SetActive(true);
        if (narracion != null) altavoz.Play();
    }

    void DejarDeHablar()
    {
        hablando = false;
        canvas.SetActive(false);
        altavoz.Stop();
    }
}
