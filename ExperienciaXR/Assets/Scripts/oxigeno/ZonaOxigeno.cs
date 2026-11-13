using System.Collections.Generic;
using UnityEngine;

namespace ElGranHongo
{
    public enum TipoZona
    {
        Bruma,
        Recarga
    }

    /// <summary>
    /// Marca un volumen de bruma (gasta oxígeno) o de recarga (refugio, puesto del Gran Hongo).
    /// Va en un objeto con Box, Sphere o Capsule Collider marcado como Is Trigger.
    /// Para purificar una zona de bruma, llama a Limpiar() desde el evento del montículo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ZonaOxigeno : MonoBehaviour
    {
        static readonly List<ZonaOxigeno> activas = new List<ZonaOxigeno>();

        [SerializeField] TipoZona tipo = TipoZona.Bruma;
        [Tooltip("Opcional: las partículas o la malla de niebla que se apagan al limpiar la zona.")]
        [SerializeField] GameObject nieblaVisual;

        Collider volumen;

        void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        void Awake()
        {
            volumen = GetComponent<Collider>();
        }

        void OnEnable()
        {
            if (!activas.Contains(this)) activas.Add(this);
        }

        void OnDisable()
        {
            activas.Remove(this);
        }

        /// <summary>Quita la bruma de esta zona: deja de gastar oxígeno y apaga la niebla visual.</summary>
        public void Limpiar()
        {
            if (nieblaVisual != null) nieblaVisual.SetActive(false);
            enabled = false;
        }

        bool Contiene(Vector3 punto)
        {
            if (volumen == null || !volumen.enabled) return false;
            if (!volumen.bounds.Contains(punto)) return false;
            // ClosestPoint devuelve el mismo punto cuando está dentro del collider.
            return (volumen.ClosestPoint(punto) - punto).sqrMagnitude < 0.0001f;
        }

        public static bool Contiene(TipoZona tipo, Vector3 punto)
        {
            foreach (ZonaOxigeno zona in activas)
                if (zona.tipo == tipo && zona.Contiene(punto))
                    return true;
            return false;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = tipo == TipoZona.Bruma ? new Color(0.3f, 0.9f, 0.2f, 0.25f) : new Color(0.3f, 0.6f, 1f, 0.25f);
            Collider c = GetComponent<Collider>();
            if (c != null) Gizmos.DrawCube(c.bounds.center, c.bounds.size);
        }
    }
}
