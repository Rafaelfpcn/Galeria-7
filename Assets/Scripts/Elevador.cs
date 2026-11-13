using System.Collections;
using UnityEngine;

// Etapas 41, 44 e 118: a gaiola. Na superfície, desce para a mina; nos andares 3, 6 e 9, sobe guardando o minério.
// A descida não é um corte: a grade fecha, a campainha dá dois toques e a gaiola afunda no poço por alguns segundos,
// com a luz do acampamento sumindo lá em cima. Quem se mexe é o mundo, que sobe em volta da gaiola: assim você
// continua podendo olhar para onde quiser.
public class Elevador : Interagivel
{
    public bool sobe;

    [Header("A descida (só na superfície)")]
    [Tooltip("Tudo o que sobe em volta da gaiola: o acampamento e as paredes do poço. Vazio = desce na hora, sem viagem.")]
    public Transform mundo;
    [Tooltip("Segundos de viagem.")]
    public float duracao = 10f;
    [Tooltip("Metros por segundo.")]
    public float velocidade = 3f;
    [Tooltip("A grade que fecha a frente da gaiola e o chão e as paredes que seguram você lá dentro. Ficam desligados até a descida.")]
    public GameObject grade;
    [Tooltip("A luz da lua, que apaga conforme a gaiola afunda.")]
    public Light luzDoCeu;
    public AudioClip somDaCampainha;
    public AudioClip somDaDescida;

    bool descendo;
    float escuro; // de 0 a 1: a tela preta do fim da viagem

    public override void Interagir()
    {
        if (descendo) return;
        if (sobe)
        {
            Jogo.Instancia.Guardar(Jogador.Instancia.minerio);
            Jogo.IrPara("Superficie");
        }
        else if (mundo == null) Jogo.IrPara("Mina");
        else StartCoroutine(Descer());
    }

    IEnumerator Descer()
    {
        descendo = true;
        aviso = "Descendo...";
        Jogador jogador = Jogador.Instancia;
        // Você entra na gaiola, de frente para a saída, e a grade fecha.
        GetComponent<Collider>().enabled = false;
        grade.SetActive(true);
        jogador.Teleportar(transform.position + Vector3.up * 0.15f, transform.eulerAngles.y + 180f);

        AudioSource fonte = gameObject.AddComponent<AudioSource>();
        fonte.spatialBlend = 0f;
        if (somDaCampainha != null) fonte.PlayOneShot(somDaCampainha);
        yield return new WaitForSeconds(1.2f);
        fonte.clip = somDaDescida;
        fonte.loop = true;
        fonte.Play();

        Color ambiente = RenderSettings.ambientLight, neblina = RenderSettings.fogColor;
        float lua = luzDoCeu != null ? luzDoCeu.intensity : 0f;
        jogador.Tremer(0.5f); // o tranco da partida
        for (float t = 0f; t < duracao; t += Time.deltaTime)
        {
            float arranque = Mathf.Clamp01(t / 1.5f); // a gaiola pega velocidade aos poucos
            mundo.position += Vector3.up * velocidade * arranque * Time.deltaTime;
            float fundo = Mathf.Clamp01(t / 4f);
            RenderSettings.ambientLight = Color.Lerp(ambiente, new Color(0.008f, 0.008f, 0.01f), fundo);
            RenderSettings.fogColor = Color.Lerp(neblina, Color.black, fundo);
            if (luzDoCeu != null) luzDoCeu.intensity = lua * (1f - fundo);
            if (Random.value < 2f * Time.deltaTime) jogador.Tremer(0.12f); // a gaiola bate nas guias
            escuro = Mathf.Clamp01((t - (duracao - 1.5f)) / 1.5f);
            fonte.volume = 1f - escuro * 0.7f;
            yield return null;
        }
        Jogo.IrPara("Mina");
    }

    void OnGUI()
    {
        if (escuro <= 0f) return;
        GUI.depth = -10;
        Estilo.Comecar();
        Estilo.Escurecer(escuro);
    }
}
