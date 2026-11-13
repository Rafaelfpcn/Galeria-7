using UnityEngine;

// Etapa 18: qualquer objeto com este script pode ser usado com E.
// Para um objeto fazer outra coisa, crie um script que herda de Interagivel e reescreva o Interagir.
public class Interagivel : MonoBehaviour
{
    public string aviso = "E: interagir";

    public virtual void Interagir()
    {
        Debug.Log("Interagiu com " + name);
    }
}
