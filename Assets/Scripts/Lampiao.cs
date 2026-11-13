using UnityEngine;

// Etapas 16, 17, 27, 75 e 79: lampião preso à câmera, com balanço ao andar, óleo que acaba, chama avivada,
// poeira no facho de luz e uma falha rápida para assustar.
public class Lampiao : MonoBehaviour
{
    public Light luzForte;  // Spot Light: ilumina para a frente
    public Light luzFraca;  // Point Light: clareia em volta

    [Tooltip("Segundos de luz com o óleo cheio.")]
    public float oleoMaximo = 120f;
    [Tooltip("Abaixo desta fração do óleo, a luz enfraquece e treme.")]
    public float reserva = 0.2f;
    [Tooltip("Quanto o lampião balança ao andar, em metros.")]
    public float balanco = 0.03f;

    [Header("Chama avivada (botão direito)")]
    [Tooltip("Quantas vezes mais óleo a chama avivada gasta.")]
    public float gastoAvivada = 3f;
    [Tooltip("Quantas vezes mais forte a luz fica.")]
    public float brilhoAvivada = 2f;

    [Tooltip("Etapa 75: as partículas de poeira, que só aparecem com a luz acesa.")]
    public ParticleSystemRenderer poeira;

    [HideInInspector] public float oleo;
    [HideInInspector] public bool ligado = true;

    // Os inimigos com medo de luz consultam isto.
    public bool Avivada { get { return ligado && oleo > 0f && Controles.AvivarChama(); } }

    float brilhoForte;
    float brilhoFraco;
    Vector3 posicaoParado;
    float passo;
    float fimDaFalha;
    CharacterController corpo;

    void Awake()
    {
        oleo = oleoMaximo;
        brilhoForte = luzForte.intensity;
        brilhoFraco = luzFraca.intensity;
        posicaoParado = transform.localPosition;
        corpo = GetComponentInParent<CharacterController>();
    }

    // Etapa 79: a luz falha por alguns segundos.
    public void Falhar(float segundos)
    {
        fimDaFalha = Time.time + segundos;
    }

    void Update()
    {
        if (Controles.LigarLampiao()) ligado = !ligado;
        bool avivada = Avivada;
        if (ligado) oleo = Mathf.Max(0f, oleo - Time.deltaTime * (avivada ? gastoAvivada : 1f));

        // Na reserva, a luz cai e treme; com óleo zero, sobra só 5% do brilho.
        float forca = Mathf.Clamp01(oleo / (oleoMaximo * reserva));
        if (oleo > 0f && forca < 1f) forca *= Random.Range(0.8f, 1f);
        float brilho = ligado ? Mathf.Lerp(0.05f, 1f, forca) : 0f;
        if (avivada) brilho *= brilhoAvivada;
        if (Time.time < fimDaFalha) brilho *= Random.value < 0.5f ? 0.1f : 0.6f;
        luzForte.intensity = brilhoForte * brilho;
        luzFraca.intensity = brilhoFraco * brilho;
        if (poeira != null) poeira.material.color = new Color(1f, 0.85f, 0.6f, 0.45f * Mathf.Clamp01(brilho));

        Balancar();
    }

    void Balancar()
    {
        float velocidade = 0f;
        if (corpo != null) velocidade = new Vector3(corpo.velocity.x, 0f, corpo.velocity.z).magnitude;
        passo += velocidade * 2f * Time.deltaTime;
        float quanto = balanco * Mathf.Clamp01(velocidade / 3f);
        if (Jogo.Instancia.dados.opcoes.menosBalanco) quanto *= 0.2f;
        Vector3 alvo = posicaoParado + new Vector3(Mathf.Sin(passo) * quanto, Mathf.Abs(Mathf.Cos(passo)) * quanto, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, alvo, 10f * Time.deltaTime);
    }
}
