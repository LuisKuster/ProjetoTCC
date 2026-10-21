using System.Collections;
using UnityEngine;
using TMPro;

public class Documento : MonoBehaviour
{
    [TextArea(5, 15)] public string conteudo;
    [SerializeField] GameObject canvas;
    [SerializeField] TMP_Text texto;
    [SerializeField] Vector3 offset = new Vector3(0, 0.25f, 0);

    [Header("Áudio")]
    [SerializeField] AudioClip audioPrimeiraVez;
    [SerializeField] AudioSource audioSource;

    [Header("Documento Final (opcional)")]
    [Tooltip("Se preenchido, dispara essa exposição depois do áudio + tempo de leitura.")]
    [SerializeField] ExpositionTrigger exposicaoFinal;
    [SerializeField] float tempoExtraLeitura = 5f;

    Transform cam;
    bool aberto;
    bool jaLido;

    void Start()
    {
        cam = Camera.main.transform;
        texto.text = conteudo;
        canvas.SetActive(false);

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void Abrir()
    {
        aberto = true;
        canvas.SetActive(true);

        if (jaLido) return;
        jaLido = true;

        float duracaoAudio = 0f;
        if (audioPrimeiraVez != null && audioSource != null)
        {
            audioSource.PlayOneShot(audioPrimeiraVez);
            duracaoAudio = audioPrimeiraVez.length;
        }

        if (exposicaoFinal != null)
            StartCoroutine(DispararDepois(duracaoAudio + tempoExtraLeitura));
    }

    public void Fechar()
    {
        aberto = false;
        canvas.SetActive(false);
    }

    IEnumerator DispararDepois(float espera)
    {
        yield return new WaitForSeconds(espera);
        exposicaoFinal.Disparar();
    }

    void LateUpdate()
    {
        if (!aberto) return;

        canvas.transform.position = transform.position + offset;
        Vector3 dir = canvas.transform.position - cam.position;
        canvas.transform.rotation = Quaternion.LookRotation(dir);
    }
}