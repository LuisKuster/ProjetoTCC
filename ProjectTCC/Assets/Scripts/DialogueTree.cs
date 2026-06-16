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
}