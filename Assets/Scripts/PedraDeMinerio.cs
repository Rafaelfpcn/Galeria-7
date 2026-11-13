using UnityEngine;

// Pedra que sai do veio. Chegue perto para pegar.
[RequireComponent(typeof(Rigidbody))]
public class PedraDeMinerio : MonoBehaviour
{
    public int valor = 1;
    public float distanciaParaPegar = 1.6f;
    [Tooltip("Segundos antes de poder pegar, para dar tempo de ver a pedra pular.")]
    public float espera = 0.5f;

    float nascimento;

    void Start()
    {
        nascimento = Time.time;
        transform.localScale *= Random.Range(0.75f, 1.35f); // etapa 139: pedras de tamanhos diferentes
        GetComponent<Rigidbody>().AddForce((Random.insideUnitSphere + Vector3.up) * 1.5f, ForceMode.Impulse);
    }

    void Update()
    {
        Jogador jogador = Jogador.Instancia;
        if (jogador == null || Time.time < nascimento + espera) return;

        Vector3 peito = jogador.transform.position + Vector3.up * 1f;
        if (Vector3.Distance(transform.position, peito) > distanciaParaPegar) return;
        if (jogador.minerio + valor > jogador.capacidade) return; // etapa 57: mochila cheia, a pedra fica no chão

        jogador.minerio += valor;
        Destroy(gameObject);
    }
}
