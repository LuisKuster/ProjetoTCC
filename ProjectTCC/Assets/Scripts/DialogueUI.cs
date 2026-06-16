using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DialogueUI : MonoBehaviour
{
    [Header("Boxes de Pergunta")]
    [SerializeField] private GameObject[]      boxObjects;      // Box_01 a Box_04
    [SerializeField] private TextMeshProUGUI[] boxLabels;       // Label de cada box
    [SerializeField] private Image[]           boxBackgrounds;  // Image de cada box

    [Header("Box de Resposta")]
    [SerializeField] private GameObject        respostaPanel;   // painel acima
    [SerializeField] private TextMeshProUGUI   respostaLabel;   // texto da resposta

    [Header("Cores")]
    [SerializeField] private Color corNormal   = Color.white;
    [SerializeField] private Color corVisitada = Color.gray;

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;

    private DialogueTree _tree;
    private int _nodeCount;

    // Chamado pelo NPCInteractable ao abrir o diálogo
    public void Setup(DialogueTree tree)
    {
        _tree = tree;
        _nodeCount = Mathf.Min(tree.nodes.Count, boxObjects.Length);

        // Toca áudio de entrada
        if (tree.audioEntrada != null)
            audioSource.PlayOneShot(tree.audioEntrada);

        // Esconde resposta
        respostaPanel.SetActive(false);

        // Configura cada box
        for (int i = 0; i < boxObjects.Length; i++)
        {
            if (i < _nodeCount)
            {
                boxObjects[i].SetActive(true);
                boxLabels[i].text = tree.nodes[i].perguntaTexto;

                // Cor: visitada ou normal
                boxBackgrounds[i].color = tree.nodes[i].jaVisitado
                    ? corVisitada
                    : corNormal;

                // Listener de clique — captura o índice corretamente
                int idx = i;
                var interactable = boxObjects[i].GetComponent<XRSimpleInteractable>();
                if (interactable != null)
                {
                    interactable.selectEntered.RemoveAllListeners();
                    interactable.selectEntered.AddListener(_ => OnBoxClicked(idx));
                }
            }
            else
            {
                // Esconde boxes que não têm nó
                boxObjects[i].SetActive(false);
            }
        }
    }

    void OnBoxClicked(int index)
    {
        DialogueNode node = _tree.nodes[index];

        // Marca como visitada e atualiza cor
        node.jaVisitado = true;
        boxBackgrounds[index].color = corVisitada;

        // Toca áudio da resposta
        if (node.respostaAudio != null)
            audioSource.PlayOneShot(node.respostaAudio);

        // Mostra texto de resposta
        respostaLabel.text = node.respostaTexto;
        respostaPanel.SetActive(true);
    }

    public void FecharResposta()
    {
        respostaPanel.SetActive(false);
    }

    public void FecharTudo()
    {
        respostaPanel.SetActive(false);
        gameObject.SetActive(false);
    }
}