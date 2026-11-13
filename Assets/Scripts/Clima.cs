using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Etapas 74, 83, 144 e 147: fica junto do Volume de pós-processamento da cena. Aplica a opção de brilho, liga ou
// desliga as cores de PS1 e, na mina, muda o granulado, a vinheta e a cor da imagem conforme o trecho.
[RequireComponent(typeof(Volume))]
public class Clima : MonoBehaviour
{
    public static Clima Instancia;

    ColorAdjustments cores;
    Vignette vinheta;
    FilmGrain granulado;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // "profile" faz uma cópia só desta cena, então o arquivo do perfil não muda.
        VolumeProfile perfil = GetComponent<Volume>().profile;
        perfil.TryGet(out cores);
        perfil.TryGet(out vinheta);
        perfil.TryGet(out granulado);
    }

    void Update()
    {
        Jogo.Opcoes opcoes = Jogo.Instancia.dados.opcoes;
        if (cores != null) cores.postExposure.Override(opcoes.brilho * 1.5f);
        Shader.SetGlobalFloat("_G7Niveis", opcoes.coresDePS1 ? 32f : 0f); // o shader CoresDePS1 lê este número
    }

    // Etapa 147: chamado pelo gerador a cada andar.
    public void Vestir(Tema tema)
    {
        if (cores == null) Start();
        if (vinheta != null) vinheta.intensity.Override(tema.vinheta);
        if (granulado != null) granulado.intensity.Override(tema.granulado);
        if (cores != null)
        {
            cores.colorFilter.Override(tema.filtro);
            cores.saturation.Override(tema.saturacao);
        }
    }
}
