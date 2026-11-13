using UnityEngine;
using UnityEngine.InputSystem;

// Todas as teclas do jogo ficam aqui. Para trocar uma tecla, mude só este arquivo.
public static class Controles
{
    // Etapa 49: com a pausa ou uma tela abertas, o jogador não anda, não olha e não golpeia.
    public static bool Pausado;

    public static Vector2 Mover()
    {
        if (Pausado) return Vector2.zero;
        float x = (Segurando(Key.D) ? 1f : 0f) - (Segurando(Key.A) ? 1f : 0f);
        float y = (Segurando(Key.W) ? 1f : 0f) - (Segurando(Key.S) ? 1f : 0f);
        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    // Quanto o mouse andou neste frame, em pixels.
    public static Vector2 Olhar()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || Pausado) return Vector2.zero;
        return mouse.delta.ReadValue();
    }

    public static bool Correr() { return !Pausado && Segurando(Key.LeftShift); }

    // C, e não Ctrl: no navegador, Ctrl+W fecha a aba.
    public static bool Agachar() { return !Pausado && Segurando(Key.C); }

    public static bool Interagir() { return !Pausado && Apertou(Key.E); }

    public static bool LigarLampiao() { return !Pausado && Apertou(Key.F); }

    public static bool Atacar()
    {
        Mouse mouse = Mouse.current;
        return !Pausado && mouse != null && mouse.leftButton.isPressed;
    }

    // Etapa 27: segurar o botão direito aviva a chama do lampião.
    public static bool AvivarChama()
    {
        Mouse mouse = Mouse.current;
        return !Pausado && mouse != null && mouse.rightButton.isPressed;
    }

    // Etapa 56: Q joga um sinalizador aceso.
    public static bool AcenderSinalizador() { return !Pausado && Apertou(Key.Q); }

    // Estas duas funcionam mesmo pausado: Esc abre e fecha a pausa e as telas, e E, Enter ou Espaço passam as telas do final.
    public static bool Voltar() { return Apertou(Key.Escape); }

    public static bool Avancar() { return Apertou(Key.E) || Apertou(Key.Enter) || Apertou(Key.Space); }

    // Etapa 114: a segunda escolha do final. Também funciona pausado.
    public static bool Ficar() { return Apertou(Key.F); }

    static bool Segurando(Key tecla)
    {
        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado[tecla].isPressed;
    }

    static bool Apertou(Key tecla)
    {
        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado[tecla].wasPressedThisFrame;
    }
}
