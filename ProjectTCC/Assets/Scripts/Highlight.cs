using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private Color cor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float intensidade = 1.5f;
    [SerializeField] private bool pulsar = true;
    [SerializeField] private float velocidadePulso = 3f;

    private Material[] _materiais;
    private bool _ligado;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        CarregarMateriais();
        AplicarEmissao(Color.black);
    }

    void Update()
    {
        if (!_ligado || !pulsar) return;

        float t = (Mathf.Sin(Time.time * velocidadePulso) + 1f) * 0.5f;
        AplicarEmissao(cor * intensidade * Mathf.Lerp(0.2f, 1f, t));
    }

    public void Ligar()
    {
        _ligado = true;
        AplicarEmissao(cor * intensidade);
    }

    public void Desligar()
    {
        _ligado = false;
        AplicarEmissao(Color.black);
    }

    private void CarregarMateriais()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        var lista = new System.Collections.Generic.List<Material>();
        foreach (var r in renderers)
            lista.AddRange(r.materials); // instancia uma vez só

        _materiais = lista.ToArray();
        foreach (var m in _materiais)
            m.EnableKeyword("_EMISSION");
    }

    private void AplicarEmissao(Color c)
    {
        if (_materiais == null) CarregarMateriais();

        foreach (var m in _materiais)
            m.SetColor(EmissionColor, c);
    }
}