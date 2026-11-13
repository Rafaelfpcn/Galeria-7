# Etapa 112: grava a voz das 20 fitas com a voz em portugues do proprio Windows (Daniel) e passa cada gravacao
# por um filtro de gravador antigo: som abafado, chiado, zumbido e a rotacao da fita oscilando.
# Uso, no PowerShell:  powershell -ExecutionPolicy Bypass -File F:\Galeria7\Ferramentas\gerar_vozes.ps1
# Le Assets\Resources\Fitas.txt e grava Assets\Resources\Vozes\fita_01.wav ... fita_20.wav.
# Para o jogo voltar a mostrar as fitas so em texto, apague a pasta Assets\Resources\Vozes.
# (Este arquivo nao tem acentos de proposito: o PowerShell do Windows le scripts sem acento com mais seguranca.)

$raiz = Split-Path -Parent $PSScriptRoot
$saida = Join-Path $raiz "Assets\Resources\Vozes"
New-Item -ItemType Directory -Force $saida | Out-Null

Add-Type -AssemblyName System.Runtime.WindowsRuntime
[Windows.Media.SpeechSynthesis.SpeechSynthesizer, Windows.Media.SpeechSynthesis, ContentType = WindowsRuntime] | Out-Null
[Windows.Storage.Streams.DataReader, Windows.Storage.Streams, ContentType = WindowsRuntime] | Out-Null
$comoTarefa = ([System.WindowsRuntimeSystemExtensions].GetMethods() | Where-Object {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and $_.GetParameters()[0].ParameterType.Name -eq 'IAsyncOperation`1' })[0]
function Esperar($operacao, $tipo) {
    $tarefa = $comoTarefa.MakeGenericMethod($tipo).Invoke($null, @($operacao))
    $tarefa.Wait()
    $tarefa.Result
}

Add-Type -TypeDefinition @"
using System;
public static class FiltroDeFita
{
    // Recebe um WAV de 16 bits mono e devolve outro, com som de gravador de fita velho.
    public static byte[] Aplicar(byte[] wav, int semente)
    {
        int taxa = 16000, inicio = 44, tamanho = wav.Length - 44;
        for (int i = 12; i + 8 < wav.Length; )
        {
            string id = System.Text.Encoding.ASCII.GetString(wav, i, 4);
            int n = BitConverter.ToInt32(wav, i + 4);
            if (id == "fmt ") taxa = BitConverter.ToInt32(wav, i + 12);
            if (id == "data") { inicio = i + 8; tamanho = Math.Min(n, wav.Length - inicio); break; }
            i += 8 + n + (n & 1);
        }
        int falas = tamanho / 2;
        int antes = (int)(taxa * 0.4), depois = (int)(taxa * 0.6);
        float[] x = new float[antes + falas + depois];
        for (int i = 0; i < falas; i++) x[antes + i] = BitConverter.ToInt16(wav, inicio + i * 2) / 32768f;

        Random sorteio = new Random(semente);
        float[] y = new float[x.Length];
        double dt = 1.0 / taxa;
        float aAlto = (float)(1.0 / (1.0 + 2 * Math.PI * 320 * dt));          // corta os graves abaixo de 320 Hz
        float aBaixo = (float)(2 * Math.PI * 2900 * dt / (1 + 2 * Math.PI * 2900 * dt)); // e os agudos acima de 2900 Hz
        float xAntes = 0, alto = 0, b1 = 0, b2 = 0, chiado = 0;
        for (int i = 0; i < x.Length; i++)
        {
            // A fita nao gira sempre igual: le a gravacao um pouco adiantada ou atrasada.
            double t = i * dt;
            double atraso = taxa * (0.0020 + 0.0012 * Math.Sin(2 * Math.PI * 0.55 * t) + 0.00018 * Math.Sin(2 * Math.PI * 6.3 * t));
            double onde = i - atraso;
            float s = 0;
            if (onde >= 0 && onde < x.Length - 1) { int k = (int)onde; float f = (float)(onde - k); s = x[k] * (1 - f) + x[k + 1] * f; }

            alto = aAlto * (alto + s - xAntes); xAntes = s;
            b1 += aBaixo * (alto - b1);
            b2 += aBaixo * (b1 - b2);
            float voz = (float)Math.Tanh(b2 * 4.5) * 0.85f; // o gravador distorce quando a voz sobe

            chiado += 0.35f * ((float)(sorteio.NextDouble() * 2 - 1) - chiado);
            float zumbido = 0.004f * (float)Math.Sin(2 * Math.PI * 60 * t);
            float falha = sorteio.NextDouble() < 0.00004 ? 0.25f : 1f; // de vez em quando o som some por um instante
            y[i] = voz * falha + chiado * 0.03f + zumbido;
        }
        // O clique do botao no comeco e no fim.
        for (int i = 0; i < taxa / 200; i++) { y[i] += 0.5f * (1 - i / (taxa / 200f)); y[y.Length - 1 - i] -= 0.4f * (1 - i / (taxa / 200f)); }

        byte[] novo = new byte[44 + y.Length * 2];
        Action<int, string> texto = (p, s) => System.Text.Encoding.ASCII.GetBytes(s).CopyTo(novo, p);
        texto(0, "RIFF"); BitConverter.GetBytes(36 + y.Length * 2).CopyTo(novo, 4); texto(8, "WAVEfmt ");
        BitConverter.GetBytes(16).CopyTo(novo, 16); BitConverter.GetBytes((short)1).CopyTo(novo, 20); BitConverter.GetBytes((short)1).CopyTo(novo, 22);
        BitConverter.GetBytes(taxa).CopyTo(novo, 24); BitConverter.GetBytes(taxa * 2).CopyTo(novo, 28);
        BitConverter.GetBytes((short)2).CopyTo(novo, 32); BitConverter.GetBytes((short)16).CopyTo(novo, 34);
        texto(36, "data"); BitConverter.GetBytes(y.Length * 2).CopyTo(novo, 40);
        for (int i = 0; i < y.Length; i++)
            BitConverter.GetBytes((short)(Math.Max(-1f, Math.Min(1f, y[i])) * 32000)).CopyTo(novo, 44 + i * 2);
        return novo;
    }
}
"@

# Cada narrador com a sua altura e a sua pressa. O nome e procurado no titulo da fita.
$narradores = @(
    @{ nome = 'Tonho';    tom = '-8%';  ritmo = '1.0'  },
    @{ nome = 'Dito';     tom = '+18%'; ritmo = '1.08' },
    @{ nome = 'lvaro';    tom = '+0%';  ritmo = '0.92' },
    @{ nome = 'Severino'; tom = '-18%'; ritmo = '0.85' },
    @{ nome = 'Louren';   tom = '-4%';  ritmo = '1.1'  },
    @{ nome = 'Joaquim';  tom = '-14%'; ritmo = '0.88' }
)

$sintetizador = New-Object Windows.Media.SpeechSynthesis.SpeechSynthesizer
$voz = [Windows.Media.SpeechSynthesis.SpeechSynthesizer]::AllVoices | Where-Object { $_.Language -eq 'pt-BR' -and $_.Gender -eq 'Male' } | Select-Object -First 1
if ($voz -eq $null) { $voz = [Windows.Media.SpeechSynthesis.SpeechSynthesizer]::AllVoices | Where-Object { $_.Language -like 'pt-*' } | Select-Object -First 1 }
if ($voz -eq $null) { Write-Host "Este Windows nao tem voz em portugues instalada. Nada foi gravado."; exit 1 }
$sintetizador.Voice = $voz
Write-Host ("Voz: " + $voz.DisplayName)

$tudo = [IO.File]::ReadAllText((Join-Path $raiz "Assets\Resources\Fitas.txt"), [Text.Encoding]::UTF8).Replace("`r", "")
$numero = 0
foreach ($bloco in ($tudo -split "`n`n")) {
    $bloco = $bloco.Trim()
    if ($bloco.Length -eq 0 -or $bloco.StartsWith("#")) { continue }
    $numero++
    $quebra = $bloco.IndexOf("`n")
    if ($quebra -lt 0) { continue }
    $titulo = $bloco.Substring(0, $quebra)
    $fala = [Security.SecurityElement]::Escape($bloco.Substring($quebra + 1).Replace("`n", " "))
    $quem = $narradores | Where-Object { $titulo -like ("*" + $_.nome + "*") } | Select-Object -First 1
    if ($quem -eq $null) { $quem = $narradores[0] }
    $ssml = '<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="pt-BR"><prosody pitch="' + $quem.tom + '" rate="' + $quem.ritmo + '">' + $fala + '</prosody></speak>'
    $fluxo = Esperar ($sintetizador.SynthesizeSsmlToStreamAsync($ssml)) ([Windows.Media.SpeechSynthesis.SpeechSynthesisStream])
    $leitor = New-Object Windows.Storage.Streams.DataReader($fluxo.GetInputStreamAt(0))
    Esperar ($leitor.LoadAsync([uint32]$fluxo.Size)) ([uint32]) | Out-Null
    $bytes = New-Object byte[] ([int]$fluxo.Size)
    $leitor.ReadBytes($bytes)
    $pronto = [FiltroDeFita]::Aplicar($bytes, $numero)
    $arquivo = Join-Path $saida ("fita_{0:00}.wav" -f $numero)
    [IO.File]::WriteAllBytes($arquivo, $pronto)
    $taxa = [BitConverter]::ToInt32($pronto, 24)
    Write-Host ("fita_{0:00}.wav  {1,5:0.0} s  {2}" -f $numero, (($pronto.Length - 44) / 2 / $taxa), $quem.nome)
}
Write-Host ("Pronto: " + $numero + " fitas em " + $saida)
