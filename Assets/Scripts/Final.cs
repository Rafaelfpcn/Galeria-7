using System.Collections.Generic;
using UnityEngine;

// Etapas 70 e 114: o final. Toca a última fita e mostra o desfecho, os créditos e volta ao menu.
// Quem achou todas as outras fitas escolhe entre dois desfechos: acender o pavio e subir, ou ficar e ouvir o canto.
// Os créditos ficam em Assets/Resources/Creditos.txt.
public class Final : MonoBehaviour
{
    const string Escolha = "O pavio está na sua mão.\n\nO canto chama pelo seu nome, com a voz do seu avô. Você ouviu os vinte e três. Sabe o que cada um deixou lá em cima, e sabe o que a coisa promete.\n\nE: acender o pavio e subir\nF: ficar e ouvir";
    const string Subir = "Você acende o pavio e sobe sem olhar para trás.\n\nA Galeria 7 está selada.";
    const string SubirSabendo = "Você acende o pavio e sobe sem olhar para trás.\n\nA Galeria 7 está selada. Lá em cima, você conta: os vinte e três de Ouro Fundo não desertaram.";
    const string Ficar = "Você apaga o lampião e encosta o ouvido na pedra.\n\nO canto tem a voz do seu avô, e a do Dito, e a de todos eles. Não tem cansaço. Não tem fome.\n\nLá em cima, ninguém acende o pavio. A companhia vai dizer que mais um desertou.\n\nOs vinte e quatro de Ouro Fundo.";

    readonly List<string> telas = new List<string>();
    int atual;
    float inicioDaTela;
    bool escolhendo; // a tela atual espera E ou F, sem limite de tempo
    string creditos;

    void Start()
    {
        Jogo jogo = Jogo.Instancia;
        int ultima = Roteiro.Fitas.Length;
        bool todas = true; // todas as fitas que se acham na mina; a última toca agora
        for (int n = 1; n < ultima; n++) todas &= jogo.TemFita(n);
        int achadas = jogo.dados.fitas.Count + (jogo.TemFita(ultima) ? 0 : 1);

        Roteiro.Gravacao gravacao = Roteiro.Fita(ultima);
        jogo.AcharFita(ultima);
        jogo.dados.zerou = true;
        jogo.Guardar(Jogador.Instancia != null ? Jogador.Instancia.minerio : 0); // quem vence sobe com o minério
        Controles.Pausado = true; // o jogador fica onde está

        TextAsset arquivo = Resources.Load<TextAsset>("Creditos");
        creditos = arquivo != null ? arquivo.text.Trim() : "Galeria 7\n\nObrigado por jogar.";
        telas.Add(gravacao.titulo + "\n\n" + gravacao.texto);
        if (todas) telas.Add(Escolha);
        else
        {
            telas.Add(Subir + "\n\nFitas encontradas: " + achadas + " de " + ultima + ". A mina ainda guarda vozes.");
            telas.Add(creditos);
        }
        inicioDaTela = Time.unscaledTime;
        Legenda.Falar(Roteiro.Voz(ultima));
    }

    void Update()
    {
        float naTela = Time.unscaledTime - inicioDaTela;
        if (naTela < 1f) return; // evita pular uma tela sem querer
        if (escolhendo)
        {
            bool ficar = Controles.Ficar();
            if (!ficar && !Controles.Avancar()) return;
            telas.Add(ficar ? Ficar : SubirSabendo);
            telas.Add(creditos);
        }
        else
        {
            float limite = Mathf.Max(10f, telas[atual].Length * 0.07f);
            if (atual == 0 && Legenda.Falando) limite = float.MaxValue; // a última fita toca até o fim, a não ser que você pule
            if (!Controles.Avancar() && naTela < limite) return;
        }
        if (atual == 0) Legenda.Falar(null);
        atual++;
        inicioDaTela = Time.unscaledTime;
        escolhendo = atual < telas.Count && telas[atual] == Escolha;
        if (atual >= telas.Count) Jogo.IrPara("Menu");
    }

    void OnGUI()
    {
        if (atual >= telas.Count) return;
        GUI.depth = -20; // por cima de tudo
        Estilo.Comecar();
        float claro = Mathf.Clamp01((Time.unscaledTime - inicioDaTela) / 1.5f);
        Estilo.Escurecer(atual == 0 ? claro : 1f); // a primeira tela escurece o jogo aos poucos
        GUI.color = new Color(1f, 1f, 1f, claro);
        GUI.Label(new Rect(Estilo.Largura / 2f - 380f, 110f, 760f, 470f), telas[atual], Estilo.paragrafoCentro);
        if (!escolhendo) GUI.Label(new Rect(0f, Estilo.Altura - 70f, Estilo.Largura, 24f), "E: continuar", Estilo.centro);
        GUI.color = Color.white;
    }
}
