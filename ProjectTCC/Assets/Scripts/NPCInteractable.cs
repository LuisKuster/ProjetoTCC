using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class NPCInteractable : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private Transform  playerHead;   // Main Camera

    [Header("Highlight")]
    [SerializeField] private Renderer[] meshRenderers;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);
    [SerializeField] private Color defaultColor   = Color.white;

    [Header("Config")]
    [SerializeField] private float interactionRadius = 2f;

    private bool _dialogueActive = false;

    void Start()
    {
        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = interactionRadius;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        SetHighlight(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SetHighlight(true);
        FacePlayer();
        OpenDialogue();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SetHighlight(false);
        CloseDialogue();
    }

    void FacePlayer()
    {
        if (playerHead == null) return;

        Vector3 dir = playerHead.position - transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void OpenDialogue()
    {
        _dialogueActive = true;
        if (dialogueUI != null)
            dialogueUI.SetActive(true);
    }

    void CloseDialogue()
    {
        _dialogueActive = false;
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    void SetHighlight(bool active)
    {
        foreach (var r in meshRenderers)
        {
            if (r == null) continue;
            r.material.color = active ? highlightColor : defaultColor;
        }
    }
}