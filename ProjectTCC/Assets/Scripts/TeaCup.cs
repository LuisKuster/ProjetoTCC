using UnityEngine;

// Fica no objeto raiz da xícara (onde está o Rigidbody / XR Grab Interactable dela)
public class TeaCup : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private bool temFolhas = false;
    [SerializeField] private bool temAgua = false;

    [Header("Quando ficar pronta")]
    [Tooltip("Tag aplicada na xícara ao ficar pronta, para a ItemDeliveryZone aceitar ela. " +
             "Precisa existir no Tag Manager. Deixe vazio para não trocar.")]
    [SerializeField] private string tagQuandoPronta = "Documentos";

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
        if (!EstaPronta) return;

        // A troca de tag é o que libera a entrega: a ItemDeliveryZone da Benedita
        // só reage a quem tem a tag dela.
        if (!string.IsNullOrWhiteSpace(tagQuandoPronta))
            gameObject.tag = tagQuandoPronta;

        print($"Xicara pronta! Tag agora: {gameObject.tag}");
        // aqui depois entra troca de material/mesh, habilitar próximo objetivo, etc.
    }
}