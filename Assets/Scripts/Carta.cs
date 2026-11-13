using UnityEngine;

// Etapa 110: um papel que se lê com E. O texto aparece como legenda, sem abrir tela nenhuma.
public class Carta : Interagivel
{
    public string titulo = "Carta";
    [TextArea(4, 12)] public string texto;

    public override void Interagir()
    {
        Legenda.Mostrar(titulo, texto);
    }
}
