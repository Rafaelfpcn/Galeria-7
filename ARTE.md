# Galeria 7 — guia de arte

Etapa 107 do plano. Todo modelo, textura e luz novos seguem estas regras, para o jogo inteiro parecer feito pela mesma mão.

## As três ideias

1. **Só existe o que o lampião ilumina.** A arte é pensada para luz quente, de perto, com sombra dura. Nada de luz geral.
2. **Silhueta antes de detalhe.** Cada inimigo e cada objeto importante se reconhece pelo contorno, a 10 metros, no escuro.
3. **Minas Gerais, 1983.** Madeira de escoramento, ferro com ferrugem, objetos de mineiro de verdade. Nada de fantasia medieval.

## Imagem

- O jogo é desenhado a 45% da resolução e esticado sem borrar (Render Scale 0,45, filtro Point). Detalhe menor que 2 cm some: não vale a pena modelar.
- Sem mapas de relevo (normal maps). O relevo vem da forma do modelo e da pintura da textura.

## Texturas

- Tamanho: 64 × 64 pixels para o que se repete (parede, chão, madeira); até 128 × 128 para um objeto ou personagem inteiro.
- Densidade: 32 pixels por metro nas paredes e no chão. Os objetos seguem a mesma densidade, para nenhum parecer mais nítido que a parede.
- Cores: de 8 a 16 por textura, tiradas da paleta do trecho.
- Importação na Unity: Filter Mode Point, sem mipmap, sem compressão.
- Origem: fotos CC0 reduzidas por script, ou pintura por código. A origem de cada uma fica anotada em `CREDITOS-ARTE.md`.

## Paletas

| Trecho | Base | Sombra | Destaque | Luz própria |
| --- | --- | --- | --- | --- |
| Galerias (andares 1 a 3) | `#5B4A3C` rocha, `#6B4326` madeira | `#2A211B` | `#8A7458` | `#FFB45A` lampiões |
| Níveis alagados (4 a 6) | `#2E3B4A` rocha molhada | `#141C24` | `#58708A` | `#7FB6D9` reflexo da água |
| O veio (7 a 9) e a arena | `#0E0E12` rocha negra, `#3A0A10` carne | `#050506` | `#1E1E28` | `#2BC45A` veio |

Cores com significado, iguais no jogo inteiro:

- **Âmbar** (`#FFB45A`): a sua luz e o que é seguro.
- **Branco-creme** (`#E6DFC8`): o que se pega, como itens e fitas.
- **Vermelho** (`#E0261A`): perigo agora, como olhos, aviso de ataque e chão do tentáculo.
- **Verde** (`#2BC45A`): o veio, e tudo o que ele tocou.

## Modelos

| Tipo | Triângulos | Exemplo |
| --- | --- | --- |
| Objeto pequeno | 20 a 120 | marmita, garrafa, fita |
| Objeto grande | 150 a 400 | carrinho, escora, bomba d'água |
| Peça de túnel | 100 a 300 | parede de rocha de 3 m |
| Inimigo | 400 a 900 | Rastejante, Ouvinte, Mineiro Oco |
| Mãos do jogador | até 600 | braços, lampião e picareta |
| Chefão | até 2.500 | Coração do Veio |

- Escala: 1 unidade é 1 metro. A origem do modelo fica no chão, no meio da base.
- Faces chapadas (sem alisar), menos em carne e tentáculos.
- A colisão é sempre uma caixa, esfera ou cápsula simples, separada do modelo.
- Os modelos são feitos por scripts do Blender, na pasta `Blender` do projeto. Refazer um modelo é rodar o script de novo. Exportação em FBX com frente `-Z`, cima `Y` e escala `FBX_SCALE_UNITS`, que já foi testada: chega à Unity com tamanho e eixos certos.

## Animação

- Poucos quadros e movimento seco, sem suavizar demais, como nos jogos da época.
- Todo inimigo tem andar, aviso de ataque, golpe, levar golpe e morrer. O aviso de ataque dura o mesmo tempo do campo Aviso do script `Inimigo`, para o jogador poder ler o golpe.

## Luz

- Só o lampião do jogador faz sombra. As outras luzes são pequenas, sem sombra e poucas por sala.
- Cada sala tem um ponto iluminado que chama o olho e um canto escuro que dá medo.
- O que importa para jogar brilha por conta própria: itens, fitas, poço, pontos fracos e avisos de ataque nunca dependem do lampião para aparecer.
