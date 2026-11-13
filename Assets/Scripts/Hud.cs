using UnityEngine;

// Etapa 80: o HUD. Discreto: barras finas no canto de baixo, minério no canto de cima,
// o aviso de interação no meio e a legenda das fitas embaixo.
public class Hud : MonoBehaviour
{
    public Jogador jogador;
    public Lampiao lampiao;
    public Interacao interacao;

    void OnGUI()
    {
        Estilo.Comecar();
        float largura = Estilo.Largura;
        float altura = Estilo.Altura;
        Vida vida = jogador.vida;

        // Etapa 21: a tela fica vermelha por um instante a cada dano, e de vez quando você morre.
        float vermelho = vida.Morto ? 0.75f : 0.45f * Mathf.Clamp01(1f - (Time.time - vida.ultimoDano) / 0.4f);
        if (vermelho > 0f) Estilo.Fundo(new Rect(0f, 0f, largura, altura), new Color(0.35f, 0f, 0f, vermelho));
        if (vida.Morto)
        {
            GUI.Label(new Rect(0f, altura * 0.38f, largura, 90f), "Você não voltou", Estilo.enorme);
            if (jogador.minerio > 0) GUI.Label(new Rect(0f, altura * 0.38f + 96f, largura, 28f), "O minério ficou lá embaixo.", Estilo.centro);
            return;
        }

        Estilo.Fundo(new Rect(largura / 2f - 1.5f, altura / 2f - 1.5f, 3f, 3f), Estilo.Apagado); // mira

        string mochila = jogador.minerio >= jogador.capacidade ? "  mochila cheia" : "";
        GUI.Label(new Rect(20f, 14f, 600f, 26f), "minério " + jogador.minerio + "/" + jogador.capacidade + mochila, Estilo.texto);
        if (jogador.sinalizadores > 0) GUI.Label(new Rect(20f, 42f, 400f, 22f), "sinalizadores " + jogador.sinalizadores + " [Q]", Estilo.pequeno);

        Barra(altura - 62f, (float)vida.atual / vida.maxima, new Color(0.8f, 0.2f, 0.15f), "vida");
        Barra(altura - 44f, lampiao.oleo / lampiao.oleoMaximo, Estilo.Ambar, "óleo");
        Barra(altura - 26f, jogador.folego / jogador.folegoMaximo, new Color(0.6f, 0.8f, 1f), "fôlego");

        if (interacao.alvo != null) GUI.Label(new Rect(0f, altura / 2f + 22f, largura, 28f), interacao.alvo.aviso, Estilo.centro);
        Legenda.Desenhar();
    }

    void Barra(float y, float cheio, Color cor, string nome)
    {
        Estilo.Fundo(new Rect(20f, y, 124f, 8f), new Color(0f, 0f, 0f, 0.55f));
        Estilo.Fundo(new Rect(22f, y + 2f, 120f * Mathf.Clamp01(cheio), 4f), cor);
        GUI.Label(new Rect(152f, y - 7f, 120f, 22f), nome, Estilo.pequeno);
    }
}
