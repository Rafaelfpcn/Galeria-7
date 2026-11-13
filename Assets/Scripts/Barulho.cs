using UnityEngine;

// Etapa 51: barulhos que os inimigos ouvem. Quem faz barulho chama Fazer; cada inimigo decide se ouviu pelo raio.
public static class Barulho
{
    public static event System.Action<Vector3, float> AoFazer;

    public static void Fazer(Vector3 ponto, float raio)
    {
        AoFazer?.Invoke(ponto, raio);
    }
}
