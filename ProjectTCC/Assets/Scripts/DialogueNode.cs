using UnityEngine;

[System.Serializable]
public class DialogueNode
{
    [Header("Identificação")]
    [Tooltip("Preenchido automaticamente. Só mexa se quiser um nome mais legível.")]
    public string id;

    [Tooltip("Desmarque para o nó NÃO ocupar box no início — " +
             "ele só aparece quando outro nó o chamar em 'Substituir Por'.")]
    public bool mostrarNoInicio = true;

    [Header("Pergunta (texto da Box)")]
    public string perguntaTexto;

    [Header("Resposta")]
    [TextArea(2, 4)]
    public string respostaTexto;
    public AudioClip respostaAudio;

    [Header("Desdobramento (opcional)")]
    [Tooltip("Id do nó que toma o lugar desta box depois da resposta. " +
             "Deixe VAZIO para a box continuar a mesma (só fica cinza).")]
    public string substituirPor;

    [Header("Evento de mundo (opcional)")]
    [Tooltip("Id do evento disparado ao responder. Quem executa é o " +
             "DialogueEventBinder da cena. Deixe vazio se o nó é só conversa.")]
    public string eventoId;

    // 'jaVisitado' saiu daqui: era gravado no .asset e vazava entre partidas.
    // O progresso agora vive em runtime dentro do DialogueUI.
}