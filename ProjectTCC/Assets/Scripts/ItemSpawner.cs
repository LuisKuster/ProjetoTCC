using UnityEngine;

/// <summary>
/// Faz nascer um item de missão num ponto da cena.
/// Quem manda spawnar é o DialogueEventBinder, via UnityEvent → Spawnar().
///
/// Coloque num Empty no lugar onde o item deve aparecer (ou em qualquer lugar,
/// e use o campo 'Ponto De Spawn').
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("O que nasce")]
    [SerializeField] private GameObject prefab;

    [Tooltip("Deixe vazio para nascer na posição deste objeto.")]
    [SerializeField] private Transform pontoDeSpawn;

    [Header("Config")]
    [Tooltip("Se marcado, ignora chamadas depois da primeira.")]
    [SerializeField] private bool umaVezSo = true;

    private GameObject _instancia;
    private bool _jaSpawnou;

    public bool JaSpawnou => _jaSpawnou;

    /// <summary>Arraste este método no UnityEvent do DialogueEventBinder.</summary>
    public void Spawnar()
    {
        if (umaVezSo && _jaSpawnou) return;

        if (prefab == null)
        {
            Debug.LogError($"[ItemSpawner] '{name}': o campo Prefab está vazio.", this);
            return;
        }

        Transform p = pontoDeSpawn != null ? pontoDeSpawn : transform;
        _instancia = Instantiate(prefab, p.position, p.rotation);
        _jaSpawnou = true;
    }

    /// <summary>
    /// Devolve o item ao ponto de spawn. Útil se o player derrubar a folha
    /// num lugar impossível — chame de um botão, gatilho de chão, etc.
    /// </summary>
    public void Devolver()
    {
        if (_instancia == null) { _jaSpawnou = false; Spawnar(); return; }

        Transform p = pontoDeSpawn != null ? pontoDeSpawn : transform;

        var rb = _instancia.GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        _instancia.transform.SetPositionAndRotation(p.position, p.rotation);
    }

    void OnDrawGizmosSelected()
    {
        Transform p = pontoDeSpawn != null ? pontoDeSpawn : transform;
        Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.6f);
        Gizmos.DrawWireSphere(p.position, 0.08f);
    }
}
