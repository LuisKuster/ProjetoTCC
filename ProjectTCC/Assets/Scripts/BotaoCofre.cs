using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Coloque este script em cada um dos 3 Push Buttons do puzzle do cofre.
/// Defina numBotao como 1, 2 ou 3 (um valor diferente por botão) e arraste
/// o gerenciador do cofre no campo Manager.
/// </summary>
public class BotaoCofre : MonoBehaviour
{
    [Tooltip("Número deste botão: 1, 2 ou 3 (cada botão do puzzle precisa de um valor diferente)")]
    public int numBotao;

    [Tooltip("Referência ao gerenciador do puzzle do cofre")]
    public CofrePuzzleManager manager;

    private void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(_ => Pressionar());
    }

    public void Pressionar()
    {
        if (manager != null)
            manager.ButtonPressed(numBotao);
        else
            Debug.LogWarning("[BotaoCofre] Manager não atribuído no botão " + numBotao);
    }
}
