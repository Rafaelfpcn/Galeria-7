using UnityEngine;

// Etapa 21: cubo de teste que fere quem encosta nele. O collider precisa estar com Is Trigger marcado.
public class Armadilha : MonoBehaviour
{
    public int dano = 99;

    void OnTriggerEnter(Collider outro)
    {
        Vida vida = outro.GetComponentInParent<Vida>();
        if (vida != null) vida.LevarDano(dano);
    }
}
