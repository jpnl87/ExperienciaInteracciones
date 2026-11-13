using UnityEngine;
using UnityEngine.Events;

namespace ElGranHongo
{
    /// <summary>
    /// Máscara de oxígeno. Al llevarla a la cara se coloca: desaparece de la mano,
    /// suena el sello de goma y aparece la barra de oxígeno.
    /// Va en la raíz de la máscara (la que tiene Grabbable y Rigidbody).
    /// </summary>
    public class MascaraOxigeno : MonoBehaviour
    {
        [Tooltip("CenterEyeAnchor del rig. Si queda vacío usa la Main Camera.")]
        [SerializeField] Transform cabeza;
        [Tooltip("Qué tan cerca de los ojos debe llegar el centro de la máscara para colocarse.")]
        [SerializeField] float distanciaColocar = 0.25f;
        [SerializeField] AudioClip sonidoColocar;

        [Tooltip("Por ejemplo: activar el borde de la máscara pegado a la cámara, cambiar el letrero.")]
        public UnityEvent alColocar;

        Renderer malla;
        bool puesta;

        void Awake()
        {
            malla = GetComponentInChildren<Renderer>();
        }

        void Update()
        {
            if (puesta) return;

            if (cabeza == null)
            {
                if (Camera.main == null) return;
                cabeza = Camera.main.transform;
            }

            // El centro visible de la máscara, no su pivote, que puede estar en un borde.
            Vector3 centro = malla != null ? malla.bounds.center : transform.position;
            if (Vector3.Distance(centro, cabeza.position) <= distanciaColocar)
                Colocar();
        }

        public void Colocar()
        {
            if (puesta) return;
            puesta = true;

            if (sonidoColocar != null)
                AudioSource.PlayClipAtPoint(sonidoColocar, cabeza != null ? cabeza.position : transform.position);

            if (SistemaOxigeno.Instancia != null)
                SistemaOxigeno.Instancia.PonerMascara();

            alColocar.Invoke();

            // Al desactivarla, el Interaction SDK la suelta de la mano.
            gameObject.SetActive(false);
        }
    }
}
