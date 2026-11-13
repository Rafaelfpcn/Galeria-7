using System.Collections.Generic;
using UnityEngine;

// Etapas 68 e 69: o Coração do Veio. Preso na rocha, ataca com tentáculos que avisam antes de sair do chão.
// Quebre os 3 pontos fracos com a picareta. Quando o primeiro quebra, ele apaga as luzes e passa a chamar Rastejantes.
public class Chefao : MonoBehaviour
{
    [Tooltip("Os pontos fracos. Cada um tem o script Vida: os golpes que aguenta ficam lá.")]
    public Vida[] pontosFracos;
    public GameObject tentaculo;
    public GameObject rastejante;
    public Transform[] pontosDeInimigo;
    [Tooltip("Luzes da arena, que apagam na segunda fase.")]
    public Light[] luzes;
    [Tooltip("Segundos entre os ataques na primeira fase.")]
    public float intervalo = 3.5f;
    [Tooltip("Segundos entre os ataques na segunda fase.")]
    public float intervaloNaFase2 = 2.4f;
    [Tooltip("Tentáculos a mais em cada ataque, além do que sai embaixo de você.")]
    public int tentaculosAMais = 2;
    [Tooltip("Metade do lado da arena: os tentáculos não saem para fora dela.")]
    public float metadeDaArena = 10f;
    public AudioClip somDeRugido;

    [HideInInspector] public int fase = 1;
    [HideInInspector] public bool vencido;

    readonly List<GameObject> chamados = new List<GameObject>();
    int quebrados;
    float proximoAtaque;
    float proximaChamada;

    void Start()
    {
        foreach (Vida ponto in pontosFracos)
        {
            Vida este = ponto;
            este.AoMorrer += () => Quebrou(este);
        }
        proximoAtaque = Time.time + 4f; // um respiro para olhar a arena
    }

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        if (vencido || jogador == null || jogador.vida.Morto) return;

        if (Time.time >= proximoAtaque)
        {
            proximoAtaque = Time.time + (fase == 1 ? intervalo : intervaloNaFase2);
            Atacar(jogador.transform.position);
        }
        if (fase == 2 && Time.time >= proximaChamada)
        {
            proximaChamada = Time.time + 12f;
            Chamar();
        }
    }

    // Um tentáculo embaixo do jogador e mais alguns em volta, para ele não fugir sempre para o mesmo lado.
    void Atacar(Vector3 alvo)
    {
        Vector3 embaixo = NaArena(alvo);
        Brotar(embaixo);
        int aMais = tentaculosAMais + (fase == 2 ? 2 : 0);
        for (int i = 0; i < aMais; i++)
        {
            Vector2 lado = Random.insideUnitCircle.normalized * Random.Range(2.5f, 6f);
            Vector3 lugar = NaArena(alvo + new Vector3(lado.x, 0f, lado.y));
            // Perto da parede, o lugar sorteado é empurrado para dentro e pode cair em cima do primeiro: aí não sai.
            if (Vector3.Distance(lugar, embaixo) > 2.4f) Brotar(lugar);
        }
    }

    // O ponto do chão da arena mais perto de "lugar".
    Vector3 NaArena(Vector3 lugar)
    {
        Vector3 centro = transform.position;
        lugar.x = Mathf.Clamp(lugar.x, centro.x - metadeDaArena, centro.x + metadeDaArena);
        lugar.z = Mathf.Clamp(lugar.z, centro.z - metadeDaArena, centro.z + metadeDaArena);
        lugar.y = centro.y;
        return lugar;
    }

    void Brotar(Vector3 lugar)
    {
        Instantiate(tentaculo, lugar, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
    }

    void Quebrou(Vida ponto)
    {
        quebrados++;
        if (somDeRugido != null) AudioSource.PlayClipAtPoint(somDeRugido, ponto.transform.position);
        if (Jogador.Instancia != null) Jogador.Instancia.Tremer(0.25f);
        Destroy(ponto.gameObject);
        if (quebrados >= pontosFracos.Length) Vencer();
        else if (fase == 1) ComecarFase2();
    }

    void ComecarFase2()
    {
        fase = 2;
        foreach (Light luz in luzes) if (luz != null) luz.enabled = false;
        proximaChamada = Time.time + 2f;
    }

    // Chama até 2 Rastejantes por vez, com no máximo 3 vivos.
    void Chamar()
    {
        chamados.RemoveAll(c => c == null);
        for (int i = 0; i < 2 && chamados.Count < 3; i++)
        {
            Transform ponto = pontosDeInimigo[Random.Range(0, pontosDeInimigo.Length)];
            chamados.Add(Instantiate(rastejante, ponto.position, ponto.rotation, transform.parent));
        }
    }

    // Etapa 70: sem os três pontos fracos, acabou. O script Final cuida do resto.
    void Vencer()
    {
        vencido = true;
        foreach (GameObject chamado in chamados) if (chamado != null) Destroy(chamado);
        foreach (Tentaculo resto in GetComponentsInChildren<Tentaculo>()) Destroy(resto.gameObject);
        gameObject.AddComponent<Final>();
    }
}
