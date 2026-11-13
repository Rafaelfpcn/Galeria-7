using UnityEngine;
using UnityEngine.SceneManagement;

// Etapas 12 a 14, 21, 51, 56 e 81: andar, olhar, correr, agachar, fôlego, barulho, sinalizador, morte e tremida da câmera.
[RequireComponent(typeof(CharacterController), typeof(Vida))]
public class Jogador : MonoBehaviour
{
    public static Jogador Instancia;

    [Header("Andar")]
    public float velocidadeAndando = 3.5f;
    public float velocidadeCorrendo = 6f;
    public float velocidadeAgachado = 1.8f;
    public float gravidade = -20f;

    [Header("Olhar")]
    [Tooltip("A câmera. A sensibilidade do mouse fica nas Opções do jogo.")]
    public Transform cabeca;

    [Header("Agachar")]
    public float alturaEmPe = 1.8f;
    public float alturaAgachado = 1f;

    [Header("Fôlego")]
    [Tooltip("Segundos de corrida com o fôlego cheio.")]
    public float folegoMaximo = 5f;
    [Tooltip("Fôlego que volta por segundo quando você não está correndo.")]
    public float recuperacao = 1f;

    [Header("Morte")]
    [Tooltip("Segundos entre morrer e a cena recomeçar.")]
    public float esperaAoMorrer = 3f;
    [Tooltip("Etapa 43: cena carregada ao morrer. Vazio recomeça a mesma cena.")]
    public string cenaAoMorrer = "";

    [Header("Mochila e itens")]
    [Tooltip("Minério que cabe na mochila. A melhoria Mochila aumenta.")]
    public int capacidade = 20;
    [Tooltip("Prefab do sinalizador aceso, jogado com Q.")]
    public GameObject sinalizador;

    [HideInInspector] public float folego;
    [HideInInspector] public int minerio; // minério carregado nesta descida
    [HideInInspector] public int sinalizadores;
    [HideInInspector] public Vida vida;
    [HideInInspector] public Lampiao lampiao;

    public bool Agachado { get { return agachado; } }
    public bool Correndo { get; private set; }

    CharacterController corpo;
    float inclinacao;          // olhar para cima e para baixo, em graus
    float velocidadeVertical;
    bool agachado;
    bool cansado;              // esvaziou o fôlego: só corre de novo com 30% de volta
    float proximoBarulho;
    float tremor;              // quanto a câmera está tremendo agora, em metros

    void Awake()
    {
        Instancia = this;
        corpo = GetComponent<CharacterController>();
        corpo.minMoveDistance = 0f; // acima de 0, o jogador deixa de andar quando o jogo roda a muitos FPS
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // os raios da câmera não acertam o próprio corpo
        if (cabeca == null) cabeca = GetComponentInChildren<Camera>().transform;
        vida = GetComponent<Vida>();
        vida.AoSofrerDano += () => Tremer(0.12f);
        vida.AoMorrer += Morrer;
        lampiao = GetComponentInChildren<Lampiao>();
        folego = folegoMaximo;
    }

    void Start()
    {
        TravarMouse();
        Jogo.Instancia.Aplicar(this); // etapa 46: melhorias compradas
    }

    void Update()
    {
        if (vida.Morto)
        {
            // Caído: a câmera desce até perto do chão.
            cabeca.localPosition = Vector3.Lerp(cabeca.localPosition, new Vector3(0f, 0.3f, 0f), 3f * Time.deltaTime);
            return;
        }

        // O Esc solta o mouse no editor e no navegador; um clique no jogo trava de novo.
        if (Cursor.lockState != CursorLockMode.Locked && Controles.Atacar()) TravarMouse();
        if (Cursor.lockState == CursorLockMode.Locked) Olhar();
        if (Controles.AcenderSinalizador()) JogarSinalizador();
        Agachar();
        Mover();
    }

    // Muda o jogador de lugar olhando para a frente. Com o CharacterController ligado, a mudança de posição seria ignorada.
    public void Teleportar(Vector3 posicao, float direcao)
    {
        corpo.enabled = false;
        transform.SetPositionAndRotation(posicao, Quaternion.Euler(0f, direcao, 0f));
        corpo.enabled = true;
        inclinacao = 0f;
        cabeca.localRotation = Quaternion.identity;
        velocidadeVertical = 0f;
    }

    // Etapa 81: tremida curta da câmera. Com "parar", o jogo congela por um instante, para o golpe pesar.
    public void Tremer(float quanto, bool parar = false)
    {
        if (Jogo.Instancia.dados.opcoes.menosBalanco) quanto *= 0.3f;
        tremor = Mathf.Max(tremor, quanto);
        if (parar && Time.timeScale > 0f) StartCoroutine(Parar());
    }

    System.Collections.IEnumerator Parar()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.05f);
        if (!Controles.Pausado) Time.timeScale = 1f; // se a pausa entrou no meio, é ela que devolve o tempo
    }

    // Etapa 56: joga um sinalizador aceso um pouco para a frente.
    void JogarSinalizador()
    {
        if (sinalizadores <= 0 || sinalizador == null) return;
        sinalizadores--;
        GameObject aceso = Instantiate(sinalizador, cabeca.position + cabeca.forward * 0.6f, Quaternion.identity);
        Rigidbody arremesso = aceso.GetComponent<Rigidbody>();
        if (arremesso != null) arremesso.AddForce(cabeca.forward * 3f, ForceMode.Impulse);
    }

    void Morrer()
    {
        Picareta picareta = GetComponentInChildren<Picareta>();
        if (picareta != null) picareta.enabled = false;
        Interacao interacao = GetComponentInChildren<Interacao>();
        if (interacao != null) interacao.enabled = false;
        Invoke(nameof(Recomecar), esperaAoMorrer);
    }

    // O minério carregado não vai junto: ele só existe neste Jogador.
    void Recomecar()
    {
        Jogo.IrPara(string.IsNullOrEmpty(cenaAoMorrer) ? SceneManager.GetActiveScene().name : cenaAoMorrer);
    }

    void TravarMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Olhar()
    {
        Jogo.Opcoes opcoes = Jogo.Instancia.dados.opcoes;
        Vector2 mouse = Controles.Olhar() * opcoes.sensibilidade;
        transform.Rotate(0f, mouse.x, 0f);
        inclinacao = Mathf.Clamp(inclinacao + (opcoes.inverterY ? mouse.y : -mouse.y), -85f, 85f);
        cabeca.localRotation = Quaternion.Euler(inclinacao, 0f, 0f);
    }

    void Agachar()
    {
        // Soltou o C embaixo de um teto baixo: continua agachado até ter espaço.
        agachado = Controles.Agachar() || (agachado && TetoBaixo());
        float alvo = agachado ? alturaAgachado : alturaEmPe;
        corpo.height = Mathf.MoveTowards(corpo.height, alvo, 5f * Time.deltaTime);
        corpo.center = new Vector3(0f, corpo.height / 2f, 0f);
        cabeca.localPosition = new Vector3(0f, corpo.height - 0.15f, 0f) + Random.insideUnitSphere * tremor;
        tremor = Mathf.MoveTowards(tremor, 0f, 0.6f * Time.unscaledDeltaTime);
    }

    bool TetoBaixo()
    {
        float raio = corpo.radius;
        Vector3 origem = transform.position + Vector3.up * raio;
        RaycastHit teto;
        return Physics.SphereCast(origem, raio * 0.9f, Vector3.up, out teto, alturaEmPe - 2f * raio, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    void Mover()
    {
        Vector2 entrada = Controles.Mover();
        bool andando = entrada.sqrMagnitude > 0.01f;
        Correndo = andando && Controles.Correr() && !agachado && !cansado;

        if (Correndo) folego -= Time.deltaTime;
        else folego += recuperacao * Time.deltaTime;
        folego = Mathf.Clamp(folego, 0f, folegoMaximo);
        if (folego <= 0f) cansado = true;
        else if (cansado && folego >= folegoMaximo * 0.3f) cansado = false;

        float velocidade = velocidadeAndando;
        if (agachado) velocidade = velocidadeAgachado;
        else if (Correndo) velocidade = velocidadeCorrendo;

        // Etapa 51: andar faz um pouco de barulho, correr faz muito, e agachado você fica em silêncio.
        if (andando && !agachado && Time.time >= proximoBarulho)
        {
            Barulho.Fazer(transform.position, Correndo ? 8f : 2.5f);
            proximoBarulho = Time.time + 0.25f;
        }

        // Um pouco de gravidade mesmo no chão, para descer rampas sem quicar.
        if (corpo.isGrounded && velocidadeVertical < 0f) velocidadeVertical = -2f;
        velocidadeVertical += gravidade * Time.deltaTime;

        Vector3 direcao = transform.right * entrada.x + transform.forward * entrada.y;
        corpo.Move((direcao * velocidade + Vector3.up * velocidadeVertical) * Time.deltaTime);
    }
}
