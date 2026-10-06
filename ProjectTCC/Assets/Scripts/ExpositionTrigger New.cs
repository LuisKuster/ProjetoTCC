using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dispara uma exposição. Três modos:
///  - Manual: alguém chama Disparar() (UnityEvent de outro script, entrega de item, botão...)
///  - AoIniciar: roda sozinho quando a cena carrega (intro)
///  - AoEntrarNoColisor: roda quando o player entra no trigger (saída da fazenda)
/// </summary>
[DisallowMultipleComponent]
public class ExpositionTrigger : MonoBehaviour
{
    public enum Modo { Manual, AoIniciar, AoEntrarNoColisor }

    [SerializeField] private Modo modo = Modo.Manual;
    [SerializeField] private ExpositionEventSO exposicao;

    [Header("Transição (opcional)")]
    [SerializeField] private GameObject salaParaDesligar;
    [SerializeField] private GameObject salaParaLigar;
    [Tooltip("Funciona sozinho também: dá pra teleportar sem trocar de sala.")]
    [SerializeField] private Transform pontoDeSpawn;

    [Header("AoIniciar")]
    [Tooltip("Espera antes de disparar. Dá tempo do rig XR assentar antes do fade.")]
    [SerializeField] private float delayInicial = 0.5f;

    [Header("AoEntrarNoColisor")]
    [SerializeField] private string tagDoPlayer = "Player";

    [Header("Geral")]
    [SerializeField] private bool umaVezSo = true;

    [Tooltip("Se outra exposição estiver rodando, espera ela acabar em vez de ignorar.")]
    [SerializeField] private bool esperarSeOcupado = true;

    [Header("Ao terminar")]
    [Tooltip("Chamado quando a exposição termina. Aqui PODE arrastar objetos da cena.")]
    [SerializeField] private UnityEvent aoTerminar;

    private bool _jaDisparou;
    private bool _naFila;

    void Start()
    {
        if (modo == Modo.AoIniciar)
            Invoke(nameof(Disparar), delayInicial);

        if (modo == Modo.AoEntrarNoColisor)
        {
            var col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
                Debug.LogWarning($"[ExpositionTrigger] '{name}': modo colisor precisa de um Collider com 'Is Trigger' marcado.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ENTROU: {other.name} | tag: {other.tag} | raiz: {other.transform.root.tag}");
        if (modo != Modo.AoEntrarNoColisor) return;
        if (!EhPlayer(other)) return;
        Disparar();
    }

    // O collider que entra pode ser um filho sem tag, então checa também o rigidbody e a raiz
    private bool EhPlayer(Collider c)
    {
        if (c.CompareTag(tagDoPlayer)) return true;
        if (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag(tagDoPlayer)) return true;
        return c.transform.root.CompareTag(tagDoPlayer);
    }

    public void Disparar()
    {
        if (umaVezSo && _jaDisparou) return;
        if (_naFila) return;

        if (exposicao == null)
        {
            Debug.LogWarning($"[ExpositionTrigger] '{name}': nenhuma exposição configurada.", this);
            return;
        }

        var manager = ExpositionManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning($"[ExpositionTrigger] '{name}': ExpositionManager não encontrado na cena.", this);
            return;
        }

        if (manager.IsShowing)
        {
            if (esperarSeOcupado) StartCoroutine(EsperarEDisparar());
            else Debug.LogWarning($"[ExpositionTrigger] '{name}': outra exposição rodando, ignorada.", this);
            return;
        }

        bool rodou = manager.TriggerExposition(exposicao, salaParaDesligar, salaParaLigar, pontoDeSpawn,
                                               () => aoTerminar?.Invoke());
        if (rodou) _jaDisparou = true; // só "queima" se rodou de verdade
    }

    private IEnumerator EsperarEDisparar()
    {
        _naFila = true;
        while (ExpositionManager.Instance != null && ExpositionManager.Instance.IsShowing)
            yield return null;
        _naFila = false;
        Disparar();
    }

    /// <summary>Libera pra disparar de novo (caso precise reusar).</summary>
    public void Resetar() => _jaDisparou = false;
}