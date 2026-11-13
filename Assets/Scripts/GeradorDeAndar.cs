using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// Etapas 33 a 40, 44, 56, 58, 62, 63, 66, 67, 72, 82 e 84: monta cada andar encaixando salas sorteadas numa grade,
// veste as salas com o tema do andar, põe veios, enfeites, itens, perigos, inimigos e uma fita, e desce pelo poço.
// No andar do chefão, usa a arena feita à mão no lugar das salas.
public class GeradorDeAndar : MonoBehaviour
{
    public static GeradorDeAndar Instancia;

    [Header("Peças")]
    [Tooltip("Todas as salas que o gerador pode usar. Para ter salas novas, crie o prefab com o script Sala e ponha aqui.")]
    public Sala[] salas;
    [Tooltip("Etapa 58: quantas salas, qual tema, quais inimigos e quantos veios, itens e perigos cada andar tem.")]
    public TabelaDeDificuldade tabela;
    public GameObject veio;
    [Tooltip("Etapa 139: outros formatos de veio, sorteados junto com o de cima.")]
    public GameObject[] outrosVeios;
    [Tooltip("Etapa 56: itens sorteados nos pontos de item das salas.")]
    public GameObject[] itens;
    [Tooltip("Etapa 55: perigos sorteados nos pontos de perigo das salas.")]
    public GameObject[] perigos;
    [Tooltip("Etapa 44: elevador que sobe guardando o minério, na sala do poço dos andares 3, 6 e 9.")]
    public GameObject elevador;
    [Tooltip("Etapa 62: a fita cassete. Cada andar tem no máximo uma.")]
    public GameObject fita;
    [Tooltip("Etapa 82: placas com os controles, postas na primeira sala do andar 1.")]
    public GameObject tutorial;
    [Tooltip("Etapa 66: o material de rocha das salas, que cada tema troca pelo seu.")]
    public Material rochaPadrao;
    [Tooltip("Etapa 143: a seta de giz no chão, na frente da porta por onde o caminho principal segue. Só nos trechos com moldura nas portas.")]
    public GameObject setaDeGiz;
    [Tooltip("Etapa 143: o número do nível, escrito a giz no chão da sala do começo. As texturas são NIVEL 1, NIVEL 2...")]
    public GameObject numeroDeGiz;
    public Texture2D[] numerosDeGiz;

    [Header("Chefão")]
    [Tooltip("Etapa 67: a arena feita à mão, usada no lugar das salas sorteadas.")]
    public GameObject arena;
    public int andarDoChefao = 10;

    [Header("Sorteio")]
    [Tooltip("0 sorteia uma semente nova a cada partida. Ponha um número para repetir os mesmos andares.")]
    public int semente;
    [Range(0f, 1f)] public float chanceDeRamo = 0.35f;
    [Range(0f, 1f)] public float chanceDeRampa = 0.5f;

    [Header("Desempenho")]
    [Tooltip("Etapa 84: salas mais longe do jogador que isto, em metros, ficam desligadas.")]
    public float distanciaDeDesligar = 38f;

    [HideInInspector] public int andar = 1;
    [HideInInspector] public List<Sala> salasDoAndar = new List<Sala>(); // a primeira é a do começo

    public Tema TemaAtual { get; private set; }
    public bool NaArena { get { return arena != null && andar >= andarDoChefao; } }

    public const float Modulo = 15f; // cada sala ocupa 15 x 15 m da grade
    // Norte, leste, sul e oeste: a mesma ordem dos giros de 90 graus no sentido do relógio.
    public static readonly Vector2Int[] Passo = { new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0) };

    class Casa
    {
        public bool[] aberta = new bool[4];
        public float altura;
        public int entrada = -1; // lado por onde o caminho chega
        public Sala rampa;       // a rampa sorteada, se esta casa for uma
        public bool saida;
        public bool inicio;
    }

    readonly Dictionary<Vector2Int, Casa> casas = new Dictionary<Vector2Int, Casa>();
    readonly List<Vector2Int> ordem = new List<Vector2Int>(); // o caminho, do começo à saída, e depois os ramos
    System.Random sorteio;
    Transform raiz;
    Transform inimigos;
    float fimDoTitulo;
    float proximaChecagem;
    Material rochaViva;  // a rocha deste andar: uma cópia do material do tema
    Color brilhoDaRocha; // o brilho dela parado

    void Awake()
    {
        Instancia = this;
        if (semente == 0) semente = Random.Range(1, 100000);
    }

    void Start()
    {
        Gerar();
    }

    // Etapa 84: a cada meio segundo, desliga as salas longe do jogador e religa as que ficaram perto.
    void Update()
    {
        // Etapa 145: do andar 7 para baixo, o brilho do veio bate como um coração, uma vez a cada dois segundos, e
        // mais forte a cada andar.
        if (rochaViva != null && andar >= 7 && brilhoDaRocha.maxColorComponent > 0f)
        {
            float forca = Mathf.Clamp((andar - 6) * 0.45f, 0f, 2f);
            float batida = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Time.time * Mathf.PI)), 6f);
            rochaViva.SetColor("_EmissionColor", brilhoDaRocha * (1f + forca * batida * 2.5f));
        }

        Jogador jogador = Jogador.Instancia;
        if (jogador == null || Time.time < proximaChecagem) return;
        proximaChecagem = Time.time + 0.5f;
        foreach (Sala sala in salasDoAndar)
        {
            bool perto = (sala.transform.position - jogador.transform.position).sqrMagnitude < distanciaDeDesligar * distanciaDeDesligar;
            if (sala.gameObject.activeSelf != perto) sala.gameObject.SetActive(perto);
        }
    }

    // Etapa 40: chamado pelo poço.
    public void Descer()
    {
        andar++;
        Gerar();
    }

    public void Gerar()
    {
        Apagar(raiz);
        Apagar(inimigos);
        // Etapa 39: um sorteio só do gerador, para outros sorteios do jogo não mudarem o andar.
        sorteio = new System.Random(semente * 31 + andar);
        raiz = new GameObject("Andar " + andar).transform;
        inimigos = new GameObject("Inimigos do andar " + andar).transform;
        salasDoAndar.Clear();
        TabelaDeDificuldade.Andar config = tabela.Do(andar);
        TemaAtual = config.tema;

        if (NaArena) GerarArena();
        else GerarSalas(config);

        VestirComOTema();
        Jogo.Instancia.RegistrarAndar(andar);
        fimDoTitulo = Time.time + 3.5f;
    }

    void GerarSalas(TabelaDeDificuldade.Andar config)
    {
        Planejar(config.salas);
        foreach (Vector2Int pos in ordem) salasDoAndar.Add(Colocar(pos, casas[pos]));

        // Antes do NavMesh entra tudo o que os inimigos têm de contornar: veios, enfeites e molduras das portas.
        // Etapa 139: o veio e os seus outros formatos. O sorteio gasta um número por ponto de qualquer jeito.
        List<GameObject> formatos = new List<GameObject> { veio };
        if (outrosVeios != null) formatos.AddRange(outrosVeios);
        GameObject[] veios = formatos.ToArray();

        // Etapa 135: os trilhos seguem o caminho principal, de porta em porta, passando pelo meio de cada sala.
        // Ficam de fora as rampas e os ramos que terminam em beco.
        if (TemaAtual != null && TemaAtual.trilho != null)
        {
            int caminho = Mathf.Min(config.salas, salasDoAndar.Count);
            for (int i = 0; i < caminho; i++)
            {
                Casa casa = casas[ordem[i]];
                if (casa.rampa != null) continue;
                if (i > 0) PorTrilho(salasDoAndar[i], casa.entrada);
                if (i < caminho - 1) PorTrilho(salasDoAndar[i], Direcao(ordem[i], ordem[i + 1]));
            }
        }

        // Etapa 143: marcas de giz dos mineiros. Em quatro de cada cinco salas do caminho principal, uma seta no chão
        // aponta a porta por onde o caminho segue. Onde não há moldura nas portas (o veio), ninguém marcou nada.
        if (TemaAtual != null && TemaAtual.portal != null)
        {
            int caminho = Mathf.Min(config.salas, salasDoAndar.Count);
            for (int i = 0; i < caminho - 1; i++)
            {
                if (casas[ordem[i]].rampa != null) continue;
                Quaternion giro = Quaternion.Euler(0f, Direcao(ordem[i], ordem[i + 1]) * 90f, 0f);
                Transform sala = salasDoAndar[i].transform;
                if (setaDeGiz != null && (i * 7 + andar * 3) % 5 != 0)
                    Instantiate(setaDeGiz, sala.position + giro * new Vector3(0.8f, 0.02f, 5.2f), giro, sala);
                if (i == 0 && numeroDeGiz != null && numerosDeGiz != null && andar - 1 < numerosDeGiz.Length)
                {
                    GameObject numero = Instantiate(numeroDeGiz, sala.position + giro * new Vector3(-1f, 0.02f, 2.4f), giro, sala);
                    numero.GetComponentInChildren<Renderer>().material.mainTexture = numerosDeGiz[andar - 1];
                }
            }
        }

        Transform pontoDaCena = PontoDaCena();
        foreach (Sala sala in salasDoAndar)
        {
            Espalhar(sala.pontosDeVeio, config.chanceDeVeio, veios, null);
            if (TemaAtual == null) continue;
            Espalhar(sala.pontosDeDecoracao, TemaAtual.chanceDeEnfeite, TemaAtual.enfeites, null, pontoDaCena);
            if (TemaAtual.portal != null) Molduras(sala);
        }
        // Etapa 113: a cena muda deste andar, no lugar de um enfeite.
        if (pontoDaCena != null)
            Instantiate(TemaAtual.cenas[(andar - 1) % TemaAtual.cenas.Length], pontoDaCena.position, pontoDaCena.rotation, pontoDaCena.parent);

        // Etapa 123: a luz do trecho, no meio de algumas salas. Um sorteio à parte, que não mexe no sorteio do andar.
        if (TemaAtual != null && TemaAtual.luminaria != null)
        {
            System.Random sorteioDaLuz = new System.Random(semente * 7 + andar);
            foreach (Sala sala in salasDoAndar)
            {
                bool acende = sorteioDaLuz.NextDouble() < TemaAtual.chanceDeLuminaria;
                if (!acende || sala.desnivel != 0f) continue;
                SubirAoTeto(Instantiate(TemaAtual.luminaria, sala.transform.position, sala.transform.rotation, sala.transform), sala);
            }
        }

        // Etapa 44: nos andares 3, 6 e 9, um elevador num canto da sala do poço.
        if (andar % 3 == 0 && elevador != null)
        {
            Transform saida = salasDoAndar.Find(s => s.saida).transform;
            Instantiate(elevador, saida.TransformPoint(new Vector3(3f, 0f, 3f)), saida.rotation, saida);
        }

        // Etapa 82: no andar 1, placas com os controles na frente de onde você começa.
        if (andar == 1 && tutorial != null) Instantiate(tutorial, salasDoAndar[0].transform);

        ConstruirNavMesh();

        // Fita, itens, perigos e inimigos entram depois do NavMesh, porque não são obstáculos.
        // Nada disso aparece na sala do começo.
        int numeroDaFita;
        Transform pontoDaFita = PontoDaFita(out numeroDaFita);
        for (int i = 1; i < salasDoAndar.Count; i++)
        {
            Sala sala = salasDoAndar[i];
            Espalhar(sala.pontosDeItem, config.chanceDeItem, itens, null, pontoDaFita);
            Espalhar(sala.pontosDePerigo, config.chanceDePerigo, perigos, null);
            Espalhar(sala.pontosDeInimigo, config.chanceDeInimigo, config.inimigos, inimigos);
        }
        if (pontoDaFita != null)
            Instantiate(fita, pontoDaFita.position, pontoDaFita.rotation, pontoDaFita.parent).GetComponent<Fita>().numero = numeroDaFita;

        int lado = System.Array.IndexOf(casas[ordem[0]].aberta, true); // de frente para a saída da sala
        if (Jogador.Instancia != null) Jogador.Instancia.Teleportar(salasDoAndar[0].transform.position + Vector3.up * 0.05f, lado * 90f);
    }

    // Etapa 67: o andar do chefão não é sorteado.
    void GerarArena()
    {
        Transform sala = Instantiate(arena, Vector3.zero, Quaternion.identity, raiz).transform;
        ConstruirNavMesh();
        Transform inicio = sala.Find("Inicio");
        if (Jogador.Instancia != null) Jogador.Instancia.Teleportar(inicio.position + Vector3.up * 0.05f, inicio.eulerAngles.y);
    }

    // Etapa 37: o NavMesh do andar novo.
    void ConstruirNavMesh()
    {
        NavMeshSurface chao = raiz.gameObject.AddComponent<NavMeshSurface>();
        chao.collectObjects = CollectObjects.Children;
        chao.useGeometry = NavMeshCollectGeometry.PhysicsColliders; // com Render Meshes, modelos importados ficam de fora na build
        chao.BuildNavMesh();
    }

    // Etapa 66: neblina, luz ambiente, rocha e água do tema deste andar.
    void VestirComOTema()
    {
        if (rochaViva != null) Destroy(rochaViva);
        rochaViva = null;
        if (TemaAtual == null) return;
        RenderSettings.fogColor = TemaAtual.neblina;
        RenderSettings.fogDensity = TemaAtual.densidadeDaNeblina;
        RenderSettings.ambientLight = TemaAtual.luzAmbiente;
        if (Clima.Instancia != null) Clima.Instancia.Vestir(TemaAtual); // etapa 147

        // Etapas 66 e 142: a rocha e os outros materiais que o trecho troca. A rocha entra como uma cópia só deste
        // andar, para o brilho dela poder pulsar (etapa 145) sem mexer no arquivo do material.
        Dictionary<Material, Material> trocas = new Dictionary<Material, Material>();
        if (TemaAtual.rocha != null && rochaPadrao != null)
        {
            rochaViva = new Material(TemaAtual.rocha);
            brilhoDaRocha = rochaViva.IsKeywordEnabled("_EMISSION") ? rochaViva.GetColor("_EmissionColor") : Color.black;
            trocas[rochaPadrao] = rochaViva;
        }
        if (TemaAtual.trocas != null)
            foreach (Tema.Troca troca in TemaAtual.trocas)
                if (troca.de != null && troca.para != null) trocas[troca.de] = troca.para;
        if (trocas.Count > 0)
        {
            foreach (Renderer visual in raiz.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materiais = visual.sharedMaterials;
                bool mudou = false;
                for (int i = 0; i < materiais.Length; i++)
                {
                    Material novo;
                    if (materiais[i] == null || !trocas.TryGetValue(materiais[i], out novo)) continue;
                    materiais[i] = novo;
                    mudou = true;
                }
                if (mudou) visual.sharedMaterials = materiais;
            }
        }
        if (TemaAtual.laminaDeAgua != null)
            foreach (Sala sala in salasDoAndar)
                if (sala.desnivel == 0f) Instantiate(TemaAtual.laminaDeAgua, sala.transform.position, sala.transform.rotation, sala.transform);
    }

    // Etapa 72: uma moldura em cada porta da sala, meio metro para dentro.
    void Molduras(Sala sala)
    {
        for (int lado = 0; lado < 4; lado++)
        {
            if (!sala.aberturas[lado]) continue;
            Quaternion giro = Quaternion.Euler(0f, lado * 90f, 0f);
            Vector3 lugar = giro * new Vector3(0f, lado == 2 ? -sala.desnivel : 0f, Modulo / 2f - 0.5f);
            // Etapa 134: a variação sai da posição da porta, e não do sorteio, para o resto do andar não mudar.
            GameObject moldura = TemaAtual.portal;
            if (TemaAtual.portais != null && TemaAtual.portais.Length > 0)
            {
                Vector3 p = sala.transform.TransformPoint(lugar);
                moldura = TemaAtual.portais[Mathf.Abs(Mathf.RoundToInt(p.x * 7f + p.z * 13f) + lado + andar) % TemaAtual.portais.Length];
            }
            Instantiate(moldura, sala.transform.TransformPoint(lugar), sala.transform.rotation * giro, sala.transform);
        }
    }

    // Em cada ponto, com a chance dada, põe uma das opções sorteada, virada como a sala.
    // "pai" vazio deixa a peça dentro da sala, para ela ser desligada junto quando a sala ficar longe.
    void Espalhar(Transform[] pontos, float chance, GameObject[] opcoes, Transform pai, Transform pular = null)
    {
        if (pontos == null || opcoes == null || opcoes.Length == 0) return;
        foreach (Transform ponto in pontos)
        {
            // Sorteia sempre os dois números, para o resto do andar sair igual com ou sem a fita.
            bool aparece = sorteio.NextDouble() < chance;
            GameObject opcao = opcoes[sorteio.Next(opcoes.Length)];
            if (!aparece || ponto == pular || opcao == null) continue;
            SubirAoTeto(Instantiate(opcao, ponto.position, ponto.rotation, pai != null ? pai : ponto.parent), ponto.GetComponentInParent<Sala>());
        }
    }

    // Etapa 119: o teto muda de altura de sala para sala. O que fica pendurado nele (estalactite, lâmpada) vem num
    // filho chamado NoTeto, e aqui esse filho sobe até o teto da sala.
    static void SubirAoTeto(GameObject peca, Sala sala)
    {
        Transform noTeto = peca.transform.Find("NoTeto");
        if (noTeto != null && sala != null) noTeto.localPosition = new Vector3(noTeto.localPosition.x, sala.teto, noTeto.localPosition.z);
    }

    // Etapa 63: a próxima fita ainda não encontrada do trecho da história deste andar.
    // Se o trecho já foi todo encontrado, uma que ficou para trás.
    int ProximaFita()
    {
        Jogo jogo = Jogo.Instancia;
        int trecho = Mathf.Clamp((andar - 1) / 3, 0, 2);
        for (int n = Roteiro.PrimeiraDoTrecho(trecho); n <= Roteiro.UltimaDoTrecho(trecho); n++)
            if (!jogo.TemFita(n)) return n;
        for (int n = 1; n < Roteiro.PrimeiraDoTrecho(trecho); n++)
            if (!jogo.TemFita(n)) return n;
        return 0;
    }

    Transform PontoDaFita(out int numero)
    {
        numero = fita != null ? ProximaFita() : 0;
        if (numero == 0) return null;
        List<Transform> pontos = new List<Transform>();
        for (int i = 1; i < salasDoAndar.Count; i++)
            if (salasDoAndar[i].pontosDeItem != null) pontos.AddRange(salasDoAndar[i].pontosDeItem);
        // Um sorteio à parte, que não mexe no sorteio do andar.
        return pontos.Count == 0 ? null : pontos[new System.Random(semente * 17 + andar).Next(pontos.Count)];
    }

    // Meio trilho, do meio da sala até a porta do lado dado (0 norte, 1 leste, 2 sul, 3 oeste, no mundo).
    void PorTrilho(Sala sala, int lado)
    {
        Instantiate(TemaAtual.trilho, sala.transform.position, Quaternion.Euler(0f, lado * 90f, 0f), sala.transform);
    }

    // Etapa 113: um ponto de enfeite fora da sala do começo, para a cena muda do andar.
    Transform PontoDaCena()
    {
        if (TemaAtual == null || TemaAtual.cenas == null || TemaAtual.cenas.Length == 0) return null;
        List<Transform> pontos = new List<Transform>();
        for (int i = 1; i < salasDoAndar.Count; i++)
            if (salasDoAndar[i].pontosDeDecoracao != null) pontos.AddRange(salasDoAndar[i].pontosDeDecoracao);
        // Um sorteio à parte, que não mexe no sorteio do andar.
        return pontos.Count == 0 ? null : pontos[new System.Random(semente * 13 + andar).Next(pontos.Count)];
    }

    static void Apagar(Transform t)
    {
        if (t == null) return;
        t.gameObject.SetActive(false); // some já: o Destroy só apaga no fim do frame
        Destroy(t.gameObject);
    }

    // Etapas 33 a 35: caminho sem repetir casa até a saída, às vezes uma rampa, e ramos curtos que terminam em beco.
    // Como cada casa recebe uma sala com exatamente as aberturas que tem vizinho, nunca sobra porta para o vazio.
    void Planejar(int tamanho)
    {
        for (int tentativa = 0; tentativa < 100 && !Caminho(tamanho); tentativa++) { }

        List<Sala> rampas = new List<Sala>();
        foreach (Sala sala in salas) if (sala.desnivel > 0f && !sala.saida && ServeNesteAndar(sala)) rampas.Add(sala);
        if (rampas.Count > 0 && sorteio.NextDouble() < chanceDeRampa)
        {
            List<int> retas = new List<int>();
            for (int i = 1; i < ordem.Count - 1; i++)
                if (Direcao(ordem[i], ordem[i + 1]) == (casas[ordem[i]].entrada + 2) % 4) retas.Add(i);
            if (retas.Count > 0) casas[ordem[retas[sorteio.Next(retas.Count)]]].rampa = rampas[sorteio.Next(rampas.Count)];
        }

        // Depois da rampa, o resto do caminho fica mais baixo.
        for (int i = 1; i < ordem.Count; i++)
        {
            Casa anterior = casas[ordem[i - 1]];
            casas[ordem[i]].altura = anterior.altura - (anterior.rampa != null ? anterior.rampa.desnivel : 0f);
        }

        int fimDoCaminho = ordem.Count - 1;
        for (int i = 1; i < fimDoCaminho; i++)
        {
            Vector2Int pos = ordem[i];
            if (casas[pos].rampa != null) continue;
            for (int ramo = 0; ramo < 2; ramo++) // às vezes dois ramos: vira um cruzamento
            {
                if (sorteio.NextDouble() >= chanceDeRamo) break;
                int d = Livre(pos);
                if (d < 0) break;
                Vector2Int ponta = Ligar(pos, d);
                int d2 = Livre(ponta);
                if (d2 >= 0 && sorteio.NextDouble() < 0.5) Ligar(ponta, d2);
            }
        }
    }

    bool Caminho(int tamanho)
    {
        casas.Clear();
        ordem.Clear();
        casas[Vector2Int.zero] = new Casa { inicio = true };
        ordem.Add(Vector2Int.zero);
        while (ordem.Count < tamanho)
        {
            Vector2Int atual = ordem[ordem.Count - 1];
            int d = Livre(atual);
            if (d < 0) return false; // o passeio se fechou: tenta de novo
            casas[Ligar(atual, d)].entrada = (d + 2) % 4;
        }
        casas[ordem[ordem.Count - 1]].saida = true;
        return true;
    }

    // Abre a passagem de "de" até uma casa nova na direção d, na mesma altura.
    Vector2Int Ligar(Vector2Int de, int d)
    {
        Vector2Int nova = de + Passo[d];
        Casa casa = new Casa();
        casa.altura = casas[de].altura;
        casa.aberta[(d + 2) % 4] = true;
        casas[de].aberta[d] = true;
        casas[nova] = casa;
        ordem.Add(nova);
        return nova;
    }

    // Uma direção sorteada que ainda não tem casa, ou -1.
    int Livre(Vector2Int pos)
    {
        int inicio = sorteio.Next(4);
        for (int i = 0; i < 4; i++)
        {
            int d = (inicio + i) % 4;
            if (!casas.ContainsKey(pos + Passo[d])) return d;
        }
        return -1;
    }

    static int Direcao(Vector2Int de, Vector2Int para)
    {
        return System.Array.IndexOf(Passo, para - de);
    }

    // Etapas 31, 32 e 59: sorteia, pelo peso, uma sala com exatamente as aberturas desta casa e gira até encaixar.
    Sala Colocar(Vector2Int pos, Casa casa)
    {
        List<Sala> opcoes = new List<Sala>();
        List<int> giros = new List<int>();
        float pesoTotal = 0f;
        foreach (Sala sala in salas)
        {
            if (sala.saida != casa.saida || (sala.desnivel > 0f) != (casa.rampa != null)) continue;
            if (casa.rampa != null && sala != casa.rampa) continue;
            if (casa.inicio && !sala.podeComecar) continue;
            if (!ServeNesteAndar(sala)) continue;
            for (int giro = 0; giro < 4; giro++)
            {
                if (casa.rampa != null && giro != casa.entrada) continue; // o lado alto, o norte, fica para quem chega
                if (!Encaixa(sala.aberturas, giro, casa.aberta)) continue;
                opcoes.Add(sala);
                giros.Add(giro);
                pesoTotal += sala.peso;
            }
        }
        double sorteado = sorteio.NextDouble() * pesoTotal;
        int escolha = 0;
        while (escolha < opcoes.Count - 1 && sorteado >= opcoes[escolha].peso)
        {
            sorteado -= opcoes[escolha].peso;
            escolha++;
        }
        Vector3 lugar = new Vector3(pos.x * Modulo, casa.altura, pos.y * Modulo);
        return Instantiate(opcoes[escolha], lugar, Quaternion.Euler(0f, giros[escolha] * 90f, 0f), raiz);
    }

    // Etapa 120: as salas especiais só aparecem no trecho a que pertencem (o refeitório, por exemplo, só nas galerias).
    bool ServeNesteAndar(Sala sala)
    {
        return andar >= sala.doAndar && andar <= sala.ateOAndar;
    }

    // O lado d de uma sala girada "giro" vezes 90 graus fica virado para (d + giro) % 4.
    public static bool Encaixa(bool[] aberturas, int giro, bool[] precisa)
    {
        for (int d = 0; d < 4; d++)
            if (aberturas[d] != precisa[(d + giro) % 4]) return false;
        return true;
    }

    // Etapas 39, 40 e 80: a semente num canto e o nome do andar ao chegar.
    void OnGUI()
    {
        Estilo.Comecar();
        GUI.Label(new Rect(Estilo.Largura - 420f, Estilo.Altura - 36f, 400f, 24f), (NaArena ? "o fundo" : "andar " + andar) + " · semente " + semente, Estilo.direita);
        float resta = fimDoTitulo - Time.time;
        if (resta <= 0f) return;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(resta)); // some aos poucos no último segundo
        float y = Estilo.Altura * 0.24f;
        GUI.Label(new Rect(0f, y, Estilo.Largura, 56f), NaArena ? "O Coração do Veio" : "Andar " + andar, Estilo.titulo);
        Estilo.Fundo(new Rect(Estilo.Largura / 2f - 90f, y + 60f, 180f, 2f), Estilo.Apagado);
        if (TemaAtual != null && !NaArena) GUI.Label(new Rect(0f, y + 68f, Estilo.Largura, 28f), TemaAtual.nome, Estilo.centro);
        GUI.color = Color.white;
    }
}
