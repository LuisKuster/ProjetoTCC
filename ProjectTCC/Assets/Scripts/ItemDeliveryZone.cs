using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class ItemDeliveryZone : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private float zoneRadius = 0.3f;
    [SerializeField] private string requiredItemTag = "Documentos";

    [Header("Exposição / Transição")]
    [SerializeField] private ExpositionEventSO expositionEvent;
    [SerializeField] private GameObject roomToDisable;   // deixe vazio no final de jogo
    [SerializeField] private GameObject roomToEnable;    // deixe vazio no final de jogo
    [SerializeField] private Transform spawnPoint;       // deixe vazio no final de jogo

    private bool _entregue = false;

    void Start()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = zoneRadius;
    }

    void OnTriggerStay(Collider other)
    {
        if (_entregue) return;

        if (!other.CompareTag(requiredItemTag) && !other.transform.root.CompareTag(requiredItemTag))
            return;

        var grab = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null && grab.isSelected) return; // ainda na mão, ignora

        _entregue = true;
        Destroy(other.transform.root.gameObject);
        ExpositionManager.Instance.TriggerExposition(expositionEvent, roomToDisable, roomToEnable, spawnPoint);
    }
}