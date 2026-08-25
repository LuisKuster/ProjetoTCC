using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Um botão de pergunta da UI nova (sample do XRI).
/// Coloque em cada "Text Button" (o objeto que tem o componente Button).
///
/// O hover NÃO é tratado aqui: o próprio Button já faz isso pelo
/// Highlighted Color das Transitions. O que este script controla é só a
/// cor de "já visitado", aplicada no Outline.
/// </summary>
public class DialogueOptionButton : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button button;

    [Tooltip("Se o label for TextMeshPro, preencha este. Se for Text legado, preencha o de baixo.")]
    [SerializeField] private TMP_Text labelTMP;
    [SerializeField] private Text     labelLegado;

    [Tooltip("O Outline dentro do Image. É ele que muda de cor quando a pergunta já foi feita.")]
    [SerializeField] private Image outline;

    public event Action<int> AoClicar;

    private int   _indice = -1;
    private Color _corOutlineOriginal;
    private bool  _preparado;

    public void Init(int indice)
    {
        _indice = indice;

        if (!_preparado)
        {
            _preparado = true;

            if (button == null) button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"[DialogueOptionButton] '{name}' não tem Button.", this);
                return;
            }

            if (labelTMP == null && labelLegado == null)
            {
                labelTMP    = GetComponentInChildren<TMP_Text>(true);
                if (labelTMP == null) labelLegado = GetComponentInChildren<Text>(true);
            }

            if (outline != null) _corOutlineOriginal = outline.color;

            button.onClick.AddListener(() => AoClicar?.Invoke(_indice));
        }
    }

    void OnDestroy()
    {
        if (button != null) button.onClick.RemoveAllListeners();
    }

    public void DefinirTexto(string texto)
    {
        if (labelTMP    != null) labelTMP.text    = texto;
        if (labelLegado != null) labelLegado.text = texto;
    }

    public void DefinirVisitada(bool visitada, Color corVisitada)
    {
        if (outline == null) return;
        outline.color = visitada ? corVisitada : _corOutlineOriginal;
    }

    public void DefinirVisivel(bool visivel)
    {
        gameObject.SetActive(visivel);
    }

    /// <summary>Tira o foco do botão para ele não ficar preso no estado "selecionado".</summary>
    public void LimparFoco()
    {
        if (button != null) button.OnDeselect(null);
    }
}