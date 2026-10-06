using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla os dois canvas do NPC: o de perguntas (QuestionBoxes) e o de
/// resposta (DialogueBox).
///
/// Coloque este componente no NPC_Base — NÃO num dos canvas, porque ele
/// precisa continuar ativo enquanto liga e desliga os dois.
///
/// O progresso (perguntas já feitas e botões substituídos) é guardado POR
/// DialogueTree. Assim árvores diferentes com ids iguais ("no_1", "no_2"...)
/// não se misturam.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Raízes de UI")]
    [Tooltip("O canvas QuestionBoxes.")]
    [SerializeField] private GameObject questionBoxesRoot;
    [Tooltip("O canvas DialogueBox (o do scroll).")]
    [SerializeField] private GameObject dialogueBoxRoot;

    [Header("Botões de Pergunta")]
    [Tooltip("Arraste Button01..Button04 NA ORDEM — a hierarquia está fora de ordem.")]
    [SerializeField] private DialogueOptionButton[] botoes;

    [Header("Resposta")]
    [SerializeField] private ScrollRect scrollResposta;
    [SerializeField] private TMP_Text   respostaTMP;
    [SerializeField] private Text       respostaLegado;

    [Tooltip("Opcional: o Header do scroll, para mostrar o nome do NPC.")]
    [SerializeField] private TMP_Text headerTMP;
    [SerializeField] private Text     headerLegado;

    [Header("Cores")]
    [Tooltip("Cor de fundo do balão quando a pergunta já foi feita.")]
    [SerializeField] private Color corVisitada = new Color(0.45f, 0.45f, 0.45f);

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Eventos de mundo")]
    [Tooltip("Opcional. Deixe vazio se nenhum nó deste NPC dispara evento.")]
    [SerializeField] private DialogueEventBinder eventBinder;

    [Header("Config")]
    [Tooltip("Trava anti-duplo-disparo entre poke e raio.")]
    [SerializeField] private float cooldownClique = 0.25f;

    [Tooltip("Esconde o painel de resposta enquanto nenhuma pergunta foi feita.")]
    [SerializeField] private bool esconderRespostaVazia = true;

    // ---------------- Estado ----------------

    /// <summary>Progresso de uma árvore: o que já foi perguntado e qual nó está em cada botão.</summary>
    private class EstadoArvore
    {
        public readonly HashSet<string> visitados = new HashSet<string>();
        public int[] noDoBotao;
        public bool  montado;
    }

    /// <summary>Um estado por DialogueTree. Não vive no ScriptableObject.</summary>
    private readonly Dictionary<DialogueTree, EstadoArvore> _estados = new Dictionary<DialogueTree, EstadoArvore>();

    private DialogueTree _tree;

    // Apontam para o estado da árvore atual.
    /// <summary>Qual nó está em cada botão agora. -1 = botão vazio.</summary>
    private int[]           _noDoBotao;
    private HashSet<string> _visitados = new HashSet<string>();

    private readonly Dictionary<string, int> _indicePorId = new Dictionary<string, int>();

    private float _ultimoClique = -999f;
    private bool  _inicializado;

    void Awake() => Inicializar();

    void Inicializar()
    {
        if (_inicializado) return;
        _inicializado = true;

        _noDoBotao = new int[botoes.Length];
        for (int i = 0; i < _noDoBotao.Length; i++) _noDoBotao[i] = -1;

        for (int i = 0; i < botoes.Length; i++)
        {
            if (botoes[i] == null)
            {
                Debug.LogError($"[DialogueUI] '{name}': botão {i} está vazio no Inspector.", this);
                continue;
            }

            botoes[i].Init(i);
            botoes[i].AoClicar += OnBotaoClicado;
        }

        if (questionBoxesRoot != null) questionBoxesRoot.SetActive(false);
        if (dialogueBoxRoot   != null) dialogueBoxRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (botoes == null) return;
        foreach (var b in botoes)
            if (b != null) b.AoClicar -= OnBotaoClicado;
    }

    // ---------------- Abertura / fechamento ----------------

    /// <summary>Chamado pelo NPCInteractable quando o player entra no raio.</summary>
    public void Setup(DialogueTree tree)
    {
        Inicializar();

        if (tree == null)
        {
            Debug.LogError($"[DialogueUI] '{name}' recebeu uma DialogueTree nula.", this);
            return;
        }

        bool arvoreNova = _tree != tree;
        _tree = tree;

        if (arvoreNova)
        {
            IndexarArvore();
            CarregarEstado(tree);

            // Tira o foco que ficou da árvore anterior.
            foreach (var b in botoes)
                if (b != null) b.LimparFoco();
        }

        if (questionBoxesRoot != null) questionBoxesRoot.SetActive(true);

        DefinirHeader(tree.npcName);
        DefinirResposta(string.Empty);

        if (dialogueBoxRoot != null)
            dialogueBoxRoot.SetActive(!esconderRespostaVazia);

        for (int i = 0; i < botoes.Length; i++) AplicarBotao(i);

        Falar(tree.audioEntrada);
    }

    public void FecharTudo()
    {
        if (audioSource != null) audioSource.Stop();

        foreach (var b in botoes)
            if (b != null) b.LimparFoco();

        if (questionBoxesRoot != null) questionBoxesRoot.SetActive(false);
        if (dialogueBoxRoot   != null) dialogueBoxRoot.SetActive(false);
    }

    public void FecharResposta()
    {
        if (dialogueBoxRoot != null) dialogueBoxRoot.SetActive(false);
    }

    // ---------------- Estado por árvore ----------------

    /// <summary>
    /// Pega (ou cria) o estado da árvore e faz _visitados/_noDoBotao apontarem
    /// para ele. Só monta os botões iniciais na primeira vez que a árvore abre;
    /// se o player voltar, cinzas e substituições continuam como estavam.
    /// </summary>
    void CarregarEstado(DialogueTree tree)
    {
        if (!_estados.TryGetValue(tree, out EstadoArvore estado))
        {
            estado = new EstadoArvore { noDoBotao = new int[botoes.Length] };
            _estados[tree] = estado;
        }

        _visitados = estado.visitados;
        _noDoBotao = estado.noDoBotao;

        if (!estado.montado)
        {
            MontarBotoesIniciais();
            estado.montado = true;
        }
    }

    // ---------------- Montagem ----------------

    void IndexarArvore()
    {
        _indicePorId.Clear();
        for (int i = 0; i < _tree.nodes.Count; i++)
        {
            string id = _tree.nodes[i].id;
            if (string.IsNullOrEmpty(id)) continue;
            if (!_indicePorId.ContainsKey(id)) _indicePorId[id] = i;
        }
    }

    void MontarBotoesIniciais()
    {
        int b = 0;

        for (int i = 0; i < _tree.nodes.Count && b < botoes.Length; i++)
        {
            if (!_tree.nodes[i].mostrarNoInicio) continue;
            _noDoBotao[b++] = i;
        }

        for (; b < botoes.Length; b++) _noDoBotao[b] = -1;
    }

    void AplicarBotao(int b)
    {
        if (botoes[b] == null) return;

        int no = _noDoBotao[b];
        bool ativo = no >= 0 && no < _tree.nodes.Count;

        botoes[b].DefinirVisivel(ativo);
        if (!ativo) return;

        botoes[b].DefinirTexto(_tree.nodes[no].perguntaTexto);
        botoes[b].DefinirVisitada(_visitados.Contains(_tree.nodes[no].id), corVisitada);
    }

    // ---------------- Clique ----------------

    void OnBotaoClicado(int b)
    {
        if (_tree == null) return;

        int no = _noDoBotao[b];
        if (no < 0 || no >= _tree.nodes.Count) return;

        if (Time.unscaledTime - _ultimoClique < cooldownClique) return;
        _ultimoClique = Time.unscaledTime;

        DialogueNode node = _tree.nodes[no];

        _visitados.Add(node.id);

        // Texto e voz saem juntos.
        if (dialogueBoxRoot != null) dialogueBoxRoot.SetActive(true);
        DefinirResposta(node.respostaTexto);
        Falar(node.respostaAudio);

        // Evento de mundo, se o nó tiver um.
        if (!string.IsNullOrEmpty(node.eventoId))
        {
            if (eventBinder != null) eventBinder.Disparar(node.eventoId);
            else Debug.LogWarning($"[DialogueUI] '{name}': o nó '{node.id}' pede o evento " +
                                  $"'{node.eventoId}', mas não há DialogueEventBinder ligado.", this);
        }

        // Substituição: o botão passa a mostrar outra pergunta.
        if (!string.IsNullOrEmpty(node.substituirPor) &&
            _indicePorId.TryGetValue(node.substituirPor, out int proximo))
        {
            _noDoBotao[b] = proximo;
        }

        AplicarBotao(b);
    }

    // ---------------- Texto ----------------

    void DefinirResposta(string texto)
    {
        if (respostaTMP    != null) respostaTMP.text    = texto;
        if (respostaLegado != null) respostaLegado.text = texto;

        if (scrollResposta != null && isActiveAndEnabled)
            StartCoroutine(VoltarScrollAoTopo());
    }

    /// <summary>
    /// O ContentSizeFitter só recalcula a altura no fim do frame. Mexer no
    /// scroll antes disso não tem efeito — daí o WaitForEndOfFrame.
    /// </summary>
    IEnumerator VoltarScrollAoTopo()
    {
        yield return new WaitForEndOfFrame();

        if (scrollResposta == null) yield break;

        Canvas.ForceUpdateCanvases();
        scrollResposta.verticalNormalizedPosition = 1f;
    }

    void DefinirHeader(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return;
        if (headerTMP    != null) headerTMP.text    = nome;
        if (headerLegado != null) headerLegado.text = nome;
    }

    // ---------------- Áudio ----------------

    /// <summary>
    /// Stop + clip + Play em vez de PlayOneShot: uma fala corta a anterior
    /// em vez de empilhar quando o jogador clica rápido em vários botões.
    /// </summary>
    void Falar(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    // ---------------- API ----------------

    /// <summary>Zera só a árvore que está aberta agora.</summary>
    public void ReiniciarProgresso()
    {
        if (_tree == null) return;

        _visitados.Clear();
        MontarBotoesIniciais();
        for (int i = 0; i < botoes.Length; i++) AplicarBotao(i);
    }

    /// <summary>Zera o progresso de todas as árvores (ex.: reiniciar a partida).</summary>
    public void ReiniciarTudo()
    {
        _estados.Clear();

        if (_tree == null) return;

        DialogueTree atual = _tree;
        CarregarEstado(atual);
        for (int i = 0; i < botoes.Length; i++) AplicarBotao(i);
    }
}