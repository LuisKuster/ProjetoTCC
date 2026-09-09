using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CestaCafe : MonoBehaviour
{
    [SerializeField] private List<XRSocketInteractor> slots = new();
    [SerializeField] private int totalNecessario = 3;

    [SerializeField] private UnityEvent<int> onCafeDepositado; // pra HUD/som
    [SerializeField] private UnityEvent onCestaCompleta;       // o trigger da história

    public int Contador { get; private set; }
    private bool completo;

    private void OnEnable()
    {
        foreach (var s in slots)
            if (s != null) s.selectEntered.AddListener(OnEncaixou);
    }

    private void OnDisable()
    {
        foreach (var s in slots)
            if (s != null) s.selectEntered.RemoveListener(OnEncaixou);
    }

    private void OnEncaixou(SelectEnterEventArgs args)
    {
        if (args.interactorObject is not XRSocketInteractor slot) return;
        if (args.interactableObject is not XRGrabInteractable grab) return;

        StartCoroutine(Cimentar(slot, grab));

        Contador++;
        onCafeDepositado.Invoke(Contador);

        if (!completo && Contador >= totalNecessario)
        {
            completo = true;
            onCestaCompleta.Invoke();
        }
    }

    private IEnumerator Cimentar(XRSocketInteractor slot, XRGrabInteractable grab)
    {
        yield return null; // deixa o socket terminar o alinhamento

        Transform obj = grab.transform;
        Vector3 pos = obj.position;
        Quaternion rot = obj.rotation;

        // tira do sistema: sem interactable, sem rigidbody, sem gravidade, sem volta
        Destroy(grab);
        if (obj.TryGetComponent(out Rigidbody rb)) Destroy(rb);

        yield return null; // espera a destruição efetivar

        Transform alvo = slot.attachTransform != null ? slot.attachTransform : slot.transform;
        obj.SetParent(alvo, true);
        obj.SetPositionAndRotation(pos, rot); // trava exatamente onde o socket deixou

        slot.socketActive = false; // slot ocupado, não aceita mais nada
    }
}