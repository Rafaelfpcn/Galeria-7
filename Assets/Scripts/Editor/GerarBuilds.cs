using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Etapas 50, 90 e 91: gera as builds para Windows e para navegador na pasta Builds, ao lado da pasta Assets.
// Menu Galeria 7 > Gerar builds. A build Web precisa do módulo Web Build Support e demora bem mais.
public static class GerarBuilds
{
    [MenuItem("Galeria 7/Gerar builds (Windows e Web)")]
    static void Menu()
    {
        Windows();
        Web();
        EditorUtility.RevealInFinder("Builds/Windows");
    }

    public static void Windows()
    {
        Gerar(BuildTarget.StandaloneWindows64, "Builds/Windows/Galeria 7.exe");
    }

    public static void Web()
    {
        // O itch.io entrega os arquivos compactados sem avisar o navegador; com esta opção, o próprio jogo descompacta.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        Gerar(BuildTarget.WebGL, "Builds/Web");
    }

    static void Gerar(BuildTarget alvo, string destino)
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(alvo), alvo))
        {
            Debug.LogError("[build] Falta o módulo de " + alvo + ". Instale pelo Unity Hub, em Installs > Add modules.");
            return;
        }
        string[] cenas = EditorBuildSettings.scenes.Where(c => c.enabled).Select(c => c.path).ToArray();
        BuildReport relatorio = BuildPipeline.BuildPlayer(cenas, destino, alvo, BuildOptions.None);
        Debug.Log("[build] " + alvo + ": " + relatorio.summary.result + ", " + relatorio.summary.totalErrors + " erro(s), " + (relatorio.summary.totalSize / 1048576) + " MB, em " + destino);
    }
}
