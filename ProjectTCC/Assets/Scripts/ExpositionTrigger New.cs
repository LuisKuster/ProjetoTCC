using UnityEngine;

/// <summary>
/// Dispara uma exposição. Três modos:
///  - Manual: alguém chama Disparar() (binder, entrega de item, botão...)
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
    [SerializeField] private Transform pontoDeSpawn;

    [Header("AoIniciar")]
    [Tooltip("Espera antes de disparar. Dá tempo do rig XR assentar antes do fade.")]
    [SerializeField] private float delayInicial = 0.5f;

    [Header("AoEntrarNoColisor")]
    [SerializeField] private string tagDoPlayer = "Player";

    [Header("Geral")]
    [SerializeField] private bool umaVezSo = true;

    private bool _jaDisparou;

    void Start()
    {
        if (modo == Modo.AoIniciar)
            Invoke(nameof(Disparar), delayInicial);
    }

    void OnTriggerEnter(Collider other)
    {
        if (modo != Modo.AoEntrarNoColisor) return;
        if (!other.CompareTag(tagDoPlayer)) return;
        Disparar();
    }

    public void Disparar()
    {
        if (umaVezSo && _jaDisparou) return;

        if (exposicao == null)
        {
            Debug.LogWarning($"[ExpositionTrigger] '{name}': nenhuma exposição configurada.", this);
            return;
        }

        if (ExpositionManager.Instance == null)
        {
            Debug.LogWarning($"[ExpositionTrigger] '{name}': ExpositionManager não encontrado na cena.", this);
            return;
        }

        _jaDisparou = true;
        ExpositionManager.Instance.TriggerExposition(exposicao, salaParaDesligar, salaParaLigar, pontoDeSpawn);
    }

    /// <summary>Libera pra disparar de novo (caso precise reusar).</summary>
    public void Resetar() => _jaDisparou = false;
}
