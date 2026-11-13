using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Etapas 71, 74, 82 e 92: a arte feita por código. Texturas pequenas de pixels, modelos simples (cone, pedra, cristal),
// placas com letras de pixel, o ícone do jogo, o pós-processamento e o visual retrô de baixa resolução.
// Tudo só é criado quando falta: para usar uma textura sua, ponha um PNG com o mesmo nome em Assets/Textures.
public static class MontarArte
{
    const int Lado = 32; // as texturas têm 32 x 32 pixels

    // ---------- Texturas ----------

    internal static Texture2D Textura(string nome)
    {
        string caminho = "Assets/Textures/" + nome + ".png";
        Texture2D pronta = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
        if (pronta != null) return pronta;

        // Etapas 133 a 136: as texturas que vêm de fora ficam em Assets/Modelos3D/Texturas, com o mesmo nome do material.
        string deFora = "Assets/Modelos3D/Texturas/" + nome + ".png";
        if (File.Exists(deFora))
        {
            TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(deFora);
            importador.filterMode = FilterMode.Point; // pixels quadrados, como as outras
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.mipmapEnabled = false;
            importador.wrapMode = TextureWrapMode.Repeat;
            importador.maxTextureSize = 128;
            importador.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(deFora);
        }

        // Etapa 143: o número do nível, escrito a giz no chão: Giz1 é "NIVEL 1", e assim até o 9.
        if (nome.StartsWith("Giz") && nome.Length == 4) return Giz(caminho, "NIVEL " + nome[3]);

        // Etapa 137: os papéis do quadro de avisos.
        if (nome == "Cartaz") return Papel(caminho, new Color(0.8f, 0.72f, 0.4f), new Color(0.35f, 0.05f, 0.03f), "SEGURANCA", "USE O CAPACETE", "PENDURE A CHAPA", "BATA O TETO");
        if (nome == "Calendario") return Papel(caminho, new Color(0.85f, 0.82f, 0.72f), new Color(0.1f, 0.1f, 0.12f), "MAIO 1983", "9 10 11 12");

        // Etapa 82: as placas do tutorial.
        if (nome == "Placa1") return Placa(caminho, "WASD: ANDAR", "SHIFT: CORRER", "C: AGACHAR", "E: USAR");
        if (nome == "Placa2") return Placa(caminho, "CLIQUE: PICARETA", "DIREITO: CHAMA", "F: LUZ", "Q: SINALIZADOR");
        if (nome == "Placa3") return Placa(caminho, "QUADRO DE CHAPAS", "GALERIA 7"); // etapa 113
        // Etapas 118 e 122: a placa da companhia, a interdição e as marcas de profundidade do poço.
        if (nome == "Placa4") return Placa(caminho, "MINERADORA", "OURO FUNDO");
        if (nome == "Placa5") return Placa(caminho, "INTERDITADO", "DESDE 1983");
        if (nome == "Placa6") return Placa(caminho, "PERIGO", "PAIOL"); // etapa 120
        if (nome == "Placa10") return Placa(caminho, "10 M");
        if (nome == "Placa20") return Placa(caminho, "20 M");
        if (nome == "Placa30") return Placa(caminho, "30 M");

        int semente = 7;
        foreach (char letra in nome) semente = semente * 31 + letra;
        System.Random sorteio = new System.Random(semente);
        float[,] manchas = new float[8, 8]; // manchas de 4 x 4 pixels, que se repetem sem emenda
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                manchas[x, y] = (float)sorteio.NextDouble();

        Color[] pixels = new Color[Lado * Lado];
        for (int y = 0; y < Lado; y++)
            for (int x = 0; x < Lado; x++)
                pixels[y * Lado + x] = Pixel(nome, x, y, manchas[x / 4, y / 4], (float)sorteio.NextDouble());
        return Salvar(pixels, Lado, Lado, caminho, TextureWrapMode.Repeat);
    }

    // A cor de um pixel. "mancha" muda devagar, de 4 em 4 pixels; "grao" muda a cada pixel.
    static Color Pixel(string nome, int x, int y, float mancha, float grao)
    {
        float tom;
        switch (nome)
        {
            case "Madeira":
                // Tábuas de 8 pixels de altura, com veios ao comprido e uma linha escura entre elas.
                tom = 0.3f + 0.05f * ((y / 8) % 2) + 0.07f * Mathf.Abs(Mathf.Sin(y * 12.9898f)) + grao * 0.04f;
                if (y % 8 == 0) tom *= 0.55f;
                return new Color(tom * 1.3f, tom * 0.85f, tom * 0.5f);
            case "Terra":
                tom = 0.17f + mancha * 0.05f + grao * 0.06f;
                return new Color(tom * 1.15f, tom * 0.95f, tom * 0.75f);
            // Etapa 143: as marcas, com transparência.
            case "Mancha": // infiltração: escorridos escuros que descem do teto, mais fortes em cima
            {
                float escorre = Mathf.Clamp01(Mathf.PerlinNoise(x * 0.33f + 3.1f, 7.7f) * 1.8f - 0.45f);
                float deCima = Mathf.Pow(y / 31f, 1.4f);
                float beira = Mathf.Clamp01(Mathf.Min(x, 31 - x) / 4f);
                float forca = escorre * Mathf.Lerp(0.08f, 0.85f, deCima) * beira * (0.75f + 0.25f * mancha);
                return new Color(0.04f, 0.05f, 0.035f, grao < 0.12f ? forca * 0.5f : forca);
            }
            case "SetaDeGiz": // uma seta riscada a giz, apontando para cima na textura
            {
                float dx = Mathf.Abs(x - 15.5f);
                bool haste = dx < 1.6f && y >= 4 && y <= 21;
                bool ponta = y >= 17 && y <= 28 && dx < (28 - y) * 0.85f && dx > (28 - y) * 0.85f - 3.2f;
                return new Color(0.92f, 0.9f, 0.82f, haste || ponta ? (grao < 0.2f ? 0.3f : 0.9f) : 0f);
            }
            case "Onda": // etapa 145: o anel na água
            {
                float raio = Mathf.Sqrt((x - 15.5f) * (x - 15.5f) + (y - 15.5f) * (y - 15.5f));
                return new Color(0.7f, 0.85f, 0.95f, Mathf.Clamp01(1f - Mathf.Abs(raio - 12.5f) / 1.8f) * 0.7f);
            }
            case "Foto":
            {
                // Etapa 110: o retrato do avô, em sépia: um homem de capacete, com a picareta no ombro, e uma moldura clara.
                if (x < 2 || x > 29 || y < 2 || y > 29) return new Color(0.8f, 0.75f, 0.6f);
                float dx = x - 15.5f;
                bool cabeca = dx * dx + (y - 19f) * (y - 19f) < 20f;
                bool capacete = y >= 21 && y <= 25 && Mathf.Abs(dx) < 7f - (y - 21) * 0.9f;
                bool ombros = y < 14 && Mathf.Abs(dx) < 4f + (14 - y) * 0.9f;
                bool cabo = Mathf.Abs((x - 22f) - (y - 8f) * 0.25f) < 0.8f && y > 5 && y < 27;
                bool ferro = y >= 25 && y <= 27 && x > 20 && x < 30;
                tom = cabeca || capacete || ombros ? 0.16f : cabo || ferro ? 0.24f : 0.5f + mancha * 0.08f;
                return new Color(tom * 1.2f, tom * 0.95f, tom * 0.62f);
            }
            // Etapas 126 a 128: a pele e a roupa dos inimigos.
            case "PeleRastejante": // cinza-esverdeada, com veias escuras
                tom = 0.2f + mancha * 0.1f + grao * 0.04f;
                if (grao < 0.06f) tom *= 0.5f;
                return new Color(tom * 0.95f, tom * 1.05f, tom * 0.85f);
            case "PeleOuvinte": // pálida, com manchas azuladas
                tom = 0.62f + mancha * 0.12f + grao * 0.05f;
                return mancha > 0.8f ? new Color(tom * 0.85f, tom * 0.9f, tom) : new Color(tom, tom * 0.95f, tom * 0.86f);
            case "Macacao": // brim azul trançado, com manchas de óleo
                tom = 0.2f + ((x + y) % 2) * 0.03f + mancha * 0.05f;
                if (mancha < 0.15f) tom *= 0.6f;
                return new Color(tom * 0.85f, tom * 0.95f, tom * 1.3f);
            case "PeleMorta":
                tom = 0.42f + mancha * 0.08f + grao * 0.04f;
                return new Color(tom, tom * 0.92f, tom * 0.85f);
            case "Minerio":
                if (grao > 0.93f) return new Color(1f, 0.85f, 0.35f);
                if (grao > 0.8f) return new Color(0.8f, 0.6f, 0.15f);
                tom = 0.2f + mancha * 0.08f;
                return new Color(tom, tom * 0.95f, tom * 0.9f);
            case "Veio":
            case "VeioBrilho":
                // Um veio verde ondulado, quebrado em pedaços tortos. Ele fecha nas bordas, para a textura se repetir sem emenda.
                float veio = 16f + 6f * Mathf.Sin(x * Mathf.PI / 16f) + (mancha - 0.5f) * 5f;
                if (mancha > 0.3f && Mathf.Abs(y - veio) < 0.7f) return new Color(0.15f, 0.75f, 0.3f);
                if (nome == "VeioBrilho") return Color.black; // só as linhas brilham
                tom = 0.035f + mancha * 0.03f + grao * 0.015f;
                return new Color(tom, tom, tom * 1.1f);
            default: // Rocha
                tom = 0.36f + mancha * 0.14f + grao * 0.06f;
                if (grao < 0.04f) tom *= 0.6f; // pontinhos escuros
                tom = Mathf.Round(tom * 24f) / 24f; // poucos tons, como em jogo antigo
                return new Color(tom, tom * 0.95f, tom * 0.88f);
        }
    }

    static Texture2D Salvar(Color[] pixels, int largura, int altura, string caminho, TextureWrapMode borda)
    {
        Texture2D imagem = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        imagem.SetPixels(pixels);
        imagem.Apply();
        File.WriteAllBytes(caminho, imagem.EncodeToPNG());
        Object.DestroyImmediate(imagem);

        AssetDatabase.ImportAsset(caminho);
        TextureImporter importador = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importador.filterMode = FilterMode.Point; // pixels quadrados, sem borrar
        importador.textureCompression = TextureImporterCompression.Uncompressed;
        importador.mipmapEnabled = false;
        importador.wrapMode = borda;
        importador.npotScale = TextureImporterNPOTScale.None;
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
    }

    // ---------- Letras de pixel ----------

    // Cada letra tem 5 x 7 pixels: 7 linhas de 5 pontos, de cima para baixo. Só existem as letras que o jogo usa.
    static readonly Dictionary<char, string> Letras = new Dictionary<char, string>
    {
        { 'A', "01110 10001 10001 11111 10001 10001 10001" },
        { 'C', "01110 10001 10000 10000 10000 10001 01110" },
        { 'D', "11110 10001 10001 10001 10001 10001 11110" },
        { 'E', "11111 10000 10000 11110 10000 10000 11111" },
        { 'F', "11111 10000 10000 11110 10000 10000 10000" },
        { 'G', "01110 10001 10000 10111 10001 10001 01111" },
        { 'H', "10001 10001 10001 11111 10001 10001 10001" },
        { 'I', "01110 00100 00100 00100 00100 00100 01110" },
        { 'L', "10000 10000 10000 10000 10000 10000 11111" },
        { 'M', "10001 11011 10101 10101 10001 10001 10001" },
        { 'N', "10001 11001 10101 10011 10001 10001 10001" },
        { 'O', "01110 10001 10001 10001 10001 10001 01110" },
        { 'P', "11110 10001 10001 11110 10000 10000 10000" },
        { 'Q', "01110 10001 10001 10001 10101 10010 01101" },
        { 'R', "11110 10001 10001 11110 10100 10010 10001" },
        { 'S', "01111 10000 10000 01110 00001 00001 11110" },
        { 'T', "11111 00100 00100 00100 00100 00100 00100" },
        { 'U', "10001 10001 10001 10001 10001 10001 01110" },
        { 'W', "10001 10001 10001 10101 10101 11011 10001" },
        { 'Z', "11111 00001 00010 00100 01000 10000 11111" },
        { ':', "00000 00100 00100 00000 00100 00100 00000" },
        { '7', "11111 00001 00010 00100 01000 01000 01000" },
        { 'B', "11110 10001 10001 11110 10001 10001 11110" },
        { 'V', "10001 10001 10001 10001 10001 01010 00100" },
        { '0', "01110 10001 10011 10101 11001 10001 01110" },
        { '1', "00100 01100 00100 00100 00100 00100 01110" },
        { '2', "01110 10001 00001 00010 00100 01000 11111" },
        { '3', "11110 00001 00001 01110 00001 00001 11110" },
        { '4', "00010 00110 01010 10010 11111 00010 00010" },
        { '5', "11111 10000 11110 00001 00001 10001 01110" },
        { '6', "00110 01000 10000 11110 10001 10001 01110" },
        { '8', "01110 10001 10001 01110 10001 10001 01110" },
        { '9', "01110 10001 10001 01111 00001 00010 01100" },
    };

    // Escreve um texto nos pixels. (x, topo) é o canto de cima à esquerda; cada ponto da letra vira um quadrado de "escala" pixels.
    static void Escrever(Color[] pixels, int largura, string texto, int x, int topo, Color cor, int escala)
    {
        foreach (char letra in texto)
        {
            string pontos;
            if (Letras.TryGetValue(letra, out pontos))
            {
                pontos = pontos.Replace(" ", "");
                for (int i = 0; i < 35; i++)
                {
                    if (pontos[i] != '1') continue;
                    for (int dy = 0; dy < escala; dy++)
                        for (int dx = 0; dx < escala; dx++)
                            pixels[(topo - (i / 5) * escala - dy) * largura + x + (i % 5) * escala + dx] = cor;
                }
            }
            x += 6 * escala;
        }
    }

    // Etapa 82: uma placa de madeira escura com os controles escritos a giz.
    static Texture2D Placa(string caminho, params string[] linhas)
    {
        int colunas = 0;
        foreach (string linha in linhas) colunas = Mathf.Max(colunas, linha.Length);
        int largura = colunas * 6 + 6;
        int altura = linhas.Length * 9 + 5;
        Color[] pixels = new Color[largura * altura];
        for (int i = 0; i < pixels.Length; i++)
        {
            float tom = (i / largura) % 6 == 0 ? 0.75f : 1f; // frestas entre as tábuas
            pixels[i] = new Color(0.2f * tom, 0.13f * tom, 0.08f * tom);
        }
        for (int i = 0; i < linhas.Length; i++)
            Escrever(pixels, largura, linhas[i], 4, altura - 4 - i * 9, new Color(0.92f, 0.86f, 0.7f), 1);
        return Salvar(pixels, largura, altura, caminho, TextureWrapMode.Clamp);
    }

    // Etapa 143: letras de giz, com falhas, sobre fundo transparente.
    static Texture2D Giz(string caminho, string texto)
    {
        int largura = texto.Length * 6 + 4;
        const int altura = 11;
        Color[] pixels = new Color[largura * altura];
        Color[] letras = new Color[pixels.Length];
        Escrever(letras, largura, texto, 2, altura - 3, Color.white, 1);
        for (int i = 0; i < pixels.Length; i++)
        {
            bool falha = (i * 7919) % 11 == 0; // o giz não pega em todo lugar
            pixels[i] = letras[i].a > 0f ? new Color(0.92f, 0.9f, 0.82f, falha ? 0.35f : 0.9f) : new Color(0.92f, 0.9f, 0.82f, 0f);
        }
        return Salvar(pixels, largura, altura, caminho, TextureWrapMode.Clamp);
    }

    // Etapa 137: um papel impresso: fundo claro, letras escuras e uma borda.
    static Texture2D Papel(string caminho, Color fundo, Color tinta, params string[] linhas)
    {
        int colunas = 0;
        foreach (string linha in linhas) colunas = Mathf.Max(colunas, linha.Length);
        int largura = colunas * 6 + 8;
        int altura = linhas.Length * 10 + 8;
        Color[] pixels = new Color[largura * altura];
        for (int i = 0; i < pixels.Length; i++)
        {
            int x = i % largura, y = i / largura;
            bool borda = x == 1 || y == 1 || x == largura - 2 || y == altura - 2;
            pixels[i] = borda ? tinta : fundo * (0.92f + 0.08f * ((x * 7 + y * 13) % 5) / 4f); // papel um pouco manchado
            pixels[i].a = 1f;
        }
        for (int i = 0; i < linhas.Length; i++)
            Escrever(pixels, largura, linhas[i], 5 + (colunas - linhas[i].Length) * 3, altura - 6 - i * 10, tinta, 1);
        return Salvar(pixels, largura, altura, caminho, TextureWrapMode.Clamp);
    }

    // Etapa 92: o ícone do jogo, um 7 cor de lampião no escuro.
    internal static Texture2D Icone()
    {
        const string caminho = "Assets/Textures/Icone.png";
        Texture2D pronto = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
        if (pronto != null) return pronto;

        const int lado = 256;
        Color[] pixels = new Color[lado * lado];
        for (int y = 0; y < lado; y++)
        {
            for (int x = 0; x < lado; x++)
            {
                float longe = Vector2.Distance(new Vector2(x, y), new Vector2(128f, 128f)) / 150f;
                float claro = Mathf.Clamp01(1f - longe);
                pixels[y * lado + x] = Color.Lerp(new Color(0.03f, 0.025f, 0.03f), new Color(0.6f, 0.3f, 0.06f), claro * claro);
            }
        }
        Escrever(pixels, lado, "7", 68, 212, new Color(1f, 0.86f, 0.55f), 24);
        return Salvar(pixels, lado, lado, caminho, TextureWrapMode.Clamp);
    }

    // ---------- Modelos ----------

    static readonly Dictionary<string, Mesh> malhas = new Dictionary<string, Mesh>();

    static Mesh Pronta(string nome)
    {
        Mesh malha;
        if (malhas.TryGetValue(nome, out malha) && malha != null) return malha;
        malha = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/" + nome + ".asset");
        if (malha != null) malhas[nome] = malha;
        return malha;
    }

    static Mesh Guardar(string nome, Mesh malha)
    {
        malha.name = nome;
        malha.RecalculateBounds();
        AssetDatabase.CreateAsset(malha, "Assets/Models/" + nome + ".asset");
        malhas[nome] = malha;
        return malha;
    }

    static string Numero(float valor)
    {
        return Mathf.Abs(valor).ToString("0.##", CultureInfo.InvariantCulture);
    }

    // Um cubo de lado 1 cuja textura se repete conforme o tamanho real da peça: uma textura a cada 2 metros.
    // Assim uma parede de 3 x 5 m e um caixote de 80 cm têm pixels do mesmo tamanho.
    internal static Mesh Cubo(Vector3 tamanho)
    {
        string nome = "Cubo_" + Numero(tamanho.x) + "x" + Numero(tamanho.y) + "x" + Numero(tamanho.z);
        Mesh malha = Pronta(nome);
        if (malha != null) return malha;

        const float metrosPorTextura = 2f;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normais = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangulos = new List<int>();
        foreach (Vector3 normal in new[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down })
        {
            // u e w são os dois eixos da face; nas paredes, w aponta para cima.
            Vector3 u = normal.y != 0f ? Vector3.right : Vector3.Cross(Vector3.up, normal);
            Vector3 w = Vector3.Cross(normal, u);
            float su = Medida(u, tamanho) / metrosPorTextura;
            float sw = Medida(w, tamanho) / metrosPorTextura;
            int i = vertices.Count;
            vertices.Add((normal - u - w) * 0.5f); uv.Add(new Vector2(0f, 0f));
            vertices.Add((normal + u - w) * 0.5f); uv.Add(new Vector2(su, 0f));
            vertices.Add((normal + u + w) * 0.5f); uv.Add(new Vector2(su, sw));
            vertices.Add((normal - u + w) * 0.5f); uv.Add(new Vector2(0f, sw));
            for (int v = 0; v < 4; v++) normais.Add(normal);
            triangulos.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        malha = new Mesh();
        malha.SetVertices(vertices);
        malha.SetNormals(normais);
        malha.SetUVs(0, uv);
        malha.SetTriangles(triangulos, 0);
        return Guardar(nome, malha);
    }

    // O tamanho da peça ao longo de um eixo.
    static float Medida(Vector3 eixo, Vector3 tamanho)
    {
        return Mathf.Abs(eixo.x * tamanho.x) + Mathf.Abs(eixo.y * tamanho.y) + Mathf.Abs(eixo.z * tamanho.z);
    }

    // Monta uma malha de faces chapadas a partir de triângulos soltos, virando cada face para fora do centro.
    static Mesh Solida(string nome, List<Vector3> pontos)
    {
        Vector3[] vertices = pontos.ToArray();
        int[] triangulos = new int[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i += 3)
        {
            Vector3 normal = Vector3.Cross(vertices[i + 1] - vertices[i], vertices[i + 2] - vertices[i]);
            Vector3 meio = (vertices[i] + vertices[i + 1] + vertices[i + 2]) / 3f;
            if (Vector3.Dot(normal, meio) >= 0f) continue;
            Vector3 troca = vertices[i + 1];
            vertices[i + 1] = vertices[i + 2];
            vertices[i + 2] = troca;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            triangulos[i] = i;
            uv[i] = new Vector2(vertices[i].x + vertices[i].z * 0.5f + 0.5f, vertices[i].y + 0.5f);
        }
        Mesh malha = new Mesh { vertices = vertices, triangles = triangulos, uv = uv };
        malha.RecalculateNormals();
        return Guardar(nome, malha);
    }

    // Etapa 119: uma malha de faces chapadas a partir de triângulos soltos, já virados para o lado certo. A textura
    // é esticada pelo lado para onde cada face olha, uma a cada 2 metros, como nos cubos.
    // Com "uvDado", a textura fica onde quem chama mandar (um ponto de textura para cada ponto da malha): é assim
    // que as manchas das paredes cobrem exatamente o pedaço de pele em que ficam.
    internal static Mesh Facetada(string nome, List<Vector3> pontos, List<Vector2> uvDado = null)
    {
        Mesh malha = Pronta(nome);
        if (malha != null) return malha;

        Vector3[] vertices = pontos.ToArray();
        int[] triangulos = new int[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i += 3)
        {
            if (uvDado != null)
            {
                for (int k = i; k < i + 3; k++) { triangulos[k] = k; uv[k] = uvDado[k]; }
                continue;
            }
            Vector3 normal = Vector3.Cross(vertices[i + 1] - vertices[i], vertices[i + 2] - vertices[i]);
            float x = Mathf.Abs(normal.x), y = Mathf.Abs(normal.y), z = Mathf.Abs(normal.z);
            for (int k = i; k < i + 3; k++)
            {
                Vector3 p = vertices[k];
                triangulos[k] = k;
                uv[k] = (y >= x && y >= z ? new Vector2(p.x, p.z) : x >= z ? new Vector2(p.z, p.y) : new Vector2(p.x, p.y)) / 2f;
            }
        }
        malha = new Mesh();
        if (vertices.Length > 65000) malha.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        malha.vertices = vertices;
        malha.triangles = triangulos;
        malha.uv = uv;
        malha.RecalculateNormals();
        return Guardar(nome, malha);
    }

    // Cone de 7 lados, com 1 de altura e 1 de largura na base, centrado como os cilindros da Unity.
    // Serve para estalagmites, estalactites e os tentáculos do chefão.
    internal static Mesh Cone()
    {
        Mesh malha = Pronta("Cone");
        if (malha != null) return malha;
        const int lados = 7;
        Vector3 ponta = new Vector3(0f, 0.5f, 0f);
        Vector3 fundo = new Vector3(0f, -0.5f, 0f);
        List<Vector3> pontos = new List<Vector3>();
        for (int i = 0; i < lados; i++)
        {
            Vector3 a = Volta(i, lados, 0.5f, -0.5f);
            Vector3 b = Volta(i + 1, lados, 0.5f, -0.5f);
            pontos.AddRange(new[] { ponta, a, b, fundo, a, b });
        }
        return Solida("Cone", pontos);
    }

    // Cristal: duas pirâmides de 5 lados coladas pela base. Serve para o minério e os pontos fracos do chefão.
    internal static Mesh Cristal()
    {
        Mesh malha = Pronta("Cristal");
        if (malha != null) return malha;
        const int lados = 5;
        Vector3 cima = new Vector3(0f, 0.5f, 0f);
        Vector3 baixo = new Vector3(0f, -0.5f, 0f);
        List<Vector3> pontos = new List<Vector3>();
        for (int i = 0; i < lados; i++)
        {
            Vector3 a = Volta(i, lados, 0.28f, -0.15f);
            Vector3 b = Volta(i + 1, lados, 0.28f, -0.15f);
            pontos.AddRange(new[] { cima, a, b, baixo, a, b });
        }
        return Solida("Cristal", pontos);
    }

    static Vector3 Volta(int i, int lados, float raio, float y)
    {
        float angulo = i * Mathf.PI * 2f / lados;
        return new Vector3(Mathf.Cos(angulo) * raio, y, Mathf.Sin(angulo) * raio);
    }

    // Pedra: uma bola de 32 faces amassada. Cada semente dá uma pedra diferente.
    internal static Mesh Pedra(int semente)
    {
        string nome = "Pedra_" + semente;
        Mesh malha = Pronta(nome);
        if (malha != null) return malha;

        Vector3[] cantos = { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        int[,] faces = { { 0, 2, 4 }, { 0, 4, 3 }, { 0, 3, 5 }, { 0, 5, 2 }, { 1, 4, 2 }, { 1, 3, 4 }, { 1, 5, 3 }, { 1, 2, 5 } };
        List<Vector3> pontos = new List<Vector3>();
        for (int f = 0; f < 8; f++)
        {
            // Cada face do octaedro vira 4 triângulos menores.
            Vector3 a = cantos[faces[f, 0]], b = cantos[faces[f, 1]], c = cantos[faces[f, 2]];
            Vector3 ab = (a + b).normalized, bc = (b + c).normalized, ca = (c + a).normalized;
            foreach (Vector3 ponto in new[] { a, ab, ca, ab, b, bc, ca, bc, c, ab, bc, ca })
                pontos.Add(ponto * 0.5f * Amassado(ponto, semente));
        }
        return Solida(nome, pontos);
    }

    // O mesmo ponto recebe sempre o mesmo valor, para as faces vizinhas não se soltarem.
    static float Amassado(Vector3 ponto, int semente)
    {
        float n = Mathf.Sin(ponto.x * 12.9898f + ponto.y * 78.233f + ponto.z * 37.719f + semente * 4.1f) * 43758.5453f;
        return 0.72f + 0.38f * (n - Mathf.Floor(n));
    }

    // ---------- Clima e estilo ----------

    // Etapa 74: o perfil de pós-processamento usado em todas as cenas.
    internal static VolumeProfile PerfilDeClima()
    {
        const string caminho = "Assets/Settings/Galeria7Clima.asset";
        VolumeProfile perfil = AssetDatabase.LoadAssetAtPath<VolumeProfile>(caminho);
        if (perfil != null) return perfil;

        perfil = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(perfil, caminho);

        Vignette vinheta = Efeito<Vignette>(perfil); // cantos escuros
        vinheta.intensity.Override(0.42f);
        vinheta.smoothness.Override(0.45f);
        FilmGrain granulado = Efeito<FilmGrain>(perfil); // chuvisco de filme antigo
        granulado.type.Override(FilmGrainLookup.Medium2);
        granulado.intensity.Override(0.5f);
        granulado.response.Override(0.7f);
        ColorAdjustments cores = Efeito<ColorAdjustments>(perfil);
        cores.contrast.Override(15f);
        cores.saturation.Override(-12f);
        cores.postExposure.Override(0f); // a opção de brilho mexe aqui
        Bloom brilho = Efeito<Bloom>(perfil); // as luzes vazam um pouco
        brilho.threshold.Override(0.9f);
        brilho.intensity.Override(0.8f);
        brilho.scatter.Override(0.65f);

        EditorUtility.SetDirty(perfil);
        AssetDatabase.SaveAssets();
        return perfil;
    }

    static T Efeito<T>(VolumeProfile perfil) where T : VolumeComponent
    {
        T efeito = perfil.Add<T>();
        efeito.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(efeito, perfil); // sem isso, o efeito some ao fechar a Unity
        return efeito;
    }

    // Etapa 71: o jogo é desenhado em baixa resolução e esticado sem borrar, como nos jogos antigos.
    // Só mexe se o Render Scale ainda estiver no valor original, para respeitar o que você ajustar depois.
    internal static void EstiloRetro()
    {
        foreach (string caminho in new[] { "Assets/Settings/PC_RPAsset.asset", "Assets/Settings/Mobile_RPAsset.asset" })
        {
            UniversalRenderPipelineAsset urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(caminho);
            if (urp == null || urp.renderScale < 0.99f) continue;
            urp.renderScale = 0.45f;
            urp.upscalingFilter = UpscalingFilterSelection.Point;
            urp.msaaSampleCount = 1;
            EditorUtility.SetDirty(urp);
        }
    }

    // Etapa 144: põe no renderizador do URP o passe de tela cheia que reduz as cores (shader Galeria7/CoresDePS1).
    // O material fica em Assets/Settings, que as montagens não apagam. Quem liga e desliga é a opção do jogo.
    internal static void CoresDePS1()
    {
        Shader shader = Shader.Find("Galeria7/CoresDePS1");
        if (shader == null) { Debug.LogWarning("Não achei o shader Galeria7/CoresDePS1 (Assets/Shaders/CoresDePS1.shader). O jogo fica sem a redução de cores."); return; }
        const string caminhoDoMaterial = "Assets/Settings/CoresDePS1.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(caminhoDoMaterial);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, caminhoDoMaterial);
        }
        foreach (string caminho in new[] { "Assets/Settings/PC_Renderer.asset", "Assets/Settings/Mobile_Renderer.asset" })
        {
            UniversalRendererData dados = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(caminho);
            if (dados == null || dados.rendererFeatures.Exists(f => f != null && f.name == "CoresDePS1")) continue;
            FullScreenPassRendererFeature passe = ScriptableObject.CreateInstance<FullScreenPassRendererFeature>();
            passe.name = "CoresDePS1";
            passe.passMaterial = material;
            passe.injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
            AssetDatabase.AddObjectToAsset(passe, dados);
            // O renderizador guarda, além da lista de passes, uma lista com o número de cada um dentro do arquivo.
            string guid;
            long numero;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(passe, out guid, out numero);
            SerializedObject serie = new SerializedObject(dados);
            SerializedProperty lista = serie.FindProperty("m_RendererFeatures"), mapa = serie.FindProperty("m_RendererFeatureMap");
            lista.arraySize++;
            lista.GetArrayElementAtIndex(lista.arraySize - 1).objectReferenceValue = passe;
            mapa.arraySize++;
            mapa.GetArrayElementAtIndex(mapa.arraySize - 1).longValue = numero;
            serie.ApplyModifiedProperties();
            EditorUtility.SetDirty(dados);
        }
        AssetDatabase.SaveAssets();
    }

    // Etapa 92: nome do jogo e ícone. O nome do jogo e o da empresa decidem a pasta do save.
    internal static void Identidade()
    {
        if (PlayerSettings.productName.StartsWith("com.unity.template")) PlayerSettings.productName = "Galeria 7";
        if (PlayerSettings.companyName == "Unity Technologies" || PlayerSettings.companyName == "DefaultCompany") PlayerSettings.companyName = "Galeria 7";
        if (PlayerSettings.bundleVersion == "0.1.0" || PlayerSettings.bundleVersion == "1.0") PlayerSettings.bundleVersion = "0.9.0"; // etapa 90
        PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { Icone() }, IconKind.Any);
    }
}
