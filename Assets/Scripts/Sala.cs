using UnityEngine;

// Etapas 31, 32 e 59: uma sala da grade, de 15 x 15 m. As aberturas ficam no meio dos lados.
public class Sala : MonoBehaviour
{
    [Tooltip("Lados abertos com a sala virada para o norte, nesta ordem: norte, leste, sul, oeste.")]
    public bool[] aberturas = new bool[4];
    [Tooltip("Quanto o chão desce do lado norte para o lado sul. Só a rampa usa.")]
    public float desnivel;
    [Tooltip("Marque na sala que tem o poço para o próximo andar.")]
    public bool saida;
    [Tooltip("Chance relativa de ser sorteada entre as salas que encaixam. 1 é o normal.")]
    public float peso = 1f;
    [Tooltip("Desmarque se o meio da sala não serve para o jogador começar, como no depósito, que tem um veio ali.")]
    public bool podeComecar = true;
    [Tooltip("Etapa 119: altura do teto no meio da sala, em metros. Nas portas o teto tem sempre 3 m, para casar com a sala vizinha.")]
    public float teto = 3f;
    [Tooltip("Etapa 120: o primeiro e o último andar em que esta sala pode aparecer. As salas comuns aparecem em todos.")]
    public int doAndar = 1;
    public int ateOAndar = 99;
    public Transform[] pontosDeVeio;
    public Transform[] pontosDeInimigo;
    public Transform[] pontosDeItem;
    public Transform[] pontosDePerigo;
    [Tooltip("Etapa 72: onde o tema do andar pode pôr um enfeite, como um caixote ou uma estalagmite.")]
    public Transform[] pontosDeDecoracao;
}
