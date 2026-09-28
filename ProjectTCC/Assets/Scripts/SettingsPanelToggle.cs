using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsPanelToggle : MonoBehaviour
{
    [SerializeField] GameObject panel;        // raiz do painel de config
    [SerializeField] Transform head;          // Main Camera do XR Origin
    [SerializeField] float distance = 1.2f;
    [SerializeField] float heightOffset = -0.15f;

    [SerializeField] InputActionProperty toggleAction = new InputActionProperty(
        new InputAction("ToggleSettings", InputActionType.Button,
            "<XRController>{LeftHand}/menuButton"));

    void OnEnable()
    {
        toggleAction.action.Enable();
        toggleAction.action.performed += OnToggle;
    }

    void OnDisable()
    {
        toggleAction.action.performed -= OnToggle;
    }

    void OnToggle(InputAction.CallbackContext _)
    {
        if (panel.activeSelf) { panel.SetActive(false); return; }

        // frente do player só no plano horizontal (ignora olhar pra cima/baixo)
        Vector3 forward = head.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = head.up; // olhando reto pra baixo
        forward.Normalize();

        panel.transform.position = head.position + forward * distance + Vector3.up * heightOffset;
        panel.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        panel.SetActive(true);
    }

    public void Close() => panel.SetActive(false); // liga num botão "Fechar" no painel
}