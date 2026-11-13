using UnityEngine;

// Etapa 55: teto que desaba. Quando você entra na área, cai poeira e a madeira estala; só depois as pedras caem.
// Quem saiu da área durante o aviso não se machuca.
public class Desmoronamento : MonoBehaviour
{
    public float aviso = 1.5f;
    public int dano = 2;
    public ParticleSystem poeira;
    public GameObject entulho; // pedras que caem, só de enfeite
    [Header("Sons (arraste arquivos de som aqui)")]
    public AudioClip somDeEstalo;
    public AudioClip somDeQueda;

    bool armado = true;

    void OnTriggerEnter(Collider outro)
    {
        if (!armado || outro.GetComponentInParent<Jogador>() == null) return;
        armado = false;
        if (poeira != null) poeira.Play();
        Tocar(somDeEstalo);
        Invoke(nameof(Cair), aviso);
    }

    void Cair()
    {
        Tocar(somDeQueda);
        Jogador jogador = Jogador.Instancia;
        if (jogador != null && GetComponent<Collider>().bounds.Contains(jogador.transform.position + Vector3.up)) jogador.vida.LevarDano(dano);
        for (int i = 0; i < 6; i++)
        {
            Vector3 lugar = transform.position + new Vector3(Random.Range(-1.2f, 1.2f), 2.6f, Random.Range(-1.2f, 1.2f));
            Instantiate(entulho, lugar, Random.rotation, transform);
        }
        Barulho.Fazer(transform.position, 12f);
    }

    void Tocar(AudioClip clip)
    {
        AudioSource som = GetComponent<AudioSource>();
        if (clip != null && som != null) som.PlayOneShot(clip);
    }
}
