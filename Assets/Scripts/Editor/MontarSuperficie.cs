using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Etapas 41, 45, 48, 57, 64 e 73: monta a cena Superficie (o acampamento), a cena Menu e as melhorias da bancada.
// Menu Galeria 7 > Montar superfície e menu.
public static class MontarSuperficie
{
    const string CaminhoSuperficie = "Assets/Scenes/Superficie.unity";
    const string CaminhoMenu = "Assets/Scenes/Menu.unity";

    [MenuItem("Galeria 7/Montar superfície e menu")]
    static void Menu()
    {
        if (!Montar.PodeMontar(CaminhoSuperficie)) return;
        ConstruirSuperficie();
        ConstruirMenu();
    }

    // Etapa 41: o acampamento à noite, com cabana, fogueira, bancada, toca-fitas e o elevador da mina.
    internal static void ConstruirSuperficie()
    {
        Montar.Preparar();
        CriarMelhorias();
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Noite: um pouco de luz azul para enxergar o acampamento, bem diferente do escuro da mina.
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.07f, 0.08f, 0.12f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.05f);
        RenderSettings.fogDensity = 0.035f;
        Light lua = Montar.Luz("Lua", null, Vector3.zero, LightType.Directional, new Color(0.6f, 0.7f, 1f), 0.25f, 10f);
        lua.shadows = LightShadows.Soft;
        lua.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
        Montar.CriarClima();

        Transform acampamento = new GameObject("Acampamento").transform;
        Material rocha = Montar.PegarMaterial("Rocha");
        Material madeira = Montar.PegarMaterial("Madeira");
        Material metal = Montar.PegarMaterial("Metal");

        // Chão de 40 x 40 m cercado por paredões, para ninguém cair para fora.
        // O chão tem um buraco de 1,9 x 1,9 m embaixo da gaiola: é a boca do poço (etapa 118). Por isso são quatro pedaços.
        Material terra = Montar.PegarMaterial("Terra");
        Montar.Peca("Chao", PrimitiveType.Cube, acampamento, new Vector3(0f, -0.5f, -1.225f), new Vector3(40f, 1f, 37.55f), terra, true);
        Montar.Peca("Chao", PrimitiveType.Cube, acampamento, new Vector3(0f, -0.5f, 19.725f), new Vector3(40f, 1f, 0.55f), terra, true);
        Montar.Peca("Chao", PrimitiveType.Cube, acampamento, new Vector3(-10.475f, -0.5f, 18.5f), new Vector3(19.05f, 1f, 1.9f), terra, true);
        Montar.Peca("Chao", PrimitiveType.Cube, acampamento, new Vector3(10.475f, -0.5f, 18.5f), new Vector3(19.05f, 1f, 1.9f), terra, true);
        Montar.Peca("Paredao", PrimitiveType.Cube, acampamento, new Vector3(0f, 3f, 20.5f), new Vector3(42f, 8f, 1f), rocha, true);
        Montar.Peca("Paredao", PrimitiveType.Cube, acampamento, new Vector3(0f, 3f, -20.5f), new Vector3(42f, 8f, 1f), rocha, true);
        Montar.Peca("Paredao", PrimitiveType.Cube, acampamento, new Vector3(20.5f, 3f, 0f), new Vector3(1f, 8f, 40f), rocha, true);
        Montar.Peca("Paredao", PrimitiveType.Cube, acampamento, new Vector3(-20.5f, 3f, 0f), new Vector3(1f, 8f, 40f), rocha, true);
        for (int i = 0; i < 6; i++)
            Montar.Modelo("Pedra", MontarArte.Pedra(1 + i % 5), acampamento, new Vector3(-17f + i * 6.5f, 0.4f, i % 2 == 0 ? -17f : 15f), new Vector3(2.2f, 1.4f + 0.3f * (i % 3), 2f), rocha);

        // A cabana, com porta e janela acesa.
        Montar.Peca("Cabana", PrimitiveType.Cube, acampamento, new Vector3(-8f, 1.5f, 6f), new Vector3(5f, 3f, 4f), madeira, true);
        Montar.Peca("Telhado", PrimitiveType.Cube, acampamento, new Vector3(-8f, 3.15f, 6f), new Vector3(5.6f, 0.3f, 4.6f), madeira, true);
        Montar.Peca("Porta", PrimitiveType.Cube, acampamento, new Vector3(-8.8f, 1f, 3.97f), new Vector3(0.9f, 2f, 0.08f), Montar.PegarMaterial("Escuro"), false);
        Montar.Peca("Janela", PrimitiveType.Cube, acampamento, new Vector3(-6.8f, 1.7f, 3.97f), new Vector3(0.8f, 0.6f, 0.08f), Montar.PegarMaterial("Chama"), false);

        // Fogueira: duas lenhas cruzadas, a brasa, uma luz laranja que treme e o estalo do fogo.
        GameObject lenha = Montar.Peca("Lenha", PrimitiveType.Cylinder, acampamento, new Vector3(0f, 0.12f, 3f), new Vector3(0.18f, 0.6f, 0.18f), madeira, true);
        lenha.transform.localRotation = Quaternion.Euler(0f, 45f, 90f);
        lenha = Montar.Peca("Lenha", PrimitiveType.Cylinder, acampamento, new Vector3(0f, 0.12f, 3f), new Vector3(0.18f, 0.6f, 0.18f), madeira, true);
        lenha.transform.localRotation = Quaternion.Euler(0f, -45f, 90f);
        GameObject brasa = Montar.Peca("Brasa", PrimitiveType.Cube, acampamento, new Vector3(0f, 0.25f, 3f), new Vector3(0.45f, 0.3f, 0.45f), Montar.PegarMaterial("Chama"), false);
        Light fogo = Montar.Luz("LuzDaFogueira", acampamento, new Vector3(0f, 1f, 3f), LightType.Point, new Color(1f, 0.6f, 0.3f), 6f, 14f);
        fogo.shadows = LightShadows.Soft;
        LuzViva chama = brasa.AddComponent<LuzViva>();
        chama.modo = LuzViva.Modo.Pulsar;
        chama.ritmo = 3f;
        chama.luz = fogo;
        Montar.Fonte(brasa, MontarSons.Pegar("fogueira"), true, 12f);
        Montar.Peca("Banco", PrimitiveType.Cylinder, acampamento, new Vector3(1.6f, 0.2f, 2.2f), new Vector3(0.5f, 0.2f, 0.5f), madeira, true);
        Montar.Peca("Banco", PrimitiveType.Cylinder, acampamento, new Vector3(-1.5f, 0.2f, 2.4f), new Vector3(0.5f, 0.2f, 0.5f), madeira, true);

        // Etapa 110: quem é você. Num caixote ao lado de onde você começa, à luz de uma vela: a foto do avô e a carta da avó.
        // Fica à esquerda e à frente: do lado direito da tela, a mão da picareta esconderia tudo.
        Montar.Peca("CaixoteDaCarta", PrimitiveType.Cube, acampamento, new Vector3(-1f, 0.3f, -0.9f), new Vector3(0.8f, 0.6f, 0.8f), madeira, true);
        MontarPecas.Bilhete(acampamento, new Vector3(-1.1f, 0.606f, -1f), 15f, "Carta da vó Zica",
            "Meu bem, quando você ler isto eu já fui. A companhia jurou que os vinte e três desertaram. Seu avô Joaquim não largava nem prato sujo na pia, que dirá a gente. Ele desceu no dia 12 de maio de 83 e a mina foi lacrada com ele dentro. Guardei a picareta velha dele, a que ficou em casa. Eu nunca tive perna pra descer. Você tem. Traz o nome dele de volta.");
        Montar.Peca("Moldura", PrimitiveType.Cube, acampamento, new Vector3(-0.8f, 0.76f, -0.68f), new Vector3(0.3f, 0.34f, 0.03f), madeira, false).transform.localRotation = Quaternion.Euler(-12f, -20f, 0f);
        Montar.Peca("Foto", PrimitiveType.Quad, acampamento, new Vector3(-0.793f, 0.76f, -0.7f), new Vector3(0.25f, 0.29f, 1f), Montar.PegarMaterial("Foto"), false).transform.localRotation = Quaternion.Euler(-12f, -20f, 0f);
        MontarPecas.Vela(acampamento, -1.25f, 0.6f, -0.7f, true);
        LuzViva vela = Montar.Luz("LuzDaVela", acampamento, new Vector3(-1.1f, 0.95f, -0.95f), LightType.Point, new Color(1f, 0.7f, 0.4f), 1.6f, 4f).gameObject.AddComponent<LuzViva>();
        vela.modo = LuzViva.Modo.Pulsar;
        vela.ritmo = 2.5f;
        vela.luz = vela.GetComponent<Light>();

        // Etapa 113: o quadro de chapas na boca da mina, à direita do elevador, virado para quem chega.
        GameObject quadro = (GameObject)PrefabUtility.InstantiatePrefab(MontarPecas.QuadroDeChapas(), acampamento);
        quadro.transform.localPosition = new Vector3(3.6f, 0f, 16.6f);
        quadro.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        Montar.Luz("LuzDoQuadro", acampamento, new Vector3(3.6f, 2.3f, 15.6f), LightType.Point, new Color(1f, 0.8f, 0.55f), 1.5f, 4f);

        // Etapa 45: a bancada das melhorias.
        GameObject bancada = Montar.Peca("Bancada", PrimitiveType.Cube, acampamento, new Vector3(4f, 0.45f, 5f), new Vector3(2f, 0.9f, 1f), madeira, true);
        Bancada telaDaBancada = bancada.AddComponent<Bancada>();
        telaDaBancada.aviso = "E: bancada de melhorias";
        telaDaBancada.somDeCompra = MontarSons.Pegar("compra");
        Montar.Peca("Ferramenta", PrimitiveType.Cube, acampamento, new Vector3(3.6f, 0.94f, 5f), new Vector3(0.5f, 0.06f, 0.12f), metal, false);

        // Etapa 64: o toca-fitas, em cima de um caixote ao lado da cabana.
        Montar.Peca("Caixote", PrimitiveType.Cube, acampamento, new Vector3(-4.6f, 0.4f, 4.6f), Vector3.one * 0.8f, madeira, true);
        GameObject tocaFitas = Montar.Peca("TocaFitas", PrimitiveType.Cube, acampamento, new Vector3(-4.6f, 0.92f, 4.6f), new Vector3(0.5f, 0.24f, 0.3f), metal, true);
        tocaFitas.AddComponent<TocaFitas>().aviso = "E: ouvir as fitas";
        Montar.Peca("Botao", PrimitiveType.Cube, acampamento, new Vector3(-4.6f, 1.05f, 4.5f), new Vector3(0.3f, 0.03f, 0.06f), Montar.PegarMaterial("Fita"), false);

        // O elevador da mina, encostado no paredão norte, embaixo de uma torre de madeira.
        // Etapa 118: a gaiola fica fora do acampamento, porque na descida é o acampamento inteiro que sobe em volta dela.
        Elevador gaiola = Montar.CriarElevador(null, false).GetComponent<Elevador>();
        gaiola.transform.position = new Vector3(0f, 0f, 18.5f);
        gaiola.mundo = acampamento;
        gaiola.duracao = 11f; // a 3 m por segundo, passa das marcas de 10, 20 e 30 metros
        gaiola.luzDoCeu = lua;
        gaiola.somDaCampainha = MontarSons.Pegar("campainha");
        gaiola.somDaDescida = MontarSons.Pegar("gaiola");
        // O poço: quatro paredes de rocha de 45 m para baixo do chão, um quadro de madeira a cada 5 m e as marcas de
        // profundidade pintadas do lado para onde você fica virado.
        Transform poco = new GameObject("Poco").transform;
        poco.SetParent(acampamento, false);
        poco.localPosition = new Vector3(0f, 0f, 18.5f);
        Montar.Peca("Parede", PrimitiveType.Cube, poco, new Vector3(0f, -23.5f, 1.25f), new Vector3(3.1f, 45f, 0.6f), rocha, false);
        Montar.Peca("Parede", PrimitiveType.Cube, poco, new Vector3(0f, -23.5f, -1.25f), new Vector3(3.1f, 45f, 0.6f), rocha, false);
        Montar.Peca("Parede", PrimitiveType.Cube, poco, new Vector3(1.25f, -23.5f, 0f), new Vector3(0.6f, 45f, 1.9f), rocha, false);
        Montar.Peca("Parede", PrimitiveType.Cube, poco, new Vector3(-1.25f, -23.5f, 0f), new Vector3(0.6f, 45f, 1.9f), rocha, false);
        for (float fundo = -4f; fundo > -45f; fundo -= 5f)
        {
            Montar.Peca("Quadro", PrimitiveType.Cube, poco, new Vector3(0f, fundo, 0.9f), new Vector3(1.9f, 0.2f, 0.1f), madeira, false);
            Montar.Peca("Quadro", PrimitiveType.Cube, poco, new Vector3(0f, fundo, -0.9f), new Vector3(1.9f, 0.2f, 0.1f), madeira, false);
            Montar.Peca("Quadro", PrimitiveType.Cube, poco, new Vector3(0.9f, fundo, 0f), new Vector3(0.1f, 0.2f, 1.9f), madeira, false);
            Montar.Peca("Quadro", PrimitiveType.Cube, poco, new Vector3(-0.9f, fundo, 0f), new Vector3(0.1f, 0.2f, 1.9f), madeira, false);
        }
        for (int marca = 10; marca <= 30; marca += 10)
        {
            // O Quad mostra a frente para quem olha no sentido do eixo Z; aqui quem olha está virado para o sul.
            GameObject placa = Montar.Peca("Marca", PrimitiveType.Quad, poco, new Vector3(0f, -marca + 1.5f, -0.93f), new Vector3(0.9f, 0.38f, 1f), Montar.PegarMaterial("Placa" + marca), false);
            placa.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }

        // Etapa 122: Ouro Fundo. Três casas fechadas com tábuas, a igreja, os postes apagados, a placa da companhia
        // em cima da boca da mina e a interdição.
        Casa(acampamento, new Vector3(-15f, 0f, -9f), -90f);
        Casa(acampamento, new Vector3(15f, 0f, -9f), 90f);
        Casa(acampamento, new Vector3(-15f, 0f, 14f), -90f);
        Igreja(acampamento, new Vector3(13.5f, 0f, 11f), 90f);
        // Etapa 140: a cerca que fecha a rua do lado sul e os lados das casas.
        Cerca(acampamento, new Vector3(-18f, 0f, -18.5f), new Vector3(18f, 0f, -18.5f));
        Cerca(acampamento, new Vector3(-19f, 0f, -3f), new Vector3(-19f, 0f, 9f));
        Cerca(acampamento, new Vector3(19f, 0f, -3f), new Vector3(19f, 0f, 4f));
        Poste(acampamento, new Vector3(-10.5f, 0f, -4f));
        Poste(acampamento, new Vector3(10.5f, 0f, 2f));
        Montar.Peca("TabuaDaPlaca", PrimitiveType.Cube, acampamento, new Vector3(0f, 4.45f, 17f), new Vector3(2.9f, 0.9f, 0.08f), madeira, false);
        Montar.Peca("PlacaDaCompanhia", PrimitiveType.Quad, acampamento, new Vector3(0f, 4.45f, 16.95f), new Vector3(2.7f, 0.74f, 1f), Montar.PegarMaterial("Placa4"), false);
        Montar.Peca("PosteDaInterdicao", PrimitiveType.Cube, acampamento, new Vector3(-2.4f, 0.8f, 16.3f), new Vector3(0.1f, 1.6f, 0.1f), madeira, true).transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
        Montar.Peca("TabuaDaInterdicao", PrimitiveType.Cube, acampamento, new Vector3(-2.45f, 1.45f, 16.26f), new Vector3(1.5f, 0.6f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
        Montar.Peca("Interdicao", PrimitiveType.Quad, acampamento, new Vector3(-2.45f, 1.45f, 16.22f), new Vector3(1.4f, 0.5f, 1f), Montar.PegarMaterial("Placa5"), false).transform.localRotation = Quaternion.Euler(0f, 0f, 6f);
        for (int i = 0; i < 4; i++)
            Montar.Peca("Torre", PrimitiveType.Cube, acampamento, new Vector3(i % 2 == 0 ? -1.4f : 1.4f, 2.5f, i < 2 ? 17.1f : 19.9f), new Vector3(0.25f, 5f, 0.25f), madeira, true);
        Montar.Peca("Torre", PrimitiveType.Cube, acampamento, new Vector3(0f, 5.1f, 18.5f), new Vector3(3.3f, 0.25f, 3.3f), madeira, false);

        Musica musica = acampamento.gameObject.AddComponent<Musica>();
        musica.calma = MontarSons.Pegar("musica_superficie");
        musica.volumeDaCalma = 0.35f;

        Montar.CriarJogador(new Vector3(0f, 0f, -3f), Vector3.forward);

        EditorSceneManager.SaveScene(cena, CaminhoSuperficie);
        Montar.OrdenarBuild();
        Debug.Log("Cena Superficie pronta.");
    }

    // Etapa 122: uma casa de Ouro Fundo, caiada, de telhado de duas águas, com a porta e as janelas pregadas com tábuas.
    // A frente da casa é o lado do z negativo; "giro" vira a casa em volta do eixo de pé.
    static void Casa(Transform pai, Vector3 lugar, float giro)
    {
        Transform casa = new GameObject("Casa").transform;
        casa.SetParent(pai, false);
        casa.localPosition = lugar;
        casa.localRotation = Quaternion.Euler(0f, giro, 0f);
        Material cal = Montar.PegarMaterial("Cal"), telha = Montar.PegarMaterial("Telha"), madeira = Montar.PegarMaterial("Madeira"), escuro = Montar.PegarMaterial("Escuro");
        Montar.Peca("Paredes", PrimitiveType.Cube, casa, new Vector3(0f, 1.5f, 0f), new Vector3(6f, 3f, 5f), cal, true);
        Montar.Peca("Empena", PrimitiveType.Cube, casa, new Vector3(0f, 3.35f, 0f), new Vector3(3f, 0.7f, 4.9f), cal, false);
        Montar.Peca("Telhado", PrimitiveType.Cube, casa, new Vector3(-1.6f, 3.6f, 0f), new Vector3(3.7f, 0.16f, 5.6f), telha, false).transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
        Montar.Peca("Telhado", PrimitiveType.Cube, casa, new Vector3(1.6f, 3.6f, 0f), new Vector3(3.7f, 0.16f, 5.6f), telha, false).transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
        Montar.Peca("Porta", PrimitiveType.Cube, casa, new Vector3(0f, 1f, -2.52f), new Vector3(1f, 2f, 0.08f), escuro, false);
        Montar.Peca("Tabua", PrimitiveType.Cube, casa, new Vector3(0f, 1.1f, -2.58f), new Vector3(1.7f, 0.16f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
        Montar.Peca("Tabua", PrimitiveType.Cube, casa, new Vector3(0f, 1.1f, -2.6f), new Vector3(1.7f, 0.16f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, -38f);
        foreach (float x in new[] { -1.9f, 1.9f })
        {
            Montar.Peca("Janela", PrimitiveType.Cube, casa, new Vector3(x, 1.6f, -2.52f), new Vector3(0.9f, 0.9f, 0.08f), escuro, false);
            Montar.Peca("Tabua", PrimitiveType.Cube, casa, new Vector3(x, 1.8f, -2.58f), new Vector3(1.2f, 0.14f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, 5f);
            Montar.Peca("Tabua", PrimitiveType.Cube, casa, new Vector3(x, 1.42f, -2.58f), new Vector3(1.2f, 0.14f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, -7f);
        }
    }

    // Etapa 122: a igreja, com a torre na frente e a cruz em cima. A porta também está pregada.
    static void Igreja(Transform pai, Vector3 lugar, float giro)
    {
        Transform igreja = new GameObject("Igreja").transform;
        igreja.SetParent(pai, false);
        igreja.localPosition = lugar;
        igreja.localRotation = Quaternion.Euler(0f, giro, 0f);
        Material cal = Montar.PegarMaterial("Cal"), telha = Montar.PegarMaterial("Telha"), madeira = Montar.PegarMaterial("Madeira"), escuro = Montar.PegarMaterial("Escuro");
        Montar.Peca("Nave", PrimitiveType.Cube, igreja, new Vector3(0f, 2.5f, 0.5f), new Vector3(6f, 5f, 8f), cal, true);
        Montar.Peca("Empena", PrimitiveType.Cube, igreja, new Vector3(0f, 5.4f, 0.5f), new Vector3(3f, 0.8f, 7.9f), cal, false);
        Montar.Peca("Telhado", PrimitiveType.Cube, igreja, new Vector3(-1.6f, 5.65f, 0.5f), new Vector3(3.8f, 0.18f, 8.6f), telha, false).transform.localRotation = Quaternion.Euler(0f, 0f, 27f);
        Montar.Peca("Telhado", PrimitiveType.Cube, igreja, new Vector3(1.6f, 5.65f, 0.5f), new Vector3(3.8f, 0.18f, 8.6f), telha, false).transform.localRotation = Quaternion.Euler(0f, 0f, -27f);
        Montar.Peca("Torre", PrimitiveType.Cube, igreja, new Vector3(0f, 4.5f, -4.5f), new Vector3(2.6f, 9f, 2.6f), cal, true);
        Montar.Peca("Chapeu", PrimitiveType.Cube, igreja, new Vector3(0f, 9.2f, -4.5f), new Vector3(3f, 0.4f, 3f), telha, false);
        Montar.Peca("Cruz", PrimitiveType.Cube, igreja, new Vector3(0f, 10.2f, -4.5f), new Vector3(0.16f, 1.6f, 0.16f), madeira, false);
        Montar.Peca("Cruz", PrimitiveType.Cube, igreja, new Vector3(0f, 10.5f, -4.5f), new Vector3(0.9f, 0.16f, 0.16f), madeira, false);
        Montar.Peca("Sineira", PrimitiveType.Cube, igreja, new Vector3(0f, 7.4f, -5.82f), new Vector3(0.9f, 1.2f, 0.08f), escuro, false);
        Montar.Peca("Porta", PrimitiveType.Cube, igreja, new Vector3(0f, 1.3f, -5.82f), new Vector3(1.4f, 2.6f, 0.08f), escuro, false);
        Montar.Peca("Tabua", PrimitiveType.Cube, igreja, new Vector3(0f, 1.4f, -5.88f), new Vector3(2.2f, 0.18f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, 40f);
        Montar.Peca("Tabua", PrimitiveType.Cube, igreja, new Vector3(0f, 1.4f, -5.9f), new Vector3(2.2f, 0.18f, 0.05f), madeira, false).transform.localRotation = Quaternion.Euler(0f, 0f, -40f);
    }

    // Etapa 140: uma cerca de mourões e duas réguas, de um ponto a outro. Algumas réguas estão caídas.
    static void Cerca(Transform pai, Vector3 de, Vector3 ate)
    {
        Material madeira = Montar.PegarMaterial("Madeira");
        Transform cerca = new GameObject("Cerca").transform;
        cerca.SetParent(pai, false);
        cerca.localPosition = de;
        cerca.localRotation = Quaternion.LookRotation((ate - de).normalized);
        int vaos = Mathf.Max(1, Mathf.RoundToInt(Vector3.Distance(de, ate) / 2f));
        float passo = Vector3.Distance(de, ate) / vaos;
        for (int i = 0; i <= vaos; i++)
        {
            Montar.Peca("Mourao", PrimitiveType.Cube, cerca, new Vector3(0f, 0.6f, i * passo), new Vector3(0.12f, 1.2f, 0.12f), madeira, true);
            if (i == vaos) break;
            bool caida = i % 5 == 3;
            Montar.Peca("Regua", PrimitiveType.Cube, cerca, new Vector3(0f, 1f, (i + 0.5f) * passo), new Vector3(0.05f, 0.1f, passo), madeira, false);
            GameObject deBaixo = Montar.Peca("Regua", PrimitiveType.Cube, cerca, new Vector3(caida ? 0.3f : 0f, caida ? 0.3f : 0.55f, (i + 0.5f) * passo), new Vector3(0.05f, 0.1f, passo), madeira, false);
            if (caida) deBaixo.transform.localRotation = Quaternion.Euler(18f, 12f, 0f);
        }
    }

    // Um poste de luz da rua, apagado.
    static void Poste(Transform pai, Vector3 lugar)
    {
        Material madeira = Montar.PegarMaterial("Madeira");
        Montar.Peca("Poste", PrimitiveType.Cube, pai, lugar + Vector3.up * 2.4f, new Vector3(0.16f, 4.8f, 0.16f), madeira, true);
        Montar.Peca("Braco", PrimitiveType.Cube, pai, lugar + new Vector3(0.35f, 4.6f, 0f), new Vector3(0.8f, 0.08f, 0.08f), madeira, false);
        Montar.Peca("LampadaApagada", PrimitiveType.Sphere, pai, lugar + new Vector3(0.7f, 4.45f, 0f), Vector3.one * 0.22f, Montar.PegarMaterial("Metal"), false);
    }

    // Etapa 48: o menu, na frente de um pedaço de túnel com um lampião no chão e dois olhos no fundo.
    internal static void ConstruirMenu()
    {
        Montar.Preparar();
        var cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Montar.Escurecer();
        Montar.CriarClima();

        Material rocha = Montar.PegarMaterial("Rocha");
        Material madeira = Montar.PegarMaterial("Madeira");
        Transform tunel = new GameObject("Tunel").transform;
        Montar.Peca("Chao", PrimitiveType.Cube, tunel, new Vector3(0f, -0.5f, 8f), new Vector3(3f, 1f, 24f), rocha, false);
        Montar.Peca("Teto", PrimitiveType.Cube, tunel, new Vector3(0f, 3.5f, 8f), new Vector3(3f, 1f, 24f), rocha, false);
        Montar.Peca("Parede", PrimitiveType.Cube, tunel, new Vector3(-2f, 1.5f, 8f), new Vector3(1f, 3f, 24f), rocha, false);
        Montar.Peca("Parede", PrimitiveType.Cube, tunel, new Vector3(2f, 1.5f, 8f), new Vector3(1f, 3f, 24f), rocha, false);
        for (int i = 0; i < 4; i++)
        {
            float z = 1f + i * 4f;
            Montar.Peca("Poste", PrimitiveType.Cube, tunel, new Vector3(-1.3f, 1.5f, z), new Vector3(0.25f, 3f, 0.25f), madeira, false);
            Montar.Peca("Poste", PrimitiveType.Cube, tunel, new Vector3(1.3f, 1.5f, z), new Vector3(0.25f, 3f, 0.25f), madeira, false);
            Montar.Peca("Travessa", PrimitiveType.Cube, tunel, new Vector3(0f, 2.85f, z), new Vector3(3f, 0.28f, 0.3f), madeira, false);
        }
        GameObject lampiao = Montar.Peca("Lampiao", PrimitiveType.Cylinder, tunel, new Vector3(0.7f, 0.12f, 2.2f), new Vector3(0.14f, 0.12f, 0.14f), Montar.PegarMaterial("Chama"), false);
        LuzViva chama = lampiao.AddComponent<LuzViva>();
        chama.modo = LuzViva.Modo.Piscar;
        chama.ritmo = 0.5f;
        chama.luz = Montar.Luz("Luz", tunel, new Vector3(0.7f, 0.5f, 2.2f), LightType.Point, new Color(1f, 0.7f, 0.4f), 5f, 9f);
        chama.luz.shadows = LightShadows.Soft;
        Montar.Peca("Olho", PrimitiveType.Sphere, tunel, new Vector3(-0.1f, 0.55f, 13f), Vector3.one * 0.08f, Montar.PegarMaterial("Olhos"), false);
        Montar.Peca("Olho", PrimitiveType.Sphere, tunel, new Vector3(0.1f, 0.55f, 13f), Vector3.one * 0.08f, Montar.PegarMaterial("Olhos"), false);

        GameObject objeto = new GameObject("Menu");
        objeto.transform.position = new Vector3(0f, 1.5f, -1.5f);
        Montar.CriarCamera(objeto);
        objeto.AddComponent<Menu>();
        Musica musica = objeto.AddComponent<Musica>();
        musica.calma = MontarSons.Pegar("musica_mina");

        EditorSceneManager.SaveScene(cena, CaminhoMenu);
        Montar.OrdenarBuild();
        Debug.Log("Cena Menu pronta. Ela é a primeira do jogo.");
    }

    // Etapas 45 e 57: as 5 melhorias. Se já existem, ficam como estão, com os valores que você ajustou.
    static void CriarMelhorias()
    {
        CriarMelhoria("Lampiao", "Lampião", "s de óleo", new[] { 10, 25, 50 }, new[] { 120f, 170f, 230f, 300f });
        CriarMelhoria("Picareta", "Picareta", "s por golpe", new[] { 10, 25, 50 }, new[] { 0.6f, 0.5f, 0.42f, 0.35f });
        CriarMelhoria("Mochila", "Mochila", "de minério", new[] { 15, 35, 60 }, new[] { 20f, 35f, 55f, 80f });
        CriarMelhoria("Botas", "Botas", "s de corrida", new[] { 10, 25, 50 }, new[] { 5f, 7f, 9f, 12f });
        CriarMelhoria("Colete", "Colete", "de vida", new[] { 20, 40, 70 }, new[] { 5f, 6f, 8f, 10f });
    }

    static void CriarMelhoria(string arquivo, string nome, string unidade, int[] custo, float[] valor)
    {
        string caminho = "Assets/Resources/Melhorias/" + arquivo + ".asset";
        if (AssetDatabase.LoadAssetAtPath<Melhoria>(caminho) != null) return;
        Melhoria melhoria = ScriptableObject.CreateInstance<Melhoria>();
        melhoria.nome = nome;
        melhoria.unidade = unidade;
        melhoria.custo = custo;
        melhoria.valor = valor;
        AssetDatabase.CreateAsset(melhoria, caminho);
    }
}
