using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Área final: player entra → liga o highlight → espera um pouco → dispara a exposição final.
/// Precisa de um Collider com "Is Trigger" no mesmo objeto.
/// O ExpositionTrigger referenciado deve estar no modo MANUAL.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AreaFinal : MonoBehaviour
{
    [SerializeField] private ExpositionTrigger exposicaoFinal;
    [SerializeField] private string tagDoPlayer = "Player";

    [Header("Highlight")]
    [Tooltip("Objetos que acendem quando o player entra (luz, partículas, mesh com material emissivo...).")]
    [SerializeField] private GameObject[] ligarAoEntrar;

    [Tooltip("Tempo brilhando antes da cutscene começar.")]
    [SerializeField] private float tempoDeBrilho = 2f;

    [Tooltip("Extra: som, animação, haptics... o que quiser ao entrar.")]
    [SerializeField] private UnityEvent aoEntrar;

    private bool _ativou;

    void Awake()
    {
        foreach (var go in ligarAoEntrar)
            if (go != null) go.SetActive(false);

        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"[AreaFinal] '{name}': marca 'Is Trigger' no Collider.", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_ativou || !EhPlayer(other)) return;
        _ativou = true;
        StartCoroutine(Sequencia());
    }

    private IEnumerator Sequencia()
    {
        foreach (var go in ligarAoEntrar)
            if (go != null) go.SetActive(true);

        aoEntrar?.Invoke();

        yield return new WaitForSeconds(tempoDeBrilho);

        if (exposicaoFinal != null) exposicaoFinal.Disparar();
        else Debug.LogWarning($"[AreaFinal] '{name}': nenhum ExpositionTrigger configurado.", this);
    }

    // Mesmo esquema do ExpositionTrigger: o collider pode ser um filho sem tag
    private bool EhPlayer(Collider c)
    {
        if (c.CompareTag(tagDoPlayer)) return true;
        if (c.attachedRigidbody != null && c.attachedRigidbody.CompareTag(tagDoPlayer)) return true;
        return c.transform.root.CompareTag(tagDoPlayer);
    }
}
