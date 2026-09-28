using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewExpositionEvent", menuName = "Narrative/Exposition Event")]
public class ExpositionEventSO : ScriptableObject
{
    [Serializable]
    public class Slide
    {
        public Sprite image;

        [Tooltip("Opcional: troca o texto neste slide. Vazio = mantém o texto da exposição.")]
        [TextArea(2, 5)]
        public string text;

        [Tooltip("Quanto tempo este slide fica na tela.")]
        public float duration = 4f;
    }

    [TextArea(3, 6)]
    public string text;

    [Tooltip("Opcional: narração/voz tocada durante a exposição inteira.")]
    public AudioClip voiceOver;

    [Header("Imagens")]
    [Tooltip("Se tiver slides, eles mandam e os campos de imagem única abaixo são ignorados.")]
    public Slide[] slides;

    [Tooltip("Imagem única. Usado só quando a lista de slides está vazia.")]
    public Sprite image;

    [Header("Tempo (só para imagem única)")]
    [Tooltip("Se 0 e tiver áudio, usa a duração do áudio.")]
    public float holdDuration = 4f;

    [Tooltip("Tempo extra no final, antes do fade out.")]
    public float extraHold = 0.5f;

    [Tooltip("Se marcado, a tela fica preta no final (sem fade out). Use para fim de jogo.")]
    public bool isEnding = false;

    [Tooltip("Disparado quando a exposição termina")]
    public UnityEvent onExpositionComplete;

    /// <summary>Soma das durações dos slides. 0 se não tiver slides.</summary>
    public float TotalSlideDuration
    {
        get
        {
            if (slides == null) return 0f;
            float total = 0f;
            foreach (var s in slides) total += s.duration;
            return total;
        }
    }
}