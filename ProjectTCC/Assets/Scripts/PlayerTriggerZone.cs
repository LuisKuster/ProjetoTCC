using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Zona que dispara um evento quando o player entra.
/// Serve pros tres casos: portal pra config, portal de saida da config, e area de inicio.
///
/// O pulo do gato eh o "armar": se o player for reposicionado JA DENTRO da zona
/// (que eh exatamente o que acontece quando ele volta da config pro portal de onde saiu),
/// a zona so volta a valer depois que ele sair dela. Sem isso vira loop infinito.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerTriggerZone : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneShot = false;
    [SerializeField] private UnityEvent onPlayerEnter;

    private Collider zoneCollider;
    private bool armed;
    private bool consumed;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        armed = false;
        StartCoroutine(ArmWhenPlayerIsOutside());
    }

    private IEnumerator ArmWhenPlayerIsOutside()
    {
        // espera o reposicionamento do player do frame acontecer
        yield return null;
        yield return new WaitForFixedUpdate();

        while (PlayerIsInside()) yield return null;
        armed = true;
    }

    private bool PlayerIsInside()
    {
        var head = ConfigRoomManager.Instance != null ? ConfigRoomManager.Instance.PlayerHead : null;
        if (head == null) return false;

        // so o XZ importa, a cabeca fica acima do collider de chao do portal
        Vector3 p = head.position;
        Bounds b = zoneCollider.bounds;
        return p.x >= b.min.x && p.x <= b.max.x && p.z >= b.min.z && p.z <= b.max.z;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!armed || consumed) return;
        if (!other.CompareTag(playerTag)) return;
        if (ConfigRoomManager.Instance != null && ConfigRoomManager.Instance.Busy) return;

        armed = false;
        if (oneShot) consumed = true;
        onPlayerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (consumed) return;
        if (!other.CompareTag(playerTag)) return;
        armed = true;
    }
}
