using UnityEngine;

// Etapas 69 e 75: luz que pisca como lâmpada velha ou pulsa como coisa viva.
public class LuzViva : MonoBehaviour
{
    public enum Modo { Piscar, Pulsar }

    public Modo modo;
    public Light luz;
    [Tooltip("Quem muda de tamanho junto com a luz. Pode ficar vazio.")]
    public Transform alvo;
    [Tooltip("Pulsos por segundo, ou a pressa do pisca-pisca.")]
    public float ritmo = 1f;

    float brilho;
    Vector3 tamanho;
    float semente;

    void Awake()
    {
        if (luz != null) brilho = luz.intensity;
        if (alvo != null) tamanho = alvo.localScale;
        semente = Random.value * 100f; // para duas luzes não piscarem juntas
    }

    void Update()
    {
        float onda;
        if (modo == Modo.Pulsar)
        {
            onda = 0.75f + 0.25f * Mathf.Sin((Time.time + semente) * ritmo * Mathf.PI * 2f);
        }
        else
        {
            float ruido = Mathf.PerlinNoise(Time.time * ritmo * 6f, semente);
            onda = ruido < 0.3f ? 0.15f : 0.8f + 0.2f * ruido; // de vez em quando quase apaga
        }
        if (luz != null) luz.intensity = brilho * onda;
        if (alvo != null) alvo.localScale = tamanho * (1f + (onda - 0.75f) * 0.12f);
    }
}
