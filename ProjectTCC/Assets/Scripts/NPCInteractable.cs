using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("O DialogueUI — agora vive no próprio NPC_Base, não num canvas.")]
    [SerializeField] private DialogueUI dialogueUI;

    [Tooltip("Deixe vazio: ele acha a Main Camera sozinho no Start.")]
    [SerializeField] private Transform playerHead;

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
    [SerializeField] private bool  voltarRotacaoOriginal = true;
    [SerializeField] private float delayParaVoltar = 3f;

    private static readonly int IdBaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int IdColor     = Shader.PropertyToID("_Color");

    private MaterialPropertyBlock _mpb;
    private Color[] _coresOriginais;

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

        if (dialogueUI == null)
            dialogueUI = GetComponent<DialogueUI>();

        if (dialogueUI == null)
            Debug.LogError($"[NPCInteractable] '{name}': não achei o DialogueUI.", this);

        PrepararHighlight();
        SetHighlight(false);
    }

    void Update()
    {
        if (_playerNoRange)
        {
            if (playerHead == null || !seguirPlayer) return;
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

    void OnTriggerEnter(Collider other)
    {
        if (_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = true;
        _tempoDeSaida  = -1f;

        SetHighlight(true);
        dialogueUI?.Setup(dialogueTree);
    }

    void OnTriggerExit(Collider other)
    {
        if (!_playerNoRange || !other.CompareTag("Player")) return;

        _playerNoRange = false;
        _tempoDeSaida  = Time.time;

        SetHighlight(false);
        dialogueUI?.FecharTudo();
    }

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