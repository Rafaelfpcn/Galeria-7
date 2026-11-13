using UnityEngine;

// Etapa 80: a cara da interface, num lugar só: fonte de máquina de escrever, cor âmbar e tamanhos.
// Todo OnGUI começa com Estilo.Comecar(), que também acerta a escala: a interface é desenhada como se
// a tela tivesse sempre 720 de altura, então fica do mesmo tamanho em qualquer resolução.
public static class Estilo
{
    public const float Altura = 720f;
    public static float Largura { get { return Screen.width * Altura / Screen.height; } }

    public static readonly Color Ambar = new Color(1f, 0.8f, 0.5f);
    public static readonly Color Apagado = new Color(1f, 0.8f, 0.5f, 0.55f);

    public static GUIStyle texto, pequeno, direita, centro, paragrafo, paragrafoCentro, titulo, enorme, botao, caixa;
    static Texture2D fundoDoBotao;

    public static void Comecar()
    {
        float escala = Screen.height / Altura;
        GUI.matrix = Matrix4x4.Scale(new Vector3(escala, escala, 1f));
        if (fundoDoBotao != null) return; // os estilos já estão montados

        // No Windows existe a Courier New. Onde ela não existir, como no navegador, fica a fonte padrão.
        Font fonte = Font.CreateDynamicFontFromOSFont(new[] { "Courier New", "Consolas", "Lucida Console" }, 32);
        if (fonte != null) fonte.hideFlags = HideFlags.HideAndDontSave;

        texto = Novo(fonte, 20, TextAnchor.UpperLeft, Ambar);
        pequeno = Novo(fonte, 15, TextAnchor.UpperLeft, Apagado);
        direita = Novo(fonte, 15, TextAnchor.LowerRight, Apagado);
        centro = Novo(fonte, 20, TextAnchor.MiddleCenter, Ambar);
        paragrafo = Novo(fonte, 19, TextAnchor.UpperLeft, Ambar);
        paragrafo.wordWrap = true;
        paragrafoCentro = Novo(fonte, 22, TextAnchor.MiddleCenter, Ambar);
        paragrafoCentro.wordWrap = true;
        titulo = Novo(fonte, 40, TextAnchor.MiddleCenter, Ambar);
        enorme = Novo(fonte, 68, TextAnchor.MiddleCenter, Ambar);

        fundoDoBotao = Cor(new Color(0f, 0f, 0f, 0.65f));
        botao = new GUIStyle(GUI.skin.button) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        if (fonte != null) botao.font = fonte;
        botao.normal.background = fundoDoBotao;
        botao.hover.background = Cor(new Color(0.35f, 0.22f, 0.05f, 0.9f));
        botao.active.background = Cor(new Color(0.6f, 0.4f, 0.1f, 0.95f));
        botao.normal.textColor = Ambar;
        botao.hover.textColor = Color.white;
        botao.active.textColor = Color.white;
        botao.margin = new RectOffset(4, 4, 4, 4);

        caixa = new GUIStyle(GUI.skin.toggle) { fontSize = 18 };
        if (fonte != null) caixa.font = fonte;
        caixa.normal.textColor = Ambar;
        caixa.onNormal.textColor = Ambar;
        caixa.hover.textColor = Color.white;
        caixa.onHover.textColor = Color.white;
    }

    static GUIStyle Novo(Font fonte, int tamanho, TextAnchor alinhamento, Color cor)
    {
        GUIStyle estilo = new GUIStyle(GUI.skin.label) { fontSize = tamanho, alignment = alinhamento };
        if (fonte != null) estilo.font = fonte;
        estilo.normal.textColor = cor;
        return estilo;
    }

    static Texture2D Cor(Color cor)
    {
        Texture2D textura = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
        textura.SetPixel(0, 0, cor);
        textura.Apply();
        return textura;
    }

    // Retângulo de uma cor só.
    public static void Fundo(Rect onde, Color cor)
    {
        Color antes = GUI.color;
        GUI.color = cor;
        GUI.DrawTexture(onde, Texture2D.whiteTexture);
        GUI.color = antes;
    }

    // Escurece a tela inteira.
    public static void Escurecer(float alfa)
    {
        Fundo(new Rect(0f, 0f, Largura, Altura), new Color(0f, 0f, 0f, alfa));
    }
}
