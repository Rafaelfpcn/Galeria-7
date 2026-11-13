using UnityEngine;

// Etapa 69: um tentáculo do chefão. O chão fica vermelho durante o aviso; só depois ele sai e fere quem ficou em cima.
public class Tentaculo : MonoBehaviour
{
    [Tooltip("Segundos de chão vermelho antes do golpe: o tempo que você tem para sair de cima.")]
    public float aviso = 1f;
    public float raio = 1.3f;
    public int dano = 1;
    [Tooltip("Segundos que ele fica para fora antes de sumir.")]
    public float tempoFora = 0.7f;
    public float altura = 3f;
    public Transform corpo;   // sobe do chão
    [Tooltip("Os gomos do modelo, da base à ponta: cada um se dobra um pouco, e o tentáculo se contorce. Pode ficar vazio.")]
    public Transform[] gomos = new Transform[0];
    public GameObject marca;  // o disco vermelho do aviso
    public AudioClip somDeSaida;

    float nascimento;
    bool golpeou;

    void Start()
    {
        nascimento = Time.time;
        corpo.localPosition = Vector3.down * altura;
    }

    void Update()
    {
        float t = Time.time - nascimento;
        if (t < aviso) return;

        if (!golpeou)
        {
            golpeou = true;
            marca.SetActive(false);
            if (somDeSaida != null) AudioSource.PlayClipAtPoint(somDeSaida, transform.position);
            Jogador jogador = Jogador.Instancia;
            if (jogador != null)
            {
                Vector3 distancia = jogador.transform.position - transform.position;
                distancia.y = 0f;
                if (distancia.magnitude < raio) jogador.vida.LevarDano(dano);
            }
        }

        // Sai depressa, fica um instante e volta para o chão.
        float fora = (t - aviso) / tempoFora;
        float quanto = fora < 0.15f ? fora / 0.15f : fora > 0.7f ? (1f - fora) / 0.3f : 1f;
        corpo.localPosition = Vector3.down * altura * (1f - Mathf.Clamp01(quanto));
        for (int i = 0; i < gomos.Length; i++)
            gomos[i].localRotation = Quaternion.Euler(Mathf.Sin(t * 9f + i * 1.3f) * 13f, 0f, Mathf.Cos(t * 7f + i * 1.7f) * 13f);
        if (fora >= 1f) Destroy(gameObject);
    }
}
