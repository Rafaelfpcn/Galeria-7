using UnityEngine;

// Etapa 56: itens que aparecem nos andares. Olhe para o item e aperte E para pegar.
public class Item : Interagivel
{
    public enum Tipo { KitMedico, Oleo, Sinalizador }

    public Tipo tipo;
    [Tooltip("Vida que o kit médico devolve.")]
    public int cura = 2;
    [Tooltip("Fração do óleo máximo que o frasco devolve.")]
    public float oleo = 0.5f;
    public AudioClip som;

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        if (jogador == null) return;
        // O aviso diz se dá para pegar agora.
        aviso = Serve(jogador) ? "E: pegar " + Nome() : Nome() + ": " + (tipo == Tipo.KitMedico ? "vida cheia" : "óleo cheio");
    }

    bool Serve(Jogador jogador)
    {
        if (tipo == Tipo.KitMedico) return jogador.vida.atual < jogador.vida.maxima;
        if (tipo == Tipo.Oleo) return jogador.lampiao.oleo < jogador.lampiao.oleoMaximo;
        return true;
    }

    string Nome()
    {
        if (tipo == Tipo.KitMedico) return "kit médico";
        if (tipo == Tipo.Oleo) return "frasco de óleo";
        return "sinalizador";
    }

    public override void Interagir()
    {
        Jogador jogador = Jogador.Instancia;
        if (!Serve(jogador)) return;
        if (tipo == Tipo.KitMedico) jogador.vida.atual = Mathf.Min(jogador.vida.maxima, jogador.vida.atual + cura);
        else if (tipo == Tipo.Oleo) jogador.lampiao.oleo = Mathf.Min(jogador.lampiao.oleoMaximo, jogador.lampiao.oleo + jogador.lampiao.oleoMaximo * oleo);
        else jogador.sinalizadores++;
        if (som != null) AudioSource.PlayClipAtPoint(som, transform.position);
        Destroy(gameObject);
    }
}
