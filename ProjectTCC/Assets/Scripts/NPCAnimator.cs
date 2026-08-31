using UnityEngine;

/// <summary>
/// Único script que toca no Animator. Ele apenas LÊ o estado dos outros
/// componentes e traduz para o parâmetro do Animator — nem o diálogo nem a
/// patrulha chamam o Animator diretamente.
///
/// Coloque no NPC_Base.
/// </summary>
public class NPCAnimator : MonoBehaviour
{
    public enum Estado { Idle = 0, Andando = 1, Falando = 2 }

    [Header("Referências")]
    [Tooltip("O Animator fica no modelo (dentro de Model), não no NPC_Base.")]
    [SerializeField] private Animator animator;

    [Tooltip("O VoiceSource. É ele que define se o NPC está falando.")]
    [SerializeField] private AudioSource voz;

    [Tooltip("Opcional. Deixe vazio para NPCs que não andam.")]
    [SerializeField] private NPCPatrol patrulha;

    [Header("Parâmetro do Animator")]
    [Tooltip("Nome do parâmetro Int no Animator Controller.")]
    [SerializeField] private string nomeParametro = "Estado";

    [Header("Comportamento")]
    [Tooltip("Falar tem prioridade sobre andar. Como o NPC para quando o player " +
             "chega perto, isso raramente acontece — mas evita animação estranha.")]
    [SerializeField] private bool falarTemPrioridade = true;

    private int _idParametro;
    private Estado _estadoAtual = Estado.Idle;

    public Estado EstadoAtual => _estadoAtual;

    void Awake()
    {
        _idParametro = Animator.StringToHash(nomeParametro);

        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (voz      == null) voz      = GetComponentInChildren<AudioSource>(true);
        if (patrulha == null) patrulha = GetComponent<NPCPatrol>();

        if (animator == null)
            Debug.LogError($"[NPCAnimator] '{name}': não achei o Animator.", this);
    }

    void Start()
    {
        // Root motion desligado: quem move o NPC é o NPCPatrol, via transform.
        // Com root motion ligado, os dois brigam e o NPC desliza ou anda em dobro.
        if (animator != null) animator.applyRootMotion = false;

        Aplicar(Estado.Idle, true);
    }

    void Update()
    {
        Aplicar(Calcular(), false);
    }

    Estado Calcular()
    {
        bool falando = voz != null && voz.isPlaying;
        bool andando = patrulha != null && patrulha.EstaAndando;

        if (falarTemPrioridade && falando) return Estado.Falando;
        if (andando)                        return Estado.Andando;
        if (falando)                        return Estado.Falando;

        return Estado.Idle;
    }

    void Aplicar(Estado novo, bool forcar)
    {
        if (!forcar && novo == _estadoAtual) return;
        if (animator == null) return;

        _estadoAtual = novo;
        animator.SetInteger(_idParametro, (int)novo);
    }
}
