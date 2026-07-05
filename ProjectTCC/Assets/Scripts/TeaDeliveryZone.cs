using UnityEngine;


[RequireComponent(typeof(SphereCollider))]
public class TeaDeliveryZone : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private float zoneRadius = 0.3f;

    [Header("Exposição / Transição")]
    [SerializeField] private ExpositionEventSO expositionEvent;
    [SerializeField] private GameObject roomToDisable;
    [SerializeField] private GameObject roomToEnable;
    [SerializeField] private Transform spawnPoint;

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

        var cup = other.GetComponentInParent<TeaCup>();
        if (cup == null) return;

        var grab = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        if (!cup.EstaPronta) return;

        _entregue = true;
        Destroy(cup.gameObject);
        ExpositionManager.Instance.TriggerExposition(expositionEvent, roomToDisable, roomToEnable, spawnPoint);
    }
}