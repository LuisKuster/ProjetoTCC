using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpositionManager : MonoBehaviour
{
    public static ExpositionManager Instance;

    [Header("Referências UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private Image imageDisplay;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Configuração")]
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Fade curto ao trocar de slide. 0 = corte seco.")]
    [SerializeField] private float slideFadeDuration = 0.25f;

    [Header("Reposicionamento")]
    [SerializeField] private string playerTag = "Player";

    private bool isShowing = false;
    private bool skip = false;

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
    }

    public void TriggerExposition(ExpositionEventSO expositionEvent, GameObject roomToDisable, GameObject roomToEnable, Transform spawnPoint)
    {
        if (isShowing)
        {
            Debug.LogWarning("ExpositionManager: já tem uma exposição rodando, ignorando chamada.");
            return;
        }
        StartCoroutine(ExpositionRoutine(expositionEvent, roomToDisable, roomToEnable, spawnPoint));
    }

    private IEnumerator ExpositionRoutine(ExpositionEventSO data, GameObject roomToDisable, GameObject roomToEnable, Transform spawnPoint)
    {
        isShowing = true;
        skip = false;

        textDisplay.text = data.text;
        bool temSlides = data.slides != null && data.slides.Length > 0;

        // Primeira imagem já entra junto com o fade da tela
        if (imageDisplay != null)
        {
            Sprite primeira = temSlides ? data.slides[0].image : data.image;
            imageDisplay.sprite = primeira;
            imageDisplay.enabled = primeira != null;
            SetImageAlpha(1f);
        }

        canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);

        // Áudio só depois do fade, pra narração não começar com a tela meio transparente
        if (data.voiceOver != null && audioSource != null)
        {
            audioSource.clip = data.voiceOver;
            audioSource.Play();
        }

        if (temSlides)
            yield return RodarSlides(data);
        else
            yield return Esperar(TempoImagemUnica(data));

        if (audioSource != null) audioSource.Stop();

        // Fim de jogo: tela fica preta, encerra aqui
        if (data.isEnding)
        {
            data.onExpositionComplete?.Invoke();
            yield break;
        }

        if (roomToDisable != null) roomToDisable.SetActive(false);
        if (roomToEnable != null)
        {
            roomToEnable.SetActive(true);
            RepositionPlayer(spawnPoint);
        }

        yield return Fade(1f, 0f);

        if (imageDisplay != null) imageDisplay.enabled = false;
        canvasGroup.blocksRaycasts = false;
        isShowing = false;
        data.onExpositionComplete?.Invoke();
    }

    private IEnumerator RodarSlides(ExpositionEventSO data)
    {
        for (int i = 0; i < data.slides.Length; i++)
        {
            var slide = data.slides[i];

            // O slide 0 já foi montado antes do fade da tela
            if (i > 0)
            {
                if (slideFadeDuration > 0f && imageDisplay != null)
                    yield return FadeImagem(1f, 0f);

                if (imageDisplay != null)
                {
                    imageDisplay.sprite = slide.image;
                    imageDisplay.enabled = slide.image != null;
                }

                if (!string.IsNullOrEmpty(slide.text)) textDisplay.text = slide.text;

                if (slideFadeDuration > 0f && imageDisplay != null)
                    yield return FadeImagem(0f, 1f);
            }
            else if (!string.IsNullOrEmpty(slide.text))
            {
                textDisplay.text = slide.text;
            }

            yield return Esperar(slide.duration);
            if (skip) yield break;
        }

        yield return Esperar(data.extraHold);
    }

    private float TempoImagemUnica(ExpositionEventSO data)
    {
        float hold = data.holdDuration > 0f
            ? data.holdDuration
            : (data.voiceOver != null ? data.voiceOver.length : 0f);
        return hold + data.extraHold;
    }

    private IEnumerator Esperar(float segundos)
    {
        float t = 0f;
        while (t < segundos && !skip)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>Corta a exposição atual (teste no editor / botão de pular).</summary>
    [ContextMenu("Pular exposição")]
    public void SkipExposition() => skip = true;

    private void SetImageAlpha(float a)
    {
        if (imageDisplay == null) return;
        var c = imageDisplay.color;
        c.a = a;
        imageDisplay.color = c;
    }

    private IEnumerator FadeImagem(float from, float to)
    {
        float t = 0f;
        SetImageAlpha(from);
        while (t < slideFadeDuration)
        {
            t += Time.deltaTime;
            SetImageAlpha(Mathf.Lerp(from, to, t / slideFadeDuration));
            yield return null;
        }
        SetImageAlpha(to);
    }

    private void RepositionPlayer(Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("ExpositionManager: spawnPoint não configurado.");
            return;
        }

        var player = GameObject.FindWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("ExpositionManager: nenhum Player encontrado.");
            return;
        }

        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null) characterController.enabled = false;

        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        if (characterController != null) characterController.enabled = true;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        canvasGroup.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}