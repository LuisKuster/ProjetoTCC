using UnityEngine;

/// <summary>
/// Coloque na RAIZ de cada cenário (o mesmo objeto que o ExpositionTrigger liga/desliga).
/// Quando o cenário é ativado, pede pro MusicManager fazer crossfade pra música dele.
/// Não para a música no OnDisable: quem assume é o próximo cenário que ligar.
/// </summary>
[DisallowMultipleComponent]
public class MusicaDoCenario : MonoBehaviour
{
    [SerializeField] private AudioClip musica;

    private bool _pendente;

    void OnEnable()
    {
        // Se o cenário já começa ativo na cena, o OnEnable dele pode rodar
        // ANTES do Awake do MusicManager. Nesse caso tenta de novo no Start.
        if (MusicManager.Instance == null)
        {
            _pendente = true;
            return;
        }
        Tocar();
    }

    void Start()
    {
        if (_pendente) Tocar();
    }

    private void Tocar()
    {
        _pendente = false;
        if (musica == null)
        {
            Debug.LogWarning($"[MusicaDoCenario] '{name}' sem música configurada.", this);
            return;
        }
        MusicManager.Instance?.TocarMusica(musica);
    }
}
