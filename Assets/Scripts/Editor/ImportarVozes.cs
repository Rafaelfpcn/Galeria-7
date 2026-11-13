using UnityEditor;
using UnityEngine;

// Etapa 112: as vozes das fitas entram comprimidas, para a build (principalmente a de navegador) não crescer 20 MB.
public class ImportarVozes : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.StartsWith("Assets/Resources/Vozes/")) return;
        AudioImporter importador = (AudioImporter)assetImporter;
        importador.forceToMono = true;
        AudioImporterSampleSettings ajustes = importador.defaultSampleSettings;
        ajustes.loadType = AudioClipLoadType.CompressedInMemory;
        ajustes.compressionFormat = AudioCompressionFormat.Vorbis;
        ajustes.quality = 0.4f;
        importador.defaultSampleSettings = ajustes;
    }
}
