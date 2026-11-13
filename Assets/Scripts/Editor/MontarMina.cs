using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Etapas 31, 32, 36, 59 e 76: cria os prefabs das salas a partir de mapas de letras e monta a cena Mina com o gerador de andares.
// Menu Galeria 7 > Montar mina.
public static class MontarMina
{
    const string CaminhoCena = "Assets/Scenes/Mina.unity";
    const float Celula = Montar.Celula;

    // Cada sala é uma grade de 5 x 5 células de 3 m, com o norte em cima e aberturas só no meio dos lados.
    // #  rocha        .  chão          -  chão mais baixo     R  rampa que sobe para o norte
    // V v  veio na parede mais próxima   I i  inimigo          P  poço para o próximo andar
    // *  item ou fita X  perigo (desmoronamento ou gás)        O  pilar de madeira
    // D  enfeite do tema, encostado na parede mais próxima
    // Letra minúscula fica na parte baixa. As células das portas, no meio de cada lado, ficam só com chão: um veio ali fecharia a passagem.
    // b  passarela de madeira por cima da parte baixa (etapa 121)
    static readonly string[] Nomes = { "Reta", "Salao", "Curva", "T", "Cruzamento", "Beco", "Rampa", "Saida", "Deposito", "Capela", "CurvaLarga", "TLarga", "Galeria", "CruzamentoLargo",
        "Refeitorio", "Lamparia", "Paiol", "CapelaDaSanta", "CasaDeBombas", "PocoInclinado", "Mezanino" };
    const int Comuns = 14; // as primeiras 14 são salas comuns; as outras têm móveis próprios e não servem para começar o andar
    // Etapa 120: o primeiro e o último andar de cada sala especial. As comuns aparecem em todos.
    static readonly int[,] Andares = { { 1, 3 }, { 1, 3 }, { 1, 3 }, { 4, 6 }, { 4, 6 }, { 4, 9 }, { 1, 9 } };
    static readonly string[][] Mapas =
    {
        new[] { "##.##", "##V##", "##D##", "##I##", "##.##" },
        new[] { "##.##", "#D.I#", "#V*.#", "#I.V#", "##.##" },
        new[] { "##.##", "##XD#", "##.I.", "##V##", "#####" },
        new[] { "##.##", "#DX##", "..I..", "##V##", "#####" },
        new[] { "##.##", "#V.D#", "..I..", "##*V#", "##.##" },
        new[] { "##.##", "#.*.#", "#VIV#", "#D..#", "#####" },
        new[] { "##.##", "##.##", "##R##", "##v##", "##-##" },
        new[] { "##.##", "#*..#", "#DP.#", "#V.V#", "#####" },
        // Etapa 59: depósito cheio de minério e de inimigos, e capela segura, sem ponto de inimigo.
        new[] { "##.##", "#VIV#", "#DVD#", "#VIV#", "#####" },
        new[] { "##.##", "#O.O#", "#D*.#", "#O.O#", "##.##" },
        new[] { "##.##", "#D.V#", "#.I..", "#X..#", "#####" },
        new[] { "##.##", "#V..#", "..I..", "#.*D#", "#####" },
        new[] { "##.##", "#O.O#", "#DI.#", "#OXO#", "##.##" },
        new[] { "##.##", "#O.V#", "..I..", "#X.D#", "##.##" },
        // Etapa 120: as seis salas especiais. Os móveis de cada uma são postos por MontarPecas.Mobiliar.
        new[] { "##.##", "#...#", "#.I.#", "#.*.#", "##.##" }, // refeitório: duas mesas compridas e o fogão
        new[] { "##.##", "#...#", "#.*.#", "#*.*#", "#####" }, // lamparia: as prateleiras de lampiões e a bancada do Severino
        new[] { "##.##", "##.##", "#...#", "#.I.#", "#####" }, // paiol: a porta de ferro e as prateleiras vazias
        new[] { "##.##", "#...#", "#...#", "#.*.#", "#####" }, // capela: bancos, cruz e o nicho da santa; sem inimigo
        new[] { "##.##", "#..X#", "#.I..", "#...#", "#####" }, // casa de bombas: as duas bombas paradas e os canos
        new[] { "##.##", "##.##", "##R##", "##i##", "##-##" }, // poço inclinado: trilho, guincho e um carrinho preso no meio da rampa
        // Etapa 121: salão de dois níveis. Entra-se por cima; a rampa desce para a parte baixa, e a passarela cruza por cima dela.
        new[] { "##.##", "#...#", "#Rb-#", "#ibv#", "##.##" },
    };
    // Chance relativa de cada sala no sorteio: o depósito é raro.
    static readonly float[] Pesos = { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.3f, 0.6f, 1f, 1f, 1f, 1f, 0.7f, 0.6f, 0.6f, 0.7f, 0.8f, 1f, 0.8f };
    // Etapa 119: a altura do teto no meio de cada sala. Corredor é baixo; salão é alto. Nas portas, sempre 3 m.
    static readonly float[] Tetos = { 3f, 4.5f, 3f, 3f, 3.6f, 3.6f, 3f, 4.2f, 3.6f, 5f, 3.6f, 3.6f, 4.2f, 4.5f, 3.6f, 3f, 3f, 5f, 4.2f, 3f, 4.5f };
    const int Quadros = 4; // a pele de rocha tem 4 x 4 quadros por célula: cada um com 75 cm

    [MenuItem("Galeria 7/Montar mina (andares gerados)")]
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

        GameObject objeto = new GameObject("Gerador");
        GeradorDeAndar gerador = objeto.AddComponent<GeradorDeAndar>();
        gerador.veio = MontarPecas.Veio();
        gerador.outrosVeios = new[] { MontarPecas.Veio(2), MontarPecas.Veio(3) };
        gerador.setaDeGiz = MontarPecas.SetaDeGiz();
        gerador.numeroDeGiz = MontarPecas.NumeroDeGiz();
        gerador.numerosDeGiz = MontarPecas.NumerosDeGiz();
        gerador.tabela = MontarPecas.Tabela();
        gerador.itens = MontarPecas.Itens();
        gerador.perigos = new[] { MontarPecas.PerigoDesmoronamento(), MontarPecas.PerigoGas() };
        gerador.elevador = Montar.Prefab("ElevadorQueSobe", () => Montar.CriarElevador(null, true));
        gerador.fita = MontarPecas.FitaCassete();
        gerador.tutorial = MontarPecas.Tutorial();
        gerador.rochaPadrao = Montar.PegarMaterial("Rocha");
        gerador.arena = MontarPecas.Arena();
        gerador.salas = new Sala[Nomes.Length];
        for (int i = 0; i < Nomes.Length; i++) gerador.salas[i] = CriarSala(i);

        // Etapa 76: o som ambiente. Etapa 79: os sustos. Etapa 78: a música.
        Ambiente ambiente = objeto.AddComponent<Ambiente>();
        ambiente.vento = MontarSons.Pegar("vento");
        ambiente.sons = new[] { MontarSons.Pegar("gota"), MontarSons.Pegar("madeira_rangendo") };
        Sustos sustos = objeto.AddComponent<Sustos>();
        sustos.somDePassos = MontarSons.Pegar("passos_atras");
        sustos.somDeSussurro = MontarSons.Pegar("sussurro");
        sustos.somDeMadeira = MontarSons.Pegar("madeira_rangendo");
        sustos.vulto = MontarPecas.VultoDoSusto();
        sustos.tabua = MontarPecas.Tabua();
        Musica musica = objeto.AddComponent<Musica>();
        musica.calma = MontarSons.Pegar("musica_mina");
        musica.tensao = MontarSons.Pegar("musica_tensao");

        // O gerador põe o jogador na sala do começo ao iniciar o Play. Etapa 43: morrer leva à superfície.
        Montar.CriarJogador(Vector3.zero, Vector3.forward).cenaAoMorrer = "Superficie";

        EditorSceneManager.SaveScene(cena, CaminhoCena);
        Montar.OrdenarBuild();
        Debug.Log("Cena Mina pronta. Cada Play gera um andar diferente.");
    }

    static Sala CriarSala(int indice)
    {
        string[] mapa = Mapas[indice];
        return Montar.Prefab("Salas/Sala_" + Nomes[indice], () =>
        {
            Material rocha = Montar.PegarMaterial("Rocha");
            Material madeira = Montar.PegarMaterial("Madeira");
            GameObject raiz = new GameObject("Sala");
            Sala sala = raiz.AddComponent<Sala>();
            List<Transform> veios = new List<Transform>();
            List<Transform> inimigos = new List<Transform>();
            List<Transform> itens = new List<Transform>();
            List<Transform> perigos = new List<Transform>();
            List<Transform> enfeites = new List<Transform>();
            System.Func<int, int, char> letra = (l, c) => l < 0 || l >= 5 || c < 0 || c >= 5 ? '#' : mapa[l][c];
            int abertas = 0;
            System.Random entulho = new System.Random(indice * 31 + 5); // sempre as mesmas pedras em cada sala
            float teto = Tetos[indice];
            bool fosso = Nomes[indice] == "Mezanino"; // a parte baixa é um fosso no meio da sala: o teto não desce com ela

            for (int linha = 0; linha < 5; linha++)
            {
                for (int coluna = 0; coluna < 5; coluna++)
                {
                    char aqui = mapa[linha][coluna];
                    Vector3 centro = new Vector3((coluna - 2) * Celula, 0f, (2 - linha) * Celula);
                    if (aqui == '#')
                    {
                        Bloco(raiz.transform, centro, Montar.Base, teto, rocha);
                        continue;
                    }
                    abertas++;

                    float chao = Montar.Altura(aqui);
                    Vector3 noChao = new Vector3(centro.x, chao, centro.z);
                    if (aqui == 'R') Montar.RampaPara(centro, Vector3.forward, madeira, raiz.transform);
                    else Bloco(raiz.transform, centro, Montar.Base, chao, rocha);
                    // Na parte baixa o teto desce junto, para casar com a sala vizinha, que fica toda mais baixa.
                    if (chao < 0f) { if (!fosso) Bloco(raiz.transform, centro, chao + Montar.Teto, teto, rocha); }
                    // Nas portas o teto fica em 3 m, mesmo em sala alta, para casar com a sala vizinha.
                    else if (NaPorta(linha, coluna) && teto > Montar.Teto) Bloco(raiz.transform, centro, Montar.Teto, teto, rocha);

                    // Etapa 133: pedras soltas do pacote ao pé de algumas paredes. Sem colisão: ficam rentes à parede, onde
                    // não se pisa. As salas especiais ficam de fora, porque têm móveis encostados.
                    if (indice < Comuns && aqui != 'R' && !NaPorta(linha, coluna))
                    {
                        for (int d = 0; d < 4; d++)
                        {
                            if (letra(linha + Montar.PassoLinha[d], coluna + Montar.PassoColuna[d]) != '#' || entulho.NextDouble() > 0.3) continue;
                            GameObject pedra = MontarPecas.Obj(raiz.transform, entulho.Next(2) == 0 ? "Kit_Pedra2" : "Kit_Pedra4", 0f, 0f, 0f, Vector3.zero, entulho.Next(360));
                            pedra.transform.localPosition = noChao + Montar.Direcao[d] * 1.15f + Vector3.Cross(Vector3.up, Montar.Direcao[d]) * ((float)entulho.NextDouble() * 2f - 1f);
                            pedra.transform.localScale = Vector3.one * (0.6f + (float)entulho.NextDouble() * 0.5f);
                        }
                    }

                    switch (aqui)
                    {
                        case 'V':
                        case 'v':
                            veios.Add(Ponto("PontoVeio", raiz.transform, Montar.NaParede(letra, linha, coluna, noChao)));
                            break;
                        case 'I':
                        case 'i':
                            inimigos.Add(Ponto("PontoInimigo", raiz.transform, noChao));
                            break;
                        case '*':
                            itens.Add(Ponto("PontoItem", raiz.transform, noChao));
                            break;
                        case 'X':
                            perigos.Add(Ponto("PontoPerigo", raiz.transform, noChao));
                            break;
                        case 'D':
                            // O enfeite fica a 60 cm da parede vizinha, de frente para o meio da sala.
                            int parede = Montar.ParedeVizinha(letra, linha, coluna);
                            Transform enfeite = Ponto("PontoEnfeite", raiz.transform, parede < 0 ? noChao : noChao + Montar.Direcao[parede] * 0.9f);
                            if (parede >= 0) enfeite.localRotation = Quaternion.LookRotation(-Montar.Direcao[parede]);
                            enfeites.Add(enfeite);
                            break;
                        case 'O':
                            Montar.Peca("Pilar", PrimitiveType.Cube, raiz.transform, noChao + Vector3.up * teto / 2f, new Vector3(0.6f, teto, 0.6f), madeira, true);
                            break;
                        case 'P':
                            ColocarPoco(raiz.transform, noChao, madeira);
                            break;
                        case 'b':
                            // Etapa 121: a passarela fica na altura do chão alto, com corrimão dos dois lados e esteios até
                            // o chão de baixo. Passa-se por baixo dela agachado.
                            Montar.Peca("Passarela", PrimitiveType.Cube, raiz.transform, new Vector3(centro.x, -0.08f, centro.z), new Vector3(2.2f, 0.16f, Celula), madeira, true);
                            foreach (float lado in new[] { -1.05f, 1.05f })
                            {
                                Montar.Peca("Corrimao", PrimitiveType.Cube, raiz.transform, new Vector3(centro.x + lado, 0.95f, centro.z), new Vector3(0.08f, 0.08f, Celula), madeira, true);
                                Montar.Peca("Esteio", PrimitiveType.Cube, raiz.transform, new Vector3(centro.x + lado, (Montar.Fundo + 0.95f) / 2f, centro.z), new Vector3(0.1f, 0.95f - Montar.Fundo, 0.1f), madeira, false);
                            }
                            break;
                    }
                }
            }

            GameObject laje = Montar.Peca("Teto", PrimitiveType.Cube, raiz.transform, new Vector3(0f, teto + 0.25f, 0f), new Vector3(5 * Celula, 0.5f, 5 * Celula), rocha, true);
            laje.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            // Etapa 119: por cima dos blocos, uma pele de rocha irregular nas paredes e no teto. Só visual: a colisão
            // continua sendo a dos blocos lisos, para o gerador e os inimigos não mudarem.
            List<Vector3> manchas = new List<Vector3>();
            List<Vector2> uvDasManchas = new List<Vector2>();
            Montar.Modelo("PeleDeRocha", MontarArte.Facetada("Pele_" + Nomes[indice], Pele(mapa, letra, teto, fosso, manchas, uvDasManchas)), raiz.transform, Vector3.zero, Vector3.one, rocha);
            if (manchas.Count > 0)
            {
                GameObject marcas = Montar.Modelo("Manchas", MontarArte.Facetada("Manchas_" + Nomes[indice], manchas, uvDasManchas), raiz.transform, Vector3.zero, Vector3.one, Montar.PegarMaterial("Mancha"));
                marcas.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            sala.teto = teto;
            if (indice >= Comuns)
            {
                sala.doAndar = Andares[indice - Comuns, 0];
                sala.ateOAndar = Andares[indice - Comuns, 1];
                MontarPecas.Mobiliar(Nomes[indice], raiz.transform, teto);
            }

            // Etapa 76: cada tipo de sala soa diferente. Salão grande ecoa como caverna; corredor, como túnel de pedra.
            AudioReverbZone eco = raiz.AddComponent<AudioReverbZone>();
            eco.reverbPreset = abertas >= 9 ? AudioReverbPreset.Cave : AudioReverbPreset.StoneCorridor;
            eco.minDistance = 5f;
            eco.maxDistance = 8.5f;

            string tudo = string.Concat(mapa);
            sala.aberturas = new[] { mapa[0][2] != '#', mapa[2][4] != '#', mapa[4][2] != '#', mapa[2][0] != '#' };
            sala.desnivel = tudo.Contains("R") && !fosso ? -Montar.Fundo : 0f; // no fosso, as portas ficam todas na mesma altura
            sala.saida = tudo.Contains("P");
            sala.peso = Pesos[indice];
            sala.podeComecar = mapa[2][2] != 'V' && indice < Comuns; // o jogador começa no meio da sala; nas especiais há móveis
            sala.pontosDeVeio = veios.ToArray();
            sala.pontosDeInimigo = inimigos.ToArray();
            sala.pontosDeItem = itens.ToArray();
            sala.pontosDePerigo = perigos.ToArray();
            sala.pontosDeDecoracao = enfeites.ToArray();
            return raiz;
        }).GetComponent<Sala>();
    }

    // As células da borda da sala: é por elas que se passa para a sala vizinha.
    static bool NaPorta(int linha, int coluna)
    {
        return linha == 0 || linha == 4 || coluna == 0 || coluna == 4;
    }

    // Etapa 119: os triângulos da pele de rocha de uma sala. Em cada face de parede e em cada pedaço de teto, uma grade
    // de quadros de 75 cm é empurrada para dentro da sala por um ruído suave (até 35 cm, menos que a largura do
    // jogador). O ruído depende só da posição, então duas células vizinhas emendam sem fresta. Nas quinas e nas
    // portas a pele fica rente ao bloco, para casar com a parede do lado e com a sala vizinha.
    // Etapa 143: de passagem, enche "manchas" e "uvDasManchas" com as manchas de infiltração: em uma de cada três
    // paredes, um pedaço da pele de 1,5 m de largura, do teto para baixo, é repetido 1,5 cm à frente, com a textura
    // da mancha esticada nele. Como usa os mesmos pontos da pele, a mancha acompanha o relevo da rocha.
    static List<Vector3> Pele(string[] mapa, System.Func<int, int, char> letra, float teto, bool fosso, List<Vector3> manchas, List<Vector2> uvDasManchas)
    {
        List<Vector3> triangulos = new List<Vector3>();
        for (int linha = 0; linha < 5; linha++)
        {
            for (int coluna = 0; coluna < 5; coluna++)
            {
                char aqui = mapa[linha][coluna];
                if (aqui == '#') continue;
                Vector3 centro = new Vector3((coluna - 2) * Celula, 0f, (2 - linha) * Celula);
                float chao = aqui == 'R' ? Montar.Fundo : Montar.Altura(aqui);
                float topo = Montar.Altura(aqui) < 0f && !fosso ? Montar.Altura(aqui) + Montar.Teto : NaPorta(linha, coluna) ? Montar.Teto : teto;
                float calma = aqui == 'V' || aqui == 'v' ? 0.3f : 1f; // onde há veio, a parede quase não avança, para não cobri-lo

                for (int d = 0; d < 4; d++)
                {
                    if (letra(linha + Montar.PassoLinha[d], coluna + Montar.PassoColuna[d]) != '#') continue;
                    Vector3 paraDentro = -Montar.Direcao[d];
                    Vector3 aoLongo = Vector3.Cross(Vector3.up, paraDentro);
                    Vector3 meio = centro + Montar.Direcao[d] * (Celula / 2f);
                    // A parede continua reta na célula do lado? Se não, a beirada fica presa.
                    bool[] continua = new bool[2];
                    for (int s = 0; s < 2; s++)
                    {
                        Vector3 lado = aoLongo * (s == 0 ? -1f : 1f);
                        int l2 = linha - Mathf.RoundToInt(lado.z), c2 = coluna + Mathf.RoundToInt(lado.x);
                        continua[s] = letra(l2, c2) != '#' && letra(l2 + Montar.PassoLinha[d], c2 + Montar.PassoColuna[d]) == '#';
                    }
                    int fileiras = Mathf.Max(1, Mathf.RoundToInt((topo - chao) / 0.75f));
                    Vector3[,] p = new Vector3[Quadros + 1, fileiras + 1];
                    for (int i = 0; i <= Quadros; i++)
                    {
                        for (int j = 0; j <= fileiras; j++)
                        {
                            Vector3 naParede = meio + aoLongo * (((float)i / Quadros - 0.5f) * Celula) + Vector3.up * Mathf.Lerp(chao, topo, (float)j / fileiras);
                            bool presa = (i == 0 && !continua[0]) || (i == Quadros && !continua[1]);
                            float ao = d < 2 ? naParede.x : naParede.z, plano = d < 2 ? naParede.z : naParede.x;
                            float a = ao * 0.5f + plano * 3.7f + 50f, b = naParede.y * 0.5f + plano * 1.3f + 50f;
                            float saliencia = 0.24f * Mathf.PerlinNoise(a, b) + 0.08f * Mathf.PerlinNoise(a * 2.7f, b * 2.7f);
                            p[i, j] = naParede + paraDentro * (0.02f + (presa ? 0f : saliencia * calma));
                        }
                    }
                    for (int i = 0; i < Quadros; i++)
                        for (int j = 0; j < fileiras; j++)
                            Quadro(triangulos, p[i, j], p[i + 1, j], p[i + 1, j + 1], p[i, j + 1]);

                    System.Random marca = new System.Random(linha * 131 + coluna * 17 + d * 7 + (int)(teto * 10f));
                    if (calma < 1f || fileiras < 3 || marca.Next(3) != 0) continue;
                    int i0 = marca.Next(Quadros - 1), j0 = fileiras - 3;
                    bool espelhada = marca.Next(2) == 0;
                    Vector3 aFrente = paraDentro * 0.015f;
                    for (int i = i0; i < i0 + 2; i++)
                    {
                        for (int j = j0; j < fileiras; j++)
                        {
                            Quadro(manchas, p[i, j] + aFrente, p[i + 1, j] + aFrente, p[i + 1, j + 1] + aFrente, p[i, j + 1] + aFrente);
                            float u0 = (i - i0) / 2f, u1 = (i + 1 - i0) / 2f, v0 = (j - j0) / 3f, v1 = (j + 1 - j0) / 3f;
                            if (espelhada) { u0 = 1f - u0; u1 = 1f - u1; }
                            uvDasManchas.AddRange(new[] { new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1), new Vector2(u0, v0), new Vector2(u1, v1), new Vector2(u0, v1) });
                        }
                    }
                }

                // O teto desta célula: desce até 38 cm, e fica rente na borda da sala.
                Vector3[,] q = new Vector3[Quadros + 1, Quadros + 1];
                for (int i = 0; i <= Quadros; i++)
                {
                    for (int j = 0; j <= Quadros; j++)
                    {
                        float x = centro.x + ((float)i / Quadros - 0.5f) * Celula, z = centro.z + ((float)j / Quadros - 0.5f) * Celula;
                        bool naBorda = Mathf.Abs(x) > 7.49f || Mathf.Abs(z) > 7.49f;
                        float desce = naBorda ? 0.02f : 0.03f + 0.35f * Mathf.PerlinNoise(x * 0.45f + 80f, z * 0.45f + 80f);
                        q[i, j] = new Vector3(x, topo - desce, z);
                    }
                }
                for (int i = 0; i < Quadros; i++)
                    for (int j = 0; j < Quadros; j++)
                        Quadro(triangulos, q[i, j], q[i + 1, j], q[i + 1, j + 1], q[i, j + 1]);
            }
        }
        return triangulos;
    }

    // Dois triângulos de um quadro. A face olha para o lado de onde os cantos a, b, c, d são vistos no sentido do relógio.
    static void Quadro(List<Vector3> triangulos, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        triangulos.AddRange(new[] { a, b, c, a, c, d });
    }

    // Bloco de rocha de uma célula, da altura "de" até a altura "ate".
    static void Bloco(Transform pai, Vector3 centro, float de, float ate, Material rocha)
    {
        Montar.Peca("Rocha", PrimitiveType.Cube, pai, new Vector3(centro.x, (de + ate) / 2f, centro.z), new Vector3(Celula, ate - de, Celula), rocha, true);
    }

    static Transform Ponto(string nome, Transform pai, Vector3 posicao)
    {
        Transform ponto = new GameObject(nome).transform;
        ponto.SetParent(pai, false);
        ponto.localPosition = posicao;
        return ponto;
    }

    // Etapa 36: boca escura com borda de madeira. Olhar para a boca e apertar E desce um andar.
    static void ColocarPoco(Transform pai, Vector3 noChao, Material madeira)
    {
        GameObject boca = Montar.Peca("Poco", PrimitiveType.Cube, pai, noChao + new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.1f, 1.6f), Montar.PegarMaterial("Escuro"), true);
        boca.AddComponent<Poco>().aviso = "E: descer para o próximo andar";
        Montar.Peca("Borda", PrimitiveType.Cube, pai, noChao + new Vector3(0f, 0.35f, 0.9f), new Vector3(2f, 0.7f, 0.2f), madeira, true);
        Montar.Peca("Borda", PrimitiveType.Cube, pai, noChao + new Vector3(0f, 0.35f, -0.9f), new Vector3(2f, 0.7f, 0.2f), madeira, true);
        Montar.Peca("Borda", PrimitiveType.Cube, pai, noChao + new Vector3(0.9f, 0.35f, 0f), new Vector3(0.2f, 0.7f, 1.6f), madeira, true);
        Montar.Peca("Borda", PrimitiveType.Cube, pai, noChao + new Vector3(-0.9f, 0.35f, 0f), new Vector3(0.2f, 0.7f, 1.6f), madeira, true);
    }
}
