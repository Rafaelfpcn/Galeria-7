using UnityEngine;

// Etapas 48 e 83: menu principal, com Novo jogo, Continuar, Opções e Sair. Fica na cena Menu.
public class Menu : MonoBehaviour
{
    bool nasOpcoes;
    bool confirmarNovo;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void NovoJogo()
    {
        Jogo.Instancia.NovoJogo();
        Jogo.IrPara("Superficie");
    }

    public void Continuar()
    {
        Jogo.IrPara("Superficie");
    }

    void OnGUI()
    {
        Estilo.Comecar();
        Jogo jogo = Jogo.Instancia;
        float x = Estilo.Largura / 2f - 180f;
        float y = Estilo.Altura * 0.42f;

        if (nasOpcoes)
        {
            Estilo.Escurecer(0.6f);
            if (MenuDeOpcoes.Desenhar(x, 150f)) nasOpcoes = false;
            return;
        }

        GUI.Label(new Rect(0f, Estilo.Altura * 0.14f, Estilo.Largura, 90f), "GALERIA 7", Estilo.enorme);
        GUI.Label(new Rect(0f, Estilo.Altura * 0.14f + 88f, Estilo.Largura, 28f), "Ouro Fundo, Minas Gerais, 1983", Estilo.centro);

        if (confirmarNovo)
        {
            GUI.Label(new Rect(0f, y, Estilo.Largura, 30f), "Começar de novo apaga o minério, as melhorias e as fitas salvos.", Estilo.centro);
            if (GUI.Button(new Rect(x, y + 50f, 360f, 48f), "Apagar e começar", Estilo.botao)) NovoJogo();
            if (GUI.Button(new Rect(x, y + 108f, 360f, 48f), "Voltar", Estilo.botao)) confirmarNovo = false;
            return;
        }

        // Etapa 151: se o save deu problema ao abrir o jogo, o menu conta o que aconteceu.
        if (!string.IsNullOrEmpty(jogo.avisoDoSave))
            GUI.Label(new Rect(Estilo.Largura / 2f - 420f, y - 62f, 840f, 52f), jogo.avisoDoSave, Estilo.centro);

        if (GUI.Button(new Rect(x, y, 360f, 48f), "Novo jogo", Estilo.botao))
        {
            // Etapa 47: nunca apaga o progresso sem perguntar.
            if (jogo.TemSave) confirmarNovo = true;
            else NovoJogo();
        }
        GUI.enabled = jogo.TemSave;
        if (GUI.Button(new Rect(x, y + 58f, 360f, 48f), "Continuar", Estilo.botao)) Continuar();
        GUI.enabled = true;
        if (GUI.Button(new Rect(x, y + 116f, 360f, 48f), "Opções", Estilo.botao)) nasOpcoes = true;
        if (GUI.Button(new Rect(x, y + 174f, 360f, 48f), "Sair", Estilo.botao)) Application.Quit();

        if (!jogo.TemSave) return;
        string progresso = "minério guardado " + jogo.dados.minerioGuardado + " · andar mais fundo " + jogo.dados.andarMaisFundo + " · fitas " + jogo.dados.fitas.Count + " de " + Roteiro.Fitas.Length;
        if (jogo.dados.zerou) progresso += " · galeria selada";
        GUI.Label(new Rect(0f, y + 240f, Estilo.Largura, 26f), progresso, Estilo.centro);
    }
}
