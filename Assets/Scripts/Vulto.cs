using UnityEngine;

// Etapa 79: um vulto que atravessa o caminho e some. Não tem colisão nem faz mal.
public class Vulto : MonoBehaviour
{
    public Vector3 velocidade;
    public float duracao = 1.2f;

    void Start()
    {
        Destroy(gameObject, duracao);
    }

    void Update()
    {
        transform.position += velocidade * Time.deltaTime;
    }
}
