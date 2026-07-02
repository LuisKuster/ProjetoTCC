using UnityEngine;

// Fica no objeto raiz da xícara (onde está o Rigidbody / XR Grab Interactable dela)
public class TeaCup : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool temFolhas = false;
    [SerializeField] private bool temAgua = false;

    public bool TemFolhas => temFolhas;
    public bool TemAgua => temAgua;
    public bool EstaPronta => temFolhas && temAgua;

    // Chamado pelo CupMouthZone quando a folha é solta dentro da zona do bocal
    public void ReceberFolha(GameObject folha)
    {
        if (temFolhas) return;

        temFolhas = true;
        Destroy(folha);

        AtualizarEstadoPronta();
    }

    // Chamado pela chaleira (PourTeaWater) quando o bico fica perto o suficiente por tempo suficiente
    public void AdicionarAgua()
    {
        if (temAgua) return;

        temAgua = true;
        AtualizarEstadoPronta();
    }

    private void AtualizarEstadoPronta()
    {
        if (EstaPronta)
        {
            print("Xicara pronta!");
            // aqui depois entra troca de material/mesh, habilitar próximo objetivo, etc.
        }
    }
}
