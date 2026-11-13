using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ElGranHongo
{
    /// <summary>
    /// Oxígeno del jugador. Baja dentro de la bruma (más rápido sin máscara), se recarga en las
    /// zonas de recarga (refugio y puesto del Gran Hongo) y hace parpadear la vista en rojo como aviso.
    /// Si llega a 0, funde a negro y devuelve al jugador al refugio con el tanque lleno.
    /// Va en un objeto vacío llamado SistemaOxigeno (uno solo por escena).
    /// </summary>
    public class SistemaOxigeno : MonoBehaviour
    {
        public static SistemaOxigeno Instancia { get; private set; }

        [Header("Cabeza del jugador")]
        [Tooltip("CenterEyeAnchor del rig. Si queda vacío usa la Main Camera.")]
        [SerializeField] Transform cabeza;

        [Header("Aire (por segundo)")]
        [SerializeField] float maximo = 100f;
        [Tooltip("0,8 = unos 125 s de aire en la bruma con máscara.")]
        [SerializeField] float consumoConMascara = 0.8f;
        [Tooltip("3,3 = unos 30 s sin máscara: tiempo justo para volver a resguardarse.")]
        [SerializeField] float consumoSinMascara = 3.3f;
        [SerializeField] float recarga = 20f;

        [Header("Barra en la mano (aparece al ponerse la máscara)")]
        [SerializeField] GameObject panelOxigeno;
        [Tooltip("Image con Image Type = Filled.")]
        [SerializeField] Image barra;
        [SerializeField] TMP_Text texto;
        [SerializeField] Color colorNormal = new Color(0.35f, 0.95f, 0.45f);
        [SerializeField] Color colorBajo = new Color(0.95f, 0.25f, 0.2f);
        [SerializeField, Range(0f, 1f)] float umbralBajo = 0.25f;

        [Header("Aviso de daño")]
        [Tooltip("CanvasGroup de la imagen roja que cubre la vista.")]
        [SerializeField] CanvasGroup flashRojo;
        [SerializeField, Range(0f, 1f)] float intensidadMaxima = 0.45f;
        [Tooltip("Parpadeos por segundo.")]
        [SerializeField] float parpadeos = 1.5f;
        [Tooltip("Opcional: pitido de alarma en bucle (sin Play On Awake).")]
        [SerializeField] AudioSource alarma;

        [Header("Al agotarse")]
        [Tooltip("CanvasGroup de una imagen negra que cubre la vista.")]
        [SerializeField] CanvasGroup fundido;
        [SerializeField] float duracionFundido = 1f;
        [Tooltip("La raíz del rig (el objeto que se mueve con el teleport).")]
        [SerializeField] Transform rigJugador;
        [SerializeField] Transform puntoRefugio;

        [Header("Eventos")]
        public UnityEvent alPonerMascara;
        public UnityEvent alAgotarse;

        float actual;
        bool tieneMascara;
        bool reiniciando;

        public bool TieneMascara => tieneMascara;
        public float Fraccion => maximo > 0f ? actual / maximo : 0f;

        void Awake()
        {
            Instancia = this;
        }

        void Start()
        {
            actual = maximo;
            if (panelOxigeno != null) panelOxigeno.SetActive(false);
            if (flashRojo != null) flashRojo.alpha = 0f;
            if (fundido != null) fundido.alpha = 0f;
            ActualizarBarra();
        }

        void Update()
        {
            if (cabeza == null)
            {
                if (Camera.main == null) return;
                cabeza = Camera.main.transform;
            }

            if (reiniciando) return;

            Vector3 posicion = cabeza.position;
            bool enBruma = ZonaOxigeno.Contiene(TipoZona.Bruma, posicion);

            if (enBruma)
                actual -= (tieneMascara ? consumoConMascara : consumoSinMascara) * Time.deltaTime;
            else if (ZonaOxigeno.Contiene(TipoZona.Recarga, posicion))
                actual += recarga * Time.deltaTime;

            actual = Mathf.Clamp(actual, 0f, maximo);
            ActualizarBarra();
            ActualizarAviso(enBruma);

            if (actual <= 0f)
                StartCoroutine(Reiniciar());
        }

        /// <summary>La llama la máscara al colocarla en la cara.</summary>
        public void PonerMascara()
        {
            if (tieneMascara) return;
            tieneMascara = true;
            if (panelOxigeno != null) panelOxigeno.SetActive(true);
            ActualizarBarra();
            alPonerMascara.Invoke();
        }

        void ActualizarBarra()
        {
            float fraccion = Fraccion;

            if (barra != null)
            {
                barra.fillAmount = fraccion;
                barra.color = fraccion < umbralBajo ? colorBajo : colorNormal;
            }

            if (texto != null)
                texto.text = "O2 " + Mathf.CeilToInt(fraccion * 100f) + "%";
        }

        void ActualizarAviso(bool enBruma)
        {
            // Parpadea en rojo si respira bruma sin máscara, o si con máscara le queda poco aire.
            bool peligro = enBruma && (!tieneMascara || Fraccion < umbralBajo);

            if (flashRojo != null)
            {
                float objetivo = 0f;
                if (peligro)
                {
                    float onda = (Mathf.Sin(Time.time * parpadeos * 2f * Mathf.PI) + 1f) * 0.5f;
                    float fuerza = tieneMascara ? 1f - Fraccion / umbralBajo : 1f;
                    objetivo = onda * intensidadMaxima * Mathf.Clamp(fuerza, 0.4f, 1f);
                }
                flashRojo.alpha = Mathf.MoveTowards(flashRojo.alpha, objetivo, Time.deltaTime * 3f);
            }

            if (alarma != null)
            {
                if (peligro && !alarma.isPlaying) alarma.Play();
                else if (!peligro && alarma.isPlaying) alarma.Stop();
            }
        }

        IEnumerator Reiniciar()
        {
            reiniciando = true;
            if (alarma != null) alarma.Stop();
            alAgotarse.Invoke();

            yield return Fundir(1f);

            if (rigJugador != null && puntoRefugio != null)
            {
                // La cabeza puede estar desplazada del centro del rig: se corrige para que quede en el punto.
                Vector3 desfase = cabeza.position - rigJugador.position;
                desfase.y = 0f;
                rigJugador.position = puntoRefugio.position - desfase;
            }

            actual = maximo;
            ActualizarBarra();
            if (flashRojo != null) flashRojo.alpha = 0f;

            yield return Fundir(0f);
            reiniciando = false;
        }

        IEnumerator Fundir(float hasta)
        {
            if (fundido == null) yield break;

            float desde = fundido.alpha;
            float tiempo = 0f;
            while (tiempo < duracionFundido)
            {
                tiempo += Time.deltaTime;
                fundido.alpha = Mathf.Lerp(desde, hasta, tiempo / duracionFundido);
                yield return null;
            }
            fundido.alpha = hasta;
        }
    }
}
