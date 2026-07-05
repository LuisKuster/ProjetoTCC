using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewExpositionEvent", menuName = "Narrative/Exposition Event")]
public class ExpositionEventSO : ScriptableObject
{
    [TextArea(3, 6)]
    public string text;

    [Tooltip("Quanto tempo o texto fica visível na tela, em segundos")]
    public float holdDuration = 4f;

    [Tooltip("Disparado quando a exposição termina")]
    public UnityEvent onExpositionComplete;
}