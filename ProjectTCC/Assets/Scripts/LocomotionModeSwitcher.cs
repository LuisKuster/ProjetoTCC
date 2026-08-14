using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class LocomotionModeSwitcher : MonoBehaviour
{
    public enum LocomotionMode { Default, Teleport }

    [Header("Modo Padrão (joystick / walking)")]
    [Tooltip("Componentes de locomoção normal: Dynamic/Continuous Move Provider, etc.")]
    [SerializeField] private List<Behaviour> defaultLocomotionComponents = new List<Behaviour>();

    [Tooltip("Controller Input Action Manager das mãos que ANDAM com o analógico. " +
             "Normalmente só o Left Controller. NÃO coloque a mão que faz o turn aqui.")]
    [SerializeField] private List<ControllerInputActionManager> smoothMotionControllers = new List<ControllerInputActionManager>();

    [Header("Modo Teleporte")]
    [Tooltip("Áreas de chão que podem receber teleporte (arraste os TeleportationArea das salas relevantes)")]
    [SerializeField] private List<TeleportationArea> teleportAreas = new List<TeleportationArea>();

    [Tooltip("Se marcado, procura sozinho todos os TeleportationArea da cena no Awake (inclusive em salas desativadas), " +
             "somando com os que você arrastou na lista acima.")]
    [SerializeField] private bool autoFindTeleportAreas = true;

    [Tooltip("Opcional: o componente Teleportation Provider. Pode deixar vazio.")]
    [SerializeField] private Behaviour teleportationProvider;

    [Header("Estado inicial")]
    [Tooltip("Só vale se ninguém mandar um modo antes. O MovementModeSelector sobrescreve isso no Start.")]
    [SerializeField] private LocomotionMode startingMode = LocomotionMode.Default;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs;

    public LocomotionMode CurrentMode { get; private set; }

    // Awake e não Start: assim o MovementModeSelector, que dispara o onSelected
    // no Start dele, sempre tem a última palavra sobre o modo inicial.
    private void Awake()
    {
        if (autoFindTeleportAreas)
        {
            var found = FindObjectsByType<TeleportationArea>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var area in found)
            {
                if (!teleportAreas.Contains(area))
                    teleportAreas.Add(area);
            }
        }

        CurrentMode = startingMode;
        ApplyMode(CurrentMode);
    }

    public void SwitchToTeleport() => SetMode(LocomotionMode.Teleport);

    public void SwitchToDefault() => SetMode(LocomotionMode.Default);

    public void SetMode(LocomotionMode mode)
    {
        if (CurrentMode == mode)
            return;

        CurrentMode = mode;
        ApplyMode(mode);
    }

    private void ApplyMode(LocomotionMode mode)
    {
        bool isDefault = mode == LocomotionMode.Default;

        foreach (var comp in defaultLocomotionComponents)
            if (comp != null) comp.enabled = isDefault;

        // Isto é o switch nativo do XRI: com smoothMotion ligado a action de Move
        // fica ativa e a de Teleport Mode fica desativada, e vice-versa.
        // Mexer aqui em vez de dar SetActive no Teleport Interactor, senão o
        // ControllerInputActionManager religa o interactor por conta própria.
        foreach (var controller in smoothMotionControllers)
            if (controller != null) controller.smoothMotionEnabled = isDefault;

        foreach (var area in teleportAreas)
            if (area != null) area.enabled = !isDefault;

        if (teleportationProvider != null)
            teleportationProvider.enabled = !isDefault;

        if (verboseLogs)
            Debug.Log($"[LocomotionModeSwitcher] Modo aplicado: {mode}", this);
    }
}