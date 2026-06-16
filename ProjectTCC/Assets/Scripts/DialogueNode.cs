using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Pergunta (texto da Box)")]
    public string perguntaTexto;

    [Header("Resposta")]
    [TextArea(2, 4)]
    public string respostaTexto;
    public AudioClip respostaAudio;   // fala do NPC

    [HideInInspector]
    public bool jaVisitado = false;   // controlado em runtime
}