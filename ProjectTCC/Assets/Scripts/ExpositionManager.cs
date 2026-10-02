using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Toca cutscenes: fade in → áudio + slideshow sincronizado → troca de sala/teleporte → fade out.
/// Um por cena. Quem dispara é o ExpositionTrigger.
/// </summary>
public class ExpositionManager : MonoBehaviour
{
    public static ExpositionManager Instance { get; private set; }

    [Header("Referências UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image imageDisplay;

    [Tooltip("Opcional: só aparece quando o slide tem legenda.")]
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("Áudio")]
    [Tooltip("Só a 'caixa de som'. NÃO coloque clip aqui: o áudio vem de cada cutscene.\nSe ficar vazio, é criado sozinho.")]
    [SerializeField] private AudioSource audioSource;

    [Header("Fades")]
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Fade da imagem ao trocar de slide. 0 = corte seco.")]
    [SerializeField] private float slideFadeDuration = 0.25f;

    [Header("Reposicionamento")]
    [Tooltip("Tag da RAIZ do rig do player (o objeto com CharacterController).")]
    [SerializeField] private string playerTag = "Player";

    private bool isShowing;
    private bool skip;

    /// <summary>True enquanto uma cutscene está rodando (ou depois de um ending).</summary>
    public bool IsShowing => isShowing;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        if (imageDisplay != null) imageDisplay.enabled = false;
        if (textDisplay != null) textDisplay.enabled = false;

        SetupAudio();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void SetupAudio()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Narração: sempre 2D, pra tocar igual não importa onde o player está
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Começa uma cutscene. Retorna false se já tinha outra rodando (e nada acontece).
    /// </summary>
    public bool TriggerExposition(ExpositionEventSO data, GameObject roomToDisable, GameObject roomToEnable,
                                  Transform spawnPoint, Action onComplete = null)
    {
        if (data == null)
        {
            Debug.LogWarning("ExpositionManager: cutscene nula.");
            return false;
        }
        if (isShowing)
        {
            Debug.LogWarning($"ExpositionManager: já tem uma cutscene rodando, ignorando '{data.name}'.");
            return false;
        }

        StartCoroutine(ExpositionRoutine(data, roomToDisable, roomToEnable, spawnPoint, onComplete));
        return true;
    }

    /// <summary>Corta a cutscene atual (teste no editor / botão de pular).</summary>
    [ContextMenu("Pular cutscene")]
    public void SkipExposition() => skip = true;

    // ---------------------------------------------------------------------

    private IEnumerator ExpositionRoutine(ExpositionEventSO data, GameObject roomToDisable, GameObject roomToEnable,
                                          Transform spawnPoint, Action onComplete)
    {
        isShowing = true;
        skip = false;

        // Primeira imagem já entra junto com o fade da tela
        int slideAtual = 0;
        ShowSlide(data, 0);
        SetImageAlpha(1f);

        canvasGroup.blocksRaycasts = true;
        yield return Lerp(fadeDuration, a => canvasGroup.alpha = a, 0f, 1f);

        // Áudio começa com a tela já preta; o relógio dos slides começa junto
        if (data.audio != null)
        {
            audioSource.clip = data.audio;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"ExpositionManager: '{data.name}' não tem áudio, usando {data.duracaoSemAudio}s.");
        }

        float duracao = data.DuracaoConteudo;
        float inicio = Time.time;

        while (!skip)
        {
            float t = Time.time - inicio;
            if (t >= duracao) break;

            int alvo = data.SlideNoTempo(t);
            if (alvo != slideAtual)
            {
                slideAtual = alvo;
                yield return ChangeSlide(data, alvo); // o relógio continua correndo durante o fade
                continue;
            }
            yield return null;
        }

        yield return Wait(data.extraHold);
        audioSource.Stop();

        // Fim de jogo: tela fica preta e o manager fica travado de propósito
        if (data.isEnding)
        {
            onComplete?.Invoke();
            yield break;
        }

        // Troca de sala e teleporte com a tela preta
        if (roomToDisable != null) roomToDisable.SetActive(false);
        if (roomToEnable != null) roomToEnable.SetActive(true);
        RepositionPlayer(spawnPoint);

        yield return Lerp(fadeDuration, a => canvasGroup.alpha = a, 1f, 0f);

        if (imageDisplay != null) imageDisplay.enabled = false;
        if (textDisplay != null) textDisplay.enabled = false;
        canvasGroup.blocksRaycasts = false;
        isShowing = false;
        onComplete?.Invoke();
    }

    private IEnumerator ChangeSlide(ExpositionEventSO data, int index)
    {
        bool withFade = slideFadeDuration > 0f && imageDisplay != null;

        if (withFade) yield return Lerp(slideFadeDuration, SetImageAlpha, 1f, 0f);
        ShowSlide(data, index);
        if (withFade) yield return Lerp(slideFadeDuration, SetImageAlpha, 0f, 1f);
    }

    private void ShowSlide(ExpositionEventSO data, int index)
    {
        Sprite sprite = null;
        string legenda = null;

        if (index >= 0 && index < data.SlideCount)
        {
            sprite = data.slides[index].imagem;
            legenda = data.slides[index].legenda;
        }

        if (imageDisplay != null)
        {
            imageDisplay.sprite = sprite;
            imageDisplay.enabled = sprite != null;
        }

        if (textDisplay != null)
        {
            textDisplay.text = legenda;
            textDisplay.enabled = !string.IsNullOrEmpty(legenda);
        }
    }

    private IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds && !skip)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>Lerp genérico usado pelo fade da tela e pelo fade da imagem.</summary>
    private IEnumerator Lerp(float duration, Action<float> set, float from, float to)
    {
        if (duration <= 0f)
        {
            set(to);
            yield break;
        }

        float t = 0f;
        set(from);
        while (t < duration)
        {
            t += Time.deltaTime;
            set(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        set(to);
    }

    private void SetImageAlpha(float a)
    {
        if (imageDisplay == null) return;
        var c = imageDisplay.color;
        c.a = a;
        imageDisplay.color = c;
    }

    private void RepositionPlayer(Transform spawnPoint)
    {
        if (spawnPoint == null) return; // sem spawn = player fica onde está

        var player = GameObject.FindWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"ExpositionManager: nenhum objeto com a tag '{playerTag}' encontrado.");
            return;
        }

        // CharacterController sobrescreve a posição se ficar ligado durante o teleporte
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

        if (cc != null) cc.enabled = true;
    }
}