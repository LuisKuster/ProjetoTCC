using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Unity.XR.CoreUtils;

/// <summary>
/// Controla a ida e a volta pro cenario de configuracao.
/// Guarda de qual sala o player saiu e onde ele estava, pra devolver no mesmo lugar.
/// Fica num GameObject que nunca eh desativado (junto do ExpositionManager).
/// </summary>
public class ConfigRoomManager : MonoBehaviour
{
    public static ConfigRoomManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private CharacterController characterController;

    [Header("Sala de config")]
    [SerializeField] private GameObject configRoom;
    [SerializeField] private Transform configSpawnPoint;
    [SerializeField] private GameObject exitPortal;   // portal de saida DENTRO da sala de config

    [Header("Inicio do jogo")]
    [SerializeField] private GameObject firstRoom;
    [SerializeField] private Transform firstRoomSpawnPoint;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Eventos")]
    public UnityEvent onEnterConfig;
    public UnityEvent onExitConfig;
    public UnityEvent onGameStarted;

    private GameObject roomToReturnTo;
    private Vector3 savedRigPosition;
    private float savedRigYaw;
    private bool isInConfig;
    private bool busy;
    private bool gameStarted;

    public bool IsInConfig => isInConfig;
    public bool Busy => busy;
    public Transform PlayerHead => xrOrigin != null ? xrOrigin.Camera.transform : null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // antes do jogo comecar nao tem pra onde voltar, entao o portal de saida fica off
        if (exitPortal != null) exitPortal.SetActive(false);
    }

    // ---------- chamadas publicas (as zonas chamam isso) ----------

    /// Portal de entrada de cada cenario chama isso passando a sala dona do portal.
    public void EnterConfig(GameObject fromRoom)
    {
        if (busy || isInConfig || !gameStarted) return;
        StartCoroutine(EnterRoutine(fromRoom));
    }

    /// Portal de saida da sala de config chama isso.
    public void ExitConfig()
    {
        if (busy || !isInConfig || roomToReturnTo == null) return;
        StartCoroutine(ExitRoutine());
    }

    /// Zona de inicio chama isso (direto, ou no fim da cutscene inicial).
    public void StartGame()
    {
        if (busy || gameStarted) return;
        StartCoroutine(StartGameRoutine());
    }

    // ---------- rotinas ----------

    private IEnumerator EnterRoutine(GameObject fromRoom)
    {
        busy = true;
        roomToReturnTo = fromRoom;
        savedRigPosition = xrOrigin.transform.position;
        savedRigYaw = xrOrigin.transform.eulerAngles.y;

        yield return FadeOut();

        if (fromRoom != null) fromRoom.SetActive(false);
        configRoom.SetActive(true);
        if (exitPortal != null) exitPortal.SetActive(true);
        Teleport(configSpawnPoint.position, configSpawnPoint.eulerAngles.y);
        isInConfig = true;

        yield return FadeIn();
        onEnterConfig?.Invoke();
        busy = false;
    }

    private IEnumerator ExitRoutine()
    {
        busy = true;
        yield return FadeOut();

        configRoom.SetActive(false);
        roomToReturnTo.SetActive(true);
        Teleport(savedRigPosition, savedRigYaw);
        isInConfig = false;

        yield return FadeIn();
        onExitConfig?.Invoke();
        busy = false;
    }

    private IEnumerator StartGameRoutine()
    {
        busy = true;
        yield return FadeOut();

        configRoom.SetActive(false);
        firstRoom.SetActive(true);
        if (exitPortal != null) exitPortal.SetActive(true);
        Teleport(firstRoomSpawnPoint.position, firstRoomSpawnPoint.eulerAngles.y);
        gameStarted = true;
        isInConfig = false;

        yield return FadeIn();
        onGameStarted?.Invoke();
        busy = false;
    }

    // ---------- util ----------

    /// Reposiciona o rig. Desliga o CharacterController antes, senao ele ignora a mudanca
    /// de Transform e joga o player de volta.
    private void Teleport(Vector3 rigPosition, float yaw)
    {
        bool hadController = characterController != null && characterController.enabled;
        if (hadController) characterController.enabled = false;

        xrOrigin.transform.SetPositionAndRotation(rigPosition, Quaternion.Euler(0f, yaw, 0f));

        if (hadController)
        {
            characterController.enabled = true;
            Physics.SyncTransforms();
        }
    }

    // TROCAR pelas chamadas do teu ExpositionManager
    private IEnumerator FadeOut()
    {
        // ex: yield return ExpositionManager.Instance.FadeOutRoutine();
        yield return new WaitForSeconds(fadeDuration);
    }

    private IEnumerator FadeIn()
    {
        // ex: yield return ExpositionManager.Instance.FadeInRoutine();
        yield return new WaitForSeconds(fadeDuration);
    }
}
