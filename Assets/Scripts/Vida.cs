using UnityEngine;

// Etapa 21: vida de quem pode se ferir, seja o jogador ou um inimigo.
public class Vida : MonoBehaviour
{
    public int maxima = 5;

    [HideInInspector] public int atual;
    [HideInInspector] public float ultimoDano = -100f; // quando levou o último golpe; a tela vermelha usa isto

    public event System.Action AoSofrerDano;
    public event System.Action AoMorrer;

    public bool Morto { get { return atual <= 0; } }

    void Awake()
    {
        atual = maxima;
    }

    public void LevarDano(int dano)
    {
        if (Morto) return;
        atual = Mathf.Max(0, atual - dano);
        ultimoDano = Time.time;
        AoSofrerDano?.Invoke();
        if (Morto) AoMorrer?.Invoke();
    }
}
