using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Liga os 'eventoId' dos nós de diálogo a coisas da cena.
/// Existe porque o DialogueTree é um ScriptableObject e não pode segurar
/// referência a objeto de cena — então o nó guarda só uma string, e este
/// componente (que vive na cena) resolve o que ela significa.
///
/// Coloque num objeto da cena, ou no próprio NPC se os eventos forem só dele.
/// </summary>
public class DialogueEventBinder : MonoBehaviour
{
    [Serializable]
    public class Binding
    {
        [Tooltip("Mesma string do campo 'Evento Id' do nó.")]
        public string eventoId;

        [Tooltip("Se marcado, dispara só na primeira vez.")]
        public bool umaVezSo = true;

        public UnityEvent acao;
    }

    [SerializeField] private List<Binding> bindings = new List<Binding>();
    [SerializeField] private bool avisarSeNaoAchar = true;

    private readonly Dictionary<string, Binding> _mapa = new Dictionary<string, Binding>();
    private readonly HashSet<string> _disparados = new HashSet<string>();

    void Awake()
    {
        foreach (var b in bindings)
        {
            if (string.IsNullOrWhiteSpace(b.eventoId)) continue;

            if (_mapa.ContainsKey(b.eventoId))
            {
                Debug.LogWarning($"[DialogueEventBinder] '{name}': eventoId duplicado '{b.eventoId}'.", this);
                continue;
            }
            _mapa[b.eventoId] = b;
        }
    }

    public void Disparar(string eventoId)
    {
        if (string.IsNullOrWhiteSpace(eventoId)) return;

        if (!_mapa.TryGetValue(eventoId, out Binding b))
        {
            if (avisarSeNaoAchar)
                Debug.LogWarning($"[DialogueEventBinder] '{name}': nenhum binding para '{eventoId}'.", this);
            return;
        }

        if (b.umaVezSo && !_disparados.Add(eventoId)) return;
        _disparados.Add(eventoId);

        b.acao?.Invoke();
    }

    public bool JaAconteceu(string eventoId) => _disparados.Contains(eventoId);
}
