using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

// Os prefabs do jogo: inimigos, minério, itens, perigos, enfeites, o chefão e sua arena, os temas e a tabela de dificuldade.
// Os modelos são montados com blocos, esferas e as malhas do MontarArte. Cada prefab só é criado se ainda não existe.
public static class MontarPecas
{
    static Material Mat(string nome) { return Montar.PegarMaterial(nome); }
    static AudioClip Som(string nome) { return MontarSons.Pegar(nome); }

    // ---------- Inimigos ----------
    // Os modelos vêm de Assets/Modelos3D e são feitos pelo script Blender/personagens.py.

    // Etapas 28 e 126: rápido e fraco, de quatro patas compridas e olhos vermelhos. Foge da luz forte.
    internal static GameObject Rastejante()
    {
        return Montar.Prefab("Rastejante", () =>
        {
            Inimigo inimigo = NovoInimigo("Rastejante", 0.9f, 0.7f, 3, "rastejante_passo", "rastejante_grito", "rastejante_golpe", "rastejante_ferido");
            Animar(inimigo, 4, 0, 30f, 2.2f, 22f, 0f, new Vector3(0f, 0f, 180f), 0.72f); // morre de patas para cima
            return inimigo.gameObject;
        });
    }

    // Etapas 52, 54 e 127: cego, magro e orelhudo. Ignora a luz e caça só pelo barulho. A respiração dele se ouve de perto.
    internal static GameObject Ouvinte()
    {
        return Montar.Prefab("Ouvinte", () =>
        {
            Inimigo inimigo = NovoInimigo("Ouvinte", 1.8f, 0.6f, 4, "passo_pedra", "ouvinte_grito", "ouvinte_golpe", "rastejante_ferido");
            inimigo.enxerga = false;
            inimigo.audicao = 1.5f;
            inimigo.medoDaLuz = false;
            inimigo.velocidadeVagando = 1f;
            inimigo.velocidadeCorrendo = 4.5f;
            inimigo.dano = 2;
            inimigo.aviso = 0.4f;
            inimigo.descanso = 1.2f;
            Animar(inimigo, 2, 2, 32f, 1.1f, 12f, 130f, new Vector3(-90f, 0f, 0f), 0.1f); // cai de costas

            GameObject folego = new GameObject("Respiracao");
            folego.transform.SetParent(inimigo.transform, false);
            folego.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            Montar.Fonte(folego, Som("ouvinte_respiracao"), true, 9f).volume = 0.8f;
            return inimigo.gameObject;
        });
    }

    // Etapas 53, 54 e 128: um mineiro sem rosto, de capacete e picareta. Lento e resistente, bate forte e não tem medo de luz.
    internal static GameObject MineiroOco()
    {
        return Montar.Prefab("MineiroOco", () =>
        {
            Inimigo inimigo = NovoInimigo("MineiroOco", 1.95f, 0.8f, 8, "mineiro_passo", "mineiro_gemido", "mineiro_golpe", "mineiro_gemido");
            inimigo.medoDaLuz = false;
            inimigo.distanciaDeVisao = 12f;
            inimigo.velocidadeVagando = 0.8f;
            inimigo.velocidadeCorrendo = 2.2f;
            inimigo.distanciaDeAtaque = 2f;
            inimigo.dano = 2;
            inimigo.aviso = 0.9f;
            inimigo.descanso = 1.6f;
            Animar(inimigo, 2, 2, 26f, 0.7f, 6f, 150f, new Vector3(-90f, 0f, 0f), 0.14f);
            return inimigo.gameObject;
        });
    }

    // A base de todo inimigo: colisão, som, o script Inimigo e o modelo do Blender, que entra com o nome Corpo.
    static Inimigo NovoInimigo(string modelo, float altura, float largura, int vida, string passo, string grito, string golpe, string ferido)
    {
        GameObject bicho = new GameObject("Inimigo");
        CapsuleCollider colisao = bicho.AddComponent<CapsuleCollider>();
        colisao.center = new Vector3(0f, altura / 2f, 0f);
        colisao.radius = largura / 2f;
        colisao.height = altura;
        Montar.Fonte(bicho, null, false, 18f);
        Inimigo inimigo = bicho.AddComponent<Inimigo>(); // traz junto o NavMeshAgent e a Vida
        bicho.GetComponent<Vida>().maxima = vida;
        NavMeshAgent agente = bicho.GetComponent<NavMeshAgent>();
        agente.baseOffset = 0f;
        agente.radius = largura / 2f;
        agente.height = altura;
        agente.stoppingDistance = 1.2f;
        agente.acceleration = 20f;
        agente.angularSpeed = 540f;
        inimigo.somPasso = Som(passo);
        inimigo.somGrito = Som(grito);
        inimigo.somGolpe = Som(golpe);
        inimigo.somFerido = Som(ferido);

        GameObject corpo = (GameObject)PrefabUtility.InstantiatePrefab(Montar.Modelo3D(modelo), bicho.transform);
        corpo.name = "Corpo";
        inimigo.corpo = corpo.transform;
        return inimigo;
    }

    // Liga a animação às peças do modelo, que têm nomes fixos: Perna_1, Canela_1, Braco_1, Antebraco_1, Queixo e Cabeca.
    static void Animar(Inimigo inimigo, int pernas, int bracos, float passo, float ritmo, float empinada, float bracosNoAviso, Vector3 giroDaMorte, float alturaDaMorte)
    {
        AnimacaoDeInimigo animacao = inimigo.gameObject.AddComponent<AnimacaoDeInimigo>();
        animacao.corpo = inimigo.corpo;
        animacao.pernas = Pecas(inimigo.corpo, "Perna_", pernas);
        animacao.joelhos = Pecas(inimigo.corpo, "Canela_", pernas);
        animacao.bracos = Pecas(inimigo.corpo, "Braco_", bracos);
        animacao.cotovelos = Pecas(inimigo.corpo, "Antebraco_", bracos);
        animacao.queixo = Achar(inimigo.corpo, "Queixo");
        animacao.cabeca = Achar(inimigo.corpo, "Cabeca");
        animacao.passo = passo;
        animacao.ritmo = ritmo;
        animacao.empinada = empinada;
        animacao.bracosNoAviso = bracosNoAviso;
        animacao.giroDaMorte = giroDaMorte;
        animacao.alturaDaMorte = alturaDaMorte;
    }

    // Etapa 131: um mineiro que ficou. É o modelo do Mineiro Oco deitado de costas, sem a picareta e sem script nenhum.
    static GameObject CorpoDeMineiro()
    {
        return Montar.Prefab("CorpoDeMineiro", () =>
        {
            GameObject raiz = new GameObject("CorpoDeMineiro");
            Transform corpo = Montar.Instalar("MineiroOco", raiz.transform, true);
            PrefabUtility.UnpackPrefabInstance(corpo.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Object.DestroyImmediate(Achar(corpo, "Cabo").gameObject);
            Object.DestroyImmediate(Achar(corpo, "Ferro").gameObject);
            corpo.localPosition = new Vector3(0f, 0.14f, 0.9f);
            corpo.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            Achar(corpo, "Braco_1").localRotation = Quaternion.Euler(0f, 0f, -40f);
            Achar(corpo, "Braco_2").localRotation = Quaternion.Euler(0f, 0f, 65f);
            Achar(corpo, "Antebraco_2").localRotation = Quaternion.Euler(0f, 0f, 50f);
            Achar(corpo, "Perna_1").localRotation = Quaternion.Euler(0f, 0f, -10f);
            Achar(corpo, "Perna_2").localRotation = Quaternion.Euler(-25f, 0f, 18f);
            Achar(corpo, "Canela_2").localRotation = Quaternion.Euler(50f, 0f, 0f);
            Achar(corpo, "Cabeca").localRotation = Quaternion.Euler(0f, 35f, 0f);
            return raiz;
        });
    }

    // Etapa 131: só o capacete, caído no chão.
    static GameObject CapaceteCaido()
    {
        return Montar.Prefab("CapaceteCaido", () =>
        {
            GameObject raiz = new GameObject("CapaceteCaido");
            CapaceteSolto(raiz.transform, new Vector3(0f, 0.06f, 0f), Quaternion.Euler(12f, 40f, -8f));
            return raiz;
        });
    }

    // Tira o capacete do modelo do Mineiro Oco e põe sozinho em 'pai'.
    static void CapaceteSolto(Transform pai, Vector3 posicao, Quaternion giro)
    {
        Transform corpo = Montar.Instalar("MineiroOco", pai, true);
        PrefabUtility.UnpackPrefabInstance(corpo.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        Transform capacete = Achar(corpo, "Capacete");
        capacete.SetParent(pai, false);
        capacete.localPosition = posicao;
        capacete.localRotation = giro;
        Object.DestroyImmediate(corpo.gameObject);
    }

    // ---------- Cenas mudas (etapa 113) ----------
    // Cada andar recebe uma cena, num ponto de enfeite: a parede fica 90 cm atrás (z negativo) e quem olha vem do lado
    // do z positivo. As cenas contam o que as fitas contam, sem palavra nenhuma, ou com um bilhete que se lê com E.

    // Etapa 137: as peças com estes nomes viram um modelo do Blender (de Blender/cenario.py) no lugar do cubo ou do
    // cilindro. Quem chama continua dando o meio e o tamanho da peça antiga; o modelo é posto com o pé no mesmo chão.
    static readonly Dictionary<string, string> Modelos = new Dictionary<string, string>
    {
        { "Marmita", "Obj_Marmita" }, { "Caneca", "Obj_Caneca" }, { "LataDeOleo", "Obj_LataDeOleo" }, { "Lampiao", "Obj_LampiaoApagado" },
        { "Bota", "Obj_Bota" }, { "Gravador", "Obj_Gravador" }, { "Banquinho", "Obj_Banquinho" },
    };

    static GameObject Caixa(Transform pai, string nome, float x, float y, float z, float largura, float altura, float fundo, string material, bool colisao = false)
    {
        string modelo;
        if (Modelos.TryGetValue(nome, out modelo)) return Obj(pai, modelo, x, y - altura / 2f, z, colisao ? new Vector3(largura, altura, fundo) : Vector3.zero);
        return Montar.Peca(nome, PrimitiveType.Cube, pai, new Vector3(x, y, z), new Vector3(largura, altura, fundo), Mat(material), colisao);
    }

    static GameObject Rolo(Transform pai, string nome, float x, float y, float z, float diametro, float altura, string material)
    {
        string modelo;
        if (Modelos.TryGetValue(nome, out modelo)) return Obj(pai, modelo, x, y - altura / 2f, z, Vector3.zero);
        return Montar.Peca(nome, PrimitiveType.Cylinder, pai, new Vector3(x, y, z), new Vector3(diametro, altura / 2f, diametro), Mat(material), false);
    }

    // Um modelo de Assets/Modelos3D, com o pé em (x, y, z). "colisao" é o tamanho de uma caixa de colisão; zero é sem colisão.
    internal static GameObject Obj(Transform pai, string modelo, float x, float y, float z, Vector3 colisao = default(Vector3), float giro = 0f)
    {
        Transform novo = Montar.Instalar(modelo, pai, true);
        novo.name = modelo;
        novo.localPosition = new Vector3(x, y, z);
        novo.localRotation = Quaternion.Euler(0f, giro, 0f);
        if (colisao != Vector3.zero)
        {
            BoxCollider caixa = novo.gameObject.AddComponent<BoxCollider>();
            caixa.center = Vector3.up * colisao.y / 2f;
            caixa.size = colisao;
        }
        return novo.gameObject;
    }

    static void Marmita(Transform pai, float x, float y, float z, float giro = 0f)
    {
        Caixa(pai, "Marmita", x, y + 0.045f, z, 0.16f, 0.09f, 0.11f, "Metal").transform.localRotation = Quaternion.Euler(0f, giro, 0f);
    }

    // (x, y, z) é onde a vela apoia.
    internal static void Vela(Transform pai, float x, float y, float z, bool acesa)
    {
        Rolo(pai, "Vela", x, y + 0.05f, z, 0.035f, 0.1f, "Papel");
        if (acesa) Montar.Peca("Chama", PrimitiveType.Sphere, pai, new Vector3(x, y + 0.125f, z), new Vector3(0.025f, 0.05f, 0.025f), Mat("Chama"), false);
    }

    // Um papel que se lê com E.
    internal static void Bilhete(Transform pai, Vector3 posicao, float giro, string titulo, string texto)
    {
        GameObject papel = Montar.Peca("Bilhete", PrimitiveType.Cube, pai, posicao, new Vector3(0.22f, 0.012f, 0.3f), Mat("Papel"), true);
        papel.transform.localRotation = Quaternion.Euler(0f, giro, 0f);
        Carta carta = papel.AddComponent<Carta>();
        carta.aviso = "E: ler";
        carta.titulo = titulo;
        carta.texto = texto;
    }

    // Galerias, fita 1: a mesa do refeitório, com as 23 marmitas que ninguém voltou para buscar.
    static GameObject CenaDoRefeitorio()
    {
        return Montar.Prefab("CenaRefeitorio", () =>
        {
            Transform t = new GameObject("CenaRefeitorio").transform;
            Caixa(t, "Tampo", 0f, 0.72f, 0.1f, 2.5f, 0.06f, 0.7f, "Madeira", true);
            for (int i = 0; i < 4; i++) Caixa(t, "Pe", i % 2 == 0 ? -1.1f : 1.1f, 0.35f, i < 2 ? -0.15f : 0.35f, 0.08f, 0.7f, 0.08f, "Madeira");
            Caixa(t, "Banco", 0f, 0.4f, -0.5f, 2.5f, 0.06f, 0.25f, "Madeira", true);
            Caixa(t, "Banco", 0f, 0.4f, 0.7f, 2.5f, 0.06f, 0.25f, "Madeira", true);
            for (int i = 0; i < 4; i++) Caixa(t, "Pe", i % 2 == 0 ? -1.1f : 1.1f, 0.2f, i < 2 ? -0.5f : 0.7f, 0.08f, 0.4f, 0.2f, "Madeira");
            for (int i = 0; i < 23; i++)
                Marmita(t, -1.1f + (i / 2) * 0.2f, 0.75f, i % 2 == 0 ? -0.08f : 0.28f, (i * 37) % 25 - 12);
            return t.gameObject;
        });
    }

    // Galerias, fita 6: a mesa do capataz, com o gravador e a folha de ponto da noite em que os Batista sumiram.
    static GameObject CenaDoCapataz()
    {
        return Montar.Prefab("CenaCapataz", () =>
        {
            Transform t = new GameObject("CenaCapataz").transform;
            Caixa(t, "Mesa", 0f, 0.4f, -0.2f, 1.3f, 0.8f, 0.7f, "Madeira", true);
            Caixa(t, "Banquinho", 0.1f, 0.22f, 0.6f, 0.4f, 0.44f, 0.4f, "Madeira", true);
            Caixa(t, "Gravador", -0.35f, 0.86f, -0.25f, 0.34f, 0.12f, 0.24f, "Metal");
            Rolo(t, "Caneca", 0.5f, 0.85f, -0.35f, 0.09f, 0.1f, "Metal");
            Bilhete(t, new Vector3(0.12f, 0.806f, -0.1f), -12f, "Folha de ponto · turno da noite · 20 de março de 1983",
                "Desceram 23. Subiram 21. Faltam Batista, A. e Batista, J. As duas chapas continuam no quadro. Avisei a companhia. Mandaram anotar: desertores. Não anotei. — Tonho");
            return t.gameObject;
        });
    }

    // Galerias, fita 6: o que os irmãos Batista deixaram.
    static GameObject CenaDasBotas()
    {
        return Montar.Prefab("CenaBotas", () =>
        {
            Transform t = new GameObject("CenaBotas").transform;
            Caixa(t, "Banco", 0f, 0.4f, -0.3f, 1.4f, 0.06f, 0.3f, "Madeira", true);
            Caixa(t, "Pe", -0.6f, 0.2f, -0.3f, 0.08f, 0.4f, 0.26f, "Madeira");
            Caixa(t, "Pe", 0.6f, 0.2f, -0.3f, 0.08f, 0.4f, 0.26f, "Madeira");
            Marmita(t, -0.35f, 0.43f, -0.3f, 8f);
            Marmita(t, 0.3f, 0.43f, -0.28f, -15f);
            float[] xs = { -0.5f, -0.33f, 0.22f };
            foreach (float x in xs) Caixa(t, "Bota", x, 0.11f, 0.15f, 0.11f, 0.22f, 0.27f, "Escuro");
            Caixa(t, "Bota", 0.5f, 0.055f, 0.3f, 0.22f, 0.11f, 0.27f, "Escuro").transform.localRotation = Quaternion.Euler(0f, 35f, 0f); // uma tombou
            return t.gameObject;
        });
    }

    // Etapa 113 e fita 1: o quadro de chapas da boca da mina. As 23 chapas ficaram: ninguém subiu para tirar a sua.
    // A número 7, do Joaquim, caiu do prego.
    internal static GameObject QuadroDeChapas()
    {
        return Montar.Prefab("QuadroDeChapas", () =>
        {
            Transform t = new GameObject("QuadroDeChapas").transform;
            Caixa(t, "Poste", -1f, 1f, -0.5f, 0.1f, 2f, 0.1f, "Madeira", true);
            Caixa(t, "Poste", 1f, 1f, -0.5f, 0.1f, 2f, 0.1f, "Madeira", true);
            Caixa(t, "Quadro", 0f, 1.3f, -0.5f, 2.2f, 1.1f, 0.06f, "Madeira", true);
            GameObject titulo = Montar.Peca("Titulo", PrimitiveType.Quad, t, new Vector3(0f, 1.7f, -0.465f), new Vector3(1.5f, 0.26f, 1f), Mat("Placa3"), false);
            titulo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // o Quad mostra a frente para quem olha no sentido do eixo Z
            for (int i = 0; i < 23; i++)
            {
                bool caida = i == 6;
                Vector3 lugar = caida ? new Vector3(0.45f, 0.012f, 0.15f) : new Vector3(-0.875f + (i % 8) * 0.25f, 1.42f - (i / 8) * 0.24f, -0.46f);
                GameObject chapa = Montar.Peca("Chapa", PrimitiveType.Cylinder, t, lugar, new Vector3(0.1f, 0.008f, 0.1f), Mat("Latao"), false);
                if (!caida) chapa.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            return t.gameObject;
        });
    }

    // Cavernas, fitas 7 e 14: o nicho de Santa Bárbara, padroeira dos mineiros, com as velas ainda acesas.
    static GameObject CenaDaCapela()
    {
        return Montar.Prefab("CenaCapela", () =>
        {
            Transform t = new GameObject("CenaCapela").transform;
            Caixa(t, "Base", 0f, 0.4f, -0.3f, 0.9f, 0.8f, 0.45f, "Madeira", true);
            Caixa(t, "Fundo", 0f, 1.25f, -0.5f, 0.9f, 0.9f, 0.05f, "Madeira");
            Caixa(t, "Lado", -0.43f, 1.25f, -0.3f, 0.05f, 0.9f, 0.45f, "Madeira");
            Caixa(t, "Lado", 0.43f, 1.25f, -0.3f, 0.05f, 0.9f, 0.45f, "Madeira");
            Caixa(t, "Telhado", -0.25f, 1.82f, -0.3f, 0.62f, 0.05f, 0.55f, "Madeira").transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            Caixa(t, "Telhado", 0.25f, 1.82f, -0.3f, 0.62f, 0.05f, 0.55f, "Madeira").transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            // A santa: manto vermelho, e a torre que sempre a acompanha.
            Montar.Modelo("Manto", MontarArte.Cone(), t, new Vector3(0f, 1.02f, -0.35f), new Vector3(0.24f, 0.44f, 0.24f), Mat("Manto"));
            Montar.Peca("Rosto", PrimitiveType.Sphere, t, new Vector3(0f, 1.27f, -0.35f), Vector3.one * 0.1f, Mat("Papel"), false);
            Caixa(t, "Torre", 0.2f, 0.92f, -0.33f, 0.09f, 0.24f, 0.09f, "Rocha");
            Vela(t, -0.25f, 0.8f, -0.2f, true);
            Vela(t, 0.32f, 0.8f, -0.15f, true);
            Vela(t, -0.05f, 0.8f, -0.12f, false);
            Vela(t, -0.6f, 0f, 0.1f, true);
            Vela(t, 0.62f, 0f, 0.05f, false);
            LuzViva chama = t.gameObject.AddComponent<LuzViva>();
            chama.modo = LuzViva.Modo.Pulsar;
            chama.ritmo = 2.5f;
            chama.luz = Montar.Luz("LuzDasVelas", t, new Vector3(0f, 1.1f, 0.25f), LightType.Point, new Color(1f, 0.7f, 0.4f), 2.2f, 6f);
            return t.gameObject;
        });
    }

    // Cavernas, fitas 7 e 9: o Chico, de ouvido colado no veio, com a marmita fechada do lado. Não se mexe mais.
    static GameObject CenaDoChico()
    {
        return Montar.Prefab("CenaChico", () =>
        {
            Transform t = new GameObject("CenaChico").transform;
            Caixa(t, "Veio", 0f, 1.1f, -0.55f, 1.5f, 2.2f, 0.5f, "RochaNegra", true);
            Montar.Modelo("Pedra", MontarArte.Pedra(2), t, new Vector3(-0.5f, 0.5f, -0.45f), new Vector3(1.3f, 1.2f, 0.8f), Mat("RochaNegra"));
            Montar.Modelo("Pedra", MontarArte.Pedra(4), t, new Vector3(0.6f, 1.7f, -0.5f), new Vector3(1.1f, 1.1f, 0.7f), Mat("RochaNegra"));
            Transform chico = Montar.Instalar("Ouvinte", t, true);
            chico.localPosition = new Vector3(0.1f, 0f, 0.2f);
            chico.localRotation = Quaternion.Euler(0f, 180f, 0f);
            Achar(chico, "Cabeca").localRotation = Quaternion.Euler(0f, 70f, 12f);
            Achar(chico, "Braco_1").localRotation = Quaternion.Euler(-65f, 0f, 0f);
            Achar(chico, "Antebraco_1").localRotation = Quaternion.Euler(-60f, 0f, 0f);
            Achar(chico, "Braco_2").localRotation = Quaternion.Euler(-45f, 0f, 0f);
            Achar(chico, "Antebraco_2").localRotation = Quaternion.Euler(-75f, 0f, 0f);
            CapsuleCollider colisao = t.gameObject.AddComponent<CapsuleCollider>();
            colisao.center = new Vector3(0.1f, 0.9f, 0.2f);
            colisao.radius = 0.3f;
            colisao.height = 1.8f;
            Caixa(t, "Pedra", 0.75f, 0.12f, 0.35f, 0.4f, 0.24f, 0.35f, "Rocha");
            Marmita(t, 0.75f, 0.24f, 0.35f, 20f);
            return t.gameObject;
        });
    }

    // Cavernas, fita 8: o cofre das amostras, arrombado por dentro pela pedra que cresceu, e o telegrama da diretoria.
    static GameObject CenaDoCofre()
    {
        return Montar.Prefab("CenaCofre", () =>
        {
            Transform t = new GameObject("CenaCofre").transform;
            Caixa(t, "Cofre", 0f, 0.5f, -0.35f, 0.8f, 1f, 0.7f, "Metal", true);
            Caixa(t, "Porta", 0.55f, 0.5f, 0.2f, 0.05f, 0.9f, 0.7f, "Metal", true).transform.localRotation = Quaternion.Euler(0f, -35f, 0f);
            Montar.Modelo("Amostra", MontarArte.Pedra(1), t, new Vector3(0f, 0.75f, 0.05f), new Vector3(0.9f, 1f, 0.8f), Mat("RochaNegra"));
            Montar.Modelo("Amostra", MontarArte.Pedra(3), t, new Vector3(-0.35f, 0.3f, 0.4f), new Vector3(0.6f, 0.6f, 0.6f), Mat("RochaNegra"));
            Montar.Modelo("Amostra", MontarArte.Pedra(5), t, new Vector3(0.2f, 1.25f, -0.2f), new Vector3(0.5f, 0.6f, 0.5f), Mat("RochaNegra"));
            for (int i = 0; i < 3; i++)
            {
                GameObject cristal = Montar.Modelo("Cristal", MontarArte.Cristal(), t, new Vector3(-0.25f + i * 0.22f, 1.1f + i * 0.12f, 0.2f - i * 0.15f), new Vector3(0.1f, 0.26f, 0.1f), Mat("Cristal"));
                cristal.transform.localRotation = Quaternion.Euler(-30f + i * 25f, 0f, 30f - i * 28f);
            }
            Montar.Luz("Brilho", t, new Vector3(0f, 1.2f, 0.5f), LightType.Point, new Color(0.3f, 1f, 0.5f), 0.9f, 3.5f);
            Bilhete(t, new Vector3(-0.75f, 0.01f, 0.7f), 25f, "Telegrama · Belo Horizonte · 31 de março de 1983",
                "AO ENGENHEIRO RESPONSÁVEL GALERIA 7 PT GEÓLOGO NEGADO PT AUMENTEM A PRODUÇÃO PT DIRETORIA");
            return t.gameObject;
        });
    }

    // Veio, fitas 14 e 19: a dinamite do paiol, que os catorze trouxeram até aqui, com o pavio estendido.
    static GameObject CenaDaDinamite()
    {
        return Montar.Prefab("CenaDinamite", () =>
        {
            Transform t = new GameObject("CenaDinamite").transform;
            Caixa(t, "Caixote", -0.35f, 0.2f, -0.3f, 0.7f, 0.4f, 0.5f, "Madeira", true);
            Caixa(t, "Caixote", 0.4f, 0.2f, -0.35f, 0.7f, 0.4f, 0.5f, "Madeira", true).transform.localRotation = Quaternion.Euler(0f, 12f, 0f);
            Caixa(t, "Caixote", -0.3f, 0.6f, -0.32f, 0.7f, 0.4f, 0.5f, "Madeira").transform.localRotation = Quaternion.Euler(0f, -8f, 0f);
            for (int i = 0; i < 6; i++)
            {
                GameObject banana = Rolo(t, "Dinamite", 0.2f + (i % 3) * 0.06f, 0.43f + (i / 3) * 0.05f, -0.35f, 0.05f, 0.24f, "Dinamite");
                banana.transform.localRotation = Quaternion.Euler(90f, 12f + i * 3f, 0f);
            }
            // O pavio sai do caixote e some na direção do meio da sala.
            Caixa(t, "Pavio", 0.45f, 0.015f, 0.4f, 0.025f, 0.02f, 1.1f, "Escuro").transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
            Caixa(t, "Pavio", 0.5f, 0.015f, 1.4f, 0.025f, 0.02f, 1.1f, "Escuro").transform.localRotation = Quaternion.Euler(0f, -25f, 0f);
            Bilhete(t, new Vector3(-0.3f, 0.806f, -0.3f), 10f, "Bilhete preso no caixote",
                "Quem achar isto: o pavio é de três minutos. Dá tempo de chegar na gaiola se não olhar pra trás. Santa Bárbara que nos valha. — Zé Lourenço");
            return t.gameObject;
        });
    }

    // Veio, fita 17: os cinco que o Tonho perdeu na descida. Cinco capacetes e uma cruz de tábua.
    static GameObject CenaDosCinco()
    {
        return Montar.Prefab("CenaCinco", () =>
        {
            Transform t = new GameObject("CenaCinco").transform;
            Caixa(t, "Cruz", 0f, 0.65f, -0.35f, 0.1f, 1.3f, 0.06f, "Madeira", true);
            Caixa(t, "Cruz", 0f, 0.98f, -0.35f, 0.7f, 0.1f, 0.06f, "Madeira");
            for (int i = 0; i < 5; i++)
                CapaceteSolto(t, new Vector3(-0.84f + i * 0.42f, 0.03f, 0.1f + (i % 2) * 0.08f), Quaternion.Euler(0f, i * 50f, 0f));
            Vela(t, -0.2f, 0f, -0.15f, false);
            Vela(t, 0.22f, 0f, -0.18f, false);
            return t.gameObject;
        });
    }

    static Transform[] Pecas(Transform corpo, string prefixo, int quantas)
    {
        Transform[] pecas = new Transform[quantas];
        for (int i = 0; i < quantas; i++)
        {
            pecas[i] = Achar(corpo, prefixo + (i + 1));
            if (pecas[i] == null) Debug.LogError("O modelo " + corpo.parent.name + " não tem a peça " + prefixo + (i + 1) + ".");
        }
        return pecas;
    }

    static Transform Achar(Transform corpo, string nome)
    {
        foreach (Transform peca in corpo.GetComponentsInChildren<Transform>(true))
            if (peca.name == nome) return peca;
        return null;
    }

    // ---------- Minério ----------

    internal static GameObject PedraDeMinerio()
    {
        return Montar.Prefab("PedraDeMinerio", () =>
        {
            GameObject pedra = new GameObject("PedraDeMinerio");
            pedra.AddComponent<SphereCollider>().radius = 0.12f;
            pedra.AddComponent<PedraDeMinerio>(); // traz junto o Rigidbody
            Montar.Modelo("Modelo", MontarArte.Pedra(4), pedra.transform, Vector3.zero, Vector3.one * 0.28f, Mat("Minerio"));
            return pedra;
        });
    }

    // Etapa 20: um pedaço de rocha com cristais de minério saindo para todos os lados.
    // Etapas 20 e 139: três formatos de veio (redondo, deitado e alto), para dois vizinhos não serem iguais. O modelo
    // tem cristais só de um lado; por isso entram duas cópias, de costas uma para a outra: o veio fica metade dentro
    // da parede, e assim sempre há um lado com minério virado para a sala.
    internal static GameObject Veio(int formato = 1)
    {
        return Montar.Prefab(formato == 1 ? "VeioDeMinerio" : "VeioDeMinerio" + formato, () =>
        {
            GameObject veio = new GameObject("VeioDeMinerio");
            veio.AddComponent<BoxCollider>().size = formato == 2 ? new Vector3(1.8f, 1f, 1.2f) : formato == 3 ? new Vector3(0.9f, 1.8f, 1.2f) : Vector3.one * 1.2f;
            veio.AddComponent<VeioDeMinerio>().pedraPrefab = PedraDeMinerio();
            Obj(veio.transform, "Obj_Veio" + formato, 0f, 0f, 0f).transform.localScale = Vector3.one * 0.85f;
            Obj(veio.transform, "Obj_Veio" + formato, 0f, 0f, 0f, Vector3.zero, 180f).transform.localScale = Vector3.one * 0.85f;
            return veio;
        });
    }

    // ---------- Itens ----------

    // Etapa 56: kit médico, frasco de óleo e sinalizador, para pegar com E.
    internal static GameObject[] Itens()
    {
        return new[]
        {
            CriarItem("KitMedico", global::Item.Tipo.KitMedico, "Obj_KitMedico", new Vector3(0.4f, 0.26f, 0.3f)),
            CriarItem("FrascoDeOleo", global::Item.Tipo.Oleo, "Obj_FrascoDeOleo", new Vector3(0.22f, 0.34f, 0.22f)),
            CriarItem("SinalizadorNoChao", global::Item.Tipo.Sinalizador, "Obj_Sinalizador", new Vector3(0.14f, 0.36f, 0.14f)),
        };
    }

    // Etapa 138: cada item tem o seu modelo (a caixa branca com a cruz, o frasco âmbar, o bastão vermelho), e uma
    // caixa de colisão um pouco maior que ele, para ser fácil de mirar.
    static GameObject CriarItem(string nome, Item.Tipo tipo, string modelo, Vector3 colisao)
    {
        return Montar.Prefab(nome, () =>
        {
            GameObject objeto = new GameObject(nome);
            Item item = objeto.AddComponent<Item>();
            item.tipo = tipo;
            item.som = Som("item");
            Obj(objeto.transform, modelo, 0f, 0f, 0f, colisao);
            return objeto;
        });
    }

    // Etapa 62: a fita cassete. Brilha de leve, para dar para achar no escuro.
    internal static GameObject FitaCassete()
    {
        return Montar.Prefab("Fita", () =>
        {
            GameObject objeto = new GameObject("Fita");
            Fita fita = objeto.AddComponent<Fita>();
            fita.aviso = "E: ouvir a fita";
            fita.som = Som("fita");
            Obj(objeto.transform, "Obj_FitaCassete", 0f, 0f, 0f, new Vector3(0.3f, 0.1f, 0.2f));
            return objeto;
        });
    }

    // Etapa 56: o sinalizador aceso que o jogador joga com Q.
    internal static GameObject SinalizadorAceso()
    {
        return Montar.Prefab("SinalizadorAceso", () =>
        {
            GameObject aceso = Montar.Peca("SinalizadorAceso", PrimitiveType.Cylinder, null, Vector3.zero, new Vector3(0.07f, 0.15f, 0.07f), Mat("Sinal"), true);
            aceso.AddComponent<Rigidbody>().mass = 0.3f;
            aceso.AddComponent<Sinalizador>();
            Montar.Luz("Luz", aceso.transform, Vector3.zero, LightType.Point, new Color(1f, 0.25f, 0.15f), 8f, 12f);
            Montar.Fonte(aceso, Som("sinalizador"), true, 14f).volume = 0.5f;
            return aceso;
        });
    }

    // ---------- Perigos ----------

    // Etapa 55: área de 3 x 3 m que solta poeira por um tempo e depois deixa cair pedras.
    internal static GameObject PerigoDesmoronamento()
    {
        return Montar.Prefab("Desmoronamento", () =>
        {
            GameObject perigo = new GameObject("Desmoronamento");
            BoxCollider area = perigo.AddComponent<BoxCollider>();
            area.isTrigger = true;
            area.center = new Vector3(0f, 1.5f, 0f);
            area.size = new Vector3(3f, 3f, 3f);
            Montar.Fonte(perigo, null, false, 20f);
            Desmoronamento desmoronamento = perigo.AddComponent<Desmoronamento>();
            desmoronamento.entulho = Entulho();
            desmoronamento.somDeEstalo = Som("estalo");
            desmoronamento.somDeQueda = Som("queda");
            GameObject poeira = new GameObject("Poeira");
            poeira.transform.SetParent(perigo.transform, false);
            poeira.transform.localPosition = new Vector3(0f, 2.9f, 0f);
            desmoronamento.poeira = Particulas(poeira, new Vector3(3f, 0.1f, 3f), new Color(0.6f, 0.55f, 0.5f, 0.7f), 0.08f, 1.5f, 0.4f, 80f, false, false);
            return perigo;
        });
    }

    // Etapa 55: névoa verde que apaga o lampião de quem entra nela.
    internal static GameObject PerigoGas()
    {
        return Montar.Prefab("BolsaoDeGas", () =>
        {
            GameObject gas = new GameObject("BolsaoDeGas");
            BoxCollider area = gas.AddComponent<BoxCollider>();
            area.isTrigger = true;
            area.center = new Vector3(0f, 1.2f, 0f);
            area.size = new Vector3(3f, 2.4f, 3f);
            gas.AddComponent<Gas>();
            Montar.Fonte(gas, Som("chiado_de_gas"), true, 8f).volume = 0.4f;
            GameObject nevoa = new GameObject("Nevoa");
            nevoa.transform.SetParent(gas.transform, false);
            nevoa.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            Particulas(nevoa, new Vector3(3f, 1.2f, 3f), new Color(0.45f, 0.8f, 0.25f, 0.18f), 0.7f, 4f, -0.02f, 18f, true, false);
            return gas;
        });
    }

    static GameObject Entulho()
    {
        return Montar.Prefab("Entulho", () =>
        {
            GameObject pedra = new GameObject("Entulho");
            pedra.AddComponent<SphereCollider>().radius = 0.16f;
            pedra.AddComponent<Rigidbody>();
            Montar.Modelo("Modelo", MontarArte.Pedra(2), pedra.transform, Vector3.zero, Vector3.one * 0.4f, Mat("Rocha"));
            return pedra;
        });
    }

    // ---------- Partículas ----------

    // Põe em "objeto" um sistema de partículas que nasce dentro de uma caixa.
    // sempre: fica soltando partículas sem parar. noMundo: as partículas ficam onde nasceram, mesmo se o objeto andar.
    static ParticleSystem Particulas(GameObject objeto, Vector3 caixa, Color cor, float tamanho, float vida, float gravidade, float porSegundo, bool sempre, bool noMundo)
    {
        ParticleSystem particulas = objeto.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // a duração só muda com o sistema parado
        ParticleSystem.MainModule principal = particulas.main;
        principal.loop = sempre;
        principal.playOnAwake = sempre;
        principal.duration = sempre ? 5f : 1.5f;
        principal.startLifetime = vida;
        principal.startSize = tamanho;
        principal.startSpeed = 0.05f;
        principal.startColor = cor;
        principal.gravityModifier = gravidade;
        principal.maxParticles = 300;
        principal.simulationSpace = noMundo ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.rateOverTime = porSegundo;
        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Box;
        forma.scale = caixa;
        forma.randomDirectionAmount = 1f;
        // O material de partículas que vem com o URP: macio e sem precisar de luz, então aparece no escuro.
        Material material = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
        if (material != null) objeto.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        else Debug.LogWarning("Não achei o material de partículas do URP; as partículas podem aparecer cor-de-rosa.");
        return particulas;
    }

    // Etapa 75: a poeira que flutua na frente do lampião. O script Lampiao acende e apaga ela junto com a luz.
    internal static ParticleSystemRenderer Poeira(Transform lampiao)
    {
        GameObject poeira = new GameObject("Poeira");
        poeira.transform.SetParent(lampiao, false);
        poeira.transform.localPosition = new Vector3(0.3f, 0.2f, 3.5f);
        Particulas(poeira, new Vector3(5f, 3f, 7f), new Color(1f, 0.85f, 0.6f, 0.4f), 0.03f, 6f, 0f, 25f, true, true);
        return poeira.GetComponent<ParticleSystemRenderer>();
    }

    // Etapa 81: as faíscas do golpe da picareta. Some sozinha depois de soltar as partículas.
    internal static GameObject Faisca()
    {
        return Montar.Prefab("Faisca", () =>
        {
            GameObject faisca = new GameObject("Faisca");
            ParticleSystem particulas = Particulas(faisca, Vector3.one, new Color(1f, 0.75f, 0.3f), 0.05f, 0.25f, 1f, 0f, false, true);
            ParticleSystem.MainModule principal = particulas.main;
            principal.duration = 0.3f;
            principal.startSpeed = 3.5f;
            principal.playOnAwake = true;
            principal.stopAction = ParticleSystemStopAction.Destroy;
            ParticleSystem.EmissionModule emissao = particulas.emission;
            emissao.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)10) });
            ParticleSystem.ShapeModule forma = particulas.shape;
            forma.shapeType = ParticleSystemShapeType.Cone; // um leque que sai da parede
            forma.angle = 35f;
            forma.radius = 0.02f;
            forma.randomDirectionAmount = 0f;
            return faisca;
        });
    }

    // ---------- Enfeites dos temas ----------

    // Etapa 72: a moldura de madeira que escora cada porta das galerias.
    // Etapa 134: três escoramentos. 1: dois esteios e a travessa. 2: com mãos-francesas. 3: o mais pesado, com uma
    // escora de reforço atravessada e tábuas quebradas no pé. A colisão é sempre a dos dois esteios.
    static GameObject Portal(int variacao = 1)
    {
        return Montar.Prefab(variacao == 1 ? "Portal" : "Portal" + variacao, () =>
        {
            GameObject portal = new GameObject("Portal");
            foreach (float x in new[] { -1.3f, 1.3f })
            {
                BoxCollider esteio = portal.AddComponent<BoxCollider>();
                esteio.center = new Vector3(x, 1.5f, 0f);
                esteio.size = new Vector3(0.25f, 3f, 0.25f);
            }
            Obj(portal.transform, "Kit_Portal" + variacao, 0f, 0f, 0f).transform.localScale = new Vector3(1f, 1f, 2f); // mais grosso: no pacote a moldura é fina
            if (variacao == 3)
            {
                GameObject escora = Obj(portal.transform, "Kit_Viga", 0.95f, 0f, 0.35f);
                escora.transform.localRotation = Quaternion.Euler(-14f, 0f, 9f);
                Obj(portal.transform, "Kit_Tabuas", -0.9f, 0.01f, 0.7f, Vector3.zero, 35f);
            }
            return portal;
        });
    }

    // Etapa 134: uma escora que caiu, atravessada no chão, com um monte de pedra do lado.
    static GameObject EscoraCaida()
    {
        return Montar.Prefab("EscoraCaida", () =>
        {
            GameObject raiz = new GameObject("EscoraCaida");
            BoxCollider colisao = raiz.AddComponent<BoxCollider>();
            colisao.center = new Vector3(0f, 0.15f, 0.2f);
            colisao.size = new Vector3(2.6f, 0.3f, 0.5f);
            GameObject escora = Obj(raiz.transform, "Kit_Escora", -1.3f, 0.12f, 0.2f);
            escora.transform.localRotation = Quaternion.Euler(0f, 8f, -84f);
            Obj(raiz.transform, "Kit_Pedra2", 0.9f, 0f, -0.2f, Vector3.zero, 40f);
            Obj(raiz.transform, "Kit_Pedra4", 0.3f, 0f, 0.6f);
            Obj(raiz.transform, "Kit_Tabuas", -0.3f, 0.01f, -0.3f, Vector3.zero, 70f);
            return raiz;
        });
    }

    // Etapa 136: a caixa de força das lâmpadas da companhia: caixa de ferro com a chave para baixo, o cabo subindo
    // pelo poste e uma arandela em cima, com a luz de emergência falhando.
    static GameObject CaixaDeForca()
    {
        return Montar.Prefab("CaixaDeForca", () =>
        {
            GameObject raiz = new GameObject("CaixaDeForca");
            Transform t = raiz.transform;
            Caixa(t, "Poste", 0f, 1.2f, -0.3f, 0.14f, 2.4f, 0.14f, "Madeira", true);
            Caixa(t, "Caixa", 0f, 1.3f, -0.18f, 0.4f, 0.55f, 0.14f, "Metal");
            Caixa(t, "Chave", 0.1f, 1.25f, -0.09f, 0.04f, 0.18f, 0.04f, "Escuro").transform.localRotation = Quaternion.Euler(40f, 0f, 0f);
            Caixa(t, "Cabo", -0.05f, 1.95f, -0.22f, 0.03f, 0.8f, 0.03f, "Escuro");
            Obj(t, "Kit_Arandela", 0f, 2.3f, -0.18f);
            LuzViva viva = raiz.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Piscar;
            viva.ritmo = 0.3f;
            viva.luz = Montar.Luz("Luz", t, new Vector3(0f, 2.3f, 0.1f), LightType.Point, new Color(1f, 0.6f, 0.3f), 1.6f, 5f);
            return raiz;
        });
    }

    // Etapa 137: o canto de um mineiro: banquinho, garrafa térmica, rádio de pilha, marmita e caneca.
    static GameObject CantoDoMineiro()
    {
        return Montar.Prefab("CantoDoMineiro", () =>
        {
            GameObject raiz = new GameObject("CantoDoMineiro");
            Transform t = raiz.transform;
            Caixa(t, "Banquinho", 0.3f, 0.22f, 0.1f, 0.4f, 0.44f, 0.4f, "Madeira", true);
            Caixa(t, "CaixoteBaixo", -0.4f, 0.2f, -0.2f, 0.6f, 0.4f, 0.5f, "Madeira", true);
            Obj(t, "Obj_Radio", -0.5f, 0.4f, -0.2f, Vector3.zero, 15f);
            Obj(t, "Obj_GarrafaTermica", -0.2f, 0.4f, -0.3f);
            Rolo(t, "Caneca", -0.22f, 0.45f, -0.05f, 0.09f, 0.1f, "Metal");
            Marmita(t, 0.3f, 0.44f, 0.1f, 20f);
            Obj(t, "Obj_Bota", 0.75f, 0f, -0.25f, Vector3.zero, -30f);
            return raiz;
        });
    }

    // Etapa 137: as ferramentas encostadas num cavalete: pá, alavanca de choco e picareta, com o balde e a caixa no chão.
    static GameObject Ferramentas()
    {
        return Montar.Prefab("Ferramentas", () =>
        {
            GameObject raiz = new GameObject("Ferramentas");
            Transform t = raiz.transform;
            Caixa(t, "Cavalete", 0f, 1.1f, -0.45f, 1.6f, 0.08f, 0.08f, "Madeira");
            Caixa(t, "Pe", -0.75f, 0.55f, -0.45f, 0.08f, 1.1f, 0.08f, "Madeira", true);
            Caixa(t, "Pe", 0.75f, 0.55f, -0.45f, 0.08f, 1.1f, 0.08f, "Madeira", true);
            Obj(t, "Obj_Pa", -0.45f, 0f, -0.15f).transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            Obj(t, "Obj_Alavanca", -0.05f, 0f, -0.05f).transform.localRotation = Quaternion.Euler(-12f, 0f, 3f);
            Obj(t, "Obj_Picareta", 0.4f, 0f, -0.2f).transform.localRotation = Quaternion.Euler(-14f, 20f, 0f);
            Obj(t, "Obj_Balde", 0.8f, 0f, 0.3f);
            Obj(t, "Obj_CaixaDeFerramentas", -0.6f, 0f, 0.35f, new Vector3(0.45f, 0.22f, 0.2f), 12f);
            return raiz;
        });
    }

    // Etapa 137: o quadro de avisos da companhia, com o cartaz de segurança e o calendário parado em maio de 1983.
    static GameObject QuadroDeAvisos()
    {
        return Montar.Prefab("QuadroDeAvisos", () =>
        {
            GameObject raiz = new GameObject("QuadroDeAvisos");
            Transform t = raiz.transform;
            Caixa(t, "Poste", -0.7f, 1f, -0.5f, 0.1f, 2f, 0.1f, "Madeira", true);
            Caixa(t, "Poste", 0.7f, 1f, -0.5f, 0.1f, 2f, 0.1f, "Madeira", true);
            Caixa(t, "Quadro", 0f, 1.45f, -0.5f, 1.5f, 0.9f, 0.06f, "Madeira", true);
            // O Quad mostra a frente para quem olha no sentido do eixo Z; aqui quem olha vem do outro lado.
            Montar.Peca("Cartaz", PrimitiveType.Quad, t, new Vector3(-0.3f, 1.45f, -0.465f), new Vector3(0.75f, 0.62f, 1f), Mat("Cartaz"), false).transform.localRotation = Quaternion.Euler(0f, 180f, 2f);
            Montar.Peca("Calendario", PrimitiveType.Quad, t, new Vector3(0.42f, 1.5f, -0.465f), new Vector3(0.45f, 0.4f, 1f), Mat("Calendario"), false).transform.localRotation = Quaternion.Euler(0f, 180f, -4f);
            return raiz;
        });
    }

    // Etapa 135: meio trilho de sala, do meio até a porta (7,5 m), com os dormentes. São só duas malhas. O gerador
    // põe um destes para cada porta por onde passa o caminho principal do andar.
    static GameObject Trilho()
    {
        return Montar.Prefab("Trilho", () =>
        {
            GameObject raiz = new GameObject("Trilho");
            List<Vector3> ferro = new List<Vector3>(), madeira = new List<Vector3>();
            foreach (float x in new[] { -0.3f, 0.3f }) BlocoDeMalha(ferro, new Vector3(x, 0.045f, 3.75f), new Vector3(0.06f, 0.07f, 7.5f));
            BlocoDeMalha(ferro, new Vector3(0f, 0.012f, 0f), new Vector3(1.1f, 0.02f, 1.1f)); // a chapa do meio, onde os trilhos se cruzam ou viram
            for (int i = 0; i < 11; i++) BlocoDeMalha(madeira, new Vector3(0f, 0.012f, 0.9f + i * 0.62f), new Vector3(0.95f, 0.025f, 0.2f));
            Montar.Modelo("Ferro", MontarArte.Facetada("Trilho_Ferro", ferro), raiz.transform, Vector3.zero, Vector3.one, Mat("Metal"));
            Montar.Modelo("Dormentes", MontarArte.Facetada("Trilho_Dormentes", madeira), raiz.transform, Vector3.zero, Vector3.one, Mat("Madeira"));
            return raiz;
        });
    }

    // Os 12 triângulos de um bloco, virados para fora.
    static void BlocoDeMalha(List<Vector3> triangulos, Vector3 centro, Vector3 tamanho)
    {
        foreach (Vector3 normal in new[] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down })
        {
            Vector3 u = normal.y != 0f ? Vector3.right : Vector3.Cross(Vector3.up, normal);
            Vector3 w = Vector3.Cross(normal, u);
            System.Func<Vector3, Vector3> canto = c => centro + Vector3.Scale(c, tamanho) * 0.5f;
            Vector3 a = canto(normal - u - w), b = canto(normal + u - w), c2 = canto(normal + u + w), d = canto(normal - u + w);
            triangulos.AddRange(new[] { a, b, c2, a, c2, d });
        }
    }

    static GameObject Caixote()
    {
        return Montar.Prefab("Caixote", () =>
        {
            GameObject caixote = new GameObject("Caixote");
            GameObject caixa = Montar.Peca("Caixa", PrimitiveType.Cube, caixote.transform, new Vector3(0f, 0.4f, 0f), Vector3.one * 0.8f, Mat("Madeira"), true);
            caixa.transform.localRotation = Quaternion.Euler(0f, 15f, 0f);
            return caixote;
        });
    }

    static GameObject Barril()
    {
        return Montar.Prefab("Barril", () =>
        {
            GameObject barril = new GameObject("Barril");
            Obj(barril.transform, "Obj_Barril", 0f, 0f, 0f, new Vector3(0.6f, 0.9f, 0.6f));
            return barril;
        });
    }

    // Etapa 135: o carrinho de mina do pacote, num pedaço de trilho ao longo da parede. Três estados: cheio de minério,
    // vazio, e tombado com a carga espalhada.
    static GameObject Carrinho(string estado = "Cheio")
    {
        return Montar.Prefab(estado == "Cheio" ? "Carrinho" : "Carrinho" + estado, () =>
        {
            GameObject carrinho = new GameObject("Carrinho");
            Transform t = carrinho.transform;
            Material metal = Mat("Metal");
            Montar.Peca("Trilho", PrimitiveType.Cube, t, new Vector3(0f, 0.04f, -0.3f), new Vector3(2.6f, 0.06f, 0.06f), metal, false);
            Montar.Peca("Trilho", PrimitiveType.Cube, t, new Vector3(0f, 0.04f, 0.3f), new Vector3(2.6f, 0.06f, 0.06f), metal, false);
            for (int i = -2; i <= 2; i++)
                Montar.Peca("Dormente", PrimitiveType.Cube, t, new Vector3(i * 0.55f, 0.02f, 0f), new Vector3(0.15f, 0.04f, 1f), Mat("Madeira"), false);
            BoxCollider colisao = carrinho.AddComponent<BoxCollider>();
            colisao.center = new Vector3(0f, 0.45f, 0f);
            colisao.size = new Vector3(1.6f, 0.9f, 0.95f);
            // No pacote, o carrinho é comprido no eixo Z; aqui ele fica ao longo do trilho, que corre no eixo X.
            GameObject modelo = Obj(t, "Kit_Carrinho", 0f, 0.07f, 0f, Vector3.zero, 90f);
            if (estado == "Tombado")
            {
                // Tombado para o lado da sala, mas sem passar de 90 cm do ponto: num corredor de 3 m tem de sobrar passagem.
                modelo.transform.localPosition = new Vector3(0.1f, 0.5f, 0.35f);
                modelo.transform.localRotation = Quaternion.Euler(80f, 90f, 0f);
                colisao.center = new Vector3(0.1f, 0.4f, 0.4f);
                colisao.size = new Vector3(1.6f, 0.8f, 0.9f);
                for (int i = 0; i < 4; i++)
                    Montar.Modelo("Carga", MontarArte.Pedra(2 + i), t, new Vector3(-0.5f + i * 0.4f, 0.1f, 0.95f + (i % 2) * 0.25f), Vector3.one * (0.24f + 0.06f * (i % 3)), Mat("Minerio"));
            }
            else if (estado == "Cheio")
            {
                Montar.Modelo("Carga", MontarArte.Pedra(4), t, new Vector3(-0.25f, 0.78f, 0.05f), new Vector3(0.6f, 0.4f, 0.6f), Mat("Minerio"));
                Montar.Modelo("Carga", MontarArte.Pedra(5), t, new Vector3(0.25f, 0.76f, -0.08f), new Vector3(0.55f, 0.35f, 0.55f), Mat("Minerio"));
            }
            return carrinho;
        });
    }

    // Etapa 75: um lampião velho pendurado num poste, com a luz falhando.
    static GameObject LampiaoDePoste()
    {
        return Montar.Prefab("LampiaoDePoste", () =>
        {
            GameObject poste = new GameObject("LampiaoDePoste");
            Transform t = poste.transform;
            Montar.Peca("Poste", PrimitiveType.Cube, t, new Vector3(0f, 1.1f, 0f), new Vector3(0.12f, 2.2f, 0.12f), Mat("Madeira"), true);
            Montar.Peca("Braco", PrimitiveType.Cube, t, new Vector3(0f, 2.1f, 0.22f), new Vector3(0.08f, 0.08f, 0.5f), Mat("Madeira"), false);
            Obj(t, "Kit_Lampiao", 0f, 2.06f, 0.42f).transform.localScale = Vector3.one * 0.6f; // etapa 136: o lampião do pacote, pendurado pelo topo
            LuzViva viva = poste.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Piscar;
            viva.luz = Montar.Luz("Luz", t, new Vector3(0f, 1.9f, 0.42f), LightType.Point, new Color(1f, 0.75f, 0.45f), 2.5f, 7f);
            return poste;
        });
    }

    static GameObject PilhaDePedras()
    {
        return Montar.Prefab("PilhaDePedras", () =>
        {
            GameObject pilha = new GameObject("PilhaDePedras");
            BoxCollider colisao = pilha.AddComponent<BoxCollider>();
            colisao.center = new Vector3(0f, 0.3f, 0f);
            colisao.size = new Vector3(1.2f, 0.6f, 1f);
            // Etapa 133: o entulho do pacote: um monte e duas pedras soltas.
            Obj(pilha.transform, "Kit_Pedras1", 0f, 0f, 0f, Vector3.zero, 20f);
            Obj(pilha.transform, "Kit_Pedra1", 0.1f, 0.25f, -0.05f, Vector3.zero, 75f);
            Obj(pilha.transform, "Kit_Pedra4", -0.75f, 0f, 0.45f);
            return pilha;
        });
    }

    // Estalagmites no chão e estalactites no teto.
    static GameObject Estalagmites()
    {
        return Montar.Prefab("Estalagmites", () =>
        {
            GameObject grupo = new GameObject("Estalagmites");
            CapsuleCollider colisao = grupo.AddComponent<CapsuleCollider>();
            colisao.center = new Vector3(0f, 0.7f, 0f);
            colisao.radius = 0.35f;
            colisao.height = 1.4f;
            // Etapa 133: as estalagmites do pacote, no chão, e as mesmas de ponta-cabeça, no teto.
            Obj(grupo.transform, "Kit_Estalagmite2", 0f, 0f, 0f);
            Obj(grupo.transform, "Kit_Estalagmite1", 0.42f, 0f, 0.2f, Vector3.zero, 50f).transform.localScale = Vector3.one * 0.7f;
            Obj(grupo.transform, "Kit_Estalagmite3", -0.32f, 0f, -0.3f, Vector3.zero, 120f);
            // As estalactites ficam num objeto chamado NoTeto: o gerador sobe esse objeto até o teto da sala, que varia.
            Transform noTeto = NoTeto(grupo.transform);
            Obj(noTeto, "Kit_Estalagmite1", 0.1f, 0.05f, 0.1f).transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
            Obj(noTeto, "Kit_Estalagmite3", -0.4f, 0.05f, 0.3f).transform.localRotation = Quaternion.Euler(180f, 40f, 0f);
            return grupo;
        });
    }

    // Um cone de ponta para baixo, pendurado no teto de 3 m.
    // ---------- Marcas no chão e na água (etapas 143 e 145) ----------

    // Uma folha deitada no chão, com a textura de pé para o lado do z positivo. Sem colisão e sem sombra.
    static GameObject Decalque(string nome, string material, float largura, float comprimento)
    {
        return Montar.Prefab(nome, () =>
        {
            GameObject raiz = new GameObject(nome);
            GameObject folha = Montar.Peca("Folha", PrimitiveType.Quad, raiz.transform, Vector3.zero, new Vector3(largura, comprimento, 1f), Mat(material), false);
            folha.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // o Quad nasce de pé; assim ele deita, virado para cima
            return raiz;
        });
    }

    internal static GameObject SetaDeGiz() { return Decalque("SetaDeGiz", "SetaDeGiz", 0.8f, 1.1f); }

    internal static GameObject NumeroDeGiz() { return Decalque("NumeroDeGiz", "NumeroDeGiz", 1.5f, 0.4f); }

    internal static Texture2D[] NumerosDeGiz()
    {
        Texture2D[] numeros = new Texture2D[9];
        for (int i = 0; i < 9; i++) numeros[i] = MontarArte.Textura("Giz" + (i + 1));
        return numeros;
    }

    // Etapa 145: o anel que se abre na água a cada passo. O script Onda faz ele crescer e sumir.
    internal static GameObject AnelDeAgua()
    {
        return Montar.Prefab("Onda", () =>
        {
            GameObject raiz = new GameObject("Onda");
            GameObject folha = Montar.Peca("Folha", PrimitiveType.Quad, raiz.transform, Vector3.zero, Vector3.one, Mat("Onda"), false);
            folha.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            raiz.AddComponent<Onda>();
            return raiz;
        });
    }

    // ---------- Salas especiais (etapa 120) ----------
    // Os móveis de cada sala especial. As medidas são em metros a partir do meio da sala: x cresce para o leste e z
    // para o norte, que é por onde se entra. As paredes de dentro ficam a 4,5 m do meio.
    internal static void Mobiliar(string nome, Transform sala, float teto)
    {
        switch (nome)
        {
            case "Refeitorio":
                foreach (float x in new[] { -3.2f, 3.2f })
                {
                    float banco = x < 0f ? x + 0.75f : x - 0.75f;
                    Caixa(sala, "Mesa", x, 0.72f, -0.3f, 0.8f, 0.06f, 5f, "Madeira", true);
                    Caixa(sala, "Banco", banco, 0.4f, -0.3f, 0.28f, 0.06f, 5f, "Madeira", true);
                    foreach (float z in new[] { -2.5f, 1.9f })
                    {
                        Caixa(sala, "Pe", x, 0.35f, z, 0.6f, 0.7f, 0.08f, "Madeira");
                        Caixa(sala, "Pe", banco, 0.2f, z, 0.24f, 0.4f, 0.08f, "Madeira");
                    }
                    for (int i = 0; i < 6; i++) Marmita(sala, x + (i % 2) * 0.3f - 0.15f, 0.75f, -2.3f + i * 0.8f, (i * 31) % 40 - 20);
                    for (int i = 0; i < 3; i++) Rolo(sala, "Caneca", x + 0.2f - (i % 2) * 0.4f, 0.8f, -1.9f + i * 1.6f, 0.09f, 0.1f, "Metal");
                }
                // O fogão a lenha, com a chaminé até o teto, e a prateleira das canecas.
                Caixa(sala, "Fogao", 3.5f, 0.45f, 3.9f, 1.4f, 0.9f, 0.8f, "Metal", true);
                Rolo(sala, "Chamine", 3.9f, (0.9f + teto) / 2f, 4.1f, 0.22f, teto - 0.9f, "Metal");
                Rolo(sala, "Panela", 3.2f, 0.98f, 3.9f, 0.4f, 0.16f, "Escuro");
                Caixa(sala, "Lenha", 2.4f, 0.12f, 4.1f, 0.6f, 0.24f, 0.3f, "Madeira");
                Caixa(sala, "Prateleira", -3.6f, 1.4f, 4.25f, 1.6f, 0.05f, 0.35f, "Madeira");
                for (int i = 0; i < 5; i++) Rolo(sala, "Caneca", -4.2f + i * 0.3f, 1.48f, 4.25f, 0.09f, 0.1f, "Metal");
                // Etapa 146: uma vela acesa na ponta de uma mesa: o ponto de luz da sala.
                Vela(sala, -3.2f, 0.75f, 1.9f, true);
                Montar.Luz("LuzDaVela", sala, new Vector3(-3.1f, 1.1f, 1.9f), LightType.Point, new Color(1f, 0.7f, 0.4f), 1.6f, 5f);
                Bilhete(sala, new Vector3(3.1f, 0.756f, 1.3f), 20f, "Papel pregado na mesa",
                    "Quem comer a marmita dos Batista vai se ver comigo. Elas ficam onde estão até eles voltarem. — Tonho");
                break;

            case "Lamparia":
                // Prateleiras dos dois lados. Do lado oeste ainda há lampiões; do leste, levaram quase todos.
                foreach (float x in new[] { -4f, 4f })
                {
                    for (int andar = 0; andar < 3; andar++)
                    {
                        float y = 0.6f + andar * 0.6f;
                        Caixa(sala, "Prateleira", x, y, -0.5f, 0.5f, 0.05f, 6.5f, "Madeira");
                        for (int i = 0; i < 8; i++)
                            if (x < 0f ? (i + andar) % 4 != 0 : (i * 3 + andar) % 7 == 0)
                                Rolo(sala, "Lampiao", x, y + 0.145f, -3.3f + i * 0.8f, 0.14f, 0.24f, "Metal");
                    }
                    foreach (float z in new[] { -3.7f, -0.5f, 2.7f }) Caixa(sala, "Montante", x, 1f, z, 0.5f, 2f, 0.08f, "Madeira", true);
                }
                // A bancada do Severino, com o único lampião aceso da mina, as latas de óleo e o caderno.
                Caixa(sala, "Bancada", 0f, 0.45f, -4f, 2.4f, 0.9f, 0.8f, "Madeira", true);
                Rolo(sala, "LampiaoAceso", -0.7f, 1.04f, -4f, 0.16f, 0.28f, "Chama");
                LuzViva acesa = sala.gameObject.AddComponent<LuzViva>();
                acesa.modo = LuzViva.Modo.Pulsar;
                acesa.ritmo = 2f;
                acesa.luz = Montar.Luz("LuzDaLamparia", sala, new Vector3(-0.7f, 1.4f, -3.6f), LightType.Point, new Color(1f, 0.75f, 0.45f), 2.2f, 7f);
                for (int i = 0; i < 3; i++) Rolo(sala, "LataDeOleo", 0.3f + i * 0.3f, 1.02f, -4.1f, 0.2f, 0.24f, "Metal");
                Rolo(sala, "Tambor", 3.5f, 0.5f, -3.9f, 0.7f, 1f, "Metal");
                Caixa(sala, "Banquinho", 0.9f, 0.22f, -3f, 0.4f, 0.44f, 0.4f, "Madeira", true);
                Bilhete(sala, new Vector3(0.2f, 0.906f, -3.75f), -10f, "Caderno do lampista",
                    "Saíram 23 lampiões. Voltaram 9. Daqui pra frente só entrego com a chapa na mão. E deixo este aqui aceso, pra quem subir no escuro achar a porta. — Severino");
                break;

            case "Paiol":
                // A porta de ferro, aberta, com a placa em cima. Lá dentro, as prateleiras e os caixotes vazios.
                Caixa(sala, "Umbral", -1.3f, 1.5f, 1.6f, 0.3f, 3f, 0.3f, "Madeira", true);
                Caixa(sala, "Umbral", 1.3f, 1.5f, 1.6f, 0.3f, 3f, 0.3f, "Madeira", true);
                Caixa(sala, "Verga", 0f, 2.75f, 1.6f, 2.9f, 0.5f, 0.3f, "Madeira");
                Caixa(sala, "PortaDeFerro", -1.2f, 1.2f, 0.7f, 0.08f, 2.4f, 1.5f, "Metal", true);
                Montar.Peca("Placa", PrimitiveType.Quad, sala, new Vector3(0f, 2.75f, 1.76f), new Vector3(1.3f, 0.42f, 1f), Mat("Placa6"), false).transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                foreach (float y in new[] { 0.6f, 1.3f, 2f }) Caixa(sala, "Prateleira", 0f, y, -4.1f, 7f, 0.05f, 0.6f, "Madeira");
                foreach (float x in new[] { -3.4f, -1.1f, 1.1f, 3.4f }) Caixa(sala, "Montante", x, 1.1f, -4.1f, 0.08f, 2.2f, 0.6f, "Madeira", true);
                Caixa(sala, "CaixoteVazio", -3.3f, 0.25f, -1.2f, 0.9f, 0.5f, 0.6f, "Madeira", true);
                Caixa(sala, "Tampa", -2.7f, 0.3f, -1.2f, 0.05f, 0.6f, 0.9f, "Madeira").transform.localRotation = Quaternion.Euler(0f, 0f, -20f);
                Caixa(sala, "CaixoteVazio", 3.2f, 0.3f, -2.6f, 0.9f, 0.6f, 0.5f, "Madeira", true).transform.localRotation = Quaternion.Euler(0f, 25f, 0f);
                Caixa(sala, "CaixoteTombado", 2.6f, 0.3f, 0.3f, 0.6f, 0.9f, 0.5f, "Madeira", true).transform.localRotation = Quaternion.Euler(0f, -15f, 90f);
                Rolo(sala, "Dinamite", 2.1f, 0.03f, -0.6f, 0.05f, 0.24f, "Dinamite").transform.localRotation = Quaternion.Euler(90f, 40f, 0f);
                Bilhete(sala, new Vector3(-1.1f, 0.636f, -4f), 8f, "Inventário do paiol · 3 de maio de 1983",
                    "Dinamite: nenhuma caixa. Espoleta: nenhuma. Pavio: nenhum metro. Levaram tudo esta noite, com a minha chave. Fui eu que abri a porta. — Zé Lourenço");
                break;

            case "CapelaDaSanta":
                // O nicho da santa serve de altar, com a cruz na parede. Três fileiras de bancos, de frente para ele.
                GameObject altar = (GameObject)PrefabUtility.InstantiatePrefab(CenaDaCapela(), sala);
                altar.transform.localPosition = new Vector3(0f, 0f, -3.6f);
                Caixa(sala, "Cruz", 0f, 3.5f, -4.3f, 0.18f, 1.8f, 0.1f, "Madeira");
                Caixa(sala, "Cruz", 0f, 3.9f, -4.3f, 1f, 0.18f, 0.1f, "Madeira");
                foreach (float z in new[] { 2.6f, 1.2f, -0.2f })
                {
                    foreach (float x in new[] { -2.4f, 2.4f })
                    {
                        Caixa(sala, "Banco", x, 0.42f, z, 2.4f, 0.06f, 0.4f, "Madeira", true);
                        Caixa(sala, "Encosto", x, 0.78f, z + 0.2f, 2.4f, 0.5f, 0.05f, "Madeira");
                        Caixa(sala, "Pe", x - 1.1f, 0.21f, z, 0.06f, 0.42f, 0.4f, "Madeira");
                        Caixa(sala, "Pe", x + 1.1f, 0.21f, z, 0.06f, 0.42f, 0.4f, "Madeira");
                    }
                }
                Vela(sala, -1.6f, 0f, -3.4f, true);
                Vela(sala, 1.7f, 0f, -3.5f, true);
                Vela(sala, -3.9f, 0f, -1.8f, false);
                Bilhete(sala, new Vector3(-2.2f, 0.456f, 1.2f), 12f, "Papel dobrado no banco",
                    "Santa Bárbara, que guarda quem trabalha com fogo e debaixo da terra: traz o Chico de volta pra gente. Ele só tem dezenove anos.");
                break;

            case "CasaDeBombas":
                // Duas bombas paradas no fundo da sala, com o volante, o motor e os canos que sobem para o teto.
                for (int i = 0; i < 2; i++)
                {
                    float x = i == 0 ? -2.7f : 1.7f, z = i == 0 ? -3f : -3.3f, escala = i == 0 ? 1f : 0.8f;
                    Caixa(sala, "Base", x, 0.2f, z, 2.3f * escala, 0.4f, 1.4f * escala, "Rocha", true);
                    Rolo(sala, "Bomba", x, 0.4f + 0.55f * escala, z, 1.1f * escala, 1.7f * escala, "Metal").transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Rolo(sala, "Volante", x + 1.05f * escala, 0.4f + 0.6f * escala, z, 1.5f * escala, 0.12f, "Escuro").transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    Caixa(sala, "Motor", x - 1.1f * escala, 0.4f + 0.4f * escala, z, 0.6f, 0.8f * escala, 0.9f * escala, "Escuro");
                    Rolo(sala, "Cano", x, (1.4f + teto) / 2f, z - 0.5f * escala, 0.24f, teto - 1.4f, "Metal");
                }
                Rolo(sala, "CanoMestre", 0f, 2.7f, -4.2f, 0.26f, 8.6f, "Metal").transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Rolo(sala, "CanoDeDescida", 4f, 1.35f, -4.2f, 0.26f, 2.7f, "Metal");
                Rolo(sala, "Registro", 0.2f, 2.7f, -4f, 0.5f, 0.05f, "Latao").transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Rolo(sala, "Registro", 4f, 1.4f, -3.95f, 0.4f, 0.05f, "Latao").transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                // O painel, na parede oeste, com as alavancas para baixo e os relógios de pressão.
                Caixa(sala, "Painel", -4.2f, 1.5f, 0.3f, 0.2f, 1.2f, 1f, "Metal");
                for (int i = 0; i < 3; i++)
                {
                    Caixa(sala, "Alavanca", -4.05f, 1.2f, i * 0.3f, 0.06f, 0.3f, 0.06f, "Escuro").transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
                    Rolo(sala, "Relogio", -4.08f, 1.85f, i * 0.3f, 0.16f, 0.03f, "Papel").transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
                // Etapa 146: a luz de alarme do painel, vermelha, ainda acesa.
                Montar.Peca("LuzDeAlarme", PrimitiveType.Sphere, sala, new Vector3(-4.05f, 2.2f, 0.3f), Vector3.one * 0.12f, Mat("Olhos"), false);
                LuzViva alarme = sala.gameObject.AddComponent<LuzViva>();
                alarme.modo = LuzViva.Modo.Pulsar;
                alarme.ritmo = 0.7f;
                alarme.luz = Montar.Luz("LuzDoPainel", sala, new Vector3(-3.7f, 2.2f, 0.3f), LightType.Point, new Color(1f, 0.15f, 0.1f), 1.8f, 6f);
                Caixa(sala, "Mesinha", -3.9f, 0.4f, 2.2f, 0.7f, 0.8f, 0.9f, "Madeira", true);
                Bilhete(sala, new Vector3(-3.85f, 0.806f, 2.2f), 80f, "Livro da casa de bombas · 27 de março de 1983",
                    "Bomba 1 parada. Bomba 2 parada. A água sobe um palmo por dia e não é de chuva: ela mina de dentro da pedra preta. Pedi peça à companhia. Mandaram trabalhar com água na canela.");
                break;

            case "PocoInclinado":
            {
                // O trilho desce a rampa. Lá em cima, o guincho; no meio, um carrinho carregado, preso pelo cabo.
                float inclinacao = Mathf.Atan2(-Montar.Fundo, Montar.Celula) * Mathf.Rad2Deg;
                Quaternion ladeira = Quaternion.Euler(-inclinacao, 0f, 0f);
                foreach (float x in new[] { 0.5f, 1.1f })
                {
                    Caixa(sala, "Trilho", x, 0.03f, 3f, 0.06f, 0.06f, 3f, "Metal");
                    Caixa(sala, "Trilho", x, Montar.Fundo / 2f + 0.03f, 0f, 0.06f, 0.06f, 3.36f, "Metal").transform.localRotation = ladeira;
                    Caixa(sala, "Trilho", x, Montar.Fundo + 0.03f, -3f, 0.06f, 0.06f, 3f, "Metal");
                }
                for (int i = 0; i < 4; i++)
                {
                    float quanto = (i + 0.5f) / 4f;
                    Caixa(sala, "Dormente", 0.8f, Montar.Fundo * quanto + 0.01f, 1.5f - quanto * 3f, 1f, 0.05f, 0.18f, "Madeira").transform.localRotation = ladeira;
                    Caixa(sala, "Dormente", 0.8f, 0.01f, 1.9f + i * 0.7f, 1f, 0.05f, 0.18f, "Madeira");
                    Caixa(sala, "Dormente", 0.8f, Montar.Fundo + 0.01f, -1.9f - i * 0.7f, 1f, 0.05f, 0.18f, "Madeira");
                }
                GameObject carrinho = Caixa(sala, "CarrinhoPreso", 0.8f, Montar.Fundo * 0.55f + 0.45f, 1.5f - 0.55f * 3f, 0.9f, 0.55f, 1.3f, "Metal", true);
                carrinho.transform.localRotation = ladeira;
                Montar.Modelo("Carga", MontarArte.Pedra(2), sala, new Vector3(0.8f, Montar.Fundo * 0.55f + 0.8f, 1.5f - 0.55f * 3f), new Vector3(0.8f, 0.5f, 1.1f), Mat("Minerio")).transform.localRotation = ladeira;
                // Etapa 146: um lampião do pacote pendurado em cima do guincho, falhando.
                Caixa(sala, "Gancho", 0.8f, 2.6f, 4f, 0.06f, 0.8f, 0.06f, "Madeira");
                Obj(sala, "Kit_Lampiao", 0.8f, 2.2f, 4f).transform.localScale = Vector3.one * 0.6f;
                LuzViva doGuincho = sala.gameObject.AddComponent<LuzViva>();
                doGuincho.modo = LuzViva.Modo.Piscar;
                doGuincho.ritmo = 0.4f;
                doGuincho.luz = Montar.Luz("LuzDoGuincho", sala, new Vector3(0.8f, 1.9f, 3.9f), LightType.Point, new Color(1f, 0.75f, 0.45f), 2f, 6f);
                Caixa(sala, "Guincho", 0.8f, 0.45f, 4f, 1f, 0.9f, 0.5f, "Madeira", true);
                Rolo(sala, "Tambor", 0.8f, 1.05f, 4f, 0.45f, 0.9f, "Metal").transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Caixa(sala, "Manivela", 1.35f, 1.05f, 4f, 0.06f, 0.5f, 0.06f, "Metal").transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
                Caixa(sala, "Cabo", 0.8f, 0.95f, 2.75f, 0.03f, 0.03f, 2.5f, "Escuro").transform.localRotation = Quaternion.Euler(4f, 0f, 0f);
                Caixa(sala, "Cabo", 0.8f, 0.42f, 0.9f, 0.03f, 0.03f, 1.5f, "Escuro").transform.localRotation = Quaternion.Euler(-inclinacao - 8f, 0f, 0f);
                break;
            }

            case "Mezanino":
                // O corrimão da beira de cima, do lado em que não há rampa: é de lá que se olha para baixo.
                Caixa(sala, "Corrimao", 3f, 0.95f, 1.45f, 3f, 0.08f, 0.08f, "Madeira", true);
                foreach (float x in new[] { 1.6f, 3f, 4.4f }) Caixa(sala, "Esteio", x, 0.48f, 1.45f, 0.1f, 0.95f, 0.1f, "Madeira");
                break;
        }
    }

    // Etapa 119: o que tem de ficar pendurado no teto vai dentro de um filho com este nome. Ao pôr a peça numa sala,
    // o gerador sobe o NoTeto até a altura do teto daquela sala.
    static Transform NoTeto(Transform pai)
    {
        Transform noTeto = new GameObject("NoTeto").transform;
        noTeto.SetParent(pai, false);
        noTeto.localPosition = Vector3.up * Montar.Teto;
        return noTeto;
    }

    // ---------- Luz de cada trecho (etapa 123) ----------
    // Uma por sala, no meio, com a chance do tema. Assim dá para saber em que trecho se está só pela luz.

    // Galerias: lâmpada elétrica da companhia, pendurada num fio, falhando.
    static GameObject LampadaDeTeto()
    {
        return Montar.Prefab("LampadaDeTeto", () =>
        {
            GameObject raiz = new GameObject("LampadaDeTeto");
            Transform noTeto = NoTeto(raiz.transform);
            // O fio corre 40 cm abaixo do teto, porque a pele de rocha desce até 38 cm.
            Caixa(noTeto, "Fio", 0f, -0.4f, 0f, 0.03f, 0.03f, 7f, "Escuro");
            Caixa(noTeto, "Pendente", 0f, -0.6f, 0f, 0.025f, 0.4f, 0.025f, "Escuro");
            Rolo(noTeto, "Bocal", 0f, -0.84f, 0f, 0.09f, 0.08f, "Metal");
            Montar.Peca("Lampada", PrimitiveType.Sphere, noTeto, new Vector3(0f, -0.95f, 0f), Vector3.one * 0.15f, Mat("Chama"), false);
            LuzViva viva = raiz.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Piscar;
            viva.ritmo = 0.5f;
            viva.luz = Montar.Luz("Luz", noTeto, new Vector3(0f, -1.05f, 0f), LightType.Point, new Color(1f, 0.85f, 0.6f), 2.4f, 8f);
            return raiz;
        });
    }

    // Cavernas: o reflexo azulado da água, que sobe e desce devagar.
    static GameObject ReflexoDaAgua()
    {
        return Montar.Prefab("ReflexoDaAgua", () =>
        {
            GameObject raiz = new GameObject("ReflexoDaAgua");
            LuzViva viva = raiz.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Pulsar;
            viva.ritmo = 0.25f;
            viva.luz = Montar.Luz("Luz", raiz.transform, new Vector3(0f, 0.4f, 0f), LightType.Point, new Color(0.25f, 0.6f, 0.8f), 1.1f, 7f);
            return raiz;
        });
    }

    // Veio: só o brilho verde dos cristais do teto, que bate como o coração: uma vez a cada dois segundos.
    static GameObject BrilhoDoVeio()
    {
        return Montar.Prefab("BrilhoDoVeio", () =>
        {
            GameObject raiz = new GameObject("BrilhoDoVeio");
            Transform noTeto = NoTeto(raiz.transform);
            for (int i = 0; i < 4; i++)
            {
                GameObject cristal = Montar.Modelo("Cristal", MontarArte.Cristal(), noTeto, new Vector3(-0.3f + i * 0.2f, -0.35f - (i % 2) * 0.15f, (i % 3) * 0.2f - 0.2f), new Vector3(0.16f, 0.5f + (i % 2) * 0.25f, 0.16f), Mat("Cristal"));
                cristal.transform.localRotation = Quaternion.Euler(170f + i * 8f, 0f, -12f + i * 9f);
            }
            LuzViva viva = raiz.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Pulsar;
            viva.ritmo = 0.5f;
            viva.luz = Montar.Luz("Luz", noTeto, new Vector3(0f, -0.9f, 0f), LightType.Point, new Color(0.3f, 1f, 0.5f), 0.9f, 8f);
            return raiz;
        });
    }

    static void Estalactite(Transform pai, Vector3 lugar, float largura, float altura, Material rocha, float teto = Montar.Teto)
    {
        GameObject cone = Montar.Modelo("Estalactite", MontarArte.Cone(), pai, new Vector3(lugar.x, teto - altura / 2f, lugar.z), new Vector3(largura, altura, largura), rocha);
        cone.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
    }

    // Um caroço do veio: carne escura, cristais verdes e uma luz que pulsa.
    static GameObject Tumor()
    {
        return Montar.Prefab("Tumor", () =>
        {
            GameObject tumor = new GameObject("Tumor");
            SphereCollider colisao = tumor.AddComponent<SphereCollider>();
            colisao.center = new Vector3(0f, 0.4f, 0f);
            colisao.radius = 0.5f;
            Transform massa = new GameObject("Massa").transform;
            massa.SetParent(tumor.transform, false);
            Montar.Modelo("Carne", MontarArte.Pedra(3), massa, new Vector3(0f, 0.4f, 0f), new Vector3(1.1f, 0.9f, 1f), Mat("Coracao"));
            Montar.Modelo("Carne", MontarArte.Pedra(5), massa, new Vector3(0.35f, 0.25f, 0.2f), new Vector3(0.6f, 0.5f, 0.6f), Mat("Coracao"));
            for (int i = 0; i < 3; i++)
            {
                GameObject cristal = Montar.Modelo("Cristal", MontarArte.Cristal(), massa, new Vector3(-0.25f + i * 0.25f, 0.75f, -0.1f + i * 0.12f), new Vector3(0.18f, 0.4f, 0.18f), Mat("Cristal"));
                cristal.transform.localRotation = Quaternion.Euler(-20f + i * 20f, 0f, 25f - i * 25f);
            }
            LuzViva viva = tumor.AddComponent<LuzViva>();
            viva.modo = LuzViva.Modo.Pulsar;
            viva.alvo = massa;
            viva.ritmo = 0.5f;
            viva.luz = Montar.Luz("Luz", tumor.transform, new Vector3(0f, 1f, 0f), LightType.Point, new Color(0.3f, 1f, 0.5f), 1.2f, 5f);
            return tumor;
        });
    }

    // Etapa 66: a água rasa das cavernas alagadas, uma folha transparente logo acima do chão da sala.
    static GameObject LaminaDeAgua()
    {
        return Montar.Prefab("LaminaDeAgua", () =>
        {
            GameObject lamina = new GameObject("LaminaDeAgua");
            Montar.Peca("Agua", PrimitiveType.Cube, lamina.transform, new Vector3(0f, 0.07f, 0f), new Vector3(15f, 0.02f, 15f), Mat("Agua"), false);
            return lamina;
        });
    }

    // ---------- Chefão ----------

    // Etapa 69: o disco vermelho do aviso e o cone escuro que sai do chão.
    static GameObject TentaculoDoChefao()
    {
        return Montar.Prefab("Tentaculo", () =>
        {
            GameObject raiz = new GameObject("Tentaculo");
            Tentaculo tentaculo = raiz.AddComponent<Tentaculo>();
            tentaculo.somDeSaida = Som("tentaculo");
            tentaculo.marca = Montar.Peca("Marca", PrimitiveType.Cylinder, raiz.transform, new Vector3(0f, 0.02f, 0f), new Vector3(2.6f, 0.01f, 2.6f), Mat("Aviso"), false);
            Montar.Luz("Luz", tentaculo.marca.transform, new Vector3(0f, 40f, 0f), LightType.Point, new Color(1f, 0.1f, 0.05f), 3f, 4f); // 40 por causa da escala do disco: fica a 40 cm do chão
            tentaculo.corpo = new GameObject("Corpo").transform;
            tentaculo.corpo.SetParent(raiz.transform, false);
            // Etapa 129: o modelo tem quatro gomos, um filho do outro, e o script Tentaculo dobra cada um.
            Transform modelo = Montar.Instalar("Tentaculo", tentaculo.corpo, true);
            tentaculo.gomos = Pecas(modelo, "Gomo_", 4);
            return raiz;
        });
    }

    // Etapas 67 e 68: a arena do andar 10, feita à mão. Um salão de 24 x 24 m, com o Coração do Veio preso na parede norte.
    internal static GameObject Arena()
    {
        return Montar.Prefab("Arena", () =>
        {
            GameObject arena = new GameObject("Arena");
            Transform t = arena.transform;
            Material rocha = Mat("Rocha");
            const float alto = 7f;

            Montar.Peca("Chao", PrimitiveType.Cube, t, new Vector3(0f, -1f, 0f), new Vector3(24f, 2f, 24f), rocha, true);
            GameObject teto = Montar.Peca("Teto", PrimitiveType.Cube, t, new Vector3(0f, alto + 0.5f, 0f), new Vector3(26f, 1f, 26f), rocha, true);
            teto.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            Montar.Peca("ParedeNorte", PrimitiveType.Cube, t, new Vector3(0f, alto / 2f, 12.5f), new Vector3(26f, alto, 1f), rocha, true);
            Montar.Peca("ParedeSul", PrimitiveType.Cube, t, new Vector3(0f, alto / 2f, -12.5f), new Vector3(26f, alto, 1f), rocha, true);
            Montar.Peca("ParedeLeste", PrimitiveType.Cube, t, new Vector3(12.5f, alto / 2f, 0f), new Vector3(1f, alto, 24f), rocha, true);
            Montar.Peca("ParedeOeste", PrimitiveType.Cube, t, new Vector3(-12.5f, alto / 2f, 0f), new Vector3(1f, alto, 24f), rocha, true);
            AudioReverbZone eco = arena.AddComponent<AudioReverbZone>();
            eco.reverbPreset = AudioReverbPreset.Cave;
            eco.minDistance = 14f;
            eco.maxDistance = 20f;

            // Estalactites no teto e pedras pelos cantos, só de enfeite.
            float[] xs = { -9f, -6f, -2f, 3f, 7f, 10f, -8f, 5f, 0f, -4f };
            float[] zs = { -8f, 2f, -4f, -9f, 0f, 6f, 8f, 4f, 3f, -1f };
            for (int i = 0; i < xs.Length; i++) Estalactite(t, new Vector3(xs[i], 0f, zs[i]), 0.6f + 0.1f * (i % 3), 1.2f + 0.5f * (i % 4), rocha, alto);
            Montar.Modelo("Pedra", MontarArte.Pedra(1), t, new Vector3(-10.5f, 0.5f, -10f), new Vector3(2.4f, 1.6f, 2f), rocha);
            Montar.Modelo("Pedra", MontarArte.Pedra(2), t, new Vector3(10.6f, 0.4f, -9.5f), new Vector3(2f, 1.3f, 2.2f), rocha);
            Montar.Modelo("Pedra", MontarArte.Pedra(3), t, new Vector3(10.8f, 0.5f, 9f), new Vector3(1.8f, 1.5f, 1.8f), rocha);

            // Etapa 113 e fita 20: o Joaquim, sentado contra uma pedra, à esquerda de quem entra (à direita, a mão da
            // picareta o esconderia). Sem o buraco no rosto: ele não foi com o canto. Do lado, o lampião no fim, que
            // ainda pisca, e a dinamite com o pavio que molhou.
            Transform joaquim = new GameObject("Joaquim").transform;
            joaquim.SetParent(t, false);
            joaquim.localPosition = new Vector3(-1.8f, 0f, -4f);
            joaquim.localRotation = Quaternion.Euler(0f, 165f, 0f);
            LuzViva fim = joaquim.gameObject.AddComponent<LuzViva>();
            fim.modo = LuzViva.Modo.Piscar;
            fim.ritmo = 0.4f;
            fim.luz = Montar.Luz("LuzNoFim", joaquim, new Vector3(-0.5f, 0.45f, 0.4f), LightType.Point, new Color(1f, 0.7f, 0.4f), 1.2f, 3.5f);
            Caixa(joaquim, "Apoio", 0f, 0.4f, -0.75f, 1.1f, 0.8f, 0.7f, "Rocha", true);
            Montar.Modelo("Pedra", MontarArte.Pedra(4), joaquim, new Vector3(0f, 0.55f, -0.75f), new Vector3(1.7f, 1.5f, 1.2f), rocha);
            Transform sentado = Montar.Instalar("MineiroOco", joaquim, true);
            PrefabUtility.UnpackPrefabInstance(sentado.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            foreach (string sobra in new[] { "RostoOco", "Cabo", "Ferro" }) Object.DestroyImmediate(Achar(sentado, sobra).gameObject);
            sentado.localPosition = new Vector3(0f, -0.84f, -0.2f);
            Achar(sentado, "Perna_1").localRotation = Quaternion.Euler(-88f, 0f, -8f);
            Achar(sentado, "Perna_2").localRotation = Quaternion.Euler(-78f, 0f, 10f);
            Achar(sentado, "Canela_2").localRotation = Quaternion.Euler(40f, 0f, 0f);
            Achar(sentado, "Cabeca").localRotation = Quaternion.Euler(28f, 0f, 8f);
            Achar(sentado, "Braco_2").localRotation = Quaternion.Euler(-25f, 0f, 0f);
            Achar(sentado, "Antebraco_2").localRotation = Quaternion.Euler(-45f, 0f, 0f);
            Rolo(joaquim, "Lampiao", -0.5f, 0.12f, 0.25f, 0.14f, 0.24f, "Metal");
            for (int i = 0; i < 3; i++)
                Rolo(joaquim, "Dinamite", 0.5f + i * 0.055f, 0.03f, 0.45f, 0.05f, 0.24f, "Dinamite").transform.localRotation = Quaternion.Euler(90f, 20f, 0f);
            Caixa(joaquim, "Pavio", 0.6f, 0.012f, 1.1f, 0.025f, 0.02f, 1.2f, "Escuro").transform.localRotation = Quaternion.Euler(0f, 15f, 0f);

            // Onde o jogador chega, olhando para o coração.
            Transform inicio = new GameObject("Inicio").transform;
            inicio.SetParent(t, false);
            inicio.localPosition = new Vector3(0f, 0f, -9.5f);

            // O coração (etapa 129): o modelo do Blender, com uma luz vermelha e a batida. Só a Massa pulsa, com os lobos
            // e as artérias; as veias, que correm pela parede até os cantos, ficam paradas.
            Transform coracao = new GameObject("Coracao").transform;
            coracao.SetParent(t, false);
            coracao.localPosition = new Vector3(0f, 2.6f, 11.3f);
            Transform modelo = Montar.Instalar("Coracao", coracao, false);
            LuzViva pulso = coracao.gameObject.AddComponent<LuzViva>();
            pulso.modo = LuzViva.Modo.Pulsar;
            pulso.alvo = Achar(modelo, "Massa");
            pulso.ritmo = 0.83f;
            pulso.luz = Montar.Luz("Pulso", t, new Vector3(0f, 2.6f, 9f), LightType.Point, new Color(1f, 0.15f, 0.1f), 3f, 13f);
            Montar.Fonte(coracao.gameObject, Som("coracao"), true, 30f);
            // Uma parede invisível na frente do coração, para ninguém entrar nele.
            BoxCollider parede = arena.AddComponent<BoxCollider>();
            parede.center = new Vector3(0f, 2.5f, 11f);
            parede.size = new Vector3(8f, 5f, 2f);

            // Etapa 68: os três pontos fracos, cristais verdes que quebram com 5 golpes de picareta cada.
            Vector3[] lugares = { new Vector3(-2.4f, 1.3f, 9.75f), new Vector3(0f, 1.55f, 9.55f), new Vector3(2.4f, 1.3f, 9.75f) };
            Vida[] pontosFracos = new Vida[lugares.Length];
            for (int i = 0; i < lugares.Length; i++)
            {
                GameObject ponto = new GameObject("PontoFraco");
                ponto.transform.SetParent(t, false);
                ponto.transform.localPosition = lugares[i];
                ponto.AddComponent<SphereCollider>().radius = 0.5f;
                pontosFracos[i] = ponto.AddComponent<Vida>();
                pontosFracos[i].maxima = 5;
                GameObject cristal = Montar.Modelo("Cristal", MontarArte.Cristal(), ponto.transform, Vector3.zero, new Vector3(1.1f, 1.3f, 1.1f), Mat("Cristal"));
                cristal.transform.localRotation = Quaternion.Euler(-25f + i * 10f, i * 40f, 15f);
                LuzViva viva = ponto.AddComponent<LuzViva>();
                viva.modo = LuzViva.Modo.Pulsar;
                viva.ritmo = 1.4f;
                viva.luz = Montar.Luz("Brilho", ponto.transform, new Vector3(0f, 0f, -0.6f), LightType.Point, new Color(0.3f, 1f, 0.5f), 2f, 5f);
            }

            Chefao chefao = arena.AddComponent<Chefao>();
            chefao.pontosFracos = pontosFracos;
            chefao.tentaculo = TentaculoDoChefao();
            chefao.rastejante = Rastejante();
            chefao.somDeRugido = Som("rugido");
            chefao.luzes = new Light[4];
            chefao.pontosDeInimigo = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -1f : 1f;
                float z = i < 2 ? -1f : 1f;
                chefao.luzes[i] = Montar.Luz("LuzDaArena", t, new Vector3(x * 8f, 4f, z * 7f), LightType.Point, new Color(0.3f, 1f, 0.5f), 1.6f, 15f);
                chefao.pontosDeInimigo[i] = new GameObject("PontoDeInimigo").transform;
                chefao.pontosDeInimigo[i].SetParent(t, false);
                chefao.pontosDeInimigo[i].localPosition = new Vector3(x * 9f, 0f, z < 0f ? -2f : 5f);
            }
            return arena;
        });
    }

    // ---------- Tutorial e sustos ----------

    // Etapa 82: duas placas de madeira com os controles, na frente de onde o jogador começa o andar 1.
    internal static GameObject Tutorial()
    {
        return Montar.Prefab("Tutorial", () =>
        {
            GameObject tutorial = new GameObject("Tutorial");
            Placa(tutorial.transform, -1.8f, "Placa1", 1.56f);
            Placa(tutorial.transform, 1.8f, "Placa2", 1.89f);
            return tutorial;
        });
    }

    static void Placa(Transform pai, float x, string material, float largura)
    {
        Material madeira = Mat("Madeira");
        Montar.Peca("Poste", PrimitiveType.Cube, pai, new Vector3(x, 0.55f, 3.33f), new Vector3(0.1f, 1.1f, 0.1f), madeira, true);
        Montar.Peca("Tabua", PrimitiveType.Cube, pai, new Vector3(x, 1.45f, 3.3f), new Vector3(largura + 0.12f, 0.88f, 0.06f), madeira, true);
        // O Quad da Unity mostra a frente para quem olha no sentido do eixo Z, que é como o jogador chega.
        Montar.Peca("Escrita", PrimitiveType.Quad, pai, new Vector3(x, 1.45f, 3.26f), new Vector3(largura, 0.76f, 1f), Mat(material), false);
    }

    // Etapa 79: o vulto que atravessa o caminho.
    internal static GameObject VultoDoSusto()
    {
        return Montar.Prefab("Vulto", () =>
        {
            GameObject vulto = new GameObject("Vulto");
            vulto.AddComponent<Vulto>();
            Montar.Peca("Corpo", PrimitiveType.Capsule, vulto.transform, new Vector3(0f, 0.85f, 0f), new Vector3(0.5f, 0.85f, 0.4f), Mat("Escuro"), false);
            Montar.Peca("Cabeca", PrimitiveType.Sphere, vulto.transform, new Vector3(0f, 1.85f, 0f), Vector3.one * 0.32f, Mat("Escuro"), false);
            return vulto;
        });
    }

    // Etapa 79: a tábua que cai.
    internal static GameObject Tabua()
    {
        return Montar.Prefab("Tabua", () =>
        {
            GameObject tabua = Montar.Peca("Tabua", PrimitiveType.Cube, null, Vector3.zero, new Vector3(0.22f, 0.05f, 1.4f), Mat("Madeira"), true);
            tabua.AddComponent<Rigidbody>();
            return tabua;
        });
    }

    // ---------- Temas e dificuldade ----------

    // Etapa 66: os três trechos da mina.
    static Tema TemaDasGalerias()
    {
        return CriarTema("Galerias", tema =>
        {
            tema.nome = "Galerias de madeira";
            tema.luzAmbiente = new Color(0.012f, 0.011f, 0.01f);
            tema.portal = Portal();
            tema.portais = new[] { Portal(1), Portal(2), Portal(3) };
            tema.trilho = Trilho();
            tema.enfeites = new[] { Caixote(), Barril(), Carrinho(), Carrinho("Vazio"), Carrinho("Tombado"), LampiaoDePoste(), PilhaDePedras(), CapaceteCaido(), CorpoDeMineiro(),
                EscoraCaida(), CaixaDeForca(), CantoDoMineiro(), Ferramentas(), QuadroDeAvisos() };
            tema.cenas = new[] { CenaDoRefeitorio(), CenaDoCapataz(), CenaDasBotas() };
            tema.luminaria = LampadaDeTeto();
            tema.chanceDeLuminaria = 0.45f;
            // Etapa 147: imagem quente e limpa.
            tema.granulado = 0.4f;
            tema.vinheta = 0.4f;
            tema.filtro = new Color(1f, 0.95f, 0.87f);
            tema.saturacao = -8f;
            tema.sonsDoAmbiente = new[] { Som("madeira_rangendo"), Som("madeira_rangendo"), Som("gota") };
        });
    }

    static Tema TemaDasCavernas()
    {
        return CriarTema("Cavernas", tema =>
        {
            tema.nome = "Cavernas alagadas";
            tema.rocha = Mat("RochaMolhada");
            tema.neblina = new Color(0.01f, 0.02f, 0.035f);
            tema.densidadeDaNeblina = 0.05f;
            tema.luzAmbiente = new Color(0.008f, 0.014f, 0.022f);
            tema.laminaDeAgua = LaminaDeAgua();
            tema.trilho = Trilho();
            tema.enfeites = new[] { Estalagmites(), Estalagmites(), PilhaDePedras(), CorpoDeMineiro(), EscoraCaida(), Ferramentas(), Carrinho("Tombado") };
            tema.cenas = new[] { CenaDaCapela(), CenaDoCofre(), CenaDoChico() };
            tema.luminaria = ReflexoDaAgua();
            tema.chanceDeLuminaria = 0.5f;
            // Etapa 142: aqui a madeira está molhada e o ferro enferrujou. Etapa 147: imagem fria.
            tema.trocas = new[] { Trocar("Madeira", "MadeiraMolhada"), Trocar("KitMadeira", "KitMadeiraMolhada"), Trocar("Metal", "FerroEnferrujado"), Trocar("KitFerro", "FerroEnferrujado") };
            tema.granulado = 0.55f;
            tema.vinheta = 0.46f;
            tema.filtro = new Color(0.84f, 0.95f, 1.06f);
            tema.saturacao = -20f;
            tema.chanceDeEnfeite = 0.7f;
            tema.sonsDoAmbiente = new[] { Som("gota") };
        });
    }

    static Tema TemaDoVeio()
    {
        return CriarTema("Veio", tema =>
        {
            tema.nome = "O veio negro";
            tema.rocha = Mat("RochaNegra");
            tema.neblina = new Color(0f, 0.025f, 0.01f);
            tema.densidadeDaNeblina = 0.07f;
            tema.luzAmbiente = new Color(0.006f, 0.016f, 0.008f);
            tema.enfeites = new[] { Tumor(), Estalagmites(), CorpoDeMineiro() };
            tema.cenas = new[] { CenaDaDinamite(), CenaDosCinco(), CenaDaDinamite() };
            tema.luminaria = BrilhoDoVeio();
            tema.chanceDeLuminaria = 0.6f;
            // Etapa 142: a madeira apodreceu. Etapa 147: imagem esverdeada, quase sem cor, com muito chuvisco.
            tema.trocas = new[] { Trocar("Madeira", "MadeiraPodre"), Trocar("KitMadeira", "KitMadeiraPodre"), Trocar("Metal", "FerroEnferrujado"), Trocar("KitFerro", "FerroEnferrujado") };
            tema.granulado = 0.8f;
            tema.vinheta = 0.52f;
            tema.filtro = new Color(0.9f, 1.05f, 0.9f);
            tema.saturacao = -32f;
            tema.sonsDoAmbiente = new[] { Som("sussurro"), Som("coracao"), Som("gota") };
        });
    }

    static Tema.Troca Trocar(string de, string para)
    {
        return new Tema.Troca { de = Mat(de), para = Mat(para) };
    }

    static Tema CriarTema(string arquivo, System.Action<Tema> ajustar)
    {
        string caminho = "Assets/Dados/Tema_" + arquivo + ".asset";
        Tema tema = AssetDatabase.LoadAssetAtPath<Tema>(caminho);
        if (tema != null) return tema;
        tema = ScriptableObject.CreateInstance<Tema>();
        ajustar(tema);
        AssetDatabase.CreateAsset(tema, caminho);
        return tema;
    }

    // Etapa 58: a tabela começa com o Ouvinte no andar 4 e o Mineiro Oco no 7, como no plano.
    internal static TabelaDeDificuldade Tabela()
    {
        const string caminho = "Assets/Dados/Dificuldade.asset";
        TabelaDeDificuldade existente = AssetDatabase.LoadAssetAtPath<TabelaDeDificuldade>(caminho);
        if (existente != null) return existente;

        GameObject rastejante = Rastejante();
        GameObject ouvinte = Ouvinte();
        GameObject mineiro = MineiroOco();
        Tema[] temas = { TemaDasGalerias(), TemaDasCavernas(), TemaDoVeio() };
        TabelaDeDificuldade tabela = ScriptableObject.CreateInstance<TabelaDeDificuldade>();
        tabela.andares = new TabelaDeDificuldade.Andar[10];
        for (int i = 0; i < tabela.andares.Length; i++)
        {
            int andar = i + 1;
            TabelaDeDificuldade.Andar linha = new TabelaDeDificuldade.Andar();
            linha.tema = temas[Mathf.Min(i / 3, 2)]; // andares 1 a 3, 4 a 6 e 7 em diante
            linha.salas = 5 + andar;
            linha.chanceDeInimigo = Mathf.Min(0.7f, 0.2f + 0.05f * andar);
            linha.inimigos = andar >= 7 ? new[] { rastejante, ouvinte, mineiro } : andar >= 4 ? new[] { rastejante, ouvinte } : new[] { rastejante };
            linha.chanceDeVeio = 0.5f;
            linha.chanceDeItem = 0.3f;
            linha.chanceDePerigo = Mathf.Min(0.4f, 0.05f + 0.03f * andar);
            tabela.andares[i] = linha;
        }
        AssetDatabase.CreateAsset(tabela, caminho);
        return tabela;
    }
}
