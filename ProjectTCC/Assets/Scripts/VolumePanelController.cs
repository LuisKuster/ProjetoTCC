using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o painel de volume: sincroniza o slider com o volume atual
/// do MusicManager ao abrir, e atualiza o texto de porcentagem em tempo real.
/// </summary>
public class VolumePanelController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text valueText; // troca pra "Text" (UnityEngine.UI) se não usar TextMeshPro

    void OnEnable()
    {
        if (volumeSlider == null || MusicManager.Instance == null)
            return;

        // Evita disparar OnValueChanged (e re-setar o volume) durante a sincronização inicial
        volumeSlider.SetValueWithoutNotify(MusicManager.Instance.CurrentVolume);
        AtualizarTexto(volumeSlider.value);

        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        MusicManager.Instance?.SetVolume(value);
        AtualizarTexto(value);
    }

    void AtualizarTexto(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
