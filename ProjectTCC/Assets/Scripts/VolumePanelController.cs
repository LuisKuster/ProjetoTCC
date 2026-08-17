using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla o painel de volume: sincroniza o slider com o volume atual
/// do MusicManager ao abrir e atualiza o texto de porcentagem em tempo real.
/// </summary>
public class VolumePanelController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text valueText;

    private const string PREF_KEY = "volume_musica";

    void OnEnable()
    {
        if (volumeSlider == null)
        {
            Debug.LogWarning("[VolumePanel] Slider não atribuído no inspector.", this);
            return;
        }

        // Garante a faixa correta mesmo que o prefab esteja com outros valores.
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.wholeNumbers = false;

        // Se o MusicManager ainda não existir (ordem de execução), cai no valor salvo.
        float volumeAtual = MusicManager.Instance != null
            ? MusicManager.Instance.CurrentVolume
            : PlayerPrefs.GetFloat(PREF_KEY, volumeSlider.value);

        // SetValueWithoutNotify evita re-setar o volume durante a sincronização inicial.
        volumeSlider.SetValueWithoutNotify(volumeAtual);
        AtualizarTexto(volumeAtual);

        // Remove antes de adicionar: protege contra listener duplicado se o
        // objeto for reativado sem passar pelo OnDisable (ex.: durante troca de sala).
        volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
        else
            PlayerPrefs.SetFloat(PREF_KEY, value); // não perde a escolha do jogador

        AtualizarTexto(value);
    }

    void AtualizarTexto(float value)
    {
        if (valueText != null)
            valueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}