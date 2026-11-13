using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// Etapas 42 a 47, 62 e 83: guarda o que vale entre as cenas e entre uma vez e outra que você abre o jogo.
// Ele se cria sozinho na primeira vez que alguém usa Jogo.Instancia e não some ao trocar de cena.
public class Jogo : MonoBehaviour
{
    // Etapa 47: o JsonUtility não salva Dictionary, então os níveis ficam numa lista.
    [System.Serializable]
    public class Dados
    {
        public int minerioGuardado;
        public int andarMaisFundo;
        public List<Nivel> niveis = new List<Nivel>();
        public List<int> fitas = new List<int>(); // números das fitas já encontradas
        public bool zerou;
        public Opcoes opcoes = new Opcoes();
    }

    [System.Serializable]
    public class Nivel
    {
        public string melhoria;
        public int nivel;
    }

    // Etapa 83: as opções ficam salvas junto e sobrevivem ao Novo jogo.
    [System.Serializable]
    public class Opcoes
    {
        public float volume = 1f;          // efeitos
        public float volumeMusica = 0.7f;
        public float sensibilidade = 0.1f; // graus que a câmera gira para cada pixel que o mouse anda
        public float brilho;               // de -1 (mais escuro) a 1 (mais claro)
        public bool inverterY;
        public bool menosBalanco;          // reduz o balanço do lampião e a tremida da câmera
        public bool coresDePS1 = true;     // etapa 144: poucas cores, com pontilhado, como num jogo antigo
    }

    static Jogo instancia;
    public static Jogo Instancia
    {
        get
        {
            if (instancia == null) new GameObject("Jogo").AddComponent<Jogo>();
            return instancia;
        }
    }

    public Dados dados = new Dados();
    [HideInInspector] public Melhoria[] melhorias;

    [HideInInspector] public string avisoDoSave; // o que aconteceu com o save ao abrir o jogo; o menu mostra
    bool falhouAoSalvar;

    string Arquivo { get { return Path.Combine(Application.persistentDataPath, "galeria7.json"); } }
    string Reserva { get { return Arquivo + ".bak"; } }
    public bool TemSave { get { return File.Exists(Arquivo); } }

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
        melhorias = Resources.LoadAll<Melhoria>("Melhorias");
        Carregar();
    }

    // Etapa 151: o save nunca some sem aviso. Além do arquivo, fica guardada a gravação anterior (.bak). Se o
    // arquivo estiver estragado, ele é posto de lado (.estragado.json) e o jogo volta para a gravação anterior.
    public void Carregar()
    {
        dados = new Dados();
        avisoDoSave = null;
        try { if (File.Exists(Arquivo + ".tmp")) File.Delete(Arquivo + ".tmp"); } // sobra de uma gravação interrompida
        catch (System.Exception) { }

        if (TemSave && !Ler(Arquivo))
        {
            string estragado = Arquivo.Replace(".json", ".estragado.json");
            try { File.Copy(Arquivo, estragado, true); }
            catch (System.Exception) { }
            if (File.Exists(Reserva) && Ler(Reserva))
                avisoDoSave = "O save estava estragado. Voltei para a gravação anterior.";
            else
            {
                dados = new Dados();
                avisoDoSave = "O save estava estragado e não havia cópia boa. O jogo começou do zero. O arquivo antigo ficou em " + estragado;
            }
        }
        else if (!TemSave && File.Exists(Reserva) && Ler(Reserva))
        {
            avisoDoSave = "O save tinha sumido. Voltei para a cópia de segurança.";
            Salvar();
        }

        if (dados.niveis == null) dados.niveis = new List<Nivel>();
        if (dados.fitas == null) dados.fitas = new List<int>();
        if (dados.opcoes == null) dados.opcoes = new Opcoes();
        dados.minerioGuardado = Mathf.Max(0, dados.minerioGuardado); // um save mexido à mão não pode travar a bancada
        AudioListener.volume = dados.opcoes.volume;
    }

    // Lê um arquivo de save. Devolve false, sem mexer em nada, se ele estiver vazio, cortado ou ilegível.
    bool Ler(string caminho)
    {
        try
        {
            string texto = File.ReadAllText(caminho).Trim();
            if (!texto.StartsWith("{") || !texto.EndsWith("}")) return false;
            Dados lidos = new Dados();
            JsonUtility.FromJsonOverwrite(texto, lidos);
            dados = lidos;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Não consegui ler o save em " + caminho + ": " + e.Message);
            return false;
        }
    }

    // Grava primeiro num arquivo à parte e só depois troca: se o jogo fechar no meio, o save antigo continua inteiro.
    // Se não der para gravar (disco cheio, pasta protegida), o jogo avisa uma vez e continua.
    public void Salvar()
    {
        try
        {
            string novo = Arquivo + ".tmp";
            File.WriteAllText(novo, JsonUtility.ToJson(dados, true));
            if (File.Exists(Arquivo)) File.Copy(Arquivo, Reserva, true);
            File.Copy(novo, Arquivo, true);
            File.Delete(novo);
            falhouAoSalvar = false;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Não consegui gravar o save em " + Arquivo + ": " + e.Message);
            if (!falhouAoSalvar) Legenda.Mostrar("Aviso", "Não consegui gravar o seu progresso. O jogo continua, mas o que você fizer agora pode não ficar guardado. Veja se o disco está cheio.");
            falhouAoSalvar = true;
        }
    }

    // Etapa 48: começa do zero, mas mantém as opções.
    public void NovoJogo()
    {
        dados = new Dados { opcoes = dados.opcoes };
        Salvar();
    }

    public int NivelDe(Melhoria melhoria)
    {
        Nivel salvo = dados.niveis.Find(n => n.melhoria == melhoria.name);
        return salvo == null ? 0 : Mathf.Min(salvo.nivel, melhoria.NivelMaximo);
    }

    // O valor da melhoria no nível comprado, ou "padrao" se ela não existir.
    public float ValorDe(string melhoria, float padrao)
    {
        Melhoria m = System.Array.Find(melhorias, x => x.name == melhoria);
        return m == null ? padrao : m.valor[NivelDe(m)];
    }

    // Etapa 45: compra o próximo nível, se houver minério.
    public bool Comprar(Melhoria melhoria)
    {
        int nivel = NivelDe(melhoria);
        if (nivel >= melhoria.NivelMaximo || dados.minerioGuardado < melhoria.custo[nivel]) return false;
        dados.minerioGuardado -= melhoria.custo[nivel];
        Nivel salvo = dados.niveis.Find(n => n.melhoria == melhoria.name);
        if (salvo == null)
        {
            salvo = new Nivel { melhoria = melhoria.name };
            dados.niveis.Add(salvo);
        }
        salvo.nivel = nivel + 1;
        Salvar();
        return true;
    }

    // Etapa 44: subir pelo elevador guarda o minério carregado.
    public void Guardar(int minerio)
    {
        dados.minerioGuardado += minerio;
        Salvar();
    }

    public void RegistrarAndar(int andar)
    {
        if (andar <= dados.andarMaisFundo) return;
        dados.andarMaisFundo = andar;
        Salvar();
    }

    // Etapa 62: as fitas encontradas ficam no save.
    public bool TemFita(int numero)
    {
        return dados.fitas.Contains(numero);
    }

    public void AcharFita(int numero)
    {
        if (TemFita(numero)) return;
        dados.fitas.Add(numero);
        Salvar();
    }

    // Etapas 46 e 57: no começo de cada cena, o jogador recebe os níveis comprados.
    public void Aplicar(Jogador jogador)
    {
        AudioListener.volume = dados.opcoes.volume;
        if (jogador.lampiao != null)
        {
            jogador.lampiao.oleoMaximo = ValorDe("Lampiao", jogador.lampiao.oleoMaximo);
            jogador.lampiao.oleo = jogador.lampiao.oleoMaximo;
        }
        Picareta picareta = jogador.GetComponentInChildren<Picareta>();
        if (picareta != null) picareta.duracao = ValorDe("Picareta", picareta.duracao);
        jogador.capacidade = Mathf.RoundToInt(ValorDe("Mochila", jogador.capacidade));
        jogador.folegoMaximo = ValorDe("Botas", jogador.folegoMaximo);
        jogador.folego = jogador.folegoMaximo;
        jogador.vida.maxima = Mathf.RoundToInt(ValorDe("Colete", jogador.vida.maxima));
        jogador.vida.atual = jogador.vida.maxima;
    }

    // Troca de cena sempre por aqui, para nunca chegar numa cena nova ainda pausado.
    public static void IrPara(string cena)
    {
        Time.timeScale = 1f;
        Controles.Pausado = false;
        Pausa.fecharTela = null;
        SceneManager.LoadScene(cena);
    }
}
