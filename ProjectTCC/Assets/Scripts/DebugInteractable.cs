using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DebugInteractable : MonoBehaviour
{
    private XRSimpleInteractable _interactable;

    void Start()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        _interactable.selectEntered.AddListener(_ => Debug.Log("SELECT"));
        _interactable.hoverEntered.AddListener(_ => Debug.Log("HOVER"));
    }
}