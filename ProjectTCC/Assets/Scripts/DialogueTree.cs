using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueTree", menuName = "NPG/Dialogue Tree")]
public class DialogueTree : ScriptableObject
{
    [Header("Dados do Personagem")]
    public string npcName;
    public AudioClip audioEntrada;    // fala quando o jogador chega

    [Header("Nós de Diálogo")]
    public List<DialogueNode> nodes = new List<DialogueNode>();

#if UNITY_EDITOR
    /// <summary>
    /// Preenche ids vazios e avisa de erros de preenchimento direto no Console,
    /// enquanto você edita o asset. Só roda no editor.
    /// </summary>
    void OnValidate()
    {
        // 1. Auto-id para nós novos, sem reaproveitar número já usado.
        int maior = 0;
        foreach (var n in nodes)
        {
            if (string.IsNullOrEmpty(n.id)) continue;
            if (n.id.StartsWith("no_") && int.TryParse(n.id.Substring(3), out int num))
                maior = Mathf.Max(maior, num);
        }

        foreach (var n in nodes)
            if (string.IsNullOrEmpty(n.id))
                n.id = $"no_{++maior}";

        // 2. Ids duplicados quebram a substituição em silêncio.
        var vistos = new HashSet<string>();
        foreach (var n in nodes)
            if (!vistos.Add(n.id))
                Debug.LogWarning($"[{name}] id duplicado: '{n.id}'. A substituição vai pegar o primeiro.", this);

        // 3. Substituição apontando para um id que não existe.
        foreach (var n in nodes)
        {
            if (string.IsNullOrEmpty(n.substituirPor)) continue;
            if (vistos.Contains(n.substituirPor)) continue;
            Debug.LogWarning($"[{name}] o nó '{n.id}' aponta para '{n.substituirPor}', que não existe.", this);
        }

        // 4. Mais de 4 nós iniciais não cabem nas boxes.
        int iniciais = 0;
        foreach (var n in nodes) if (n.mostrarNoInicio) iniciais++;
        if (iniciais > 4)
            Debug.LogWarning($"[{name}] {iniciais} nós marcados como 'Mostrar No Inicio', " +
                             $"mas só há 4 boxes. Os excedentes vão ser ignorados.", this);
    }
#endif
}