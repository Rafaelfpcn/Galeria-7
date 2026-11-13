using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

// Etapas 11, 15 e 22: monta a cena Teste, um túnel fixo no escuro com jogador, inimigos e NavMesh, bom para testar mecânicas.
// Menu Galeria 7 > Montar cena de teste.
public static class MontarCenaDeTeste
{
    const string CaminhoCena = "Assets/Scenes/Teste.unity";
    const string CaminhoNavMesh = "Assets/Scenes/Teste_NavMesh.asset";
    const float VigaBaixa = 1.4f; // altura livre na passagem baixa

    // Letra maiúscula fica no chão de cima; minúscula, na parte baixa.
    // #  rocha                 .  chão                 -  chão da parte baixa
    // S  início do jogador     B  passagem baixa (só agachado)
    // C  caixa de teste        R  rampa entre as duas alturas
    // V v  veio de minério     I i  Rastejante         A a  armadilha
    static readonly string[] Mapa =
    {
        "##########",
        "#S..B..C.#",
        "########.#",
        "######V..#",
        "######...#",
        "######I.V#",
        "#######R##",
        "######i-a#",
        "######v-i#",
        "##########",
    };

    [MenuItem("Galeria 7/Montar cena de teste")]
    static void Menu()
    {
        if (Montar.PodeMontar(CaminhoCena)) Construir();
    }

    internal static void Construir()
    {
        Montar.Preparar();
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Montar.Escurecer();
        Montar.CriarClima();

        Material rocha = Montar.PegarMaterial("Rocha");
        Material madeira = Montar.PegarMaterial("Madeira");
        GameObject veio = MontarPecas.Veio();
        GameObject rastejante = MontarPecas.Rastejante();

        Transform tunel = new GameObject("Tunel").transform;
        Transform inimigos = new GameObject("Inimigos").transform;
        Vector3 inicio = Vector3.zero;
        Vector3 olharPara = Vector3.forward;
        float celula = Montar.Celula;

        for (int linha = 0; linha < Mapa.Length; linha++)
        {
            for (int coluna = 0; coluna < Mapa[linha].Length; coluna++)
            {
                char letra = Mapa[linha][coluna];
                Vector3 centro = new Vector3(coluna * celula, 0f, -linha * celula);

                if (letra == '#')
                {
                    Montar.Peca("Rocha", PrimitiveType.Cube, tunel, new Vector3(centro.x, (Montar.Base + Montar.Teto) / 2f, centro.z), new Vector3(celula, Montar.Teto - Montar.Base, celula), rocha, true);
                    continue;
                }

                float chao = Montar.Altura(letra);
                Vector3 noChao = new Vector3(centro.x, chao, centro.z);
                if (letra == 'R') Montar.RampaPara(centro, SubidaDaRampa(linha, coluna), madeira, tunel);
                else Montar.Peca("Chao", PrimitiveType.Cube, tunel, new Vector3(centro.x, (Montar.Base + chao) / 2f, centro.z), new Vector3(celula, chao - Montar.Base, celula), rocha, true);

                switch (letra)
                {
                    case 'S':
                        inicio = noChao;
                        olharPara = DirecaoLivre(linha, coluna);
                        break;
                    case 'B':
                        Montar.Peca("VigaBaixa", PrimitiveType.Cube, tunel, new Vector3(centro.x, (VigaBaixa + Montar.Teto) / 2f, centro.z), new Vector3(celula, Montar.Teto - VigaBaixa, celula), madeira, true);
                        break;
                    case 'C':
                        Montar.Peca("CaixaDeTeste", PrimitiveType.Cube, tunel, noChao + Vector3.up * 0.4f, Vector3.one * 0.8f, madeira, true).AddComponent<Interagivel>();
                        break;
                    case 'V':
                    case 'v':
                        GameObject novoVeio = (GameObject)PrefabUtility.InstantiatePrefab(veio, tunel);
                        novoVeio.transform.position = Montar.NaParede(Letra, linha, coluna, noChao);
                        break;
                    case 'I':
                    case 'i':
                        ((GameObject)PrefabUtility.InstantiatePrefab(rastejante, inimigos)).transform.position = noChao;
                        break;
                    case 'A':
                    case 'a':
                        // Fora do Tunel, para não entrar no NavMesh.
                        GameObject armadilha = Montar.Peca("Armadilha", PrimitiveType.Cube, null, noChao + Vector3.up * 0.5f, Vector3.one, Montar.PegarMaterial("Armadilha"), true);
                        armadilha.GetComponent<Collider>().isTrigger = true;
                        armadilha.AddComponent<Armadilha>();
                        break;
                }
            }
        }

        int linhas = Mapa.Length;
        int colunas = Mapa[0].Length;
        GameObject teto = Montar.Peca("Teto", PrimitiveType.Cube, tunel, new Vector3((colunas - 1) * celula / 2f, Montar.Teto + 0.25f, -(linhas - 1) * celula / 2f), new Vector3(colunas * celula, 0.5f, linhas * celula), rocha, true);

        // Etapa 22: o chão por onde os inimigos andam, a área azul na janela Scene. O teto fica de fora.
        teto.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
        NavMeshSurface navegacao = tunel.gameObject.AddComponent<NavMeshSurface>();
        navegacao.collectObjects = CollectObjects.Children;
        navegacao.useGeometry = NavMeshCollectGeometry.PhysicsColliders; // com Render Meshes, modelos importados ficam de fora na build
        navegacao.BuildNavMesh();
        AssetDatabase.DeleteAsset(CaminhoNavMesh);
        AssetDatabase.CreateAsset(navegacao.navMeshData, CaminhoNavMesh);

        Montar.CriarJogador(inicio, olharPara);

        EditorSceneManager.SaveScene(cena, CaminhoCena);
        Montar.OrdenarBuild();
        Debug.Log("Cena Teste pronta. Aperte Play.");
    }

    // A rampa sobe na direção do vizinho alto.
    static Vector3 SubidaDaRampa(int linha, int coluna)
    {
        Vector3 sobe = Vector3.forward;
        for (int i = 0; i < 4; i++)
        {
            char vizinho = Letra(linha + Montar.PassoLinha[i], coluna + Montar.PassoColuna[i]);
            if (vizinho != '#' && vizinho != 'R' && Montar.Altura(vizinho) == 0f) sobe = Montar.Direcao[i];
        }
        return sobe;
    }

    static Vector3 DirecaoLivre(int linha, int coluna)
    {
        for (int i = 0; i < 4; i++)
            if (Letra(linha + Montar.PassoLinha[i], coluna + Montar.PassoColuna[i]) != '#') return Montar.Direcao[i];
        return Vector3.forward;
    }

    static char Letra(int linha, int coluna)
    {
        if (linha < 0 || linha >= Mapa.Length || coluna < 0 || coluna >= Mapa[linha].Length) return '#';
        return Mapa[linha][coluna];
    }
}
