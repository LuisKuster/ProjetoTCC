using UnityEngine;

// Fica no objeto filho "ZonaBocal" da xícara (mesmo padrão do ZonaDerramar/Beaker do tutorial)
public class CupMouthZone : MonoBehaviour
{
    [SerializeField] private TeaCup teaCup; // referência à xícara pai

    [Header("Folha de cha")]
    // (usa OnTriggerEnter abaixo)

    [Header("Agua da chaleira")]
    [SerializeField] private Transform bicoChaleira; // AttachPoint/ponto do bico, arraste aqui
    [SerializeField] private float distanciaMaxima = 0.05f; // quão perto o bico precisa estar
    [SerializeField] private float tempoParaEncher = 1f; // segundos parado perto o suficiente

    private float tempoNaZona = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TeaLeaf") && !teaCup.TemFolhas)
        {
            print("Folha tocou a zona do bocal, consumindo.");
            teaCup.ReceberFolha(other.gameObject);
        }
    }

    private void Update()
    {
        if (teaCup.TemAgua || bicoChaleira == null) return;

        float distancia = Vector3.Distance(transform.position, bicoChaleira.position);

        if (distancia <= distanciaMaxima)
        {
            tempoNaZona += Time.deltaTime;

            if (tempoNaZona >= tempoParaEncher)
            {
                print("Bico proximo tempo suficiente, adicionando agua.");
                teaCup.AdicionarAgua();
            }
        }
        else
        {
            tempoNaZona = 0f;
        }
    }
}