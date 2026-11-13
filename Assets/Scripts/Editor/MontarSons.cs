using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// Etapas 29, 54 e 76 a 78: todos os sons do jogo, feitos por código e gravados como WAV em Assets/Audio.
// Cada som só é criado quando falta: para usar um som seu, ponha um WAV com o mesmo nome nessa pasta.
public static class MontarSons
{
    const int Taxa = 22050; // amostras por segundo: som de rádio antigo, que combina com o jogo
    static System.Random sorteio;

    internal static AudioClip Pegar(string nome)
    {
        string caminho = "Assets/Audio/" + nome + ".wav";
        AudioClip pronto = AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
        if (pronto != null) return pronto;

        sorteio = new System.Random(nome.Length * 131 + nome[0]);
        Gravar(Normalizar(Compor(nome)), caminho);
        AssetDatabase.ImportAsset(caminho);
        AudioImporter importador = (AudioImporter)AssetImporter.GetAtPath(caminho);
        AudioImporterSampleSettings ajustes = importador.defaultSampleSettings;
        ajustes.compressionFormat = AudioCompressionFormat.PCM; // sem compressão, os sons em laço não dão estalo na emenda
        importador.defaultSampleSettings = ajustes;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<AudioClip>(caminho);
    }

    // A receita de cada som: tons e chiados somados. Os tempos são em segundos e as frequências em hertz.
    static float[] Compor(string nome)
    {
        float[] o;
        switch (nome)
        {
            // O jogador
            case "passo_pedra":
                o = Novo(0.14f); Chiado(o, 0f, 0.12f, 900f, 1f, 0.03f); Tom(o, 0f, 0.1f, 95f, 60f, 0.7f, 0.03f); return o;
            case "passo_madeira":
                o = Novo(0.18f); Tom(o, 0f, 0.15f, 190f, 120f, 1f, 0.035f); Chiado(o, 0f, 0.08f, 2200f, 0.4f, 0.015f); return o;
            case "passo_agua":
                o = Novo(0.3f); Chiado(o, 0f, 0.28f, 3200f, 0.9f, 0.08f); Tom(o, 0.02f, 0.12f, 380f, 900f, 0.3f, 0.04f); return o;
            case "respiracao": // em laço: puxa o ar, solta o ar
                o = Novo(1.8f); Chiado(o, 0f, 0.7f, 1300f, 0.6f, 0f); Chiado(o, 0.85f, 0.85f, 800f, 1f, 0f); return o;
            case "picareta_ar":
                o = Novo(0.2f); Chiado(o, 0f, 0.2f, 1800f, 1f, 0f); return o;
            case "picareta_pedra":
                o = Novo(0.35f);
                Tom(o, 0f, 0.3f, 1850f, 1800f, 0.5f, 0.05f); Tom(o, 0f, 0.3f, 2740f, 2700f, 0.35f, 0.04f); Tom(o, 0f, 0.2f, 4100f, 4050f, 0.2f, 0.03f);
                Chiado(o, 0f, 0.05f, 6000f, 0.8f, 0.008f); Tom(o, 0f, 0.15f, 120f, 80f, 0.6f, 0.04f);
                return o;
            case "golpe_na_carne":
                o = Novo(0.25f); Tom(o, 0f, 0.2f, 90f, 45f, 1f, 0.06f); Chiado(o, 0f, 0.12f, 700f, 0.8f, 0.03f); return o;

            // Rastejante
            case "rastejante_passo":
                o = Novo(0.07f); Chiado(o, 0f, 0.06f, 5000f, 1f, 0.012f); return o;
            case "rastejante_grito":
                o = Novo(0.6f); Tom(o, 0f, 0.25f, 900f, 1500f, 0.7f, 0f, 1); Tom(o, 0.2f, 0.4f, 1500f, 700f, 0.7f, 0f, 1);
                Chiado(o, 0f, 0.6f, 4000f, 0.3f, 0f); Tremer(o, 32f, 0.5f); return o;
            case "rastejante_golpe":
                o = Novo(0.28f); Chiado(o, 0f, 0.25f, 1600f, 1f, 0.08f); Tom(o, 0f, 0.2f, 300f, 150f, 0.5f, 0.07f, 1); Tremer(o, 45f, 0.7f); return o;
            case "rastejante_ferido":
                o = Novo(0.3f); Tom(o, 0f, 0.28f, 1300f, 500f, 1f, 0.09f, 1); Chiado(o, 0f, 0.1f, 3000f, 0.4f, 0.03f); return o;

            // Ouvinte
            case "ouvinte_respiracao": // em laço: um chiado comprido, com um estalinho na garganta
                o = Novo(2.6f); Chiado(o, 0f, 1.1f, 1100f, 0.5f, 0f); Chiado(o, 1.3f, 1.3f, 700f, 1f, 0f); Tremer(o, 17f, 0.45f); return o;
            case "ouvinte_grito":
                o = Novo(0.9f); Tom(o, 0f, 0.9f, 220f, 950f, 0.8f, 0f, 1); Tom(o, 0f, 0.9f, 331f, 1420f, 0.4f, 0f);
                Chiado(o, 0.3f, 0.6f, 3500f, 0.4f, 0f); Tremer(o, 11f, 0.5f); return o;
            case "ouvinte_golpe":
                o = Novo(0.3f); Chiado(o, 0f, 0.3f, 2500f, 1f, 0.07f); Tom(o, 0f, 0.25f, 500f, 200f, 0.6f, 0.08f, 2); return o;

            // Mineiro Oco
            case "mineiro_passo":
                o = Novo(0.32f); Tom(o, 0f, 0.3f, 70f, 45f, 1f, 0.09f); Chiado(o, 0f, 0.15f, 450f, 0.7f, 0.04f); return o;
            case "mineiro_gemido":
                o = Novo(1.3f); Tom(o, 0f, 1.3f, 95f, 68f, 1f, 0f, 1); Tom(o, 0f, 1.3f, 142f, 101f, 0.4f, 0f); Tremer(o, 6f, 0.35f); return Abafar(o, 600f);
            case "mineiro_golpe":
                o = Novo(0.45f); Chiado(o, 0f, 0.3f, 1200f, 0.7f, 0f); Tom(o, 0.22f, 0.2f, 80f, 40f, 1f, 0.07f); return o;

            // A mina
            case "gota":
                o = Novo(0.5f); Tom(o, 0f, 0.12f, 1500f, 700f, 1f, 0.03f); Tom(o, 0.16f, 0.1f, 1300f, 650f, 0.25f, 0.03f); return o;
            case "madeira_rangendo":
                o = Novo(1.6f); Tom(o, 0f, 0.7f, 120f, 165f, 0.8f, 0f, 1); Tom(o, 0.75f, 0.8f, 170f, 105f, 0.7f, 0f, 1); Tremer(o, 23f, 0.8f); return Abafar(o, 1800f);
            case "vento": // em laço
                o = Novo(6.5f); Chiado(o, 0f, 6.5f, 420f, 1f, -1f); o = Laco(o, 0.5f); Tremer(o, 2f / 6f, 0.6f); return o;
            case "estalo": // a madeira avisando que o teto vai cair
                o = Novo(0.6f);
                for (int i = 0; i < 5; i++) Chiado(o, Sorteio(0f, 0.5f), 0.03f, 6000f, Sorteio(0.6f, 1f), 0.006f);
                Tom(o, 0.1f, 0.4f, 140f, 190f, 0.4f, 0f, 1); return o;
            case "queda":
                o = Novo(1.6f); Chiado(o, 0f, 1.6f, 260f, 1f, 0.5f);
                for (int i = 0; i < 6; i++) Tom(o, Sorteio(0f, 1f), 0.2f, 70f, 40f, 0.6f, 0.06f);
                return o;
            case "chiado_de_gas": // em laço
                o = Novo(2.4f); Chiado(o, 0f, 2.4f, 6000f, 1f, -1f); return Laco(o, 0.4f);
            case "sinalizador": // em laço
                o = Novo(1.9f); Chiado(o, 0f, 1.9f, 7500f, 1f, -1f); Tremer(o, 40f, 0.3f); return Laco(o, 0.4f);
            case "fogueira": // em laço: um chiado baixo e estalos soltos
                o = Novo(3.4f); Chiado(o, 0f, 3.4f, 1500f, 0.3f, -1f);
                for (int i = 0; i < 25; i++) Chiado(o, Sorteio(0f, 3.3f), 0.02f, 7000f, Sorteio(0.4f, 1f), 0.004f);
                return Laco(o, 0.4f);

            // Etapa 118: a descida de gaiola.
            case "gaiola": // em laço: o ronco do cabo, a gaiola batendo nas guias e o rangido do ferro
                o = Novo(4.4f); Chiado(o, 0f, 4.4f, 180f, 1f, -1f); Tom(o, 0f, 4.4f, 48f, 48f, 0.35f, -1f, 1);
                for (int i = 0; i < 14; i++) Chiado(o, Sorteio(0f, 4.3f), 0.04f, 3500f, Sorteio(0.3f, 0.8f), 0.01f);
                for (int i = 0; i < 3; i++) Tom(o, Sorteio(0f, 3.6f), 0.5f, 210f, 260f, 0.25f, 0f, 1);
                return Laco(o, 0.4f);
            case "campainha": // dois toques: descer
                o = Novo(1.3f);
                for (int i = 0; i < 2; i++) { Tom(o, i * 0.45f, 0.7f, 1900f, 1900f, 0.7f, 0.12f); Tom(o, i * 0.45f, 0.7f, 2850f, 2850f, 0.3f, 0.08f); }
                return o;

            // Itens, fitas e bancada
            case "fita": // o clique do gravador e o chiado da fita
                o = Novo(1.1f); Chiado(o, 0f, 0.03f, 5000f, 1f, 0.006f); Tom(o, 0.05f, 0.05f, 900f, 880f, 0.3f, 0.02f, 2); Chiado(o, 0.1f, 1f, 3500f, 0.5f, 0.35f); return o;
            case "item":
                o = Novo(0.2f); Tom(o, 0f, 0.18f, 660f, 990f, 0.7f, 0.06f); return o;
            case "compra":
                o = Novo(0.3f); Tom(o, 0f, 0.12f, 880f, 880f, 0.6f, 0.05f); Tom(o, 0.1f, 0.2f, 1320f, 1320f, 0.6f, 0.07f); return o;

            // Chefão
            case "coracao": // em laço: tum-tum
                o = Novo(1.2f); Tom(o, 0f, 0.25f, 62f, 40f, 1f, 0.07f); Tom(o, 0.3f, 0.25f, 52f, 36f, 0.7f, 0.07f); return o;
            case "rugido":
                o = Novo(1.6f); Tom(o, 0f, 1.6f, 70f, 42f, 1f, 0f, 1); Tom(o, 0f, 1.6f, 105f, 63f, 0.5f, 0f, 1); Chiado(o, 0f, 1.6f, 700f, 0.8f, 0f); Tremer(o, 19f, 0.6f); return o;
            case "tentaculo":
                o = Novo(0.5f); Chiado(o, 0f, 0.4f, 900f, 1f, 0.12f); Tom(o, 0f, 0.3f, 110f, 50f, 0.9f, 0.1f); return o;

            // Sustos
            case "sussurro": // sílabas de chiado, como alguém falando baixinho
                o = Novo(1.6f);
                for (int i = 0; i < 7; i++) Chiado(o, i * 0.21f, 0.18f, 4500f, Sorteio(0.5f, 1f), 0f);
                return o;
            case "passos_atras": // quatro passos chegando perto
                o = Novo(1.8f);
                for (int i = 0; i < 4; i++)
                {
                    Chiado(o, i * 0.42f, 0.12f, 900f, 0.4f + 0.2f * i, 0.03f);
                    Tom(o, i * 0.42f, 0.1f, 95f, 60f, 0.3f + 0.15f * i, 0.03f);
                }
                return o;

            // Música, toda em laço. As frequências fecham um número inteiro de voltas, para a emenda não estalar.
            case "musica_superficie": // quatro acordes lentos: lá menor, fá, lá menor, mi
                o = Novo(16f);
                float[][] acordes = { new[] { 110f, 130.8f, 164.8f }, new[] { 87.3f, 130.8f, 174.6f }, new[] { 110f, 130.8f, 164.8f }, new[] { 82.4f, 123.5f, 164.8f } };
                for (int i = 0; i < 4; i++)
                    foreach (float nota in acordes[i])
                    {
                        Tom(o, i * 4f, 4f, nota, nota, 0.5f, 0f);
                        Tom(o, i * 4f, 4f, nota * 2f, nota * 2f, 0.15f, 0f);
                    }
                return o;
            case "musica_mina": // um ronco grave que bate devagar e, bem ao fundo, o veio cantando
                o = Novo(12f);
                Tom(o, 0f, 12f, 55f, 55f, 1f, -1f); Tom(o, 0f, 12f, 55.75f, 55.75f, 0.8f, -1f); Tom(o, 0f, 12f, 82.5f, 82.5f, 0.5f, -1f);
                Tom(o, 1f, 5f, 1318f, 1340f, 0.07f, 0f); Tom(o, 7f, 4.5f, 1340f, 1310f, 0.05f, 0f);
                return o;
            case "musica_tensao": // batidas de coração sobre duas notas que brigam
                o = Novo(4f);
                Tom(o, 0f, 4f, 110f, 110f, 0.35f, -1f); Tom(o, 0f, 4f, 116.5f, 116.5f, 0.35f, -1f); Chiado(o, 0f, 4f, 900f, 0.25f, 0f);
                for (int i = 0; i < 4; i++)
                {
                    Tom(o, i, 0.2f, 60f, 38f, 1f, 0.07f);
                    Tom(o, i + 0.25f, 0.2f, 55f, 36f, 0.7f, 0.07f);
                }
                return o;

            default:
                Debug.LogWarning("MontarSons não tem receita para o som " + nome + ". Ficou em silêncio.");
                return Novo(0.1f);
        }
    }

    static float[] Novo(float segundos)
    {
        return new float[(int)(segundos * Taxa)];
    }

    static float Sorteio(float minimo, float maximo)
    {
        return minimo + (float)sorteio.NextDouble() * (maximo - minimo);
    }

    // O volume ao longo de um som. queda > 0: começa forte e cai, e "queda" é o tempo para cair a um terço.
    // queda = 0: sobe e desce suave. queda < 0: fica igual o tempo todo.
    static float Volume(float queda, int i, int total)
    {
        if (queda > 0f) return Mathf.Exp(-i / (queda * Taxa)) * Mathf.Min(1f, i / 40f);
        if (queda < 0f) return 1f;
        return Mathf.Sin(Mathf.PI * i / total);
    }

    // Soma um tom que vai de f0 a f1 hertz. forma: 0 é seno (macio), 1 é serra (áspero) e 2 é quadrada (oco).
    static void Tom(float[] onda, float inicio, float duracao, float f0, float f1, float volume, float queda, int forma = 0)
    {
        int primeira = (int)(inicio * Taxa);
        int total = (int)(duracao * Taxa);
        double fase = 0.0;
        for (int i = 0; i < total && primeira + i < onda.Length; i++)
        {
            fase += Mathf.Lerp(f0, f1, i / (float)total) / Taxa;
            float volta = (float)(fase - System.Math.Floor(fase)); // de 0 a 1 a cada volta da onda
            float valor = forma == 0 ? Mathf.Sin(volta * Mathf.PI * 2f) : forma == 1 ? volta * 2f - 1f : volta < 0.5f ? 1f : -1f;
            onda[primeira + i] += valor * volume * Volume(queda, i, total);
        }
    }

    // Soma um chiado. "corte" é até onde vão os agudos: baixo dá um ronco abafado, alto dá um chiado claro.
    static void Chiado(float[] onda, float inicio, float duracao, float corte, float volume, float queda)
    {
        int primeira = (int)(inicio * Taxa);
        int total = (int)(duracao * Taxa);
        float passo = 1f - Mathf.Exp(-2f * Mathf.PI * corte / Taxa);
        float ganho = 1f / Mathf.Sqrt(passo / (2f - passo)); // compensa o volume que o filtro tira
        float filtrado = 0f;
        for (int i = 0; i < total && primeira + i < onda.Length; i++)
        {
            filtrado += passo * ((float)sorteio.NextDouble() * 2f - 1f - filtrado);
            onda[primeira + i] += filtrado * ganho * volume * Volume(queda, i, total);
        }
    }

    // Faz o volume do som inteiro subir e descer "hertz" vezes por segundo.
    static void Tremer(float[] onda, float hertz, float quanto)
    {
        for (int i = 0; i < onda.Length; i++)
            onda[i] *= 1f - quanto * (0.5f + 0.5f * Mathf.Sin(i * hertz * Mathf.PI * 2f / Taxa));
    }

    // Tira os agudos do som inteiro.
    static float[] Abafar(float[] onda, float corte)
    {
        float passo = 1f - Mathf.Exp(-2f * Mathf.PI * corte / Taxa);
        float filtrado = 0f;
        for (int i = 0; i < onda.Length; i++)
        {
            filtrado += passo * (onda[i] - filtrado);
            onda[i] = filtrado;
        }
        return onda;
    }

    // Prepara um som para tocar em laço: mistura o fim com o começo, e o som fica "emenda" segundos mais curto.
    static float[] Laco(float[] onda, float emenda)
    {
        int n = (int)(emenda * Taxa);
        float[] laco = new float[onda.Length - n];
        System.Array.Copy(onda, laco, laco.Length);
        for (int i = 0; i < n; i++)
        {
            float entra = i / (float)n;
            laco[i] = onda[i] * entra + onda[laco.Length + i] * (1f - entra);
        }
        return laco;
    }

    static float[] Normalizar(float[] onda)
    {
        float pico = 0.0001f;
        foreach (float amostra in onda) pico = Mathf.Max(pico, Mathf.Abs(amostra));
        for (int i = 0; i < onda.Length; i++) onda[i] *= 0.9f / pico;
        return onda;
    }

    // Arquivo WAV simples: mono, 16 bits.
    static void Gravar(float[] onda, string caminho)
    {
        using (BinaryWriter arquivo = new BinaryWriter(File.Create(caminho)))
        {
            int bytes = onda.Length * 2;
            arquivo.Write(Encoding.ASCII.GetBytes("RIFF"));
            arquivo.Write(36 + bytes);
            arquivo.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            arquivo.Write(16);
            arquivo.Write((short)1);  // sem compressão
            arquivo.Write((short)1);  // um canal
            arquivo.Write(Taxa);
            arquivo.Write(Taxa * 2);  // bytes por segundo
            arquivo.Write((short)2);  // bytes por amostra
            arquivo.Write((short)16); // bits por amostra
            arquivo.Write(Encoding.ASCII.GetBytes("data"));
            arquivo.Write(bytes);
            foreach (float amostra in onda) arquivo.Write((short)(Mathf.Clamp(amostra, -1f, 1f) * 32767f));
        }
    }
}
