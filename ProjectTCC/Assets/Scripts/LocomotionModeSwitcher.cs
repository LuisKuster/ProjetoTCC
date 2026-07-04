using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class LocomotionModeSwitcher : MonoBehaviour
{
    public enum LocomotionMode { Default, Teleport }

    [Header("Modo Padrão (joystick / walking)")]
    [Tooltip("Componentes de locomoção normal: Dynamic/Continuous Move Provider, Snap Turn, etc.")]
    [SerializeField] private List<Behaviour> defaultLocomotionComponents = new List<Behaviour>();

    [Header("Modo Teleporte")]
    [Tooltip("Áreas de chão que podem receber teleporte (arraste os TeleportationArea das salas relevantes)")]
    [SerializeField] private List<TeleportationArea> teleportAreas = new List<TeleportationArea>();

    [Tooltip("Opcional: o componente Teleportation Provider. Pode deixar vazio.")]
    [SerializeField] private Behaviour teleportationProvider;

    [Header("Estado inicial")]
    [SerializeField] private LocomotionMode startingMode = LocomotionMode.Default;

    public LocomotionMode CurrentMode { get; private set; }

    private void Start()
    {
        CurrentMode = startingMode;
        ApplyMode(CurrentMode);
    }

    public void SwitchToTeleport()
    {
        Debug.Log("SwitchToTeleport chamado. Modo atual: " + CurrentMode);
        if (CurrentMode == LocomotionMode.Teleport) return;
        CurrentMode = LocomotionMode.Teleport;
        ApplyMode(CurrentMode);
    }

    public void SwitchToDefault()
    {
        Debug.Log("SwitchToDefault chamado. Modo atual: " + CurrentMode);
        if (CurrentMode == LocomotionMode.Default) return;
        CurrentMode = LocomotionMode.Default;
        ApplyMode(CurrentMode);
    }

    private void ApplyMode(LocomotionMode mode)
    {
        bool isDefault = mode == LocomotionMode.Default;

        foreach (var comp in defaultLocomotionComponents)
            if (comp != null) comp.enabled = isDefault;

        foreach (var area in teleportAreas)
            if (area != null) area.enabled = !isDefault;

        if (teleportationProvider != null)
            teleportationProvider.enabled = !isDefault;

        Debug.Log("[LocomotionModeSwitcher] Modo aplicado: " + mode);
    }
}