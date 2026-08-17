using UnityEngine;
using System.Collections;

/// <summary>
/// Gerencia a música de fundo do jogo.
/// O volume final do AudioSource é sempre o produto de dois fatores independentes:
///   _volumeUsuario -> controlado pelo slider do painel de config (0..1)
///   _fade          -> controlado internamente pelas corrotinas de transição (0..1)
/// Isso impede que uma troca de música "resete" o volume escolhido pelo jogador.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Quanto maior, mais rápido o crossfade entre músicas.")]
    [SerializeField] private float fadeSpeed = 1f;

    [Tooltip("Volume usado na primeira vez que o jogo roda (antes de existir save).")]
    [SerializeField, Range(0f, 1f)] private float volumePadrao = 0.5f;

    [Header("Música inicial (opcional)")]
    [SerializeField] private AudioClip clipInicial;

    private const string PREF_KEY = "volume_musica";

    private AudioSource _audioSource;
    private float _volumeUsuario = 1f;
    private float _fade = 1f;

    /// <summary>Volume escolhido pelo jogador (0..1). É isso que o slider deve exibir.</summary>
    public float CurrentVolume => _volumeUsuario;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;

        _volumeUsuario = PlayerPrefs.GetFloat(PREF_KEY, volumePadrao);
        AplicarVolume();
    }

    void Start()
    {
        if (clipInicial != null)
            TocarMusica(clipInicial);
    }

    // ---------------------------------------------------------------- VOLUME

    /// <summary>Chamado pelo painel de configurações (RF-02).</summary>
    public void SetVolume(float v)
    {
        _volumeUsuario = Mathf.Clamp01(v);
        AplicarVolume();

        PlayerPrefs.SetFloat(PREF_KEY, _volumeUsuario);
        PlayerPrefs.Save();
    }

    private void AplicarVolume()
    {
        _audioSource.volume = _volumeUsuario * _fade;
    }

    // ----------------------------------------------------------- TROCA/FADE

    /// <summary>Faz crossfade para o clip informado. Ignora se já for o clip atual.</summary>
    public void TocarMusica(AudioClip novoClip)
    {
        if (novoClip == null) return;
        if (_audioSource.clip == novoClip && _audioSource.isPlaying) return;

        StopAllCoroutines();
        StartCoroutine(RotinaTrocar(novoClip));
    }

    /// <summary>Faz fade out e para o áudio.</summary>
    public void PararMusica()
    {
        StopAllCoroutines();
        StartCoroutine(RotinaParar());
    }

    private IEnumerator RotinaTrocar(AudioClip novoClip)
    {
        if (_audioSource.isPlaying)
            yield return FadeAte(0f);

        _audioSource.clip = novoClip;
        _audioSource.Play();

        yield return FadeAte(1f);
    }

    private IEnumerator RotinaParar()
    {
        yield return FadeAte(0f);
        _audioSource.Stop();
        _audioSource.clip = null;
    }

    private IEnumerator FadeAte(float alvo)
    {
        // Time.unscaledDeltaTime: se em algum momento você pausar o jogo com
        // timeScale = 0 pra abrir o menu, o fade não trava.
        while (!Mathf.Approximately(_fade, alvo))
        {
            _fade = Mathf.MoveTowards(_fade, alvo, fadeSpeed * Time.unscaledDeltaTime);
            AplicarVolume();
            yield return null;
        }

        _fade = alvo;
        AplicarVolume();
    }
}