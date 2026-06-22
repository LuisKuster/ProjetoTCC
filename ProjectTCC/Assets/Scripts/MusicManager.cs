using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Músicas por Cena")]
    [SerializeField] private SceneMusic[] sceneMusics;

    [Header("Config")]
    [SerializeField] private float fadeSpeed = 1f;

    private AudioSource _audioSource;
    private string _currentScene = "";

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;   // nome exato da cena no Build Settings
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, 
                       UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == _currentScene) return;
        _currentScene = scene.name;

        foreach (var sm in sceneMusics)
        {
            if (sm.sceneName == scene.name && sm.clip != null)
            {
                TrocarMusica(sm.clip);
                return;
            }
        }

        // Nenhuma música definida pra essa cena — para o áudio
        _audioSource.Stop();
    }

    void TrocarMusica(AudioClip novoClip)
    {
        if (_audioSource.clip == novoClip) return;
        StopAllCoroutines();
        StartCoroutine(FadeTrocar(novoClip));
    }

    System.Collections.IEnumerator FadeTrocar(AudioClip novoClip)
    {
        // Fade out
        while (_audioSource.volume > 0)
        {
            _audioSource.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        _audioSource.Stop();
        _audioSource.clip = novoClip;
        _audioSource.Play();

        // Fade in
        while (_audioSource.volume < 1)
        {
            _audioSource.volume += fadeSpeed * Time.deltaTime;
            yield return null;
        }
    }

    // Chamado pelo painel de configurações (RF-02)
    public void SetVolume(float v) => _audioSource.volume = v;
}