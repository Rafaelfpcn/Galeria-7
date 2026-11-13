using UnityEngine;

// Etapa 77: o corpo do jogador. Passos que mudam com o chão (pedra, madeira ou água)
// e a respiração ofegante quando o fôlego está acabando.
[RequireComponent(typeof(Jogador), typeof(CharacterController))]
public class SonsDoJogador : MonoBehaviour
{
    public AudioClip passoNaPedra;
    public AudioClip passoNaMadeira;
    public AudioClip passoNaAgua;
    [Tooltip("Etapa 145: o anel que aparece na água a cada passo.")]
    public GameObject onda;
    public AudioClip respiracao;
    [Tooltip("Metros andados entre um passo e outro.")]
    public float metrosPorPasso = 1.7f;

    Jogador jogador;
    CharacterController corpo;
    AudioSource passos;
    AudioSource ofego;
    float andado;

    void Awake()
    {
        jogador = GetComponent<Jogador>();
        corpo = GetComponent<CharacterController>();
        passos = gameObject.AddComponent<AudioSource>();
        ofego = gameObject.AddComponent<AudioSource>();
        ofego.clip = respiracao;
        ofego.loop = true;
        ofego.volume = 0f;
        if (respiracao != null) ofego.Play();
    }

    void Update()
    {
        Vector3 velocidade = corpo.velocity;
        velocidade.y = 0f;
        if (corpo.isGrounded) andado += velocidade.magnitude * Time.deltaTime;
        if (andado >= metrosPorPasso)
        {
            andado = 0f;
            AudioClip som = SomDoChao();
            passos.pitch = Random.Range(0.9f, 1.1f);
            if (som != null) passos.PlayOneShot(som, jogador.Agachado ? 0.2f : jogador.Correndo ? 1f : 0.6f);
            // Etapa 145: na água, cada passo abre um anel, logo acima da lâmina.
            if (som == passoNaAgua && onda != null) Instantiate(onda, transform.position + Vector3.up * 0.09f, Quaternion.identity);
        }

        // Ofega quando o fôlego cai abaixo da metade.
        float falta = 1f - Mathf.Clamp01(jogador.folego / (jogador.folegoMaximo * 0.5f));
        ofego.volume = Mathf.MoveTowards(ofego.volume, falta * 0.8f, Time.deltaTime);
    }

    AudioClip SomDoChao()
    {
        GeradorDeAndar gerador = GeradorDeAndar.Instancia;
        if (gerador != null && gerador.TemaAtual != null && gerador.TemaAtual.laminaDeAgua != null && !gerador.NaArena) return passoNaAgua;

        // O material do que está embaixo do pé decide: as peças de madeira usam um material com Madeira no nome.
        RaycastHit chao;
        if (Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out chao, 1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            Renderer visual = chao.collider.GetComponent<Renderer>();
            if (visual != null && visual.sharedMaterial != null && visual.sharedMaterial.name.Contains("Madeira")) return passoNaMadeira;
        }
        return passoNaPedra;
    }
}
