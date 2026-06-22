using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class ConfigHUD : MonoBehaviour
{
    [Header("Volume")]
    [SerializeField] private float volumeStep = 0.1f;
    [SerializeField] private TextMeshPro volumeLabel;  // ← era TextMesh, agora TextMeshPro

    private float _volume = 1f;

    void Start()
    {
        GetComponent<Transform>()
            .Find("BotaoMais")
            .GetComponent<XRSimpleInteractable>()
            .selectEntered.AddListener(_ => AumentarVolume());

        GetComponent<Transform>()
            .Find("BotaoMenos")
            .GetComponent<XRSimpleInteractable>()
            .selectEntered.AddListener(_ => DiminuirVolume());

        AtualizarLabel();
    }

    public void AumentarVolume()
    {
        _volume = Mathf.Clamp(_volume + volumeStep, 0f, 1f);
        MusicManager.Instance?.SetVolume(_volume);
        AtualizarLabel();
    }

    public void DiminuirVolume()
    {
        _volume = Mathf.Clamp(_volume - volumeStep, 0f, 1f);
        MusicManager.Instance?.SetVolume(_volume);
        AtualizarLabel();
    }

    void AtualizarLabel()
    {
        if (volumeLabel != null)
            volumeLabel.text = $"Volume: {Mathf.RoundToInt(_volume * 100)}%";
    }
}