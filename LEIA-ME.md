# Galeria 7

Roguelite de terror em primeira pessoa numa mina abandonada, feito na Unity 6. O jogo está completo do menu ao final: acampamento com melhorias, 9 andares gerados em 3 trechos, 3 tipos de inimigo, perigos, itens, 20 fitas com a história, o chefão no andar 10 e os créditos.

Tudo foi montado por código, inclusive os modelos, as texturas e os sons, e testado na Unity 6.6 com testes automáticos e uma build que joga sozinha do menu ao chefão.

Outros arquivos: `HISTORIA.md` tem a linha do tempo e explica as fitas; `itch/pagina.md` tem o texto pronto para a página do itch.io, com capa e prints.

## Como abrir

1. Abra o Unity Hub e deixe ele aberto: sem o Hub, a Unity não acha a licença.
2. Em Projetos, adicione um projeto do disco e escolha a pasta `F:\Galeria7`.
3. Abra o projeto e aperte Play. As cenas já estão montadas e o jogo começa pelo Menu.

## O menu Galeria 7

No topo da Unity há um menu **Galeria 7**:

- **Montar tudo**: recria as cenas Menu, Superficie, Mina e Teste. Use depois de mudar um mapa de sala ou um script de montagem.
- **Montar mina**, **Montar superfície e menu**, **Montar cena de teste**: recriam uma parte só.
- **Gerar builds (Windows e Web)**: gera o jogo na pasta `Builds`. A build Web demora bastante.

As peças (prefabs, materiais, texturas, modelos e sons) só são criadas quando faltam, para não apagar o que você ajustar. Para refazer uma peça a partir do código, apague o arquivo dela e rode o menu de novo.

## Como é uma partida

1. No **Menu**, Novo jogo leva ao acampamento. Continuar volta para o progresso salvo.
2. No **acampamento**, a bancada vende 5 melhorias com o minério guardado, o toca-fitas mostra as fitas encontradas e o elevador desce para a mina.
3. Na **mina**, cada descida começa no andar 1. O poço leva ao próximo andar. Nos andares 3, 6 e 9, um elevador na sala do poço sobe guardando o minério.
4. Os andares 1 a 3 são galerias de madeira; os 4 a 6, cavernas alagadas; os 7 a 9, o veio negro. O andar 10 é a arena do chefão.
5. Se morrer, você perde o minério carregado e acorda no acampamento. As melhorias e as fitas continuam.

O que tem nos andares:

- **Rastejante**: rápido e fraco. Vê você até 15 m se não houver parede no meio e foge da chama avivada e do sinalizador.
- **Ouvinte** (a partir do andar 4): cego. Caça pelo barulho de andar, correr, golpear e minerar. Agachado, você passa em silêncio.
- **Mineiro Oco** (a partir do andar 7): lento e resistente, bate forte, demora para golpear e não tem medo de luz.
- **Desmoronamento**: cai poeira por um segundo e meio antes das pedras. **Bolsão de gás**: a névoa verde apaga o lampião.
- **Itens**: kit médico, frasco de óleo e sinalizador. **Fitas**: uma por andar, no máximo.
- **Chefão**: o Coração do Veio. O chão fica vermelho um segundo antes de cada tentáculo. Quebre os 3 cristais verdes com a picareta; depois do primeiro, as luzes apagam e ele chama Rastejantes.

## Controles

- WASD: andar
- Mouse: olhar
- Shift: correr (gasta fôlego e faz barulho)
- C: agachar (sem barulho)
- Clique esquerdo: golpear com a picareta
- Botão direito (segurar): avivar a chama, que espanta Rastejantes e gasta óleo 3 vezes mais rápido
- E: interagir (itens, fitas, bancada, toca-fitas, elevador, poço)
- F: ligar e desligar o lampião
- Q: jogar um sinalizador aceso
- Esc: pausar, com Continuar, Opções e Voltar ao menu

As opções (volume dos efeitos, volume da música, sensibilidade do mouse, brilho, inverter o eixo Y e menos balanço) ficam no menu e na pausa.

## O save

O progresso fica no arquivo `galeria7.json`, em `C:\Users\<seu usuário>\AppData\LocalLow\Galeria 7\Galeria 7`. Ele guarda o minério, as melhorias, as fitas, o andar mais fundo, se você já zerou e as opções.

O nome da pasta vem de Company Name e Product Name, em Edit > Project Settings > Player. Se quiser pôr o nome do seu estúdio em Company Name, troque antes de publicar: depois de trocar, o jogo passa a procurar o save na pasta nova.

## Onde mexer

- Teclas: `Assets/Scripts/Controles.cs`.
- Texto das fitas: `Assets/Resources/Fitas.txt`. Créditos do final: `Assets/Resources/Creditos.txt`.
- Voz das fitas: `Assets/Resources/Vozes`. Depois de mudar um texto, rode `Ferramentas/gerar_vozes.ps1`. Sem a pasta, as fitas voltam a ser só texto.
- História, cenas mudas, bilhetes e os dois finais: veja `HISTORIA.md`. As cenas de cada trecho ficam na lista **Cenas** dos temas; os textos dos finais, no começo de `Assets/Scripts/Final.cs`; a carta do acampamento, em `Assets/Scripts/Editor/MontarSuperficie.cs`.
- O que cada andar tem (tema, salas, inimigos, veios, itens e perigos): `Assets/Dados/Dificuldade`.
- Aparência de cada trecho (rocha, neblina, enfeites, sons): `Assets/Dados/Tema_Galerias`, `Tema_Cavernas` e `Tema_Veio`.
- Preço e efeito das melhorias: `Assets/Resources/Melhorias`.
- Inimigos: os prefabs `Rastejante`, `Ouvinte` e `MineiroOco`. Todos usam os scripts `Inimigo` e `AnimacaoDeInimigo`.
- Chefão: o prefab `Arena`, no script `Chefao`. Os golpes que cada cristal aguenta ficam no script `Vida` de cada PontoFraco.
- Jogador (velocidade, fôlego, vida, mochila): **Jogador** na Hierarchy de cada cena. Lampião: **Jogador > Camera > Lampiao**.
- Gerador (chances de rampa e de ramos, semente, distância para desligar salas): **Gerador** na cena Mina. A semente aparece no canto da tela.
- Sustos e som ambiente: os scripts `Sustos` e `Ambiente`, no Gerador.
- Salas: os prefabs em `Assets/Prefabs/Salas`, feitos dos mapas de letras em `Assets/Scripts/Editor/MontarMina.cs`. A altura do teto de cada sala fica na lista `Tetos`, no mesmo arquivo. O mapa da mina inteira está em `MAPA.md`.
- Paredes de rocha: a pele irregular é feita pela função `Pele`, em `MontarMina.cs`. Os números `0.24f` e `0.08f` dizem quanto a rocha avança; não passe de 35 cm no total, que é a largura do jogador.
- Luz de cada trecho: o campo **Luminaria** e a **Chance De Luminaria** dos temas.
- Descida de gaiola: **ElevadorDaMina**, na cena Superficie. **Duracao** e **Velocidade** mudam a viagem; com **Mundo** vazio, desce na hora.
- Ouro Fundo: as casas, a igreja e as placas são montadas em `Assets/Scripts/Editor/MontarSuperficie.cs`.
- Visual retrô: `Assets/Settings/PC_RPAsset`, campo Render Scale (0,45; 1 desliga). Pós-processamento: `Assets/Settings/Galeria7Clima`.
- Cores de jogo antigo (32 tons por canal, com pontilhado): liga e desliga nas Opções do jogo. O efeito é o shader `Assets/Shaders/CoresDePS1.shader`; o número de tons fica em `Assets/Scripts/Clima.cs`.
- Texturas base (rocha, madeira, terra, macacão, ferro): saem de fotos, pelo menu **Galeria 7 > Refazer texturas das fotos**. A claridade, a cor e o número de cores de cada uma ficam na lista `Receitas`, em `Assets/Scripts/Editor/MontarTexturas.cs`. Depois de refazer, use **Galeria 7 > Refazer tudo**.
- Cara de cada trecho: nos temas, a lista **Trocas** (madeira molhada, ferro enferrujado) e os campos **Granulado**, **Vinheta**, **Filtro** e **Saturacao**.
- Marcas: a mancha de infiltração é a textura `Mancha` (em `MontarArte.cs`) e aparece em uma de cada três paredes. As setas e o número do nível a giz são postos pelo gerador; os prefabs ficam nos campos **Seta De Giz** e **Numero De Giz** do **Gerador**.
- Sons e texturas: `Assets/Audio` e `Assets/Textures`. Para usar um arquivo seu, ponha um com o mesmo nome no lugar.

Ajuste com o jogo parado: o que você muda durante o Play volta ao valor antigo quando o Play acaba.

## Os modelos 3D

Os inimigos, as suas mãos, o Coração do Veio e o tentáculo são modelos feitos no Blender por script. Nada é desenhado à mão: cada modelo é um texto que diz por onde a forma passa.

- Os scripts ficam na pasta `Blender`: `personagens.py` (Rastejante, Ouvinte, Mineiro Oco), `objetos.py` (mão com lampião, mão com picareta, Coração, tentáculo), `cenario.py` (22 objetos: marmita, rádio, gravador, ferramentas, barril, os quatro itens e os três veios) e `g7.py` (as funções que todos usam).
- `pacote.py` é diferente: ele não modela nada, só converte peças do pacote de mina baixado (`Baixados/free_mine_assets`) para o tamanho e os materiais do jogo. Uso: `blender.exe --background --python F:\Galeria7\Blender\pacote.py -- F:\Galeria7\Baixados\free_mine_assets\mine_parts.blend`. Para trazer outra peça do pacote, acrescente uma linha na lista `PECAS`.
- Para um cubo ou cilindro dos enfeites virar modelo, basta pôr o nome dele na lista `Modelos`, no começo das cenas mudas em `Assets/Scripts/Editor/MontarPecas.cs`.
- Os modelos prontos ficam em `Assets/Modelos3D`, em FBX.
- Cada modelo é feito de peças soltas (tronco, cabeça, cada perna, cada canela), sem esqueleto. Quem mexe as peças é o código: `AnimacaoDeInimigo` para os inimigos e `Tentaculo` para o tentáculo.
- O corpo de mineiro e o capacete caído que aparecem pelo cenário são o próprio modelo do Mineiro Oco, deitado.

Para mudar um modelo, com o Blender em `F:\Blender`:

1. Edite os números do modelo no script. Cada linha de `tubo` é uma lista de anéis: posição (lado, frente, altura) e os dois raios.
2. Rode `F:\Blender\blender-5.2.4-windows-x64\blender.exe --background --python F:\Galeria7\Blender\personagens.py` (ou `objetos.py`).
3. Na Unity, use o menu **Galeria 7 > Montar tudo**.

Se mudar o nome de uma peça (`Perna_1`, `Canela_1`, `Braco_1`, `Antebraco_1`, `Cabeca`, `Queixo`, `Gomo_1`, `Massa`), a animação deixa de achá-la e a Unity avisa no Console.

## O que ainda é com você

- Jogar e ajustar a dificuldade (etapas 30, 60 e 87) e testar com outras pessoas (etapas 85 a 89).
- Gravar o trailer (etapa 94) e publicar no itch.io (etapas 95 a 100), com a sua conta.
- Opcional: gravar vozes para as fitas (etapa 65) e trocar modelos ou sons por arquivos melhores.
