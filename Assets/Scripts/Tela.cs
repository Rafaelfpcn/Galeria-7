using UnityEngine;

// Etapas 45 e 64: uma tela que abre com E e fecha com Esc, como a bancada e o toca-fitas.
// Enquanto ela está aberta, o jogador fica parado e o mouse fica solto.
public abstract class Tela : Interagivel
{
    protected bool aberta;

    public override void Interagir()
    {
        aberta = true;
        Controles.Pausado = true;
        Pausa.fecharTela = Fechar;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Fechar()
    {
        aberta = false;
        Controles.Pausado = false;
        Pausa.fecharTela = null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if (!aberta) return;
        Estilo.Comecar();
        Rect quadro = new Rect(Estilo.Largura / 2f - 380f, Estilo.Altura / 2f - 230f, 760f, 460f);
        Estilo.Fundo(quadro, new Color(0f, 0f, 0f, 0.88f));
        GUILayout.BeginArea(new Rect(quadro.x + 24f, quadro.y + 18f, quadro.width - 48f, quadro.height - 36f));
        Desenhar();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Fechar (Esc)", Estilo.botao, GUILayout.Height(40f))) Fechar();
        GUILayout.EndArea();
    }

    // O miolo da tela, desenhado com GUILayout.
    protected abstract void Desenhar();
}
