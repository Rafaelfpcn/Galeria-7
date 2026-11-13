using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// O que os scripts de montagem têm em comum: o menu Montar tudo, as peças simples, os materiais, o jogador e as pastas.
public static class Montar
{
    internal const float Celula = 3f;   // cada letra dos mapas vira um quadrado de 3 x 3 m
    internal const float Teto = 3f;     // altura do teto
    internal const float Base = -2f;    // fundo da rocha, abaixo de todos os chãos
    internal const float Fundo = -1.5f; // chão da parte baixa

    // Aumente quando as peças geradas mudarem: o menu pergunta se pode refazer as antigas.
    const int Versao = 14;
    const string ArquivoDaVersao = "Assets/Dados/versao.txt";

    // Vizinhos de uma célula: norte, sul, leste e oeste.
    internal static readonly int[] PassoLinha = { -1, 1, 0, 0 };
    internal static readonly int[] PassoColuna = { 0, 0, 1, -1 };
    internal static readonly Vector3[] Direcao = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left };

    [MenuItem("Galeria 7/Montar tudo", priority = 0)]
    static void MenuTudo()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Galeria 7", "Saia do modo Play antes de montar as cenas.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!EditorUtility.DisplayDialog("Galeria 7", "Recriar as cenas Teste, Mina, Superficie e Menu?", "Recriar", "Cancelar")) return;
        ConferirVersao();
        Tudo();
    }

    // Recria as quatro cenas. Termina com o Menu aberto, porque o Play começa por ele.
    public static void Tudo()
    {
        MontarCenaDeTeste.Construir();
        MontarMina.Construir();
        MontarSuperficie.ConstruirSuperficie();
        MontarSuperficie.ConstruirMenu();
    }

    // Apaga as peças geradas e monta tudo de novo, sem perguntar nada. Serve para a linha de comando:
    // Unity.exe -batchmode -quit -projectPath <pasta do projeto> -executeMethod Montar.RefazerTudo
    public static void RefazerTudo()
    {
        ApagarPecas();
        Tudo();
    }

    // Não monta durante o Play, guarda a cena aberta e pergunta antes de substituir uma cena que já existe.
    internal static bool PodeMontar(string caminhoCena)
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Galeria 7", "Saia do modo Play antes de montar a cena.", "OK");
            return false;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(caminhoCena) != null &&
            !EditorUtility.DisplayDialog("Galeria 7", "A cena " + Path.GetFileNameWithoutExtension(caminhoCena) + " já existe e será substituída. Continuar?", "Substituir", "Cancelar"))
            return false;
        ConferirVersao();
        return true;
    }

    // As peças (prefabs, materiais, texturas, modelos e sons) só são criadas quando faltam, para não apagar seus ajustes.
    // Quando o código que cria as peças muda, as antigas ficam desatualizadas: aí o menu oferece refazer todas.
    static void ConferirVersao()
    {
        int atual = 0;
        if (File.Exists(ArquivoDaVersao)) int.TryParse(File.ReadAllText(ArquivoDaVersao), out atual);
        if (atual == Versao) return;
        bool refazer = AssetDatabase.IsValidFolder("Assets/Prefabs") && EditorUtility.DisplayDialog("Galeria 7",
            "As peças do jogo (inimigos, salas, itens, materiais, sons e a tabela de dificuldade) mudaram nesta versão do código. Refazer todas? O que você ajustou nelas será perdido.",
            "Refazer", "Manter as atuais");
        if (refazer) ApagarPecas();
        else AnotarVersao();
    }

    static void ApagarPecas()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); // solta as peças da cena aberta
        foreach (string caminho in new[] { "Assets/Prefabs", "Assets/Materials", "Assets/Dados", "Assets/Models", "Assets/Textures", "Assets/Audio", "Assets/Settings/Galeria7Clima.asset" })
            AssetDatabase.DeleteAsset(caminho);
        modelosProntos.Clear(); // os materiais foram apagados: os modelos precisam ser ligados aos novos
        AnotarVersao();
    }

    static void AnotarVersao()
    {
        CriarPasta("Dados");
        File.WriteAllText(ArquivoDaVersao, Versao.ToString());
        AssetDatabase.ImportAsset(ArquivoDaVersao);
    }

    // Chamado no começo de cada montagem: pastas, visual retrô e nome do jogo.
    internal static void Preparar()
    {
        foreach (string pasta in new[] { "Scenes", "Materials", "Prefabs", "Prefabs/Salas", "Resources", "Resources/Melhorias", "Dados", "Models", "Textures", "Audio" })
            CriarPasta(pasta);
        MontarArte.EstiloRetro();
        MontarArte.CoresDePS1();
        MontarArte.Identidade();
    }

    static void CriarPasta(string caminho)
    {
        if (AssetDatabase.IsValidFolder("Assets/" + caminho)) return;
        string pai = Path.GetDirectoryName("Assets/" + caminho).Replace('\\', '/');
        AssetDatabase.CreateFolder(pai, Path.GetFileName(caminho));
    }

    // Um prefab de Assets/Prefabs. Se ainda não existe, "criar" monta o objeto e ele é salvo.
    internal static GameObject Prefab(string nome, System.Func<GameObject> criar)
    {
        string caminho = "Assets/Prefabs/" + nome + ".prefab";
        GameObject pronto = AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
        if (pronto != null) return pronto;
        GameObject objeto = criar();
        objeto.name = Path.GetFileName(nome);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(objeto, caminho);
        Object.DestroyImmediate(objeto);
        return prefab;
    }

    // Cria um bloco simples. comColisao = false para peças só visuais, como o lampião e a picareta.
    internal static GameObject Peca(string nome, PrimitiveType tipo, Transform pai, Vector3 posicao, Vector3 tamanho, Material material, bool comColisao)
    {
        GameObject peca = GameObject.CreatePrimitive(tipo);
        peca.name = nome;
        peca.transform.SetParent(pai, false);
        peca.transform.localPosition = posicao;
        peca.transform.localScale = tamanho;
        // Os cubos ganham uma malha com a textura repetida conforme o tamanho, para os pixels terem sempre o mesmo tamanho.
        if (tipo == PrimitiveType.Cube) peca.GetComponent<MeshFilter>().sharedMesh = MontarArte.Cubo(tamanho);
        Renderer visual = peca.GetComponent<Renderer>();
        visual.sharedMaterial = material;
        if (!comColisao)
        {
            Object.DestroyImmediate(peca.GetComponent<Collider>());
            visual.shadowCastingMode = ShadowCastingMode.Off;
        }
        return peca;
    }

    // Etapas 126 a 128: um modelo feito no Blender, de Assets/Modelos3D. Troca os materiais que vieram no arquivo
    // pelos materiais do jogo com o mesmo nome. Os modelos são gerados pelos scripts da pasta Blender do projeto.
    // Põe uma cópia do modelo dentro de 'pai'. Sem sombra é para o que fica colado numa luz, como a mão do lampião.
    internal static Transform Instalar(string nome, Transform pai, bool comSombra)
    {
        GameObject copia = (GameObject)PrefabUtility.InstantiatePrefab(Modelo3D(nome), pai);
        copia.name = "Modelo";
        if (!comSombra)
            foreach (Renderer visual in copia.GetComponentsInChildren<Renderer>()) visual.shadowCastingMode = ShadowCastingMode.Off;
        return copia.transform;
    }

    // Os modelos já acertados nesta montagem: reimportar um modelo demora, e algumas cenas usam o mesmo várias vezes.
    static readonly Dictionary<string, GameObject> modelosProntos = new Dictionary<string, GameObject>();

    internal static GameObject Modelo3D(string nome)
    {
        GameObject pronto;
        if (modelosProntos.TryGetValue(nome, out pronto) && pronto != null) return pronto;
        pronto = ImportarModelo(nome);
        if (pronto != null) modelosProntos[nome] = pronto;
        return pronto;
    }

    static GameObject ImportarModelo(string nome)
    {
        string caminho = "Assets/Modelos3D/" + nome + ".fbx";
        ModelImporter importador = AssetImporter.GetAtPath(caminho) as ModelImporter;
        if (importador == null)
        {
            Debug.LogError("Falta o modelo " + caminho + ". Ele é gerado pelo script Blender/personagens.py.");
            return null;
        }
        importador.bakeAxisConversion = true; // o Blender usa Z para cima; isto deixa todas as peças sem giro na Unity
        importador.importAnimation = false;
        importador.importCameras = false;
        importador.importLights = false;
        // Tira as trocas de material antigas antes: se os materiais foram apagados e refeitos, elas apontam para o nada.
        foreach (AssetImporter.SourceAssetIdentifier antigo in new List<AssetImporter.SourceAssetIdentifier>(importador.GetExternalObjectMap().Keys))
            importador.RemoveRemap(antigo);
        importador.SaveAndReimport();
        foreach (Renderer visual in AssetDatabase.LoadAssetAtPath<GameObject>(caminho).GetComponentsInChildren<Renderer>())
            foreach (Material material in visual.sharedMaterials)
                if (material != null) importador.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), material.name), PegarMaterial(material.name));
        importador.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<GameObject>(caminho);
    }

    // Uma peça com um dos modelos feitos por código (cone, pedra ou cristal). Não tem colisão.
    internal static GameObject Modelo(string nome, Mesh malha, Transform pai, Vector3 posicao, Vector3 tamanho, Material material)
    {
        GameObject peca = new GameObject(nome);
        peca.transform.SetParent(pai, false);
        peca.transform.localPosition = posicao;
        peca.transform.localScale = tamanho;
        peca.AddComponent<MeshFilter>().sharedMesh = malha;
        peca.AddComponent<MeshRenderer>().sharedMaterial = material;
        return peca;
    }

    internal static Light Luz(string nome, Transform pai, Vector3 posicao, LightType tipo, Color cor, float intensidade, float alcance)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(pai, false);
        objeto.transform.localPosition = posicao;
        Light luz = objeto.AddComponent<Light>();
        luz.type = tipo;
        luz.intensity = intensidade;
        luz.range = alcance;
        luz.color = cor;
        return luz;
    }

    // Som que vem de um lugar da cena e some com a distância.
    internal static AudioSource Fonte(GameObject objeto, AudioClip clip, bool emLaco, float alcance)
    {
        AudioSource fonte = objeto.AddComponent<AudioSource>();
        fonte.clip = clip;
        fonte.loop = emLaco;
        fonte.playOnAwake = emLaco;
        fonte.spatialBlend = 1f; // som 3D: de fones, dá para saber de que lado ele vem
        fonte.rolloffMode = AudioRolloffMode.Linear;
        fonte.minDistance = 1.5f;
        fonte.maxDistance = alcance;
        return fonte;
    }

    // Todos os materiais do jogo, com as cores num lugar só. Se o material já existe, fica como está.
    internal static Material PegarMaterial(string nome)
    {
        switch (nome)
        {
            case "Rocha": return CriarMaterial(nome, Color.white, 0f, "Rocha");
            case "RochaMolhada": return CriarMaterial(nome, new Color(0.5f, 0.65f, 0.85f), 0f, "Rocha", null, 0f, 0.7f);
            case "RochaNegra": return CriarMaterial(nome, Color.white, 0.5f, "Veio", "VeioBrilho");
            case "Madeira": return CriarMaterial(nome, Color.white, 0f, "Madeira");
            case "Terra": return CriarMaterial(nome, Color.white, 0f, "Terra");
            case "Minerio": return CriarMaterial(nome, Color.white, 0.3f, "Minerio", "Minerio", 0.5f, 0.5f);
            case "Metal": return CriarMaterial(nome, new Color(0.8f, 0.8f, 0.85f), 0f, "Ferro", null, 0.6f, 0.4f);
            // Etapa 142: a madeira e o ferro de cada trecho. Molhados nos níveis alagados; podres e enferrujados no veio.
            case "MadeiraMolhada": return CriarMaterial(nome, new Color(0.6f, 0.75f, 0.8f), 0f, "Madeira", null, 0f, 0.6f);
            case "KitMadeiraMolhada": return CriarMaterial(nome, new Color(0.6f, 0.75f, 0.8f), 0f, "KitMadeira", null, 0f, 0.6f);
            case "MadeiraPodre": return CriarMaterial(nome, new Color(0.45f, 0.52f, 0.4f), 0f, "Madeira");
            case "KitMadeiraPodre": return CriarMaterial(nome, new Color(0.45f, 0.52f, 0.4f), 0f, "KitMadeira");
            case "FerroEnferrujado": return CriarMaterial(nome, new Color(0.85f, 0.5f, 0.32f), 0f, "Ferro", null, 0.2f, 0.25f);
            // Etapas 143 e 145: as marcas, com transparência (o 0,99 liga a transparência do material).
            case "Mancha": return Fosco(CriarMaterial(nome, new Color(1f, 1f, 1f, 0.99f), 0f, "Mancha"));
            case "SetaDeGiz": return Fosco(CriarMaterial(nome, new Color(1f, 1f, 1f, 0.99f), 0f, "SetaDeGiz"));
            case "NumeroDeGiz": return Fosco(CriarMaterial(nome, new Color(1f, 1f, 1f, 0.99f), 0f, "Giz1"));
            case "Onda": return Fosco(CriarMaterial(nome, new Color(1f, 1f, 1f, 0.99f), 0f, "Onda"));
            case "Chama": return CriarMaterial(nome, new Color(1f, 0.7f, 0.35f), 1f);
            case "Rastejante": return CriarMaterial(nome, Color.white, 0f, "PeleRastejante");
            case "Ouvinte": return CriarMaterial(nome, Color.white, 0f, "PeleOuvinte");
            case "Roupa": return CriarMaterial(nome, Color.white, 0f, "Macacao");
            case "Pele": return CriarMaterial(nome, Color.white, 0f, "PeleMorta");
            case "Olhos": return CriarMaterial(nome, new Color(1f, 0.15f, 0.05f), 2f);
            case "Armadilha": return CriarMaterial(nome, new Color(0.8f, 0.05f, 0.05f), 1f);
            // Os itens brilham de leve, para dar para achar no escuro.
            case "Kit": return CriarMaterial(nome, new Color(0.9f, 0.9f, 0.85f), 0.6f);
            case "Frasco": return CriarMaterial(nome, new Color(1f, 0.6f, 0.2f), 0.6f);
            case "Sinal": return CriarMaterial(nome, new Color(1f, 0.2f, 0.1f), 0.8f);
            case "Fita": return CriarMaterial(nome, new Color(0.9f, 0.85f, 0.6f), 0.7f);
            case "Agua": return CriarMaterial(nome, new Color(0.1f, 0.25f, 0.35f, 0.45f), 0f, null, null, 0f, 0.9f);
            case "Cristal": return CriarMaterial(nome, new Color(0.3f, 1f, 0.5f), 2.5f);
            case "Coracao": return CriarMaterial(nome, new Color(0.25f, 0.03f, 0.05f), 0.4f);
            case "Aviso": return CriarMaterial(nome, new Color(1f, 0.1f, 0.05f), 3f);
            case "Tentaculo": return CriarMaterial(nome, new Color(0.03f, 0.03f, 0.035f), 0f, null, null, 0f, 0.6f);
            case "Placa1": return CriarMaterial(nome, Color.white, 0.35f, "Placa1", "Placa1");
            case "Placa2": return CriarMaterial(nome, Color.white, 0.35f, "Placa2", "Placa2");
            // Etapas 110 e 113: os objetos que contam a história.
            case "Placa3":
            case "Placa4":
            case "Placa5":
            case "Placa6":
            case "Placa10":
            case "Placa20":
            case "Placa30": return CriarMaterial(nome, Color.white, 0.35f, nome, nome);
            // Etapas 133 a 137: a madeira e o ferro do pacote de mina, com as texturas dele reduzidas a 128 pixels, e os
            // papéis do quadro de avisos.
            case "KitMadeira": return CriarMaterial(nome, Color.white, 0f, "KitMadeira");
            case "KitFerro": return CriarMaterial(nome, new Color(0.7f, 0.7f, 0.74f), 0f, "KitFerro", null, 0.5f, 0.35f);
            case "Cartaz":
            case "Calendario": return CriarMaterial(nome, Color.white, 0.3f, nome, nome);
            // Etapa 122: as casas de Ouro Fundo.
            case "Cal": return CriarMaterial(nome, new Color(0.62f, 0.6f, 0.54f));
            case "Telha": return CriarMaterial(nome, new Color(0.38f, 0.16f, 0.1f));
            case "Foto": return CriarMaterial(nome, Color.white, 0.3f, "Foto", "Foto");
            case "Papel": return CriarMaterial(nome, new Color(0.85f, 0.8f, 0.65f), 0.25f);
            case "Latao": return CriarMaterial(nome, new Color(0.75f, 0.58f, 0.2f), 0.15f, null, null, 0.8f, 0.5f);
            case "Manto": return CriarMaterial(nome, new Color(0.5f, 0.08f, 0.08f));
            case "Dinamite": return CriarMaterial(nome, new Color(0.55f, 0.14f, 0.08f));
            default: return CriarMaterial("Escuro", new Color(0.02f, 0.02f, 0.02f));
        }
    }

    // Para as marcas transparentes (giz, mancha, onda): sem reflexo nenhum. Com reflexo, a parte transparente da
    // folha aparece como um retângulo acinzentado quando a luz do lampião bate.
    static Material Fosco(Material material)
    {
        material.SetFloat("_BlendModePreserveSpecular", 0f);
        material.SetFloat("_SpecularHighlights", 0f);
        material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
        material.SetFloat("_EnvironmentReflections", 0f);
        material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        EditorUtility.SetDirty(material);
        return material;
    }

    // brilho: quanto o material emite luz própria. textura e texturaDoBrilho: nomes de texturas do MontarArte.
    // Uma cor com transparência (alfa menor que 1) faz um material transparente.
    static Material CriarMaterial(string nome, Color cor, float brilho = 0f, string textura = null, string texturaDoBrilho = null, float metal = 0f, float liso = 0.2f)
    {
        string caminho = "Assets/Materials/" + nome + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (material != null) return material;

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = cor;
        material.SetFloat("_Metallic", metal);
        material.SetFloat("_Smoothness", liso);
        if (textura != null) material.mainTexture = MontarArte.Textura(textura);
        if (brilho > 0f)
        {
            Texture2D mapa = texturaDoBrilho != null ? MontarArte.Textura(texturaDoBrilho) : null;
            material.EnableKeyword("_EMISSION");
            if (mapa != null) material.SetTexture("_EmissionMap", mapa);
            material.SetColor("_EmissionColor", (mapa != null ? Color.white : cor) * brilho);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive; // sem isso, o URP apaga o brilho ao importar
        }
        if (cor.a < 1f)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }
        AssetDatabase.CreateAsset(material, caminho);
        return material;
    }

    // Etapa 15: escuridão total. Sem céu, sem sol e com luz ambiente quase preta.
    internal static void Escurecer()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.012f);
        RenderSettings.reflectionIntensity = 0f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = 0.06f;
    }

    // Etapa 74: o pós-processamento da cena (vinheta, granulado, cor e brilho das luzes).
    internal static void CriarClima()
    {
        GameObject clima = new GameObject("Clima");
        Volume volume = clima.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = MontarArte.PerfilDeClima();
        clima.AddComponent<Clima>();
    }

    internal static Camera CriarCamera(GameObject objeto)
    {
        Camera cam = objeto.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.05f;
        cam.fieldOfView = 70f;
        cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        objeto.AddComponent<AudioListener>();
        return cam;
    }

    internal static Jogador CriarJogador(Vector3 posicao, Vector3 olharPara)
    {
        GameObject jogador = new GameObject("Jogador");
        jogador.transform.position = posicao + Vector3.up * 0.05f;
        jogador.transform.rotation = Quaternion.LookRotation(olharPara);

        CharacterController corpo = jogador.AddComponent<CharacterController>();
        corpo.height = 1.8f;
        corpo.radius = 0.35f;
        corpo.center = new Vector3(0f, 0.9f, 0f);
        corpo.stepOffset = 0.35f;
        Jogador controle = jogador.AddComponent<Jogador>(); // traz junto a Vida
        controle.sinalizador = MontarPecas.SinalizadorAceso();

        GameObject olho = new GameObject("Camera");
        olho.tag = "MainCamera";
        olho.transform.SetParent(jogador.transform, false);
        olho.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        CriarCamera(olho);
        Interacao interacao = olho.AddComponent<Interacao>();
        controle.cabeca = olho.transform;

        // Lampião na mão esquerda: a luz sai de fora da câmera, então as sombras aparecem.
        Color quente = new Color(1f, 0.78f, 0.5f);
        GameObject objetoLampiao = new GameObject("Lampiao");
        objetoLampiao.transform.SetParent(olho.transform, false);
        objetoLampiao.transform.localPosition = new Vector3(-0.32f, -0.25f, 0.42f);
        // Uma luz fraca e curta junto do rosto: sem ela, as mãos ficam pretas, porque a luz do lampião sai para a frente.
        Luz("LuzDasMaos", olho.transform, new Vector3(0f, -0.05f, 0.05f), LightType.Point, quente, 0.5f, 1.2f);
        Instalar("LampiaoDeMao", objetoLampiao.transform, false); // etapa 125: a mão, a manga e o lampião, feitos no Blender
        Light forte = Luz("LuzForte", objetoLampiao.transform, Vector3.zero, LightType.Spot, quente, 12f, 18f);
        forte.spotAngle = 75f;
        forte.innerSpotAngle = 35f;
        forte.shadows = LightShadows.Soft;
        Lampiao lampiao = objetoLampiao.AddComponent<Lampiao>();
        lampiao.luzForte = forte;
        lampiao.luzFraca = Luz("LuzFraca", objetoLampiao.transform, Vector3.zero, LightType.Point, quente, 1.5f, 6f);
        lampiao.poeira = MontarPecas.Poeira(objetoLampiao.transform);

        // Picareta na mão direita: cabo para cima, ponta no plano do golpe.
        GameObject objetoPicareta = new GameObject("Picareta");
        objetoPicareta.transform.SetParent(olho.transform, false);
        objetoPicareta.transform.localPosition = new Vector3(0.35f, -0.4f, 0.5f);
        objetoPicareta.transform.localRotation = Quaternion.Euler(20f, -10f, 0f);
        Instalar("PicaretaDeMao", objetoPicareta.transform, false);
        objetoPicareta.AddComponent<AudioSource>().playOnAwake = false;
        Picareta picareta = objetoPicareta.AddComponent<Picareta>();
        picareta.olho = olho.transform;
        picareta.faisca = MontarPecas.Faisca();
        picareta.somNoAr = MontarSons.Pegar("picareta_ar");
        picareta.somNaPedra = MontarSons.Pegar("picareta_pedra");
        picareta.somNaCarne = MontarSons.Pegar("golpe_na_carne");

        SonsDoJogador sons = jogador.AddComponent<SonsDoJogador>();
        sons.passoNaPedra = MontarSons.Pegar("passo_pedra");
        sons.passoNaMadeira = MontarSons.Pegar("passo_madeira");
        sons.passoNaAgua = MontarSons.Pegar("passo_agua");
        sons.onda = MontarPecas.AnelDeAgua();
        sons.respiracao = MontarSons.Pegar("respiracao");

        Hud hud = jogador.AddComponent<Hud>();
        hud.jogador = controle;
        hud.lampiao = lampiao;
        hud.interacao = interacao;
        jogador.AddComponent<Pausa>();
        return controle;
    }

    // Etapas 41 e 44: uma gaiola de madeira e metal. Olhar para ela e apertar E sobe ou desce.
    internal static GameObject CriarElevador(Transform pai, bool sobe)
    {
        GameObject gaiola = new GameObject(sobe ? "ElevadorQueSobe" : "ElevadorDaMina");
        gaiola.transform.SetParent(pai, false);
        BoxCollider colisao = gaiola.AddComponent<BoxCollider>();
        colisao.center = new Vector3(0f, 1.3f, 0f);
        colisao.size = new Vector3(1.6f, 2.6f, 1.6f);
        Elevador elevador = gaiola.AddComponent<Elevador>();
        elevador.sobe = sobe;
        elevador.aviso = sobe ? "E: subir e guardar o minério" : "E: descer para a mina";
        Material madeira = PegarMaterial("Madeira");
        Material metal = PegarMaterial("Metal");
        Peca("Piso", PrimitiveType.Cube, gaiola.transform, new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.1f, 1.6f), madeira, false);
        Peca("Cobertura", PrimitiveType.Cube, gaiola.transform, new Vector3(0f, 2.55f, 0f), new Vector3(1.6f, 0.1f, 1.6f), madeira, false);
        for (int i = 0; i < 4; i++)
        {
            Vector3 canto = new Vector3(i % 2 == 0 ? -0.75f : 0.75f, 1.3f, i < 2 ? -0.75f : 0.75f);
            Peca("Coluna", PrimitiveType.Cube, gaiola.transform, canto, new Vector3(0.1f, 2.5f, 0.1f), metal, false);
        }
        // Guarda-corpo dos lados e do fundo. A frente, por onde se entra, fica aberta.
        Peca("Barra", PrimitiveType.Cube, gaiola.transform, new Vector3(0f, 1.1f, 0.75f), new Vector3(1.5f, 0.05f, 0.05f), metal, false);
        Peca("Barra", PrimitiveType.Cube, gaiola.transform, new Vector3(-0.75f, 1.1f, 0f), new Vector3(0.05f, 0.05f, 1.5f), metal, false);
        Peca("Barra", PrimitiveType.Cube, gaiola.transform, new Vector3(0.75f, 1.1f, 0f), new Vector3(0.05f, 0.05f, 1.5f), metal, false);
        if (!sobe)
        {
            // Etapa 118: o que só entra na descida. A grade fecha a frente, e um chão e quatro paredes invisíveis
            // seguram o jogador dentro da gaiola enquanto o mundo sobe em volta.
            GameObject grade = new GameObject("Grade");
            grade.transform.SetParent(gaiola.transform, false);
            for (int i = 0; i < 5; i++)
                Peca("Barra", PrimitiveType.Cube, grade.transform, new Vector3(-0.56f + i * 0.28f, 1.3f, -0.75f), new Vector3(0.04f, 2.4f, 0.04f), metal, false);
            Peca("Travessa", PrimitiveType.Cube, grade.transform, new Vector3(0f, 1.1f, -0.75f), new Vector3(1.5f, 0.05f, 0.05f), metal, false);
            Vector3[] centros = { new Vector3(0f, 0f, 0f), new Vector3(0f, 1.4f, 0.85f), new Vector3(0f, 1.4f, -0.85f), new Vector3(0.85f, 1.4f, 0f), new Vector3(-0.85f, 1.4f, 0f) };
            Vector3[] tamanhos = { new Vector3(1.9f, 0.2f, 1.9f), new Vector3(1.9f, 2.8f, 0.1f), new Vector3(1.9f, 2.8f, 0.1f), new Vector3(0.1f, 2.8f, 1.9f), new Vector3(0.1f, 2.8f, 1.9f) };
            for (int i = 0; i < centros.Length; i++)
            {
                BoxCollider parede = grade.AddComponent<BoxCollider>();
                parede.center = centros[i];
                parede.size = tamanhos[i];
            }
            grade.SetActive(false);
            elevador.grade = grade;
        }
        return gaiola;
    }

    // Rampa de madeira de uma célula, ligando o chão alto ao chão baixo.
    internal static void RampaPara(Vector3 centro, Vector3 sobe, Material material, Transform pai)
    {
        float angulo = Mathf.Atan2(-Fundo, Celula) * Mathf.Rad2Deg;
        GameObject rampa = Peca("Rampa", PrimitiveType.Cube, pai, new Vector3(centro.x, Fundo / 2f - 0.15f, centro.z), new Vector3(Celula, 0.3f, Celula * 1.15f), material, true);
        rampa.transform.localRotation = Quaternion.LookRotation(sobe) * Quaternion.Euler(-angulo, 0f, 0f);
    }

    // O índice do vizinho de rocha de uma célula (norte, sul, leste ou oeste), ou -1 se não houver.
    internal static int ParedeVizinha(System.Func<int, int, char> letra, int linha, int coluna)
    {
        for (int i = 0; i < 4; i++)
            if (letra(linha + PassoLinha[i], coluna + PassoColuna[i]) == '#') return i;
        return -1;
    }

    // O veio fica na parede de rocha vizinha, metade dentro dela. Sem parede em volta, fica no meio da célula.
    internal static Vector3 NaParede(System.Func<int, int, char> letra, int linha, int coluna, Vector3 noChao)
    {
        int parede = ParedeVizinha(letra, linha, coluna);
        return parede < 0 ? noChao + Vector3.up * 0.6f : noChao + Vector3.up * 1f + Direcao[parede] * (Celula / 2f);
    }

    // Letra minúscula ou traço: parte baixa.
    internal static float Altura(char letra)
    {
        return letra == '-' || char.IsLower(letra) ? Fundo : 0f;
    }

    // Etapa 48: o Menu é a primeira cena do jogo; depois vêm Superficie, Mina e Teste. Outras cenas ficam no fim.
    internal static void OrdenarBuild()
    {
        string[] ordem = { "Assets/Scenes/Menu.unity", "Assets/Scenes/Superficie.unity", "Assets/Scenes/Mina.unity", "Assets/Scenes/Teste.unity" };
        List<EditorBuildSettingsScene> cenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        cenas.RemoveAll(c => System.Array.IndexOf(ordem, c.path) >= 0); // uma entrada antiga pode apontar para uma cena apagada
        int posicao = 0;
        foreach (string caminho in ordem)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(caminho) != null) cenas.Insert(posicao++, new EditorBuildSettingsScene(caminho, true));
        EditorBuildSettings.scenes = cenas.ToArray();
        AssetDatabase.SaveAssets(); // sem isso, a lista só vai para o disco quando a Unity fecha normalmente
    }
}
