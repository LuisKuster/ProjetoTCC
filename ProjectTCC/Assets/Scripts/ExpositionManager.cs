using System.Collections;
using UnityEngine;
using TMPro;
using Unity.XR.CoreUtils;

public class ExpositionManager : MonoBehaviour
{
    public static ExpositionManager Instance;

    [Header("Referências UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textDisplay;

    [Header("Configuração")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Reposicionamento")]
    [SerializeField] private string playerTag = "Player";

    private bool isShowing = false;

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
        textDisplay.text = data.text;
        canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(data.holdDuration);

        if (roomToDisable != null) roomToDisable.SetActive(false);
        if (roomToEnable != null)
        {
            roomToEnable.SetActive(true);
            RepositionPlayer(spawnPoint);
        }

        yield return Fade(1f, 0f);

        canvasGroup.blocksRaycasts = false;
        isShowing = false;
        data.onExpositionComplete?.Invoke();
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