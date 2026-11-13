using UnityEngine;

// Etapa 20: veio que quebra com alguns golpes e solta pedras de minério.
public class VeioDeMinerio : MonoBehaviour
{
    public int golpesParaQuebrar = 3;
    public int pedrasQueSolta = 3;
    public GameObject pedraPrefab;

    int golpes;

    // pontoDeSaida: onde as pedras aparecem, um pouco à frente do ponto atingido.
    public void LevarGolpe(int dano, Vector3 pontoDeSaida)
    {
        golpes += dano;
        transform.localScale *= 0.92f; // encolhe um pouco a cada golpe
        if (golpes < golpesParaQuebrar) return;

        // As pedras ficam dentro do mesmo pai do veio, para sumirem junto quando o andar for trocado.
        for (int i = 0; i < pedrasQueSolta; i++)
            Instantiate(pedraPrefab, pontoDeSaida + Random.insideUnitSphere * 0.2f, Random.rotation, transform.parent);
        Destroy(gameObject);
    }
}
