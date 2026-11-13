# Galeria 7 — o mapa da mina

Etapa 117 do plano. Cada trecho tem uma função, uma luz e marcos próprios, para a mina parecer um lugar de trabalho abandonado às pressas, e não um labirinto de corredores iguais.

```
        OURO FUNDO (superfície)
        três casas fechadas · igreja · postes apagados
        acampamento: fogueira, cabana, bancada, toca-fitas, carta e foto
        boca da mina: placa da companhia, interdição, quadro de chapas
                       |
                    GAIOLA  — 11 segundos de descida, marcas de 10, 20 e 30 m
                       |
  ┌─────────────────────────────────────────────────────────────┐
  │ ANDARES 1 a 3 — AS GALERIAS DA COMPANHIA                    │
  │ luz: lâmpadas elétricas falhando, penduradas num fio        │
  │ madeira nas portas, caixotes, barris, carrinhos, trilhos    │
  │ cenas: refeitório · mesa do capataz · botas dos Batista     │
  │ inimigo: Rastejante                                         │
  └─────────────────────────────────────────────────────────────┘
                       |  poço (e gaiola de volta no andar 3)
  ┌─────────────────────────────────────────────────────────────┐
  │ ANDARES 4 a 6 — OS NÍVEIS ALAGADOS                          │
  │ luz: o reflexo azulado da água, que sobe e desce devagar    │
  │ água rasa no chão, rocha molhada, estalagmites              │
  │ cenas: nicho de Santa Bárbara · cofre das amostras · Chico  │
  │ inimigos: Rastejante e Ouvinte                              │
  └─────────────────────────────────────────────────────────────┘
                       |  poço (e gaiola de volta no andar 6)
  ┌─────────────────────────────────────────────────────────────┐
  │ ANDARES 7 a 9 — O VEIO                                      │
  │ luz: só o brilho verde dos cristais, batendo como coração   │
  │ a madeira acaba; a rocha é preta e tem veias                │
  │ cenas: dinamite e pavio · os cinco capacetes                │
  │ inimigos: os três                                           │
  └─────────────────────────────────────────────────────────────┘
                       |  poço (e gaiola de volta no andar 9)
  ┌─────────────────────────────────────────────────────────────┐
  │ ANDAR 10 — O CORAÇÃO DO VEIO                                │
  │ salão de 24 x 24 m, o coração preso na parede norte         │
  │ o Joaquim, sentado contra uma pedra, com o lampião no fim   │
  └─────────────────────────────────────────────────────────────┘
```

## O que cada trecho tem de diferente

| | Galerias (1 a 3) | Níveis alagados (4 a 6) | Veio (7 a 9) |
| --- | --- | --- | --- |
| Para que servia | Transporte e apoio: é onde a companhia tinha estrutura. | Lavra: é onde se tirava o minério, e onde a água começou a minar. | Não servia para nada: ninguém devia ter chegado lá. |
| Rocha | Parda. | Molhada, azulada. | Preta, com veias verdes. |
| Luz própria | Lâmpada elétrica falhando. | Reflexo da água. | Brilho dos cristais. |
| Chão | Seco. | Água rasa. | Seco. |
| Enfeites | Caixote, barril, carrinho, lampião de poste, capacete, corpo. | Estalagmites, pedras, corpo. | Caroços do veio, estalagmites, corpo. |
| Neblina | Pouca. | Azul. | Verde. |

Tudo isto se ajusta nos arquivos `Assets/Dados/Tema_Galerias`, `Tema_Cavernas` e `Tema_Veio`.

## As salas

As 14 salas comuns são sorteadas em todos os trechos. O que muda de uma para outra é a largura e a altura do teto:

| Sala | Teto | Sala | Teto |
| --- | --- | --- | --- |
| Reta, Curva, T, Rampa | 3,0 m | Saída (a do poço), Galeria | 4,2 m |
| Cruzamento, Beco, Depósito, Curva larga, T larga | 3,6 m | Salão, Cruzamento largo | 4,5 m |
| | | Capela (a sala sem inimigos) | 5,0 m |

Nas portas, o teto tem sempre 3 m, para casar com a sala vizinha. As paredes e o teto têm uma pele de rocha irregular por cima dos blocos; a colisão continua sendo a dos blocos lisos.

Para mudar a altura de uma sala: a lista `Tetos`, em `Assets/Scripts/Editor/MontarMina.cs`, e depois o menu **Galeria 7 > Refazer tudo**.

## As salas especiais

Além das 14 salas comuns, há 7 com móveis próprios. Cada uma só aparece no trecho a que pertence, e nenhuma serve de sala de começo.

| Sala | Andares | Entradas | O que tem |
| --- | --- | --- | --- |
| Refeitório | 1 a 3 | norte e sul | Duas mesas compridas com marmitas, o fogão a lenha e um recado do Tonho. |
| Lamparia | 1 a 3 | só norte | Prateleiras de lampiões, a bancada do Severino com o único lampião aceso, e três pontos de item. Sem inimigo. |
| Paiol | 1 a 3 | só norte | Porta de ferro, placa de perigo, prateleiras e caixotes vazios, e o inventário do Zé Lourenço. |
| Capela | 4 a 6 | só norte | Bancos, cruz na parede e o nicho de Santa Bárbara com as velas. Sem inimigo. |
| Casa de bombas | 4 a 6 | norte e leste | Duas bombas paradas, canos, painel e o livro de ocorrências. |
| Poço inclinado | 4 a 9 | norte (alto) e sul (baixo) | Uma rampa com trilho, o guincho em cima e um carrinho carregado preso pelo cabo. |
| Mezanino | 1 a 9 | norte e sul | Salão de dois níveis: entra-se por cima, uma rampa desce para o fosso, e uma passarela cruza por cima dele. Passa-se por baixo da passarela agachado. |

Cinco delas têm um papel que se lê com E.

Para mudar em que andares uma sala aparece: a lista `Andares`, em `Assets/Scripts/Editor/MontarMina.cs`. Os móveis de cada uma ficam na função `Mobiliar`, em `Assets/Scripts/Editor/MontarPecas.cs`.

## Para fazer uma sala nova

1. Em `MontarMina.cs`, acrescente o nome em `Nomes`, o mapa de letras em `Mapas`, o peso em `Pesos` e a altura do teto em `Tetos`. As quatro listas têm de ficar na mesma ordem.
2. Se for sala especial, acrescente também os andares em `Andares` e os móveis em `Mobiliar`.
3. Use o menu **Galeria 7 > Refazer tudo**.

O teste automático gerou 207 andares (23 sementes, andares 1 a 9) com as 21 salas e conferiu, em todos: as portas casam com as da sala vizinha, existe caminho do começo até o poço, os inimigos nascem no chão e nenhuma sala especial aparece fora do seu trecho.
