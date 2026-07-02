using UnityEngine;

// Fica no objeto filho "ZonaBocal" da xícara (mesmo padrão do ZonaDerramar/Beaker do tutorial)
public class CupMouthZone : MonoBehaviour
{
    [SerializeField] private TeaCup teaCup; // referência à xícara pai

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeaLeaf") && !teaCup.TemFolhas)
        {
            print("Folha tocou a zona do bocal, consumindo.");
            teaCup.ReceberFolha(other.gameObject);
        }
    }
}