using System.Collections.Generic;
using UnityEngine;

// Etapa 61: as fitas da história. O texto fica em Assets/Resources/Fitas.txt, que dá para editar em qualquer editor de texto.
public static class Roteiro
{
    public class Gravacao
    {
        public string titulo;
        public string texto;
    }

    // Etapa 63: a última fita de cada trecho da história. O trecho 0 são os andares 1 a 3; o 1, os andares 4 a 6;
    // o 2, os andares 7 a 9. A fita que sobra depois do último trecho é a do final.
    static readonly int[] fimDoTrecho = { 6, 13, 19 };

    static Gravacao[] fitas;

    public static Gravacao[] Fitas
    {
        get
        {
            if (fitas == null) Carregar();
            return fitas;
        }
    }

    // As fitas são numeradas a partir de 1.
    public static Gravacao Fita(int numero)
    {
        return Fitas[Mathf.Clamp(numero, 1, Fitas.Length) - 1];
    }

    // Etapa 112: a voz da fita, se existir o arquivo Assets/Resources/Vozes/fita_01.wav (02, 03...). Sem arquivo, a fita é só texto.
    public static AudioClip Voz(int numero)
    {
        return Resources.Load<AudioClip>("Vozes/fita_" + numero.ToString("00"));
    }

    public static int PrimeiraDoTrecho(int trecho)
    {
        return trecho == 0 ? 1 : fimDoTrecho[trecho - 1] + 1;
    }

    public static int UltimaDoTrecho(int trecho)
    {
        return Mathf.Min(fimDoTrecho[trecho], Fitas.Length - 1);
    }

    // Cada bloco do arquivo, separado por uma linha em branco, é uma fita: a primeira linha é o título.
    // Blocos que começam com # são anotações e ficam de fora.
    static void Carregar()
    {
        List<Gravacao> lista = new List<Gravacao>();
        TextAsset arquivo = Resources.Load<TextAsset>("Fitas");
        string tudo = arquivo != null ? arquivo.text.Replace("\r", "") : "";
        foreach (string bloco in tudo.Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries))
        {
            string limpo = bloco.Trim();
            if (limpo.Length == 0 || limpo.StartsWith("#")) continue;
            int quebra = limpo.IndexOf('\n');
            lista.Add(new Gravacao
            {
                titulo = quebra < 0 ? limpo : limpo.Substring(0, quebra),
                texto = quebra < 0 ? "" : limpo.Substring(quebra + 1).Replace("\n", " "),
            });
        }
        if (lista.Count == 0) lista.Add(new Gravacao { titulo = "Fita apagada", texto = "Só chiado." });
        fitas = lista.ToArray();
    }
}
