using UnityEngine;

/// <summary>
/// Coloque este componente no objeto raiz de cada sala/área.
/// Quando a sala é ativada (SetActive(true) pelo ExpositionManager), a música troca.
///
/// Se preferir disparar por posição do player em vez de por ativação de sala,
/// marque "usarTrigger", coloque um Collider com IsTrigger na área e garanta
/// que o player tenha a tag definida em "tagDoPlayer".
/// </summary>
public class MusicZone : MonoBehaviour
{
    [Header("Música desta área")]
    [SerializeField] private AudioClip clip;

    [Tooltip("Se marcado e o clip estiver vazio, a música para ao entrar na área (silêncio proposital).")]
    [SerializeField] private bool silenciar = false;

    [Header("Modo de disparo")]
    [Tooltip("Desmarcado: troca quando o GameObject é ativado. Marcado: troca quando o player entra no trigger.")]
    [SerializeField] private bool usarTrigger = false;
    [SerializeField] private string tagDoPlayer = "Player";

    void OnEnable()
    {
        if (!usarTrigger)
            Aplicar();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!usarTrigger) return;
        if (!other.CompareTag(tagDoPlayer)) return;
        Aplicar();
    }

    private void Aplicar()
    {
        if (MusicManager.Instance == null) return;

        if (clip != null)
            MusicManager.Instance.TocarMusica(clip);
        else if (silenciar)
            MusicManager.Instance.PararMusica();
    }
}
