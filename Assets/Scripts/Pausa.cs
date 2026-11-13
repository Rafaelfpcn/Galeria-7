using UnityEngine;

// Etapas 49 e 83: Esc pausa o jogo, solta o mouse e mostra Continuar, Opções e Voltar ao menu. Fica no Jogador.
public class Pausa : MonoBehaviour
{
    // Uma tela aberta, como a da bancada: o Esc fecha ela em vez de pausar.
    public static System.Action fecharTela;
    // Pausar sozinho quando o mouse se solta do jogo, como acontece com o Esc no navegador.
    public static bool pausaAutomatica = true;

    bool pausado;
    bool nasOpcoes;
    bool estavaTravado;

    // Com a recarga de scripts desligada no editor, estes valores sobreviveriam de um Play para o outro.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Zerar()
    {
        fecharTela = null;
        pausaAutomatica = true;
        Controles.Pausado = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        bool travado = Cursor.lockState == CursorLockMode.Locked;
        // No navegador e no editor, o Esc solta o mouse antes de chegar ao jogo: mouse solto também pausa.
        bool soltou = pausaAutomatica && estavaTravado && !travado && !Controles.Pausado;
        estavaTravado = travado;

        if (Controles.Voltar())
        {
            if (fecharTela != null) fecharTela();
            else if (pausado) Continuar();
            else Pausar();
        }
        else if (soltou && !pausado)
        {
            Pausar();
        }
    }

    void Pausar()
    {
        pausado = true;
        nasOpcoes = false;
        Time.timeScale = 0f; // para inimigos, física e tudo que usa o tempo do jogo
        Controles.Pausado = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        pausado = false;
        Time.timeScale = 1f;
        Controles.Pausado = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        estavaTravado = false; // só pausa sozinho de novo depois que o mouse travar de verdade
    }

    public void VoltarAoMenu()
    {
        Jogo.IrPara("Menu");
    }

    void OnGUI()
    {
        if (!pausado) return;
        GUI.depth = -10; // por cima do HUD
        Estilo.Comecar();
        Estilo.Escurecer(0.75f);
        float x = Estilo.Largura / 2f - 180f;
        if (nasOpcoes)
        {
            if (MenuDeOpcoes.Desenhar(x, 150f)) nasOpcoes = false;
            return;
        }
        float y = Estilo.Altura / 2f - 60f;
        GUI.Label(new Rect(0f, y - 90f, Estilo.Largura, 60f), "Pausado", Estilo.titulo);
        if (GUI.Button(new Rect(x, y, 360f, 46f), "Continuar", Estilo.botao)) Continuar();
        if (GUI.Button(new Rect(x, y + 56f, 360f, 46f), "Opções", Estilo.botao)) nasOpcoes = true;
        if (GUI.Button(new Rect(x, y + 112f, 360f, 46f), "Voltar ao menu", Estilo.botao)) VoltarAoMenu();
    }
}
