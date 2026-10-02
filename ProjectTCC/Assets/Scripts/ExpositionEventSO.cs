using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Uma cutscene: 1 áudio (narração) + slideshow de imagens sincronizado com ele.
/// Cada cutscene é um asset: Create > Narrative > Exposition Event.
/// O que acontece DEPOIS dela (abrir porta, próxima cutscene...) fica no ExpositionTrigger da cena.
/// </summary>
[CreateAssetMenu(fileName = "NovaCutscene", menuName = "Narrative/Exposition Event")]
public class ExpositionEventSO : ScriptableObject
{
    [Serializable]
    public class Slide
    {
        [FormerlySerializedAs("image")]
        public Sprite imagem;

        [Tooltip("Segundo do áudio em que esta imagem aparece.\n" +
                 "Deixe TODOS em 0 pra dividir o tempo igualmente entre as imagens.\n" +
                 "O primeiro slide sempre começa no 0.")]
        public float aparecerEm;

        [Tooltip("Opcional: legenda mostrada junto com esta imagem.")]
        [FormerlySerializedAs("text")]
        [TextArea(1, 4)]
        public string legenda;
    }

    [Header("Áudio")]
    [Tooltip("Narração desta cutscene. A cutscene dura o tamanho deste áudio.")]
    [FormerlySerializedAs("voiceOver")]
    public AudioClip audio;

    [Header("Slideshow")]
    public Slide[] slides;

    [Header("Tempo")]
    [Tooltip("Só usado se NÃO tiver áudio.")]
    public float duracaoSemAudio = 8f;

    [Tooltip("Pausa no final, depois do áudio, antes do fade out.")]
    public float extraHold = 0.5f;

    [Header("Fim de jogo")]
    [Tooltip("Tela fica preta no final, sem fade out. Só na última cutscene.")]
    public bool isEnding = false;

    // ---------------------------------------------------------------------

    public int SlideCount => slides != null ? slides.Length : 0;

    /// <summary>Quanto tempo a cutscene dura (sem contar fades e extraHold).</summary>
    public float DuracaoConteudo => audio != null ? audio.length : duracaoSemAudio;

    /// <summary>True se nenhum slide (fora o primeiro) tem tempo definido.</summary>
    public bool DivisaoAutomatica
    {
        get
        {
            for (int i = 1; i < SlideCount; i++)
                if (slides[i].aparecerEm > 0f) return false;
            return true;
        }
    }

    public float InicioDoSlide(int i)
    {
        if (i <= 0 || SlideCount == 0) return 0f;
        if (DivisaoAutomatica) return DuracaoConteudo * i / SlideCount;
        return slides[i].aparecerEm;
    }

    /// <summary>Qual slide deve estar na tela no tempo t.</summary>
    public int SlideNoTempo(float t)
    {
        for (int i = SlideCount - 1; i > 0; i--)
            if (t >= InicioDoSlide(i)) return i;
        return 0;
    }

    /// <summary>Preenche os "Aparecer Em" dividindo igualmente. Depois é só ajustar à mão.</summary>
    [ContextMenu("Distribuir slides igualmente")]
    private void DistribuirIgualmente()
    {
        for (int i = 0; i < SlideCount; i++)
            slides[i].aparecerEm = DuracaoConteudo * i / SlideCount;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnValidate()
    {
        if (DivisaoAutomatica) return;

        for (int i = 1; i < SlideCount; i++)
        {
            float atual = slides[i].aparecerEm;
            float anterior = slides[i - 1].aparecerEm;

            if (atual <= anterior)
                Debug.LogWarning($"[{name}] Slide {i} aparece em {atual}s, mas tem que ser depois do slide {i - 1} ({anterior}s).", this);
            else if (atual >= DuracaoConteudo)
                Debug.LogWarning($"[{name}] Slide {i} aparece em {atual}s, mas a cutscene só tem {DuracaoConteudo:0.0}s.", this);
        }
    }
}