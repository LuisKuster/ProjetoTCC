using UnityEngine;

// Fica no objeto filho "ZonaBocal" da xícara (mesmo padrão do ZonaDerramar/Beaker do tutorial)
public class CupMouthZone : MonoBehaviour
{
    [SerializeField] private TeaCup teaCup; // referência à xícara pai

    [Header("Folha de cha")]
    // (usa OnTriggerEnter abaixo)

    [Header("Agua da chaleira")]
    [SerializeField] private string tagBicoChaleira = "BicoChaleira"; // tag no collider do bico

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeaLeaf") && !teaCup.TemFolhas)
        {
            print("Folha tocou a zona do bocal, consumindo.");
            teaCup.ReceberFolha(other.gameObject);
            return;
        }

        if (other.CompareTag(tagBicoChaleira) && !teaCup.TemAgua)
        {
            print("Bico encostou na zona, adicionando agua.");
            teaCup.AdicionarAgua();
        }
    }
}