using UnityEngine;

// Etapa 83: as opções, usadas no menu principal e na pausa. Ficam salvas junto com o progresso.
public static class MenuDeOpcoes
{
    // Desenha as opções a partir de (x, y) e devolve true quando o jogador aperta Voltar.
    public static bool Desenhar(float x, float y)
    {
        Jogo jogo = Jogo.Instancia;
        Jogo.Opcoes opcoes = jogo.dados.opcoes;

        opcoes.volume = Barra(x, ref y, "Volume dos efeitos", opcoes.volume, 0f, 1f);
        opcoes.volumeMusica = Barra(x, ref y, "Volume da música", opcoes.volumeMusica, 0f, 1f);
        opcoes.sensibilidade = Barra(x, ref y, "Sensibilidade do mouse", opcoes.sensibilidade, 0.02f, 0.3f);
        opcoes.brilho = Barra(x, ref y, "Brilho", opcoes.brilho, -1f, 1f);
        opcoes.inverterY = GUI.Toggle(new Rect(x, y, 400f, 26f), opcoes.inverterY, " Inverter o eixo Y do mouse", Estilo.caixa);
        opcoes.menosBalanco = GUI.Toggle(new Rect(x, y + 32f, 400f, 26f), opcoes.menosBalanco, " Menos balanço e tremida da câmera", Estilo.caixa);
        opcoes.coresDePS1 = GUI.Toggle(new Rect(x, y + 64f, 400f, 26f), opcoes.coresDePS1, " Cores de jogo antigo (pontilhado)", Estilo.caixa);
        AudioListener.volume = opcoes.volume; // vale na hora, sem esperar o Voltar

        if (!GUI.Button(new Rect(x, y + 108f, 360f, 44f), "Voltar", Estilo.botao)) return false;
        jogo.Salvar();
        return true;
    }

    static float Barra(float x, ref float y, string nome, float valor, float minimo, float maximo)
    {
        int porcento = Mathf.RoundToInt(Mathf.InverseLerp(minimo, maximo, valor) * 100f);
        GUI.Label(new Rect(x, y, 360f, 24f), nome + ": " + porcento + "%", Estilo.texto);
        valor = GUI.HorizontalSlider(new Rect(x, y + 28f, 360f, 20f), valor, minimo, maximo);
        y += 58f;
        return valor;
    }
}
