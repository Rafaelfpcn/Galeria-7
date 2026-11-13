using UnityEngine;
using UnityEngine.AI;

// Etapas 28, 54 e 126 a 130: anima o modelo por código, sem Animator. O modelo é feito de peças soltas, e este script
// gira cada uma em torno da própria origem: as pernas balançam conforme a velocidade e dobram o joelho; no aviso
// do ataque, os braços e o corpo se levantam devagar e a boca abre; no golpe, tudo desce de uma vez. Ao apanhar,
// o corpo dá um tranco para trás; ao morrer, tomba (cada inimigo do seu jeito) e encolhe as pernas.
[RequireComponent(typeof(Inimigo), typeof(NavMeshAgent), typeof(Vida))]
public class AnimacaoDeInimigo : MonoBehaviour
{
    public Transform corpo;
    [Tooltip("Pernas na ordem em que alternam: a primeira e a terceira vão juntas, a segunda e a quarta também.")]
    public Transform[] pernas;
    [Tooltip("A parte de baixo de cada perna, na mesma ordem. Pode ficar vazio.")]
    public Transform[] joelhos = new Transform[0];
    public Transform[] bracos;
    [Tooltip("A parte de baixo de cada braço, na mesma ordem. Pode ficar vazio.")]
    public Transform[] cotovelos = new Transform[0];
    [Tooltip("Abre no aviso do ataque. Pode ficar vazio.")]
    public Transform queixo;
    [Tooltip("Vira para o lado do barulho que ele foi investigar. Pode ficar vazio.")]
    public Transform cabeca;

    [Tooltip("Graus que as pernas balançam.")]
    public float passo = 35f;
    [Tooltip("Passos por metro andado.")]
    public float ritmo = 1.2f;
    [Tooltip("Graus que o corpo empina no aviso do ataque.")]
    public float empinada = 20f;
    [Tooltip("Graus que os braços sobem no aviso do ataque.")]
    public float bracosNoAviso = 120f;
    [Tooltip("Como o corpo fica ao morrer, em graus: (-90, 0, 0) cai de costas; (0, 0, 180) vira de patas para cima.")]
    public Vector3 giroDaMorte = new Vector3(-90f, 0f, 0f);
    [Tooltip("Quanto o corpo caído sobe, para não afundar no chão.")]
    public float alturaDaMorte = 0.12f;

    Inimigo inimigo;
    NavMeshAgent agente;
    Vida vida;
    Quaternion[] pernasParadas, joelhosParados, bracosParados, cotovelosParados;
    Quaternion corpoParado, queixoParado, cabecaParada;
    Vector3 corpoNoLugar;
    float fase;
    float ataque; // 0 parado, 1 com tudo levantado
    float olhar;  // para que lado a cabeça está virada, em graus
    float queda;  // 0 de pé, 1 caído

    void Awake()
    {
        inimigo = GetComponent<Inimigo>();
        agente = GetComponent<NavMeshAgent>();
        vida = GetComponent<Vida>();
        pernasParadas = Posturas(pernas);
        joelhosParados = Posturas(joelhos);
        bracosParados = Posturas(bracos);
        cotovelosParados = Posturas(cotovelos);
        corpoParado = corpo.localRotation;
        corpoNoLugar = corpo.localPosition;
        if (queixo != null) queixoParado = queixo.localRotation;
        if (cabeca != null) cabecaParada = cabeca.localRotation;
    }

    static Quaternion[] Posturas(Transform[] partes)
    {
        Quaternion[] posturas = new Quaternion[partes.Length];
        for (int i = 0; i < partes.Length; i++) posturas[i] = partes[i].localRotation;
        return posturas;
    }

    void LateUpdate()
    {
        if (vida.Morto)
        {
            // Tomba em um terço de segundo, devagar no começo e depressa no fim, e encolhe as pernas.
            queda = Mathf.MoveTowards(queda, 1f, 3f * Time.deltaTime);
            float caido = queda * queda;
            corpo.localRotation = corpoParado * Quaternion.Euler(giroDaMorte * caido);
            corpo.localPosition = corpoNoLugar + Vector3.up * alturaDaMorte * caido;
            for (int i = 0; i < joelhos.Length; i++) joelhos[i].localRotation = joelhosParados[i] * Quaternion.Euler(45f * caido, 0f, 0f);
            return;
        }

        float velocidade = agente.velocity.magnitude;
        float andando = Mathf.Clamp01(velocidade);
        fase += velocidade * ritmo * Mathf.PI * 2f * Time.deltaTime;
        for (int i = 0; i < pernas.Length; i++)
        {
            float balanco = Mathf.Sin(fase + i * Mathf.PI);
            pernas[i].localRotation = pernasParadas[i] * Quaternion.Euler(balanco * passo * andando, 0f, 0f);
            // O joelho dobra só na metade do passo em que o pé sai do chão.
            if (i < joelhos.Length) joelhos[i].localRotation = joelhosParados[i] * Quaternion.Euler(Mathf.Max(0f, balanco) * passo * 0.8f * andando, 0f, 0f);
        }

        float alvo = inimigo.EmAviso ? 1f : 0f;
        float rapidez = alvo > ataque ? 1f / Mathf.Max(0.1f, inimigo.aviso) : 12f;
        ataque = Mathf.MoveTowards(ataque, alvo, rapidez * Time.deltaTime);
        for (int i = 0; i < bracos.Length; i++)
        {
            float balanco = Mathf.Sin(fase + (i + 1) * Mathf.PI) * passo * 0.6f * andando;
            bracos[i].localRotation = bracosParados[i] * Quaternion.Euler(balanco - bracosNoAviso * ataque, 0f, 0f);
            if (i < cotovelos.Length) cotovelos[i].localRotation = cotovelosParados[i] * Quaternion.Euler(-35f * ataque, 0f, 0f);
        }
        // Ao levar um golpe, o corpo dá um tranco para trás e volta em um quarto de segundo.
        float tranco = Mathf.Clamp01(1f - (Time.time - vida.ultimoDano) / 0.25f);
        corpo.localRotation = corpoParado * Quaternion.Euler(-empinada * ataque - 18f * tranco, 0f, 0f);
        corpo.localPosition = corpoNoLugar + Vector3.up * Mathf.Abs(Mathf.Sin(fase)) * 0.04f * andando;
        if (queixo != null) queixo.localRotation = queixoParado * Quaternion.Euler(35f * ataque, 0f, 0f);

        if (cabeca == null) return;
        float lado = 0f;
        if (inimigo.estado == Inimigo.Estado.Investigar)
        {
            Vector3 para = inimigo.Destino - transform.position;
            para.y = 0f;
            if (para.sqrMagnitude > 0.01f) lado = Mathf.Clamp(Vector3.SignedAngle(transform.forward, para, Vector3.up), -70f, 70f);
        }
        olhar = Mathf.MoveTowards(olhar, lado, 180f * Time.deltaTime);
        cabeca.localRotation = cabecaParada * Quaternion.Euler(0f, olhar, 0f);
    }
}
