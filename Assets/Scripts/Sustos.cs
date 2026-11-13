using System.Collections.Generic;
using UnityEngine;

// Etapa 79: sustos justos. São raros, não machucam e nunca acontecem duas vezes na mesma sala:
// passos atrás de você, um sussurro, a luz falhando, uma tábua caindo ou um vulto atravessando o caminho.
public class Sustos : MonoBehaviour
{
    [Tooltip("Chance de um susto cada vez que você entra numa sala nova.")]
    [Range(0f, 1f)] public float chance = 0.15f;
    [Tooltip("Segundos de sossego garantidos entre um susto e outro.")]
    public float intervaloMinimo = 45f;
    public AudioClip somDePassos;
    public AudioClip somDeSussurro;
    public AudioClip somDeMadeira;
    public GameObject vulto;
    public GameObject tabua;

    readonly HashSet<Vector2Int> salasVistas = new HashSet<Vector2Int>();
    int andarVisto;
    float proximo = 30f; // nenhum susto no primeiro meio minuto

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        GeradorDeAndar gerador = GeradorDeAndar.Instancia;
        if (jogador == null || gerador == null || gerador.NaArena || jogador.vida.Morto) return;

        if (gerador.andar != andarVisto)
        {
            andarVisto = gerador.andar;
            salasVistas.Clear();
        }
        Vector3 lugar = jogador.transform.position;
        Vector2Int sala = new Vector2Int(Mathf.RoundToInt(lugar.x / GeradorDeAndar.Modulo), Mathf.RoundToInt(lugar.z / GeradorDeAndar.Modulo));
        if (!salasVistas.Add(sala)) return; // cada sala só tem uma chance
        if (Time.time < proximo || Random.value > chance) return;

        proximo = Time.time + intervaloMinimo;
        Assustar(jogador, Random.Range(0, 5));
    }

    public void Assustar(Jogador jogador, int qual)
    {
        Vector3 olho = jogador.cabeca.position;
        Vector3 frente = jogador.transform.forward;
        Vector3 lado = jogador.transform.right;
        switch (qual)
        {
            case 0:
                Tocar(somDePassos, olho - frente * 4f);
                break;
            case 1:
                Tocar(somDeSussurro, olho + lado * (Random.value < 0.5f ? -1.5f : 1.5f));
                break;
            case 2:
                if (jogador.lampiao != null) jogador.lampiao.Falhar(1.5f);
                break;
            case 3:
                if (tabua != null) Destroy(Instantiate(tabua, olho + frente * 5f + Vector3.up * 0.8f, Random.rotation), 20f);
                Tocar(somDeMadeira, olho + frente * 5f);
                break;
            default:
                if (vulto != null) Instantiate(vulto, jogador.transform.position + frente * 9f - lado * 4f, Quaternion.LookRotation(lado)).GetComponent<Vulto>().velocidade = lado * 8f;
                break;
        }
    }

    static void Tocar(AudioClip clip, Vector3 lugar)
    {
        if (clip != null) AudioSource.PlayClipAtPoint(clip, lugar);
    }
}
