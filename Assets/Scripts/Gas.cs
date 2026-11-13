using UnityEngine;

// Etapa 55: bolsão de gás. A névoa verde avisa; dentro dela, o lampião apaga e não acende até você sair.
public class Gas : MonoBehaviour
{
    void OnTriggerStay(Collider outro)
    {
        Jogador jogador = outro.GetComponentInParent<Jogador>();
        if (jogador != null && jogador.lampiao != null) jogador.lampiao.ligado = false;
    }
}
