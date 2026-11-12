using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(Button))]
public class VideoChangeButton : MonoBehaviour
{
    [Tooltip("El VideoPlayerPanel que reproduce el video")]
    public VideoPlayerPanel videoPlayerPanel;

    [Tooltip("El video del proyecto (si no usas enlace)")]
    public VideoClip clip;

    [Tooltip("El enlace del video (si marcas Use URL)")]
    public string url;

    [Tooltip("Marcado: usa el enlace. Sin marcar: usa el archivo del proyecto")]
    public bool useURL = false;

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonPressed);
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonPressed);
    }

    public void OnButtonPressed()
    {
        if (videoPlayerPanel == null)
        {
            return;
        }

        if (useURL)
        {
            videoPlayerPanel.LoadVideoURL(url);
        }
        else
        {
            videoPlayerPanel.LoadVideoClip(clip);
        }
    }
}
