using UnityEngine;

// Etapa 58: o que cada andar tem. O andar 1 usa a primeira linha; andares além do fim da lista usam a última.
// Mudar esta tabela no Inspector muda os andares sem mexer em código.
[CreateAssetMenu(menuName = "Galeria 7/Tabela de dificuldade")]
public class TabelaDeDificuldade : ScriptableObject
{
    [System.Serializable]
    public class Andar
    {
        [Tooltip("Etapa 66: a aparência do andar.")]
        public Tema tema;
        [Tooltip("Salas do caminho até o poço.")]
        public int salas = 6;
        [Range(0f, 1f)] public float chanceDeInimigo = 0.25f;
        [Tooltip("Inimigos que podem aparecer. Cada ponto de inimigo sorteia um deles.")]
        public GameObject[] inimigos;
        [Range(0f, 1f)] public float chanceDeVeio = 0.5f;
        [Range(0f, 1f)] public float chanceDeItem = 0.3f;
        [Range(0f, 1f)] public float chanceDePerigo = 0.15f;
    }

    public Andar[] andares;

    public Andar Do(int andar)
    {
        return andares[Mathf.Clamp(andar - 1, 0, andares.Length - 1)];
    }
}
