using UnityEngine;

// Etapa 76: o som da mina. Um vento baixinho o tempo todo e, de vez em quando,
// um som solto perto de você: uma gota, a madeira rangendo. Cada tema pode ter os seus.
public class Ambiente : MonoBehaviour
{
    public AudioClip vento;
    [Tooltip("Sons soltos, usados quando o tema do andar não tem os seus.")]
    public AudioClip[] sons;
    public float intervaloMinimo = 4f;
    public float intervaloMaximo = 11f;

    float proximo;

    void Start()
    {
        if (vento == null) return;
        AudioSource fonte = gameObject.AddComponent<AudioSource>();
        fonte.clip = vento;
        fonte.loop = true;
        fonte.volume = 0.12f;
        fonte.Play();
    }

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        if (jogador == null || Time.time < proximo) return;
        proximo = Time.time + Random.Range(intervaloMinimo, intervaloMaximo);

        Tema tema = GeradorDeAndar.Instancia != null ? GeradorDeAndar.Instancia.TemaAtual : null;
        AudioClip[] opcoes = tema != null && tema.sonsDoAmbiente != null && tema.sonsDoAmbiente.Length > 0 ? tema.sonsDoAmbiente : sons;
        if (opcoes == null || opcoes.Length == 0) return;

        Vector3 longe = Random.onUnitSphere * Random.Range(5f, 12f);
        longe.y *= 0.2f;
        AudioSource.PlayClipAtPoint(opcoes[Random.Range(0, opcoes.Length)], jogador.cabeca.position + longe, Random.Range(0.4f, 1f));
    }
}
