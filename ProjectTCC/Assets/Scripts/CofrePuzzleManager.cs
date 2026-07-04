using System.Collections;
using UnityEngine;

/// <summary>
/// Gerencia o puzzle do cofre: precisa que os N botões sejam pressionados
/// (em qualquer ordem) para a porta abrir e o item ficar disponível.
/// Uma vez aberto, o cofre não fecha mais.
/// </summary>
public class CofrePuzzleManager : MonoBehaviour
{
    [Header("Porta do cofre")]
    [Tooltip("Transform que vai rotacionar (a Porta, ou o PortaPivot se o pivô não estiver na dobradiça)")]
    [SerializeField] private Transform porta;

    [Tooltip("Rotação local (euler) de quando a porta está totalmente aberta")]
    [SerializeField] private Vector3 anguloAberto = new Vector3(0f, 100f, 0f);

    [Tooltip("Duração da animação de abertura, em segundos")]
    [SerializeField] private float duracaoAbertura = 1.5f;

    [Header("Item dentro do cofre")]
    [Tooltip("O objeto que deve aparecer/ficar pegável só depois que a porta abrir")]
    [SerializeField] private GameObject item;

    [Header("Configuração dos botões")]
    [Tooltip("Quantos botões precisam ser pressionados para abrir")]
    [SerializeField] private int totalBotoes = 3;

    private bool[] botoesPressionados;
    private Vector3 anguloFechado;
    private bool cofreAberto = false;

    private void Awake()
    {
        botoesPressionados = new bool[totalBotoes];
        anguloFechado = porta.localEulerAngles;

        if (item != null)
            item.SetActive(false);
    }

    /// <summary>Chamado por cada botão (via BotaoCofre), passando seu número de 1 a totalBotoes.</summary>
    public void ButtonPressed(int numBotao)
    {
        if (cofreAberto) return;

        int index = numBotao - 1;
        if (index < 0 || index >= botoesPressionados.Length)
        {
            Debug.LogWarning("[CofrePuzzleManager] Número de botão inválido: " + numBotao);
            return;
        }

        botoesPressionados[index] = true;
        Debug.Log($"[CofrePuzzleManager] Botão {numBotao} pressionado.");

        VerificarTodosPressionados();
    }

    private void VerificarTodosPressionados()
    {
        foreach (bool pressionado in botoesPressionados)
        {
            if (!pressionado)
                return; // ainda falta algum botão
        }

        cofreAberto = true;
        StartCoroutine(AbrirPorta());
    }

    private IEnumerator AbrirPorta()
    {
        Debug.Log("[CofrePuzzleManager] Todos os botões pressionados, abrindo cofre!");

        Quaternion rotInicial = Quaternion.Euler(anguloFechado);
        Quaternion rotFinal = Quaternion.Euler(anguloAberto);
        float tempo = 0f;

        while (tempo < duracaoAbertura)
        {
            tempo += Time.deltaTime;
            porta.localRotation = Quaternion.Slerp(rotInicial, rotFinal, tempo / duracaoAbertura);
            yield return null;
        }

        porta.localRotation = rotFinal;

        if (item != null)
            item.SetActive(true);

        Debug.Log("[CofrePuzzleManager] Cofre aberto, item disponível.");
    }
}
