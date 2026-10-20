using UnityEngine;
using TMPro;

public class Documento : MonoBehaviour
{
    [TextArea(5, 15)] public string conteudo;
    [SerializeField] GameObject canvas;
    [SerializeField] TMP_Text texto;
    [SerializeField] Vector3 offset = new Vector3(0, 0.25f, 0);

    Transform cam;
    bool aberto;

    void Start()
    {
        cam = Camera.main.transform;
        texto.text = conteudo;
        canvas.SetActive(false);
    }

    public void Abrir()
    {
        aberto = true;
        canvas.SetActive(true);
    }

    public void Fechar()
    {
        aberto = false;
        canvas.SetActive(false);
    }

    void LateUpdate()
    {
        if (!aberto) return;

        // fica sempre acima do pergaminho, independente de como ele tá girado na mão
        canvas.transform.position = transform.position + offset;

        // vira pro jogador
        Vector3 dir = canvas.transform.position - cam.position;
        canvas.transform.rotation = Quaternion.LookRotation(dir);
    }
}