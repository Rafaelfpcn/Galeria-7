using UnityEngine;

// Etapa 78: a trilha. Uma faixa calma toca sempre; quando um inimigo persegue você, ou no chefão, entra a de tensão.
// O volume vem da opção "Volume da música", separada do volume dos efeitos.
public class Musica : MonoBehaviour
{
    public AudioClip calma;
    public AudioClip tensao;
    [Range(0f, 1f)] public float volumeDaCalma = 0.5f;
    [Range(0f, 1f)] public float volumeDaTensao = 0.8f;

    AudioSource fonteCalma;
    AudioSource fonteTensao;

    void Start()
    {
        fonteCalma = Criar(calma);
        fonteTensao = Criar(tensao);
    }

    AudioSource Criar(AudioClip faixa)
    {
        AudioSource fonte = gameObject.AddComponent<AudioSource>();
        fonte.clip = faixa;
        fonte.loop = true;
        fonte.volume = 0f;
        fonte.ignoreListenerVolume = true; // o volume dos efeitos não mexe na música
        fonte.ignoreListenerPause = true;
        if (faixa != null) fonte.Play();
        return fonte;
    }

    void Update()
    {
        bool perigo = false;
        foreach (Inimigo inimigo in Inimigo.Todos) if (inimigo.Cacando) perigo = true;
        GeradorDeAndar gerador = GeradorDeAndar.Instancia;
        if (gerador != null && gerador.NaArena) perigo = true;

        float geral = Jogo.Instancia.dados.opcoes.volumeMusica;
        float passo = Time.unscaledDeltaTime * 0.6f; // troca em mais ou menos um segundo e meio
        fonteCalma.volume = Mathf.MoveTowards(fonteCalma.volume, (perigo ? 0.3f : 1f) * volumeDaCalma * geral, passo);
        fonteTensao.volume = Mathf.MoveTowards(fonteTensao.volume, (perigo ? 1f : 0f) * volumeDaTensao * geral, passo);
    }
}
