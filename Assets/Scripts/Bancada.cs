using UnityEngine;

// Etapas 45 e 57: a bancada da superfície, onde o minério guardado vira melhorias.
public class Bancada : Tela
{
    public AudioClip somDeCompra;

    protected override void Desenhar()
    {
        Jogo jogo = Jogo.Instancia;
        GUILayout.Label("Bancada · minério guardado: " + jogo.dados.minerioGuardado, Estilo.texto);
        GUILayout.Space(10f);
        foreach (Melhoria melhoria in jogo.melhorias)
        {
            int nivel = jogo.NivelDe(melhoria);
            GUILayout.BeginHorizontal();
            GUILayout.Label(melhoria.nome + " " + nivel + "/" + melhoria.NivelMaximo + " · " + melhoria.valor[nivel] + " " + melhoria.unidade, Estilo.texto);
            if (nivel < melhoria.NivelMaximo)
            {
                GUI.enabled = jogo.dados.minerioGuardado >= melhoria.custo[nivel];
                bool clicou = GUILayout.Button("Comprar por " + melhoria.custo[nivel], Estilo.botao, GUILayout.Width(210f), GUILayout.Height(34f));
                GUI.enabled = true;
                if (clicou && jogo.Comprar(melhoria) && somDeCompra != null) AudioSource.PlayClipAtPoint(somDeCompra, transform.position);
            }
            GUILayout.EndHorizontal();
        }
    }
}
