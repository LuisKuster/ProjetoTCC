using System;
using UnityEngine;

/// <summary>
/// Faz o NPC percorrer um caminho fixo, com pausa configurável em cada ponto.
/// Coloque no NPC_Base.
///
/// Não usa NavMesh de propósito: o caminho é fixo e conhecido, então
/// não há o que calcular em runtime.
/// </summary>
public class NPCPatrol : MonoBehaviour
{
    [Serializable]
    public class Ponto
    {
        [Tooltip("Objeto vazio na cena marcando o local. Deixe FORA do prefab do NPC.")]
        public Transform destino;

        [Tooltip("Segundos parado ao chegar. 0 = não para, segue direto.")]
        [Min(0f)] public float pausa = 2f;

        [Tooltip("Opcional: para onde olhar durante a pausa. Vazio = mantém a direção da chegada.")]
        public Transform olharPara;
    }

    [Header("Caminho")]
    [SerializeField] private Ponto[] pontos;

    [Tooltip("Marcado: ao chegar no último ponto, volta ao primeiro. " +
             "Desmarcado: faz o caminho de trás pra frente (vai e volta).")]
    [SerializeField] private bool circular = true;

    [Tooltip("Começa a andar sozinho. Desmarque para NPCs parados.")]
    [SerializeField] private bool andarAoIniciar = true;

    [Header("Velocidades")]
    [SerializeField] private float velocidade = 0.9f;
    [SerializeField] private float velocidadeGiro = 4f;

    [Tooltip("Distância para considerar que chegou no ponto.")]
    [SerializeField] private float distanciaChegada = 0.15f;

    /// <summary>True enquanto o NPC está de fato se deslocando (não pausado, não parado).</summary>
    public bool EstaAndando { get; private set; }

    public bool Pausado => _pausadoExterno;

    public event Action<int> AoChegarNoPonto;

    private int   _indice;
    private int   _direcao = 1;
    private float _fimDaPausa;
    private bool  _ativo;
    private bool  _pausadoExterno;

    void Start()
    {
        if (pontos == null || pontos.Length == 0)
        {
            enabled = false;   // NPC parado: nada a fazer, e economiza o Update
            return;
        }

        _ativo = andarAoIniciar;
    }

    void Update()
    {
        if (!_ativo || _pausadoExterno)
        {
            EstaAndando = false;
            return;
        }

        // Em pausa num ponto do caminho.
        if (Time.time < _fimDaPausa)
        {
            EstaAndando = false;
            OlharDuranteAPausa();
            return;
        }

        Transform destino = pontos[_indice].destino;
        if (destino == null)
        {
            Debug.LogWarning($"[NPCPatrol] '{name}': ponto {_indice} está sem destino.", this);
            Avancar();
            return;
        }

        Vector3 alvo = destino.position;
        alvo.y = transform.position.y;   // o NPC não sobe nem desce até o ponto

        GirarPara(alvo);
        transform.position = Vector3.MoveTowards(transform.position, alvo, velocidade * Time.deltaTime);
        EstaAndando = true;

        if (Vector3.Distance(transform.position, alvo) > distanciaChegada) return;

        AoChegarNoPonto?.Invoke(_indice);
        _fimDaPausa = Time.time + pontos[_indice].pausa;
        EstaAndando = false;
        Avancar();
    }

    // ---------------- API ----------------

    /// <summary>Para o percurso. Chamado quando o player chega perto.</summary>
    public void Pausar() => _pausadoExterno = true;

    /// <summary>Retoma de onde parou.</summary>
    public void Retomar() => _pausadoExterno = false;

    public void Iniciar() => _ativo = true;
    public void Parar()   { _ativo = false; EstaAndando = false; }

    // ---------------- Interno ----------------

    void Avancar()
    {
        if (pontos.Length == 1) return;

        if (circular)
        {
            _indice = (_indice + 1) % pontos.Length;
            return;
        }

        _indice += _direcao;

        if (_indice >= pontos.Length)      { _indice = pontos.Length - 2; _direcao = -1; }
        else if (_indice < 0)              { _indice = 1;                 _direcao =  1; }
    }

    void OlharDuranteAPausa()
    {
        // O índice já avançou, então o ponto onde estamos parados é o anterior.
        int atual = IndiceAnterior();
        Transform alvo = pontos[atual].olharPara;
        if (alvo == null) return;

        GirarPara(alvo.position);
    }

    int IndiceAnterior()
    {
        if (pontos.Length == 1) return 0;

        if (circular)
            return (_indice - 1 + pontos.Length) % pontos.Length;

        int anterior = _indice - _direcao;
        return Mathf.Clamp(anterior, 0, pontos.Length - 1);
    }

    void GirarPara(Vector3 alvo)
    {
        Vector3 dir = alvo - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            velocidadeGiro * Time.deltaTime);
    }

    // ---------------- Gizmos ----------------

    void OnDrawGizmos()
    {
        if (pontos == null || pontos.Length == 0) return;

        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.9f);

        for (int i = 0; i < pontos.Length; i++)
        {
            if (pontos[i].destino == null) continue;

            Vector3 p = pontos[i].destino.position;
            Gizmos.DrawWireSphere(p, 0.2f);

            int prox = i + 1;
            if (prox >= pontos.Length)
            {
                if (!circular) break;
                prox = 0;
            }

            if (pontos[prox].destino != null)
                Gizmos.DrawLine(p, pontos[prox].destino.position);
        }
    }
}
