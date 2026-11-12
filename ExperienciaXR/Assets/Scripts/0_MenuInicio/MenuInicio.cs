//
using UnityEngine;
using UnityEngine.SceneManagement;   // para poder usar SceneManager

public class MenuInicio : MonoBehaviour
{
    [Tooltip("Nombre EXACTO de la escena que abre PLAY (mayúsculas y espacios cuentan)")]
    public string escenaDelJuego = "Estaciones";

    // Lo llama el botón "PLAY".
    public void Jugar()
    {
        SceneManager.LoadScene(escenaDelJuego);
    }

    // Lo llama el botón "Salir".
    public void Salir()
    {
#if UNITY_EDITOR
        // Estamos probando dentro de Unity: detenemos el Play.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Es el juego ya construido (en las gafas o en el PC): se cierra.
        Application.Quit();
#endif
    }
}
