using UnityEngine;

// Etapa 62: uma fita cassete caída na mina. Ao pegar, a gravação aparece como legenda e a fita fica guardada no save.
public class Fita : Interagivel
{
    public int numero = 1;
    public AudioClip som;

    public override void Interagir()
    {
        Jogo.Instancia.AcharFita(numero);
        Roteiro.Gravacao gravacao = Roteiro.Fita(numero);
        Legenda.Mostrar(gravacao.titulo, gravacao.texto, Roteiro.Voz(numero));
        if (som != null) AudioSource.PlayClipAtPoint(som, transform.position);
        Destroy(gameObject);
    }
}
