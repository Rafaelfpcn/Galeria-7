using System.Collections.Generic;
using UnityEngine;

// Etapa 56: sinalizador aceso. Dá luz forte por alguns segundos e espanta os inimigos com medo de luz que estiverem perto.
public class Sinalizador : MonoBehaviour
{
    public static readonly List<Sinalizador> Acesos = new List<Sinalizador>();

    public float duracao = 10f;

    void OnEnable() { Acesos.Add(this); }
    void OnDisable() { Acesos.Remove(this); }

    void Start()
    {
        Destroy(gameObject, duracao);
    }
}
