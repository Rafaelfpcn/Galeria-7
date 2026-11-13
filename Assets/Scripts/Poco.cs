using UnityEngine;

// Etapas 36 e 40: o poço da sala de saída. Com E, desce para o próximo andar.
public class Poco : Interagivel
{
    public override void Interagir()
    {
        GeradorDeAndar.Instancia.Descer();
    }
}
