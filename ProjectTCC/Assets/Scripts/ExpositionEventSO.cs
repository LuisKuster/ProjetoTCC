using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewExpositionEvent", menuName = "Narrative/Exposition Event")]
public class ExpositionEventSO : ScriptableObject
{
    [TextArea(3, 6)]
    public string text;

    [Tooltip("Quanto tempo o texto fica visível antes de encerrar")]
    public float holdDuration = 4f;

    [Tooltip("Se marcado, a tela fica preta no final (sem fade out). Use para fim de jogo.")]
    public bool isEnding = false;

    [Tooltip("Disparado quando a exposição termina")]
    public UnityEvent onExpositionComplete;
}