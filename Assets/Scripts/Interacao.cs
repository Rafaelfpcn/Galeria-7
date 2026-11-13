using UnityEngine;

// Etapa 18: mira no centro da tela e usa o Interagivel com E. Fica na câmera.
public class Interacao : MonoBehaviour
{
    public float alcance = 2.5f;

    [HideInInspector] public Interagivel alvo;

    void Update()
    {
        alvo = null;
        RaycastHit acerto;
        if (Physics.Raycast(transform.position, transform.forward, out acerto, alcance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            alvo = acerto.collider.GetComponentInParent<Interagivel>();

        if (alvo != null && Controles.Interagir()) alvo.Interagir();
    }
}
