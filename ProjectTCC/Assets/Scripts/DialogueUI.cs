using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DialogueUI : MonoBehaviour
{
    [Header("Boxes de Pergunta")]
    [SerializeField] private GameObject[]      boxObjects;      // Box_01 a Box_04
    [SerializeField] private TextMeshProUGUI[] boxLabels;
    [SerializeField] private Image[]           boxBackgrounds;

    [Header("Box de Resposta")]
    [SerializeField] private GameObject        respostaPanel;
    [SerializeField] private TextMeshProUGUI   respostaLabel;

    [Header("Cores")]
    [SerializeField] private Color corNormal        = Color.white;
    [SerializeField] private Color corHover         = new Color(1f, 0.95f, 0.6f);
    [SerializeField] private Color corVisitada      = Color.gray;
    [SerializeField] private Color corVisitadaHover = new Color(0.75f, 0.75f, 0.75f);

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Eventos de mundo")]
    [Tooltip("Opcional. Deixe vazio se nenhum nó deste NPC dispara evento.")]
    [SerializeField] private DialogueEventBinder eventBinder;

    [Header("Config")]
    [Tooltip("Trava anti-duplo-disparo entre poke e ray.")]
    [SerializeField] private float cooldownClique = 0.25f;

    private DialogueTree _tree;
    private XRSimpleInteractable[] _interactables;

    /// <summary>Qual nó está em cada box agora. -1 = box vazia.</summary>
    private int[] _noDaBox;

    /// <summary>Progresso de runtime, por id de nó. Não vive no ScriptableObject.</summary>
    private readonly HashSet<string> _visitados = new HashSet<string>();

    private readonly Dictionary<string, int> _indicePorId = new Dictionary<string, int>();

    private int   _emHover = -1;
    private float _ultimoClique = -999f;
    private bool  _inicializado;
    private bool  _montado;

    void Awake() => Inicializar();

    void Inicializar()
    {
        if (_inicializado) return;
        _inicializado = true;

        _interactables = new XRSimpleInteractable[boxObjects.Length];
        _noDaBox = new int[boxObjects.Length];
        for (int i = 0; i < _noDaBox.Length; i++) _noDaBox[i] = -1;

        for (int i = 0; i < boxObjects.Length; i++)
        {
            if (boxObjects[i] == null) continue;

            var it = boxObjects[i].GetComponent<XRSimpleInteractable>();
            _interactables[i] = it;

            if (it == null)
            {
                Debug.LogError($"[DialogueUI] '{boxObjects[i].name}' não tem XRSimpleInteractable.", boxObjects[i]);
                continue;
            }

            int idx = i;
            it.selectEntered.AddListener(_ => OnBoxClicked(idx));
            it.hoverEntered.AddListener(_ => OnBoxHover(idx, true));
            it.hoverExited .AddListener(_ => OnBoxHover(idx, false));
        }
    }

    void OnDestroy()
    {
        if (_interactables == null) return;

        foreach (var it in _interactables)
        {
            if (it == null) continue;
            it.selectEntered.RemoveAllListeners();
            it.hoverEntered .RemoveAllListeners();
            it.hoverExited  .RemoveAllListeners();
        }
    }

    // ---------------- Abertura ----------------

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

        if (arvoreNova) IndexarArvore();

        // Só remonta na primeira vez ou se a árvore mudou. Assim as boxes que já
        // foram substituídas continuam substituídas quando o player volta.
        if (arvoreNova || !_montado)
        {
            MontarBoxesIniciais();
            _montado = true;
        }

        _emHover = -1;
        respostaPanel.SetActive(false);

        Falar(tree.audioEntrada);

        for (int i = 0; i < boxObjects.Length; i++) AplicarBox(i);
    }

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

    void MontarBoxesIniciais()
    {
        int box = 0;

        for (int i = 0; i < _tree.nodes.Count && box < boxObjects.Length; i++)
        {
            if (!_tree.nodes[i].mostrarNoInicio) continue;
            _noDaBox[box++] = i;
        }

        for (; box < boxObjects.Length; box++) _noDaBox[box] = -1;
    }

    /// <summary>Escreve na box o conteúdo do nó que está nela agora.</summary>
    void AplicarBox(int box)
    {
        if (boxObjects[box] == null) return;

        int no = _noDaBox[box];
        bool ativa = no >= 0 && no < _tree.nodes.Count;

        boxObjects[box].SetActive(ativa);
        if (!ativa) return;

        boxLabels[box].text = _tree.nodes[no].perguntaTexto;
        AtualizarCor(box);
    }

    // ---------------- Interação ----------------

    void OnBoxHover(int box, bool entrou)
    {
        if (_tree == null || _noDaBox[box] < 0) return;

        if (entrou) _emHover = box;
        else if (_emHover == box) _emHover = -1;

        AtualizarCor(box);
    }

    void OnBoxClicked(int box)
    {
        if (_tree == null) return;

        int no = _noDaBox[box];
        if (no < 0 || no >= _tree.nodes.Count) return;

        if (Time.unscaledTime - _ultimoClique < cooldownClique) return;
        _ultimoClique = Time.unscaledTime;

        DialogueNode node = _tree.nodes[no];

        _visitados.Add(node.id);

        // Texto e voz saem juntos.
        respostaLabel.text = node.respostaTexto;
        respostaPanel.SetActive(true);
        Falar(node.respostaAudio);

        // Evento de mundo, se o nó tiver um.
        if (!string.IsNullOrEmpty(node.eventoId))
        {
            if (eventBinder != null) eventBinder.Disparar(node.eventoId);
            else Debug.LogWarning($"[DialogueUI] '{name}': o nó '{node.id}' pede o evento " +
                                  $"'{node.eventoId}', mas não há DialogueEventBinder ligado.", this);
        }

        // Substituição: a box passa a mostrar outra pergunta.
        if (!string.IsNullOrEmpty(node.substituirPor) &&
            _indicePorId.TryGetValue(node.substituirPor, out int proximo))
        {
            _noDaBox[box] = proximo;
            AplicarBox(box);   // o nó novo entra com a cor padrão, se ainda não foi visto
            return;
        }

        AtualizarCor(box);
    }

    // ---------------- Áudio ----------------

    void Falar(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    // ---------------- Cores ----------------

    void AtualizarCor(int box)
    {
        if (boxBackgrounds == null || box >= boxBackgrounds.Length) return;
        if (boxBackgrounds[box] == null) return;

        int no = _noDaBox[box];
        if (no < 0) return;

        bool visitada = _visitados.Contains(_tree.nodes[no].id);
        bool hover    = _emHover == box;

        boxBackgrounds[box].color = visitada
            ? (hover ? corVisitadaHover : corVisitada)
            : (hover ? corHover         : corNormal);
    }

    // ---------------- API ----------------

    public void FecharResposta() => respostaPanel.SetActive(false);

    public void FecharTudo()
    {
        if (audioSource != null) audioSource.Stop();

        _emHover = -1;
        respostaPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    /// <summary>Volta a conversa ao estado inicial (boxes originais, nada visitado).</summary>
    public void ReiniciarProgresso()
    {
        _visitados.Clear();
        if (_tree == null) return;

        MontarBoxesIniciais();
        for (int i = 0; i < boxObjects.Length; i++) AplicarBox(i);
    }
}