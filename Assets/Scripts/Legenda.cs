using UnityEngine;

// Etapas 62 e 112: texto que fica um tempo na parte de baixo da tela, como a gravação de uma fita, e a voz que toca
// junto quando a fita tem uma. O Hud é quem desenha.
public static class Legenda
{
    static string titulo;
    static string texto;
    static float fim;
    static AudioSource fonte;

    public static bool Visivel { get { return texto != null && Time.unscaledTime < fim; } }
    public static bool Falando { get { return fonte != null && fonte.isPlaying; } }

    // Fica na tela por um tempo proporcional ao tamanho do texto. Com voz, fica até a gravação acabar.
    public static void Mostrar(string novoTitulo, string novoTexto, AudioClip voz = null)
    {
        titulo = novoTitulo;
        texto = novoTexto;
        fim = Time.unscaledTime + (voz != null ? voz.length + 1.5f : Mathf.Clamp(novoTexto.Length * 0.07f, 6f, 30f));
        Falar(voz);
    }

    // Toca uma gravação sem posição no mundo: sai igual nos dois ouvidos. Sem gravação, cala a que estiver tocando.
    public static void Falar(AudioClip voz)
    {
        if (fonte == null)
        {
            if (voz == null) return;
            GameObject objeto = new GameObject("VozDaFita");
            Object.DontDestroyOnLoad(objeto);
            fonte = objeto.AddComponent<AudioSource>();
            fonte.spatialBlend = 0f;
            fonte.playOnAwake = false;
        }
        fonte.Stop();
        fonte.clip = voz;
        if (voz != null) fonte.Play();
    }

    // Com a recarga de scripts desligada no editor, a legenda sobreviveria de um Play para o outro.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Zerar()
    {
        texto = null;
        fonte = null;
    }

    public static void Desenhar()
    {
        if (!Visivel) return;
        const float largura = 760f;
        float x = Estilo.Largura / 2f - largura / 2f;
        float alturaDoTexto = Estilo.paragrafo.CalcHeight(new GUIContent(texto), largura - 32f);
        float y = Estilo.Altura - alturaDoTexto - 150f;
        Estilo.Fundo(new Rect(x, y, largura, alturaDoTexto + 50f), new Color(0f, 0f, 0f, 0.78f));
        GUI.Label(new Rect(x + 16f, y + 8f, largura - 32f, 24f), titulo, Estilo.pequeno);
        GUI.Label(new Rect(x + 16f, y + 34f, largura - 32f, alturaDoTexto), texto, Estilo.paragrafo);
    }
}
