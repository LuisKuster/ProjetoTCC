using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject dialogueUI;
    [Tooltip("Deixe vazio: ele acha a Main Camera sozinho no Start.")]
    [SerializeField] private Transform  playerHead;

    [Header("Highlight")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Header("Diálogo")]
    [SerializeField] private DialogueTree dialogueTree;

    [Header("Config")]
    [SerializeField] private float interactionRadius = 2f;

    [Header("Giro")]
    [Tooltip("Quanto maior, mais rápido o NPC vira. 3-6 dá uma virada natural.")]
    [SerializeField] private float velocidadeGiro = 4f;
    [Tooltip("Continua acompanhando o player enquanto ele estiver no range.")]
    [SerializeField] private bool  seguirPlayer = true;
    [Tooltip("Volta para a rotação original ao sair do range.")]
    [SerializeField] private bool  voltarRotacaoOriginal = true;
    [Tooltip("Segundos de espera antes de voltar (o design pede 3s).")]
    [SerializeField] private float delayParaVoltar = 3f;

    // IDs de shader: URP usa _BaseColor, built-in usa _Color.
    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor     = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock _mpb;
    private Color[] _coresOriginais;

    private DialogueUI _dialogueUIRef;
    private bool       _playerNoRange;
    private Quaternion _rotacaoInicial;
    private float      _tempoDeSaida = -1f;

    void Start()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = interactionRadius;

        _rotacaoInicial = transform.rotation;

        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;

        // Null check que faltava: antes isso quebrava em runtime se o campo
        // ficasse vazio em algum dos NPCs.
        if (dialogueUI != null)
        {
            _dialogueUIRef = dialogueUI.GetComponent<DialogueUI>();
            if (_dialogueUIRef == null)
                Debug.LogError($"[NPCInteractable] '{name}': o objeto de UI não tem DialogueUI.", this);

            dialogueUI.SetActive(false);
        }
        else
        {
            Debug.LogError($"[NPCInteractable] '{name}': campo Dialogue UI vazio.", this);
        }

        PrepararHighlight();
        SetHighlight(false);
    }

    void Update()
    {
        if (_playerNoRange)
        {
            if (playerHead == null) return;
            if (!seguirPlayer) return;

            GirarPara(playerHead.position);
            return;
        }

        if (!voltarRotacaoOriginal || _tempoDeSaida < 0f) return;
        if (Time.time - _tempoDeSaida < delayParaVoltar) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, _rotacaoInicial, velocidadeGiro * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, _rotacaoInicial) < 0.5f)
        {
            transform.rotation = _rotacaoInicial;
            _tempoDeSaida = -1f;
        }
    }

    // ---------------- Trigger ----------------

    void OnTriggerEnter(Collider other)
    {
        if (_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = true;
        _tempoDeSaida  = -1f;

        SetHighlight(true);
        OpenDialogue();
    }

    void OnTriggerExit(Collider other)
    {
        if (!_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = false;
        _tempoDeSaida  = Time.time;

        SetHighlight(false);
        CloseDialogue();
    }

    // ---------------- Giro ----------------

    /// <summary>
    /// Giro gradual por Slerp, só no eixo Y. Antes era um LookRotation seco
    /// no OnTriggerEnter, que fazia o NPC "teleportar" a rotação.
    /// </summary>
    void GirarPara(Vector3 alvo)
    {
        Vector3 dir = alvo - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            velocidadeGiro * Time.deltaTime);
    }

    // ---------------- Diálogo ----------------

    void OpenDialogue()
    {
        if (dialogueUI == null || _dialogueUIRef == null) return;

        dialogueUI.SetActive(true);
        _dialogueUIRef.Setup(dialogueTree);
    }

    void CloseDialogue()
    {
        _dialogueUIRef?.FecharTudo();
    }

    // ---------------- Highlight ----------------

    /// <summary>
    /// Lê a cor original do sharedMaterial uma vez, sem instanciar nada.
    /// </summary>
    void PrepararHighlight()
    {
        _mpb = new MaterialPropertyBlock();
        _coresOriginais = new Color[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] == null) { _coresOriginais[i] = Color.white; continue; }

            var mat = meshRenderers[i].sharedMaterial;
            _coresOriginais[i] = mat == null           ? Color.white
                : mat.HasProperty(IdBaseColor)         ? mat.GetColor(IdBaseColor)
                : mat.HasProperty(IdColor)             ? mat.GetColor(IdColor)
                : Color.white;
        }
    }

    /// <summary>
    /// MaterialPropertyBlock em vez de renderer.material: não instancia material
    /// por renderer (era leak no Quest) e não apaga a cor original com branco.
    /// </summary>
    void SetHighlight(bool active)
    {
        if (_mpb == null) return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] == null) continue;

            Color c = active ? highlightColor : _coresOriginais[i];

            meshRenderers[i].GetPropertyBlock(_mpb);
            _mpb.SetColor(IdBaseColor, c);
            _mpb.SetColor(IdColor, c);
            meshRenderers[i].SetPropertyBlock(_mpb);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}