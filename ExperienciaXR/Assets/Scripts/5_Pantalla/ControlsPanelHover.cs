using UnityEngine;
using UnityEngine.EventSystems;

public class ControlsPanelHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("El VideoPlayerPanel que oculta y muestra los controles")]
    public VideoPlayerPanel controller;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mientras el rayo esté sobre el panel, los controles siguen visibles.
        controller.ShowControlsInstant();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Cuando sale, si el video sigue corriendo, vuelve a empezar la cuenta atrás.
        if (controller.IsPlaying)
        {
            controller.StartHide();
        }
    }
}
