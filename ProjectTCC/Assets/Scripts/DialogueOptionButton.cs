using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Um botão de pergunta. Coloque no "Text Button" (o objeto que tem o Button).
/// A cor de fundo (o azul) é controlada pelo ColorBlock do Button.
/// Quando a pergunta já foi feita, troca a Normal/Selected Color pela cor de visitado;
/// Highlighted e Pressed continuam as originais, então o hover segue funcionando.
/// </summary>
public class DialogueOptionButton : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button button;

    [Tooltip("Se o label for TextMeshPro, preencha este. Se for Text legado, preencha o de baixo.")]
    [SerializeField] private TMP_Text labelTMP;
    [SerializeField] private Text     labelLegado;

    public event Action<int> AoClicar;

    private int         _indice = -1;
    private ColorBlock  _coresOriginais;
    private bool        _preparado;

    public void Init(int indice)
    {
        _indice = indice;
        if (_preparado) return;
        _preparado = true;

        if (button == null) button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"[DialogueOptionButton] '{name}' não tem Button.", this);
            return;
        }

        if (labelTMP == null && labelLegado == null)
        {
            labelTMP = GetComponentInChildren<TMP_Text>(true);
            if (labelTMP == null) labelLegado = GetComponentInChildren<Text>(true);
        }

        _coresOriginais = button.colors;

        if (button.transition != Selectable.Transition.ColorTint)
            Debug.LogWarning($"[DialogueOptionButton] '{name}': a Transition do Button não é Color Tint, " +
                             "a cor de visitado não vai aparecer.", this);

        button.onClick.AddListener(() => AoClicar?.Invoke(_indice));
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
        if (button == null) return;

        ColorBlock cores = _coresOriginais;
        if (visitada)
        {
            cores.normalColor   = corVisitada;
            cores.selectedColor = corVisitada; // depois do clique o botão fica "Selected"
        }
        button.colors = cores; // a Unity já reaplica a cor na hora
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