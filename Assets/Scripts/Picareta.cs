using UnityEngine;

// Etapas 19, 26, 51 e 81: golpe da picareta, animado por código, com um raycast curto no fim da descida.
// Quebra veios, fere quem tem Vida, faz barulho e dá peso ao acerto com som, faísca e tremida.
public class Picareta : MonoBehaviour
{
    public Transform olho; // a câmera: o golpe acerta o que está na mira
    public float alcance = 2.2f;
    public int dano = 1;
    [Tooltip("Segundos de um golpe inteiro.")]
    public float duracao = 0.6f;

    [Header("Impacto (etapa 81)")]
    public GameObject faisca;
    public AudioClip somNoAr;
    public AudioClip somNaPedra;
    public AudioClip somNaCarne;

    Quaternion rotacaoParada;
    float inicio = -100f;
    bool acertou = true;
    AudioSource fonte;

    void Awake()
    {
        rotacaoParada = transform.localRotation;
        if (olho == null) olho = GetComponentInParent<Camera>().transform;
        fonte = GetComponent<AudioSource>();
    }

    void Update()
    {
        float t = (Time.time - inicio) / duracao; // de 0 a 1 durante o golpe
        if (t >= 1f && Controles.Atacar())
        {
            inicio = Time.time;
            acertou = false;
            t = 0f;
            Tocar(somNoAr, 0.5f);
        }

        // Levanta (0 a 0,3), desce rápido (0,3 a 0,45) e volta (0,45 a 1).
        float angulo;
        if (t < 0.3f) angulo = Mathf.Lerp(0f, -50f, t / 0.3f);
        else if (t < 0.45f) angulo = Mathf.Lerp(-50f, 40f, (t - 0.3f) / 0.15f);
        else angulo = Mathf.Lerp(40f, 0f, (t - 0.45f) / 0.55f);
        transform.localRotation = rotacaoParada * Quaternion.Euler(angulo, 0f, 0f);

        if (!acertou && t >= 0.45f)
        {
            acertou = true;
            Acertar();
        }
    }

    void Acertar()
    {
        RaycastHit acerto;
        if (!Physics.Raycast(olho.position, olho.forward, out acerto, alcance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return;

        Debug.Log("Picareta acertou: " + acerto.collider.name);
        VeioDeMinerio veio = acerto.collider.GetComponentInParent<VeioDeMinerio>();
        Vida vida = acerto.collider.GetComponentInParent<Vida>();
        bool carne = acerto.collider.GetComponentInParent<Inimigo>() != null;
        bool valeu = veio != null || vida != null;

        // O impacto vem antes do dano, porque o dano pode destruir o que foi acertado.
        Tocar(carne ? somNaCarne : somNaPedra, 1f);
        if (faisca != null && !carne) Instantiate(faisca, acerto.point, Quaternion.LookRotation(acerto.normal));
        if (Jogador.Instancia != null) Jogador.Instancia.Tremer(valeu ? 0.08f : 0.03f, valeu);
        Barulho.Fazer(acerto.point, veio != null ? 15f : 10f); // etapa 51: minerar se ouve de longe

        if (veio != null) veio.LevarGolpe(dano, acerto.point + acerto.normal * 0.3f);
        if (vida != null) vida.LevarDano(dano);
    }

    void Tocar(AudioClip clip, float volume)
    {
        if (clip != null && fonte != null) fonte.PlayOneShot(clip, volume);
    }
}
