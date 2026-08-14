using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controla a escolha do modo de locomoção (teleporte / joystick) usando os
/// Toggles do sample do XR Interaction Toolkit, sem precisar mexer nos scripts
/// do sample (XRPokeFollowAffordance, ToggleColorToggler).
///
/// Coloque este componente no objeto raiz do painel (MovementChoose).
/// </summary>
[DisallowMultipleComponent]
public class MovementModeSelector : MonoBehaviour
{
    [Serializable]
    public class ModeOption
    {
        [Tooltip("Só pra você se achar no Inspector. Ex.: Teleport, Joystick.")]
        public string label;

        [Tooltip("O Toggle do botão. No seu caso: ToggleContentTeleport / ToggleContentJoystick.")]
        public Toggle toggle;

        [Tooltip("A Image 'Outline' que fica dentro desse botão.")]
        public Graphic outline;

        [Tooltip("Objetos que ficam ativos só enquanto este modo está escolhido. " +
                 "Ex.: TeleportationArea, interactor de teleporte, etc.")]
        public GameObject[] objectsActiveWhileSelected;

        [Tooltip("Disparado quando este modo passa a ser o escolhido.")]
        public UnityEvent onSelected;
    }

    [Header("Opções")]
    [SerializeField]
    List<ModeOption> m_Options = new List<ModeOption>();

    [Header("Cores do Outline")]
    [SerializeField]
    [Tooltip("Cor do outline do modo escolhido.")]
    Color m_SelectedOutlineColor = new Color(32 / 255f, 150 / 255f, 243 / 255f, 1f);

    [SerializeField]
    [Tooltip("Cor do outline dos modos não escolhidos.")]
    Color m_UnselectedOutlineColor = new Color(1f, 1f, 1f, 0.25f);

    [Header("Estado inicial")]
    [SerializeField]
    [Tooltip("Índice da opção usada como padrão se nenhum Toggle estiver ligado.")]
    int m_DefaultOptionIndex;

    [SerializeField]
    [Tooltip("Se ligado, dispara o evento da opção inicial no Start (útil pra já aplicar o modo padrão).")]
    bool m_InvokeOnStart = true;

    /// <summary>Índice da opção escolhida no momento. -1 se nenhuma.</summary>
    public int selectedIndex => m_SelectedIndex;

    readonly List<UnityAction<bool>> m_Callbacks = new List<UnityAction<bool>>();
    int m_SelectedIndex = -1;
    bool m_Suppress;

    void OnEnable()
    {
        m_Callbacks.Clear();
        for (var i = 0; i < m_Options.Count; i++)
        {
            var index = i;
            UnityAction<bool> callback = isOn => OnToggleValueChanged(index, isOn);
            m_Callbacks.Add(callback);

            if (m_Options[i].toggle != null)
                m_Options[i].toggle.onValueChanged.AddListener(callback);
        }
    }

    void OnDisable()
    {
        for (var i = 0; i < m_Options.Count && i < m_Callbacks.Count; i++)
        {
            if (m_Options[i].toggle != null)
                m_Options[i].toggle.onValueChanged.RemoveListener(m_Callbacks[i]);
        }

        m_Callbacks.Clear();
    }

    void Start()
    {
        // Se algum toggle já veio ligado do Inspector, respeita ele. Senão usa o padrão.
        var index = m_Options.FindIndex(option => option.toggle != null && option.toggle.isOn);
        if (index < 0)
            index = Mathf.Clamp(m_DefaultOptionIndex, 0, m_Options.Count - 1);

        Apply(index, m_InvokeOnStart);
    }

    /// <summary>
    /// Escolhe um modo por código (ex.: carregando uma preferência salva).
    /// </summary>
    public void SelectMode(int index)
    {
        Apply(index, true);
    }

    void OnToggleValueChanged(int index, bool isOn)
    {
        if (m_Suppress || !isOn)
            return;

        Apply(index, true);
    }

    void Apply(int index, bool invokeEvents)
    {
        if (m_Options.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, m_Options.Count - 1);
        m_SelectedIndex = index;

        // m_Suppress evita que os toggles que a gente desliga aqui reentrem no callback.
        m_Suppress = true;

        for (var i = 0; i < m_Options.Count; i++)
        {
            var option = m_Options[i];
            var isSelected = i == index;

            if (option.toggle != null && option.toggle.isOn != isSelected)
                option.toggle.isOn = isSelected;

            if (option.outline != null)
                option.outline.color = isSelected ? m_SelectedOutlineColor : m_UnselectedOutlineColor;

            if (option.objectsActiveWhileSelected != null)
            {
                foreach (var go in option.objectsActiveWhileSelected)
                {
                    if (go != null)
                        go.SetActive(isSelected);
                }
            }
        }

        m_Suppress = false;

        if (invokeEvents)
            m_Options[index].onSelected?.Invoke();
    }
}
