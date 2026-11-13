using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Etapa 141: faz as texturas base do jogo a partir de fotos. Cada foto é encolhida para poucos pixels, acertada de
// claridade e de cor, e reduzida a uma paleta pequena, para ficar com cara de jogo antigo.
// As fotos ficam fora do jogo, na pasta Baixados (ver CREDITOS-ARTE.md). O resultado vai para
// Assets/Modelos3D/Texturas, com o nome da textura que substitui. Para trocar uma foto: ponha outra no lugar e use
// o menu Galeria 7 > Refazer texturas das fotos, e depois Galeria 7 > Refazer tudo.
public static class MontarTexturas
{
    class Receita
    {
        public string nome;    // a textura do jogo que esta foto vira
        public string foto;    // dentro da pasta Baixados
        public int lado;       // pixels de lado
        public int cores;      // tamanho da paleta
        public float claro;    // claridade média depois de pronta, de 0 (preto) a 1 (branco)
        public Color tinta;    // puxa a cor para um lado
    }

    static readonly Receita[] Receitas =
    {
        new Receita { nome = "Rocha", foto = "Rock060_1K-JPG/Rock060_1K-JPG_Color.jpg", lado = 64, cores = 12, claro = 0.47f, tinta = new Color(1.02f, 0.98f, 0.9f) },
        new Receita { nome = "Madeira", foto = "Planks040_1K-JPG/Planks040_1K-JPG_Color.jpg", lado = 64, cores = 12, claro = 0.36f, tinta = new Color(1.2f, 0.9f, 0.62f) },
        new Receita { nome = "Terra", foto = "Ground111_1K-JPG/Ground111_1K-JPG_Color.jpg", lado = 64, cores = 10, claro = 0.21f, tinta = new Color(1.12f, 0.95f, 0.76f) },
        new Receita { nome = "Macacao", foto = "Fabric086_1K-JPG/Fabric086_1K-JPG_Color.jpg", lado = 64, cores = 8, claro = 0.2f, tinta = new Color(0.72f, 0.85f, 1.15f) },
        new Receita { nome = "Ferro", foto = "Metal063_1K-JPG/Metal063_1K-JPG_Color.jpg", lado = 64, cores = 10, claro = 0.36f, tinta = new Color(0.95f, 0.96f, 1.02f) },
        new Receita { nome = "KitMadeira", foto = "free_mine_assets/textures/wood1.png", lado = 64, cores = 12, claro = 0.36f, tinta = new Color(1.15f, 0.92f, 0.68f) },
        new Receita { nome = "KitFerro", foto = "free_mine_assets/textures/iron_diffuse.png", lado = 64, cores = 10, claro = 0.34f, tinta = Color.white },
    };

    [MenuItem("Galeria 7/Refazer texturas das fotos")]
    internal static void Refazer()
    {
        string pasta = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Baixados");
        Directory.CreateDirectory("Assets/Modelos3D/Texturas");
        int feitas = 0;
        foreach (Receita receita in Receitas)
        {
            string caminho = Path.Combine(pasta, receita.foto);
            if (!File.Exists(caminho)) { Debug.LogWarning("Falta a foto " + caminho + ". A textura " + receita.nome + " fica como está."); continue; }
            Texture2D foto = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            foto.LoadImage(File.ReadAllBytes(caminho));
            Color[] pixels = Paleta(Acertar(Encolher(foto, receita.lado), receita.claro, receita.tinta), receita.cores);
            Object.DestroyImmediate(foto);
            Texture2D pronta = new Texture2D(receita.lado, receita.lado, TextureFormat.RGBA32, false);
            pronta.SetPixels(pixels);
            pronta.Apply();
            File.WriteAllBytes("Assets/Modelos3D/Texturas/" + receita.nome + ".png", pronta.EncodeToPNG());
            Object.DestroyImmediate(pronta);
            feitas++;
        }
        AssetDatabase.Refresh();
        Debug.Log("Texturas das fotos: " + feitas + " de " + Receitas.Length + " refeitas. Use Galeria 7 > Refazer tudo para o jogo pegar as novas.");
    }

    // Cada pixel novo é a média do quadrado de pixels da foto que ele cobre. Uma foto que emenda continua emendando.
    static Color[] Encolher(Texture2D foto, int lado)
    {
        Color32[] origem = foto.GetPixels32();
        int largura = foto.width, altura = foto.height;
        Color[] pequena = new Color[lado * lado];
        for (int y = 0; y < lado; y++)
        {
            for (int x = 0; x < lado; x++)
            {
                int x0 = x * largura / lado, x1 = Mathf.Max(x0 + 1, (x + 1) * largura / lado);
                int y0 = y * altura / lado, y1 = Mathf.Max(y0 + 1, (y + 1) * altura / lado);
                float r = 0f, g = 0f, b = 0f;
                for (int v = y0; v < y1; v++)
                    for (int u = x0; u < x1; u++)
                    {
                        Color32 c = origem[v * largura + u];
                        r += c.r; g += c.g; b += c.b;
                    }
                float n = (x1 - x0) * (y1 - y0) * 255f;
                pequena[y * lado + x] = new Color(r / n, g / n, b / n, 1f);
            }
        }
        return pequena;
    }

    // Leva a claridade média para o alvo, aumenta um pouco o contraste e aplica a tinta.
    static Color[] Acertar(Color[] pixels, float claro, Color tinta)
    {
        float media = 0f;
        foreach (Color c in pixels) media += c.grayscale;
        media /= pixels.Length;
        float ganho = claro / Mathf.Max(0.01f, media);
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            float cinza = c.grayscale;
            float novo = Mathf.Max(0f, claro + (cinza * ganho - claro) * 1.25f); // contraste em volta da média
            float fator = novo / Mathf.Max(0.01f, cinza);
            pixels[i] = new Color(Mathf.Clamp01(c.r * fator * tinta.r), Mathf.Clamp01(c.g * fator * tinta.g), Mathf.Clamp01(c.b * fator * tinta.b), 1f);
        }
        return pixels;
    }

    // Corte pela mediana: reparte as cores em caixas, sempre cortando ao meio a caixa mais espalhada, até ter uma
    // caixa para cada cor da paleta. Cada pixel fica com a cor média da sua caixa.
    static Color[] Paleta(Color[] pixels, int cores)
    {
        List<List<int>> caixas = new List<List<int>> { new List<int>() };
        for (int i = 0; i < pixels.Length; i++) caixas[0].Add(i);
        while (caixas.Count < cores)
        {
            int pior = -1, canalDoPior = 0;
            float maiorFaixa = 0f;
            for (int k = 0; k < caixas.Count; k++)
            {
                if (caixas[k].Count < 2) continue;
                for (int canal = 0; canal < 3; canal++)
                {
                    float menor = 1f, maior = 0f;
                    foreach (int i in caixas[k]) { float v = pixels[i][canal]; menor = Mathf.Min(menor, v); maior = Mathf.Max(maior, v); }
                    if (maior - menor > maiorFaixa) { maiorFaixa = maior - menor; pior = k; canalDoPior = canal; }
                }
            }
            if (pior < 0) break;
            List<int> caixa = caixas[pior];
            int c = canalDoPior;
            caixa.Sort((a, b) => pixels[a][c].CompareTo(pixels[b][c]));
            caixas[pior] = caixa.GetRange(0, caixa.Count / 2);
            caixas.Add(caixa.GetRange(caixa.Count / 2, caixa.Count - caixa.Count / 2));
        }
        Color[] pronto = new Color[pixels.Length];
        foreach (List<int> caixa in caixas)
        {
            Color media = Color.clear;
            foreach (int i in caixa) media += pixels[i];
            media /= caixa.Count;
            media.a = 1f;
            foreach (int i in caixa) pronto[i] = media;
        }
        return pronto;
    }
}
