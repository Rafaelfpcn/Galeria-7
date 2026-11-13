using UnityEngine;

// Etapa 145: o anel que se abre na água a cada passo. Cresce, some e se apaga sozinho.
public class Onda : MonoBehaviour
{
    public float duracao = 0.9f;

    Material material;
    float idade;

    void Awake()
    {
        material = GetComponentInChildren<Renderer>().material; // uma cópia só deste anel
    }

    void Update()
    {
        idade += Time.deltaTime / duracao;
        transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 1.9f, idade);
        Color cor = material.color;
        cor.a = 1f - idade;
        material.color = cor;
        if (idade >= 1f) Destroy(gameObject);
    }

    void OnDestroy()
    {
        Destroy(material);
    }
}
