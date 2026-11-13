using UnityEngine;

// Etapa 64: o toca-fitas da superfície. Lista as fitas encontradas, para ler e ouvir de novo.
public class TocaFitas : Tela
{
    int escolhida;
    Vector2 rolagem;

    protected override void Desenhar()
    {
        Jogo jogo = Jogo.Instancia;
        Roteiro.Gravacao[] fitas = Roteiro.Fitas;
        GUILayout.Label("Toca-fitas · " + jogo.dados.fitas.Count + " de " + fitas.Length + " fitas", Estilo.texto);
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();

        rolagem = GUILayout.BeginScrollView(rolagem, GUILayout.Width(190f), GUILayout.Height(330f));
        for (int numero = 1; numero <= fitas.Length; numero++)
        {
            if (!jogo.TemFita(numero)) GUILayout.Label("Fita " + numero + " · ?", Estilo.pequeno);
            else if (GUILayout.Button("Fita " + numero, Estilo.botao, GUILayout.Height(28f)))
            {
                escolhida = numero;
                Legenda.Falar(Roteiro.Voz(numero)); // etapa 112: toca a gravação; escolher outra fita troca a voz
            }
        }
        GUILayout.EndScrollView();

        GUILayout.BeginVertical();
        if (escolhida > 0)
        {
            GUILayout.Label(fitas[escolhida - 1].titulo, Estilo.pequeno);
            GUILayout.Label(fitas[escolhida - 1].texto, Estilo.paragrafo);
        }
        else
        {
            GUILayout.Label(jogo.dados.fitas.Count == 0 ? "Nenhuma fita ainda. Elas estão espalhadas pelos andares da mina." : "Escolha uma fita.", Estilo.paragrafo);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }
}
