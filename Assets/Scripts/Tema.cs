using UnityEngine;

// Etapa 66: a aparência de um trecho da mina: galerias de madeira, cavernas alagadas ou o veio negro.
// Cada linha da tabela de dificuldade aponta para um tema.
[CreateAssetMenu(menuName = "Galeria 7/Tema")]
public class Tema : ScriptableObject
{
    [System.Serializable]
    public class Troca
    {
        public Material de;
        public Material para;
    }

    [Tooltip("Nome que aparece embaixo do número do andar.")]
    public string nome = "Galerias de madeira";
    [Tooltip("Material que entra no lugar da rocha padrão das salas. Vazio mantém a rocha padrão.")]
    public Material rocha;
    public Color neblina = Color.black;
    public float densidadeDaNeblina = 0.06f;
    public Color luzAmbiente = new Color(0.01f, 0.01f, 0.012f);
    [Tooltip("Lâmina de água posta no chão de cada sala. Com ela, os passos fazem som de água.")]
    public GameObject laminaDeAgua;
    [Tooltip("Moldura posta em cada porta das salas.")]
    public GameObject portal;
    [Tooltip("Etapa 134: variações da moldura. Se houver alguma aqui, cada porta recebe uma delas; senão, vale a moldura de cima.")]
    public GameObject[] portais;
    [Tooltip("Etapa 135: meio trilho de sala, do meio até a porta. O gerador põe um para cada porta do caminho principal do andar. Vazio = sem trilho.")]
    public GameObject trilho;
    [Tooltip("Enfeites sorteados nos pontos de decoração das salas.")]
    public GameObject[] enfeites;
    [Range(0f, 1f)] public float chanceDeEnfeite = 0.6f;
    [Tooltip("Cenas mudas que contam a história (etapa 113). Cada andar recebe uma, na ordem: a primeira no primeiro andar do trecho, e assim por diante.")]
    public GameObject[] cenas;
    [Tooltip("Etapa 123: a luz própria deste trecho, posta no meio de algumas salas: lâmpada elétrica, reflexo da água ou brilho do veio.")]
    public GameObject luminaria;
    [Range(0f, 1f)] public float chanceDeLuminaria = 0.5f;
    [Tooltip("Etapa 142: materiais que mudam neste trecho, como a madeira que fica molhada ou o ferro que enferruja.")]
    public Troca[] trocas;
    [Header("Imagem (etapa 147)")]
    [Range(0f, 1f)] public float granulado = 0.5f;
    [Range(0f, 1f)] public float vinheta = 0.42f;
    [Tooltip("Cor que tinge a imagem inteira. Branco não muda nada.")]
    public Color filtro = Color.white;
    [Range(-100f, 100f)] public float saturacao = -12f;
    [Tooltip("Sons soltos que tocam de vez em quando, perto de você.")]
    public AudioClip[] sonsDoAmbiente;
}
