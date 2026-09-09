using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private DialogueUI dialogueUI;

    [Tooltip("Deixe vazio: ele acha a Main Camera sozinho no Start.")]
    [SerializeField] private Transform playerHead;

    [Tooltip("Opcional. Se preenchido, o NPC para de andar enquanto conversa.")]
    [SerializeField] private NPCPatrol patrulha;

    [Header("Highlight")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

    [Header("Diálogo")]
    [SerializeField] private DialogueTree dialogueTree;

    [Header("Config")]
    [SerializeField] private float interactionRadius = 2f;

    [Header("Giro")]
    [SerializeField] private float velocidadeGiro = 4f;
    [SerializeField] private bool  seguirPlayer = true;

    [Tooltip("Segundos antes de voltar ao normal depois que o player sai.")]
    [SerializeField] private float delayParaVoltar = 3f;

    [Tooltip("Só para NPCs SEM patrulha: volta à rotação original após o delay. " +
             "Com patrulha, o NPC retoma o caminho e este campo é ignorado.")]
    [SerializeField] private bool voltarRotacaoOriginal = true;

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor     = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock _mpb;
    private Color[] _coresOriginais;

    private bool       _playerNoRange;
    private Quaternion _rotacaoInicial;
    private float      _tempoDeSaida = -1f;

    /// <summary>Qual árvore este NPC está usando agora. Só leitura.</summary>
    public DialogueTree ArvoreAtual => dialogueTree;

    void Start()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = interactionRadius;

        _rotacaoInicial = transform.rotation;

        if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
        if (dialogueUI == null) dialogueUI = GetComponent<DialogueUI>();
        if (patrulha   == null) patrulha   = GetComponent<NPCPatrol>();

        if (dialogueUI == null)
            Debug.LogError($"[NPCInteractable] '{name}': não achei o DialogueUI.", this);

        PrepararHighlight();
        SetHighlight(false);
    }

    void Update()
    {
        // Conversando: encara o player e ignora o resto.
        if (_playerNoRange)
        {
            if (playerHead == null || !seguirPlayer) return;
            GirarPara(playerHead.position);
            return;
        }

        if (_tempoDeSaida < 0f) return;
        if (Time.time - _tempoDeSaida < delayParaVoltar) return;

        // Passou o delay. Com patrulha, retoma o caminho — o próprio NPCPatrol
        // cuida da rotação a partir daqui.
        if (patrulha != null)
        {
            patrulha.Retomar();
            _tempoDeSaida = -1f;
            return;
        }

        if (!voltarRotacaoOriginal) { _tempoDeSaida = -1f; return; }

        transform.rotation = Quaternion.Slerp(
            transform.rotation, _rotacaoInicial, velocidadeGiro * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, _rotacaoInicial) < 0.5f)
        {
            transform.rotation = _rotacaoInicial;
            _tempoDeSaida = -1f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = true;
        _tempoDeSaida  = -1f;

        patrulha?.Pausar();   // para de andar para conversar
        SetHighlight(true);
        dialogueUI?.Setup(dialogueTree);
    }

    void OnTriggerExit(Collider other)
    {
        if (!_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = false;
        _tempoDeSaida  = Time.time;   // começa a contagem dos 3 segundos

        SetHighlight(false);
        dialogueUI?.FecharTudo();
    }

    // ---------------- Troca de árvore ----------------

    /// <summary>
    /// Troca a árvore de diálogo deste NPC. Arraste este método no UnityEvent
    /// que marca a conclusão da missão (ex.: On Cesta Completa da CestaCafe)
    /// e escolha o asset da árvore nova.
    /// </summary>
    public void TrocarDialogo(DialogueTree novaTree)
    {
        if (novaTree == null)
        {
            Debug.LogError($"[NPCInteractable] '{name}': TrocarDialogo recebeu null.", this);
            return;
        }

        if (dialogueTree == novaTree) return;
        dialogueTree = novaTree;

        // Se o player já está no raio, o OnTriggerEnter não roda de novo —
        // remonta na hora para ele não continuar vendo a árvore velha.
        if (_playerNoRange) dialogueUI?.Setup(novaTree);
    }

    // ---------------- Giro / Highlight ----------------

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

    void PrepararHighlight()
    {
        _mpb = new MaterialPropertyBlock();
        _coresOriginais = new Color[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] == null) { _coresOriginais[i] = Color.white; continue; }

            var mat = meshRenderers[i].sharedMaterial;
            _coresOriginais[i] = mat == null   ? Color.white
                : mat.HasProperty(IdBaseColor) ? mat.GetColor(IdBaseColor)
                : mat.HasProperty(IdColor)     ? mat.GetColor(IdColor)
                : Color.white;
        }
    }

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