using UnityEngine;

public class Diapositivas : MonoBehaviour
{
    [Tooltip("Las diapositivas en orden (arrastra los Panels)")]
    public GameObject[] paginas;

    int paginaActual = 0;   // en cuál vamos

    void Start()
    {
        MostrarPagina(0);   // al empezar, la primera
    }

    // Lo llama el botón "Siguiente".
    public void Siguiente()
    {
        if (paginaActual < paginas.Length - 1)   // si no es la última...
        {
            MostrarPagina(paginaActual + 1);
        }
    }

    // Lo llama el botón "Anterior".
    public void Anterior()
    {
        if (paginaActual > 0)                    // si no es la primera...
        {
            MostrarPagina(paginaActual - 1);
        }
    }

    void MostrarPagina(int numero)
    {
        paginaActual = numero;

        // Apagamos todas y encendemos solo la que toca.
        for (int i = 0; i < paginas.Length; i++)
        {
            paginas[i].SetActive(i == numero);
        }
    }
}
