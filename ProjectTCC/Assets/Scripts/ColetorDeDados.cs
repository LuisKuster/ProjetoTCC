using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Coleta anônima de dados da sessão: tempo em cada cenário + se chegou na cutscene final.
/// Manda tudo pra uma planilha do Google (Apps Script publicado como Web App).
///
/// Como funciona:
///  - A sessão começa quando o app abre (cenário 1 = onde o player nasce).
///  - Toda vez que o ExpositionManager teleporta o player pra um spawn point
///    cadastrado aqui, fecha o tempo do cenário anterior e abre o do novo.
///  - Quando começa uma cutscene com 'isEnding' marcado, marca chegou_final = SIM.
///  - Manda a linha atualizada a cada troca de cenário, no final e a cada 'intervaloPing'.
///    A planilha atualiza a mesma linha (mesmo session_id), não cria duplicata.
///  - Guarda uma cópia local em JSON no óculos, caso a internet falhe.
///
/// Coloque num GameObject que NUNCA é desativado (fora das RoomFloors).
/// </summary>
public class ColetorDeDados : MonoBehaviour
{
    [Serializable]
    public class Cenario
    {
        [Tooltip("Nome que aparece na coluna 'cenario_atual' da planilha.")]
        public string nome;

        [Tooltip("Spawn point pra onde o player é teleportado ao ENTRAR neste cenário.\n" +
                 "Deixe VAZIO no cenário 1 (o player já nasce nele).")]
        public Transform spawn;
    }

    [Header("Planilha")]
    [Tooltip("URL do Web App do Apps Script (termina em /exec).")]
    [SerializeField] private string urlWebApp = "";

    [Tooltip("De quanto em quanto tempo manda um ping com os tempos atualizados (segundos).")]
    [SerializeField] private float intervaloPing = 60f;

    [SerializeField] private int timeoutSegundos = 15;

    [Header("Cenários (NA ORDEM do jogo, exatamente 3)")]
    [SerializeField] private Cenario[] cenarios =
    {
        new Cenario { nome = "RoomFloor01" },
        new Cenario { nome = "RoomFloor02" },
        new Cenario { nome = "Outdoor" },
    };

    [Header("Debug")]
    [SerializeField] private bool logs = true;
    [Tooltip("Salva um JSON por sessão em persistentDataPath/sessoes (backup se a internet cair).")]
    [SerializeField] private bool salvarCopiaLocal = true;

    // ---------------- O que vai pra planilha ----------------

    [Serializable]
    private class Payload
    {
        public string session_id;
        public string headset;
        public string inicio;
        public string cenario_atual;
        public int    tempo_cenario1_s;
        public int    tempo_cenario2_s;
        public int    tempo_cenario3_s;
        public string chegou_final;
        public int    tempo_total_s;
        public string ultima_atualizacao;
        public string motivo;
    }

    // ---------------- Estado ----------------

    private string   _sessionId;
    private string   _headset;
    private DateTime _inicioLocal;
    private DateTime _inicioUtc;
    private DateTime _inicioCenarioUtc;
    private DateTime _fimUtc;
    private int      _cenarioAtual;
    private double[] _acumulado;
    private bool     _terminou;

    private bool   _enviando;
    private bool   _pendente;
    private string _ultimoJson;

    public string SessionId => _sessionId;

    // ---------------- Ciclo de vida ----------------

    void Awake()
    {
        _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        _headset   = HashCurto(SystemInfo.deviceUniqueIdentifier);
        _acumulado = new double[cenarios.Length];

        if (cenarios.Length != 3)
            Debug.LogWarning($"[ColetorDeDados] Esperava 3 cenários, tem {cenarios.Length}. " +
                             "A planilha só tem colunas pra 3.", this);
    }

    void OnEnable()
    {
        ExpositionManager.OnPlayerReposicionado += AoReposicionar;
        ExpositionManager.OnExposicaoIniciada   += AoIniciarExposicao;
    }

    void OnDisable()
    {
        ExpositionManager.OnPlayerReposicionado -= AoReposicionar;
        ExpositionManager.OnExposicaoIniciada   -= AoIniciarExposicao;
    }

    void Start()
    {
        _inicioLocal      = DateTime.Now;
        _inicioUtc        = DateTime.UtcNow;
        _inicioCenarioUtc = _inicioUtc;
        _cenarioAtual     = 0;

        Log($"Sessão {_sessionId} começou (headset {_headset}).");
        Enviar("inicio");
        StartCoroutine(RotinaPing());
    }

    /// <summary>
    /// Quando tira o óculos o app pausa. Tenta mandar e salva local
    /// (o local é garantido, a internet é melhor esforço).
    /// </summary>
    void OnApplicationPause(bool pausado)
    {
        if (pausado && !_terminou) Enviar("pausa");
    }

    // ---------------- Eventos do jogo ----------------

    private void AoReposicionar(Transform spawn)
    {
        if (_terminou || spawn == null) return;

        int indice = Array.FindIndex(cenarios, c => c.spawn == spawn);
        if (indice < 0 || indice == _cenarioAtual) return; // spawn não cadastrado ou mesmo cenário

        FecharCenarioAtual();
        _cenarioAtual = indice;

        Log($"Entrou no cenário {indice + 1} ({cenarios[indice].nome}).");
        Enviar("troca_cenario");
    }

    private void AoIniciarExposicao(ExpositionEventSO data)
    {
        if (_terminou || data == null || !data.isEnding) return;

        FecharCenarioAtual();
        _terminou = true;
        _fimUtc   = DateTime.UtcNow;

        Log("Chegou na cutscene final.");
        Enviar("final");
    }

    // ---------------- Tempo ----------------

    private void FecharCenarioAtual()
    {
        DateTime agora = DateTime.UtcNow;
        _acumulado[_cenarioAtual] += (agora - _inicioCenarioUtc).TotalSeconds;
        _inicioCenarioUtc = agora;
    }

    private int TempoDoCenario(int i)
    {
        if (i < 0 || i >= _acumulado.Length) return 0;

        double t = _acumulado[i];
        if (!_terminou && i == _cenarioAtual)
            t += (DateTime.UtcNow - _inicioCenarioUtc).TotalSeconds;

        return (int)Math.Round(t);
    }

    private int TempoTotal()
    {
        DateTime fim = _terminou ? _fimUtc : DateTime.UtcNow;
        return (int)Math.Round((fim - _inicioUtc).TotalSeconds);
    }

    private IEnumerator RotinaPing()
    {
        var espera = new WaitForSecondsRealtime(intervaloPing);
        while (!_terminou)
        {
            yield return espera;
            if (!_terminou) Enviar("ping");
        }
    }

    // ---------------- Envio ----------------

    private Payload MontarPayload(string motivo) => new Payload
    {
        session_id         = _sessionId,
        headset            = _headset,
        inicio             = _inicioLocal.ToString("yyyy-MM-dd HH:mm:ss"),
        cenario_atual      = cenarios[_cenarioAtual].nome,
        tempo_cenario1_s   = TempoDoCenario(0),
        tempo_cenario2_s   = TempoDoCenario(1),
        tempo_cenario3_s   = TempoDoCenario(2),
        chegou_final       = _terminou ? "SIM" : "NAO",
        tempo_total_s      = TempoTotal(),
        ultima_atualizacao = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        motivo             = motivo,
    };

    private void Enviar(string motivo)
    {
        string json = JsonUtility.ToJson(MontarPayload(motivo));

        if (salvarCopiaLocal) SalvarLocal(json);

        if (string.IsNullOrWhiteSpace(urlWebApp))
        {
            Log("URL do Web App vazia, só salvei local.");
            return;
        }

        _ultimoJson = json;

        // Um envio por vez: garante que a planilha recebe na ordem certa.
        // Se já tem um indo, o próximo sai logo depois com o dado mais novo.
        if (_enviando) { _pendente = true; return; }
        StartCoroutine(RotinaEnviar());
    }

    private IEnumerator RotinaEnviar()
    {
        _enviando = true;

        do
        {
            _pendente = false;
            string json = _ultimoJson;

            using (var req = new UnityWebRequest(urlWebApp, UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "text/plain;charset=utf-8");
                req.timeout = timeoutSegundos;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                    Log($"Enviado: {json}");
                else
                    Debug.LogWarning($"[ColetorDeDados] Falha ao enviar ({req.error}). " +
                                     "Vai de novo no próximo ping.", this);
            }
        }
        while (_pendente);

        _enviando = false;
    }

    private void SalvarLocal(string json)
    {
        try
        {
            string pasta = Path.Combine(Application.persistentDataPath, "sessoes");
            Directory.CreateDirectory(pasta);
            string arquivo = Path.Combine(pasta, $"{_inicioLocal:yyyyMMdd_HHmmss}_{_sessionId}.json");
            File.WriteAllText(arquivo, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ColetorDeDados] Não consegui salvar local: {e.Message}", this);
        }
    }

    // ---------------- Util ----------------

    /// <summary>Hash FNV-1a curto e estável (o GetHashCode de string pode mudar entre execuções).</summary>
    private static string HashCurto(string s)
    {
        if (string.IsNullOrEmpty(s)) return "??????";

        uint h = 2166136261;
        foreach (char c in s)
        {
            h ^= c;
            h *= 16777619;
        }
        return h.ToString("X8").Substring(0, 6);
    }

    private void Log(string msg)
    {
        if (logs) Debug.Log($"[ColetorDeDados] {msg}", this);
    }
}
