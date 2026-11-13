using UnityEngine;

// Etapa 45: uma melhoria da bancada, com 3 níveis para comprar. O nível 0 é o de quem nunca comprou.
// Os arquivos ficam em Assets/Resources/Melhorias; o nome do arquivo é o que vai para o save.
[CreateAssetMenu(menuName = "Galeria 7/Melhoria")]
public class Melhoria : ScriptableObject
{
    [Tooltip("Nome que aparece na bancada.")]
    public string nome = "Lampião";
    [Tooltip("O que o valor mede, para mostrar na bancada.")]
    public string unidade = "s de óleo";
    [Tooltip("Minério para subir para os níveis 1, 2 e 3.")]
    public int[] custo = { 10, 25, 50 };
    [Tooltip("Valor nos níveis 0, 1, 2 e 3.")]
    public float[] valor = { 120f, 170f, 230f, 300f };

    public int NivelMaximo { get { return custo.Length; } }
}
