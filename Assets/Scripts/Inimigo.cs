using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Etapas 23 a 27, 29 e 51 a 54: um inimigo da mina. O mesmo script serve para o Rastejante, o Ouvinte e o Mineiro Oco;
// o que muda entre eles são os números e as opções de cada prefab.
[RequireComponent(typeof(NavMeshAgent), typeof(Vida))]
public class Inimigo : MonoBehaviour
{
    public enum Estado { Vagar, Investigar, Perseguir, Atacar, Fugir }

    // Todos os inimigos ativos na cena. A música usa esta lista para saber se alguém está perseguindo você.
    public static readonly List<Inimigo> Todos = new List<Inimigo>();

    [Tooltip("O que ele está fazendo agora. Durante o Play, dá para ver mudar aqui.")]
    public Estado estado = Estado.Vagar;
    [Tooltip("O modelo: é ele que pisca, fica vermelho no aviso e tomba na morte.")]
    public Transform corpo;

    [Header("Sentidos")]
    [Tooltip("Desligado: é cego e só acha você pelo barulho ou encostando, como o Ouvinte.")]
    public bool enxerga = true;
    [Tooltip("Até onde ele vê você, se não houver parede no meio.")]
    public float distanciaDeVisao = 15f;
    [Tooltip("Multiplica o alcance dos barulhos que ele ouve.")]
    public float audicao = 1f;
    [Tooltip("Quem é cego sente você a esta distância, mesmo sem barulho.")]
    public float distanciaDeToque = 1.2f;

    [Header("Movimento")]
    public float velocidadeVagando = 1.2f;
    public float velocidadeCorrendo = 4f;

    [Header("Ataque")]
    public float distanciaDeAtaque = 1.6f;
    public int dano = 1;
    [Tooltip("Segundos parado e vermelho antes de cada golpe: o tempo que você tem para desviar.")]
    public float aviso = 0.5f;
    [Tooltip("Segundos de descanso depois de cada golpe.")]
    public float descanso = 1f;
    public Color corDeAviso = new Color(0.9f, 0.1f, 0.05f);

    [Header("Medo da luz")]
    [Tooltip("Desligado: a chama avivada e o sinalizador não espantam, como no Ouvinte e no Mineiro Oco.")]
    public bool medoDaLuz = true;
    [Tooltip("A luz forte só espanta se ele estiver mais perto que isto.")]
    public float distanciaDoMedo = 8f;
    public float tempoDeFuga = 2f;

    [Header("Sons")]
    public AudioClip somPasso;
    public AudioClip somGrito;
    public AudioClip somGolpe;
    public AudioClip somFerido;

    // No aviso: parado, vermelho, prestes a golpear.
    public bool EmAviso { get { return estado == Estado.Atacar && !golpeou; } }
    public bool Cacando { get { return estado == Estado.Perseguir || estado == Estado.Atacar; } }
    // Onde ele ouviu o barulho ou viu você por último. A animação vira a cabeça para lá.
    public Vector3 Destino { get { return destino; } }

    NavMeshAgent agente;
    Vida vida;
    AudioSource som;
    Renderer[] visuais;
    Color[] cores;
    Color tinta;       // a cor por cima do modelo agora; transparente quer dizer nenhuma
    Vector3 destino;   // onde ouviu o barulho ou viu você por último
    float fimDoEstado;
    bool golpeou;
    float proximoPasso;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        vida = GetComponent<Vida>();
        som = GetComponent<AudioSource>();
        if (corpo == null) corpo = transform.childCount > 0 ? transform.GetChild(0) : transform;
        visuais = corpo.GetComponentsInChildren<Renderer>();
        cores = new Color[visuais.Length];
        // Etapa 131: cada um nasce com um tom e um tamanho um pouco diferentes, para dois lado a lado não serem idênticos.
        float tom = Random.Range(0.65f, 1.1f);
        corpo.localScale *= Random.Range(0.94f, 1.07f);
        for (int i = 0; i < visuais.Length; i++)
        {
            Color cor = visuais[i].sharedMaterial.color;
            cores[i] = new Color(cor.r * tom, cor.g * tom, cor.b * tom, cor.a);
            visuais[i].material.color = cores[i];
        }
        vida.AoSofrerDano += Ferido;
        vida.AoMorrer += Morrer;
    }

    void OnEnable()
    {
        Todos.Add(this);
        Barulho.AoFazer += Ouviu;
    }

    void OnDisable()
    {
        Todos.Remove(this);
        Barulho.AoFazer -= Ouviu;
    }

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        if (vida.Morto || jogador == null || !agente.isOnNavMesh) return;

        Vector3 alvo = jogador.transform.position;
        float distancia = Vector3.Distance(transform.position, alvo);
        bool percebe = !jogador.vida.Morto && Percebe(jogador, distancia);

        if (medoDaLuz && estado != Estado.Fugir && LuzForteEmMim(jogador)) Mudar(Estado.Fugir, tempoDeFuga);

        switch (estado)
        {
            case Estado.Fugir:
                agente.SetDestination(transform.position + (transform.position - alvo).normalized * 4f);
                if (Time.time >= fimDoEstado) Mudar(Estado.Vagar, 0f);
                break;
            case Estado.Atacar:
                Atacar(jogador, distancia);
                break;
            default:
                if (percebe && distancia <= distanciaDeAtaque) Mudar(Estado.Atacar, aviso);
                else if (percebe) Perseguir(alvo);
                else if (estado == Estado.Perseguir) Investigar(destino, 6f); // perdeu você de vista: vai até onde te viu por último
                else if (estado == Estado.Investigar) SeguirPista();
                else Vagar();
                break;
        }

        agente.speed = estado == Estado.Vagar ? velocidadeVagando : velocidadeCorrendo;
        Colorir();
        Passos();
    }

    // Quem enxerga precisa estar perto e sem parede no meio. Quem é cego só sente encostando,
    // ou quando já veio atrás de um barulho seu e chegou perto.
    bool Percebe(Jogador jogador, float distancia)
    {
        if (!enxerga) return distancia <= distanciaDeToque || (estado == Estado.Investigar && distancia <= distanciaDeAtaque);
        if (distancia > distanciaDeVisao) return false;
        Vector3 olho = transform.position + Vector3.up * (agente.height * 0.8f);
        Vector3 peito = jogador.transform.position + Vector3.up * 1.2f;
        return !Physics.Linecast(olho, peito, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    // Etapa 51: um barulho perto o bastante faz ele ir ver o que foi.
    void Ouviu(Vector3 ponto, float raio)
    {
        if (vida.Morto || estado == Estado.Atacar || estado == Estado.Fugir) return;
        if (estado == Estado.Perseguir && enxerga) return; // já está vendo você
        if (Vector3.Distance(transform.position, ponto) > raio * audicao) return;
        if (estado == Estado.Vagar) Tocar(somGrito);
        Investigar(ponto, 8f);
    }

    void Investigar(Vector3 ponto, float duracao)
    {
        estado = Estado.Investigar;
        destino = ponto;
        fimDoEstado = Time.time + duracao;
        agente.isStopped = false;
        agente.SetDestination(ponto);
    }

    void SeguirPista()
    {
        bool chegou = !agente.pathPending && agente.remainingDistance < 1f;
        if (chegou || Time.time >= fimDoEstado) Mudar(Estado.Vagar, 0f);
    }

    void Vagar()
    {
        estado = Estado.Vagar;
        if (Time.time < fimDoEstado) return;
        fimDoEstado = Time.time + Random.Range(2f, 4f);
        NavMeshHit ponto;
        if (NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * 5f, out ponto, 2f, NavMesh.AllAreas))
            agente.SetDestination(ponto.position);
    }

    void Perseguir(Vector3 alvo)
    {
        if (estado == Estado.Vagar) Tocar(somGrito); // acabou de ver você
        estado = Estado.Perseguir;
        destino = alvo;
        agente.SetDestination(alvo);
    }

    // Etapa 25: para, fica vermelho durante o aviso e só então golpeia. Se você já se afastou, ele erra.
    void Atacar(Jogador jogador, float distancia)
    {
        agente.isStopped = true;
        Vector3 olhar = jogador.transform.position - transform.position;
        olhar.y = 0f;
        if (olhar.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(olhar);

        if (Time.time < fimDoEstado) return;
        if (!golpeou)
        {
            golpeou = true;
            Tocar(somGolpe);
            if (distancia <= distanciaDeAtaque + 0.3f) jogador.vida.LevarDano(dano);
            fimDoEstado = Time.time + descanso;
        }
        else Mudar(Estado.Perseguir, 0f);
    }

    void Mudar(Estado novo, float duracao)
    {
        if (novo == Estado.Fugir) Tocar(somGrito);
        estado = novo;
        fimDoEstado = Time.time + duracao;
        golpeou = false;
        agente.isStopped = false;
    }

    // Etapas 27 e 56: a chama avivada espanta quando ele está perto e dentro do facho; um sinalizador aceso, sempre que está perto.
    bool LuzForteEmMim(Jogador jogador)
    {
        foreach (Sinalizador sinalizador in Sinalizador.Acesos)
            if (Vector3.Distance(transform.position, sinalizador.transform.position) < distanciaDoMedo) return true;
        Lampiao lampiao = jogador.lampiao;
        if (lampiao == null || !lampiao.Avivada) return false;
        Vector3 paraMim = transform.position + Vector3.up * (agente.height / 2f) - lampiao.transform.position;
        return paraMim.magnitude < distanciaDoMedo && Vector3.Angle(lampiao.transform.forward, paraMim) < lampiao.luzForte.spotAngle / 2f;
    }

    // Etapa 26: a cada golpe ele pisca (em Colorir) e recua um pouco.
    void Ferido()
    {
        Tocar(somFerido);
        Jogador jogador = Jogador.Instancia;
        if (jogador == null || !agente.isOnNavMesh) return;
        Vector3 recuo = transform.position - jogador.transform.position;
        recuo.y = 0f;
        agente.Move(recuo.normalized * 0.8f);
    }

    void Morrer()
    {
        agente.enabled = false;
        GetComponent<Collider>().enabled = false;
        // Quem derruba o corpo é a AnimacaoDeInimigo; aqui ele só escurece e some depois de 3 segundos.
        for (int i = 0; i < visuais.Length; i++) visuais[i].material.color = cores[i] * 0.4f;
        Destroy(gameObject, 3f);
    }

    // Vermelho no aviso do ataque, branco por um instante ao levar um golpe.
    void Colorir()
    {
        Color nova = Color.clear;
        if (EmAviso) nova = corDeAviso;
        if (Time.time - vida.ultimoDano < 0.15f) nova = Color.white;
        if (nova == tinta) return;
        tinta = nova;
        for (int i = 0; i < visuais.Length; i++) visuais[i].material.color = tinta.a > 0f ? tinta : cores[i];
    }

    void Passos()
    {
        if (agente.velocity.sqrMagnitude < 0.25f || Time.time < proximoPasso) return;
        Tocar(somPasso);
        proximoPasso = Time.time + (estado == Estado.Vagar ? 0.6f : 0.3f);
    }

    void Tocar(AudioClip clip)
    {
        if (clip != null && som != null) som.PlayOneShot(clip);
    }
}
