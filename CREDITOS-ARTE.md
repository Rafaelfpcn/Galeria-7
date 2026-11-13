# Galeria 7 — origem da arte

Tudo o que não foi feito por código vem de fontes com licença CC0 (domínio público): pode ser usado, modificado e publicado sem pedir permissão e sem dar crédito. O crédito abaixo é por gentileza e para saber de onde veio cada coisa.

Os arquivos originais ficam na pasta `Baixados`, fora do jogo. Para dentro do jogo só vão versões reduzidas e adaptadas.

| Arquivo | O que é | Origem | Licença | Baixado em |
| --- | --- | --- | --- | --- |
| `free_mine_assets.zip` | 127 peças de mina: escoras, trilhos, carrinho, lampiões, baú, pedras, estalagmites | [Free mine assets pack](https://opengameart.org/content/free-mine-assets-pack), de rubberduck, no OpenGameArt | CC0 | 11/11/2026 |
| `Rock060_1K-JPG.zip` | Foto de rocha, para as paredes | [ambientCG](https://ambientcg.com/view?id=Rock060) | CC0 1.0 | 11/11/2026 |
| `Planks040_1K-JPG.zip` | Foto de tábuas, para a madeira | [ambientCG](https://ambientcg.com/view?id=Planks040) | CC0 1.0 | 11/11/2026 |
| `Metal063_1K-JPG.zip` | Foto de metal com ferrugem | [ambientCG](https://ambientcg.com/view?id=Metal063) | CC0 1.0 | 11/11/2026 |
| `Ground111_1K-JPG.zip` | Foto de chão de terra e cascalho | [ambientCG](https://ambientcg.com/view?id=Ground111) | CC0 1.0 | 11/11/2026 |
| `Fabric086_1K-JPG.zip` | Foto de tecido, para a roupa do Mineiro Oco | [ambientCG](https://ambientcg.com/view?id=Fabric086) | CC0 1.0 | 11/11/2026 |

## O que já está no jogo

Do pacote **Free mine assets pack** (CC0), convertidas pelo script `Blender/pacote.py` para `Assets/Modelos3D/Kit_*.fbx`:

- três molduras de porta, uma viga, uma escora e um feixe de tábuas;
- quatro pedras soltas e dois montes de pedra;
- três estalagmites e um grupo delas;
- o carrinho de mina, o lampião de pendurar e a arandela;
- as texturas de madeira e de ferro, reduzidas a 128 pixels (`Assets/Modelos3D/Texturas`).

As cinco fotos do ambientCG (CC0) viraram as texturas base do jogo, pelo script `Assets/Scripts/Editor/MontarTexturas.cs` (menu **Galeria 7 > Refazer texturas das fotos**). Cada foto é reduzida a 64 pixels e a uma paleta de 8 a 12 cores:

| Foto | Textura do jogo | Onde aparece |
| --- | --- | --- |
| Rock060 | `Rocha` | Paredes, teto, chão e pedras. |
| Planks040 | `Madeira` | Tábuas, mesas, postes, a gaiola. |
| Ground111 | `Terra` | O chão de Ouro Fundo. |
| Fabric086 | `Macacao` | A roupa do Mineiro Oco e a sua manga. |
| Metal063 | `Ferro` | Tudo o que é de metal. |

As texturas de madeira e de ferro do pacote de mina passam pelo mesmo redutor (`KitMadeira` e `KitFerro`).

## Ferramentas

- **Unity 6**, plano Personal.
- **Blender 5.2.4 LTS**, em `F:\Blender`. O programa é software livre (GPL); os modelos feitos nele são do autor do jogo.
- **Voz Microsoft Daniel (pt-BR)**, que vem com o Windows. Fala as 20 fitas (`Assets/Resources/Vozes`), pelo script `Ferramentas/gerar_vozes.ps1`. Atenção: os termos do Windows não dizem com clareza se a fala gerada por essas vozes pode ser distribuída num jogo. Para não ter dúvida ao publicar, grave as fitas com a sua voz por cima dos mesmos arquivos, ou apague a pasta e deixe as fitas só em texto.

## Feito por código neste projeto

Os sons (`Assets/Audio`), as texturas de 32 pixels (`Assets/Textures`) e os modelos simples (`Assets/Models`) da primeira versão foram gerados pelos scripts `MontarSons`, `MontarArte` e `MontarPecas`.
