
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referencias UI")]
    public RawImage videoDisplay;
    public RectTransform videoPanel;
    public GameObject controlsPanel;

    [Tooltip("Texto opcional para avisar 'Cargando...' o que hubo un error")]
    public Text textoEstado;

    [Header("Video y audio")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [Tooltip("Marcado: usa el enlace. Sin marcar: usa el archivo del proyecto")]
    public bool useURL = false;
    public string videoURL;
    public VideoClip videoClip;

    [Header("Controles UI")]
    public Button playPauseBtn;
    public Sprite playIcon, pauseIcon;
    public Slider seekSlider;
    public Slider volumeSlider;
    public Button muteBtn;
    public Sprite muteOnIcon, muteOffIcon;
    public Button fullscreenBtn;

    [Header("Comportamiento del panel")]
    [Tooltip("Segundos antes de ocultar los controles")]
    public float hideDelay = 3f;
    [Tooltip("Segundos que tarda en desvanecerse")]
    public float fadeDuration = 0.5f;

    CanvasGroup controlsCanvasGroup;
    Coroutine hideCoroutine;
    bool isFullscreen = false;
    bool isMuted = false;
    float noActualizarBarraHasta = 0f;   // hasta cuándo la barra no sigue al video

    int originalSiblingIndex;
    Vector2 origAnchorMin, origAnchorMax, origOffsetMin, origOffsetMax;

    // Awake corre una sola vez, la primera vez que el objeto se enciende.
    void Awake()
    {
        origAnchorMin = videoPanel.anchorMin;
        origAnchorMax = videoPanel.anchorMax;
        origOffsetMin = videoPanel.offsetMin;
        origOffsetMax = videoPanel.offsetMax;
        originalSiblingIndex = videoPanel.GetSiblingIndex();

        controlsCanvasGroup = controlsPanel.GetComponent<CanvasGroup>();
        if (controlsCanvasGroup == null)
        {
            controlsCanvasGroup = controlsPanel.AddComponent<CanvasGroup>();
        }

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;   // nosotros pasamos la imagen al RawImage
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        if (audioSource != null)
        {
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        // Nos suscribimos aquí, una sola vez, a los avisos del VideoPlayer.
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoTerminado;
        videoPlayer.errorReceived += OnError;

        // Conectamos los controles por código para no hacerlo botón por botón en el Inspector.
        playPauseBtn.onClick.AddListener(TogglePlayPause);
        seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        muteBtn.onClick.AddListener(ToggleMute);
        fullscreenBtn.onClick.AddListener(ToggleFullscreen);

        muteBtn.image.sprite = muteOffIcon;
        volumeSlider.value = 1f;
    }

    // OnEnable corre cada vez que el panel se enciende: aquí cargamos el video.
    void OnEnable()
    {
        ShowControlsInstant();

        if (useURL)
        {
            LoadVideoURL(videoURL);
        }
        else
        {
            LoadVideoClip(videoClip);
        }
    }

    void OnDisable()
    {
        StopHide();
        videoPlayer.Stop();

        // Si se apagó en pantalla completa, vuelve a su tamaño para la próxima vez.
        // (Aquí no se cambia el orden en la Hierarchy: Unity no lo deja mientras se apaga.)
        if (isFullscreen)
        {
            RestaurarTamano();
            isFullscreen = false;
        }
    }

    void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnVideoTerminado;
        videoPlayer.errorReceived -= OnError;
    }

    void Update()
    {
        // Si tocaste la barra hace muy poco, no la movemos: el video todavía está saltando.
        if (videoPlayer.isPrepared && videoPlayer.length > 0 && Time.unscaledTime > noActualizarBarraHasta)
        {
            seekSlider.SetValueWithoutNotify((float)(videoPlayer.time / videoPlayer.length));
        }
    }

    // También la usa VideoChangeButton para cambiar de video.
    public void LoadVideoClip(VideoClip newClip)
    {
        useURL = false;
        videoClip = newClip;

        if (newClip == null)
        {
            MostrarEstado("Falta asignar el video");
            return;
        }

        PrepararPantalla();
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = newClip;
        videoPlayer.Prepare();
    }

    // También la usa VideoChangeButton para cambiar de video.
    public void LoadVideoURL(string newURL)
    {
        useURL = true;
        videoURL = newURL;

        if (string.IsNullOrEmpty(newURL))
        {
            videoPlayer.Stop();
            videoDisplay.texture = null;
            videoDisplay.color = Color.black;
            MostrarEstado("Pega un enlace en el Inspector");
            return;
        }

        PrepararPantalla();
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = ConvertirEnlaceDeDrive(newURL);
        videoPlayer.Prepare();
    }

    void PrepararPantalla()
    {
        videoPlayer.Stop();
        videoDisplay.texture = null;
        videoDisplay.color = Color.black;   // sin textura, un RawImage negro se ve negro
        playPauseBtn.image.sprite = playIcon;
        MostrarEstado("Cargando...");
    }

    // Un enlace de Drive abre una página, no el video. Hay que pedirle la descarga directa:
    //   https://drive.google.com/file/d/CODIGO/view?usp=sharing
    //   https://drive.google.com/uc?export=download&id=CODIGO
    // Drive no deja descargar así los archivos pesados: por eso mejor un video liviano.
    string ConvertirEnlaceDeDrive(string enlace)
    {
        string marca = "drive.google.com/file/d/";
        int inicio = enlace.IndexOf(marca);
        if (inicio < 0)
        {
            return enlace;   // no es de Drive: se usa tal cual
        }

        inicio = inicio + marca.Length;
        int fin = enlace.IndexOf('/', inicio);
        if (fin < 0)
        {
            fin = enlace.Length;
        }

        string codigo = enlace.Substring(inicio, fin - inicio);
        return "https://drive.google.com/uc?export=download&id=" + codigo;
    }

    void OnPrepared(VideoPlayer vp)
    {
        videoDisplay.texture = vp.texture;
        videoDisplay.color = Color.white;
        MostrarEstado("");

        // Si el RawImage tiene Aspect Ratio Fitter, le decimos la forma del video para que no se estire.
        AspectRatioFitter ajustador = videoDisplay.GetComponent<AspectRatioFitter>();
        if (ajustador != null && vp.height > 0)
        {
            ajustador.aspectRatio = (float)vp.width / vp.height;
        }

        vp.Play();
        playPauseBtn.image.sprite = pauseIcon;
        StartHide();
    }

    void OnVideoTerminado(VideoPlayer vp)
    {
        vp.Pause();
        playPauseBtn.image.sprite = playIcon;
        ShowControlsInstant();
    }

    void OnError(VideoPlayer vp, string mensaje)
    {
        Debug.LogWarning("El video no cargó: " + mensaje);
        MostrarEstado("No se pudo cargar el video. Revisa el enlace y el internet");
    }

    void MostrarEstado(string mensaje)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }

    // ---------- Controles ----------

    public void TogglePlayPause()
    {
        if (!videoPlayer.isPrepared)
        {
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            playPauseBtn.image.sprite = playIcon;
            ShowControlsInstant();
        }
        else
        {
            videoPlayer.Play();
            playPauseBtn.image.sprite = pauseIcon;
            StartHide();
        }
    }

    public void OnSeekSliderChanged(float val)
    {
        if (!videoPlayer.isPrepared)
        {
            return;
        }

        noActualizarBarraHasta = Time.unscaledTime + 0.5f;
        videoPlayer.time = val * videoPlayer.length;
    }

    public void OnVolumeChanged(float val)
    {
        if (audioSource != null)
        {
            audioSource.volume = val;
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (audioSource != null)
        {
            audioSource.mute = isMuted;
        }
        muteBtn.image.sprite = isMuted ? muteOnIcon : muteOffIcon;
    }

    // "Pantalla completa" aquí significa: ocupar toda la diapositiva donde está el video.
    // El video no se saca de su diapositiva; por eso se apaga con ella y no tapa los botones.
    public void ToggleFullscreen()
    {
        if (!isFullscreen)
        {
            videoPanel.SetAsLastSibling();   // encima de lo demás de la diapositiva
            videoPanel.anchorMin = Vector2.zero;
            videoPanel.anchorMax = Vector2.one;
            videoPanel.offsetMin = Vector2.zero;
            videoPanel.offsetMax = Vector2.zero;
        }
        else
        {
            videoPanel.SetSiblingIndex(originalSiblingIndex);
            RestaurarTamano();
        }

        isFullscreen = !isFullscreen;
    }

    // Devuelve el panel al tamaño y lugar que tenía al empezar.
    void RestaurarTamano()
    {
        videoPanel.anchorMin = origAnchorMin;
        videoPanel.anchorMax = origAnchorMax;
        videoPanel.offsetMin = origOffsetMin;
        videoPanel.offsetMax = origOffsetMax;
    }

    // ---------- Ocultar y mostrar los controles ----------

    public bool IsPlaying
    {
        get { return videoPlayer != null && videoPlayer.isPlaying; }
    }

    public void ShowControlsInstant()
    {
        StopHide();
        controlsCanvasGroup.alpha = 1f;
        controlsCanvasGroup.interactable = true;
        controlsCanvasGroup.blocksRaycasts = true;
    }

    public void StartHide()
    {
        StopHide();
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    public void StopHide()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);

        if (videoPlayer.isPlaying)
        {
            // El desvanecimiento va aquí mismo. En el original era otra corrutina dentro de esta,
            // y StopHide no podía detenerla: los controles podían ocultarse mientras apuntabas.
            float pasado = 0f;
            while (pasado < fadeDuration)
            {
                pasado += Time.unscaledDeltaTime;
                controlsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, pasado / fadeDuration);
                yield return null;
            }

            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }

        hideCoroutine = null;
    }

    // Apuntar al panel muestra los controles; dejar de apuntar los vuelve a ocultar.
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowControlsInstant();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsPlaying)
        {
            StartHide();
        }
    }
}
