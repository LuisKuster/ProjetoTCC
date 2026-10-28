using System.Collections;
using UnityEngine;

/// <summary>
/// Toca um áudio X segundos depois que o objeto é ativado.
/// Coloca dentro do objeto do cenário 2: quando o cenário liga, o timer começa.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class TocarAudioComDelay : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    [Tooltip("Segundos de espera antes de tocar.")]
    [SerializeField] private float delay = 5f;

    [Tooltip("Se marcado, o delay só começa a contar depois que a cutscene acabar.")]
    [SerializeField] private bool esperarCutsceneAcabar = true;

    [SerializeField] private bool umaVezSo = true;

    private AudioSource _source;
    private bool _jaTocou;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = false;
    }

    void OnEnable()
    {
        if (umaVezSo && _jaTocou) return;
        StartCoroutine(Rotina());
    }

    private IEnumerator Rotina()
    {
        if (esperarCutsceneAcabar)
        {
            yield return null; // dá um frame pro manager atualizar o estado
            while (ExpositionManager.Instance != null && ExpositionManager.Instance.IsShowing)
                yield return null;
        }

        yield return new WaitForSeconds(delay);

        if (audioClip != null) _source.clip = audioClip;
        _source.Play();
        _jaTocou = true;
    }
}
