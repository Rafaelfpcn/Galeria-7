# Etapas 125, 129 e 131: as mãos do jogador (com o lampião e a picareta), o Coração do Veio e o tentáculo dele.
# Uso: blender.exe --background --python Blender/objetos.py -- <pasta para as fotos de conferência>
# Grava os modelos em Assets/Modelos3D. A Unity reimporta sozinha na próxima vez que abrir o projeto.
import os
import sys

AQUI = os.path.dirname(os.path.abspath(__file__))
sys.path.append(AQUI)

import g7
from g7 import P, tubo, caixa, bola, vazio

MODELOS = os.path.join(AQUI, "..", "Assets", "Modelos3D")
FOTOS = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else None


def lampiao_de_mao():
    """A mão esquerda segurando o lampião pela alça. A origem é o meio do vidro: é de lá que a luz sai."""
    g7.limpar()
    raiz = vazio("Raiz")
    tubo("Fundo", [(P(0, 0, -0.105), 0.03, 0.03), (P(0, 0, -0.10), 0.052, 0.052), (P(0, 0, -0.07), 0.052, 0.052), (P(0, 0, -0.055), 0.034, 0.034)], lados=8, mat="Metal", pai=raiz)
    tubo("Vidro", [(P(0, 0, -0.055), 0.03, 0.03), (P(0, 0, -0.02), 0.043, 0.043), (P(0, 0, 0.03), 0.043, 0.043), (P(0, 0, 0.06), 0.028, 0.028)], lados=8, mat="Chama", pai=raiz)
    tubo("Tampa", [(P(0, 0, 0.058), 0.048, 0.048), (P(0, 0, 0.075), 0.032, 0.032), (P(0, 0, 0.10), 0.012, 0.012)], lados=8, mat="Metal", pai=raiz)
    for x, frente in ((0.047, 0), (-0.047, 0), (0, 0.047), (0, -0.047)):
        tubo("Grade", [(P(x, frente, -0.06), 0.004, 0.004), (P(x * 1.05, frente * 1.05, 0.0), 0.004, 0.004), (P(x, frente, 0.062), 0.004, 0.004)], lados=3, mat="Metal", pai=raiz)
    tubo("Alca", [
        (P(-0.05, 0, 0.06), 0.004, 0.004), (P(-0.055, 0, 0.12), 0.004, 0.004), (P(-0.03, 0, 0.165), 0.004, 0.004),
        (P(0.03, 0, 0.165), 0.004, 0.004), (P(0.055, 0, 0.12), 0.004, 0.004), (P(0.05, 0, 0.06), 0.004, 0.004),
    ], lados=3, mat="Metal", pai=raiz)

    # O punho fechado em volta da alça, com os dedos para a frente, e a manga que vem de baixo, do canto da tela.
    caixa("Mao", P(0, -0.012, 0.178), (0.085, 0.07, 0.05), mat="Pele", pai=raiz)
    caixa("Dedos", P(0, 0.022, 0.158), (0.08, 0.038, 0.034), mat="Pele", pai=raiz)
    tubo("Polegar", [(P(0.045, -0.02, 0.175), 0.014, 0.014), (P(0.05, 0.02, 0.15), 0.01, 0.01)], lados=4, mat="Pele", pai=raiz)
    # A manga vai para a esquerda e para baixo, para fora da tela, sem passar na frente do vidro.
    tubo("Manga", [(P(-0.005, -0.045, 0.18), 0.036, 0.034), (P(-0.04, -0.08, 0.17), 0.044, 0.04), (P(-0.30, -0.30, -0.07), 0.055, 0.05)], lados=6, mat="Roupa", pai=raiz)
    return 0.3


def picareta_de_mao():
    """A mão direita segurando a picareta do avô. A origem é a base do cabo: o golpe gira em torno dela."""
    g7.limpar()
    raiz = vazio("Raiz")
    tubo("Cabo", [(P(0, 0, -0.04), 0.021, 0.023), (P(0, 0, 0.30), 0.017, 0.019), (P(0, 0, 0.64), 0.021, 0.023)], lados=6, mat="Madeira", pai=raiz)
    tubo("Ferro", [
        (P(0, -0.31, 0.50), 0.005, 0.008), (P(0, -0.16, 0.578), 0.017, 0.022), (P(0, 0, 0.60), 0.03, 0.036),
        (P(0, 0.16, 0.578), 0.017, 0.022), (P(0, 0.31, 0.50), 0.005, 0.008),
    ], lados=4, mat="Metal", pai=raiz)
    caixa("Anel", P(0, 0, 0.60), (0.062, 0.085, 0.075), mat="Metal", pai=raiz)

    caixa("Mao", P(0, -0.005, 0.14), (0.078, 0.085, 0.095), mat="Pele", pai=raiz)
    for n in range(4):
        caixa("Dedo", P(0, 0.042, 0.105 + n * 0.024), (0.07, 0.022, 0.02), mat="Pele", pai=raiz)
    tubo("Polegar", [(P(-0.04, -0.01, 0.15), 0.015, 0.015), (P(-0.035, 0.035, 0.185), 0.01, 0.01)], lados=4, mat="Pele", pai=raiz)
    tubo("Manga", [(P(0.0, -0.045, 0.135), 0.036, 0.04), (P(0.01, -0.10, 0.11), 0.046, 0.048), (P(0.16, -0.50, -0.26), 0.06, 0.06)], lados=6, mat="Roupa", pai=raiz)
    return 0.7


def coracao():
    """O Coração do Veio: uma massa de carne presa na parede norte da arena. A Massa pulsa (com os lobos e as
    artérias, que são filhos dela); as veias correm pela parede e ficam paradas. Quem olha vê o lado de frente negativa."""
    g7.limpar()
    raiz = vazio("Raiz")
    massa = tubo("Massa", [
        (P(0.1, 0, -2.5), 0.3, 0.2), (P(0.25, 0, -1.9), 1.6, 0.85), (P(0.1, 0, -1.0), 2.6, 1.2), (P(-0.1, 0, 0.0), 3.0, 1.4),
        (P(-0.2, 0, 1.0), 2.7, 1.35), (P(0.0, 0, 1.9), 1.8, 1.0), (P(0.2, 0, 2.5), 0.5, 0.3),
    ], lados=12, mat="Coracao", origem=P(0, 0, 0), pai=raiz, torto=0.12, semente=7)
    tubo("Lobo_1", [
        (P(-2.6, -0.25, -2.1), 0.3, 0.2), (P(-2.9, -0.3, -1.4), 1.3, 0.85), (P(-2.8, -0.3, -0.6), 1.6, 1.0),
        (P(-2.6, -0.3, 0.2), 1.1, 0.8), (P(-2.5, -0.25, 0.7), 0.2, 0.2),
    ], lados=9, mat="Coracao", pai=massa, torto=0.08, semente=3)
    tubo("Lobo_2", [
        (P(2.5, -0.2, -0.9), 0.25, 0.2), (P(2.7, -0.25, -0.2), 1.2, 0.85), (P(2.6, -0.25, 0.7), 1.5, 0.95),
        (P(2.4, -0.2, 1.5), 1.0, 0.75), (P(2.2, -0.2, 2.0), 0.2, 0.2),
    ], lados=9, mat="Coracao", pai=massa, torto=0.08, semente=5)
    # Caroços na frente, para a superfície não ficar lisa.
    for x, cima, raio in ((-1.4, 0.9, 0.55), (0.9, 1.4, 0.45), (1.6, -0.2, 0.6), (-0.3, -0.1, 0.5), (-1.9, -1.2, 0.4), (0.4, 2.0, 0.35)):
        bola("Caroco", P(x, -1.15, cima), (raio, raio * 0.6, raio), mat="Coracao", pai=massa)
    # Artérias grossas que sobem para o teto (o teto fica 4,4 m acima do meio do coração).
    tubo("Arteria", [(P(0.9, 0.2, 1.7), 0.38, 0.36), (P(1.5, 0.4, 3.1), 0.3, 0.3), (P(1.3, 0.5, 4.5), 0.26, 0.26)], lados=6, mat="Coracao", pai=massa)
    tubo("Arteria", [(P(-1.1, 0.2, 1.7), 0.33, 0.32), (P(-1.9, 0.4, 2.9), 0.26, 0.26), (P(-1.6, 0.5, 4.5), 0.22, 0.22)], lados=6, mat="Coracao", pai=massa)

    # As veias: coladas na parede (0,66 m atrás do meio do coração), saindo dele para todos os lados.
    fundo = 0.6
    caminhos = [
        [(-2.8, 0.6), (-5.0, 1.4), (-7.5, 1.0), (-10.5, 2.2)],
        [(-2.9, -0.4), (-5.5, -0.9), (-8.0, -0.3), (-11.0, -1.4)],
        [(-2.4, -1.6), (-4.2, -2.2), (-7.0, -2.0), (-9.5, -2.55)],
        [(-1.5, 2.0), (-3.2, 3.0), (-5.2, 3.2), (-7.4, 4.2)],
        [(2.8, 1.2), (5.2, 0.8), (7.6, 1.8), (10.8, 1.3)],
        [(2.9, -0.3), (5.0, -1.2), (8.2, -0.9), (11.0, -2.0)],
        [(2.0, -1.8), (4.0, -2.4), (6.5, -2.2), (8.5, -2.55)],
        [(1.8, 2.2), (3.6, 3.3), (6.0, 3.4), (8.0, 4.3)],
        [(0.3, -2.3), (0.9, -2.55), (2.6, -2.55), (4.4, -2.58)],
    ]
    for n, caminho in enumerate(caminhos):
        aneis = []
        for i, (x, cima) in enumerate(caminho):
            raio = 0.2 * (1.0 - i / len(caminho)) + 0.03
            aneis.append((P(x, fundo, cima), raio, raio * 0.7))
        tubo("Veia", aneis, lados=5, mat="Coracao", pai=raiz, torto=0.02, semente=n)
    return 5.0


def tentaculo():
    """O tentáculo do chefão: quatro gomos, cada um filho do anterior, para ele se dobrar ao sair do chão. 3 m."""
    g7.limpar()
    raiz = vazio("Raiz")
    gomos = [
        (0.0, [(P(0, 0, -0.35), 0.44, 0.44), (P(0, 0, 0.0), 0.38, 0.38), (P(0.02, 0, 0.8), 0.30, 0.30)]),
        (0.75, [(P(0.02, 0, 0.70), 0.30, 0.30), (P(0.0, 0.02, 1.55), 0.215, 0.215)]),
        (1.5, [(P(0.0, 0.02, 1.45), 0.215, 0.215), (P(-0.02, 0.0, 2.30), 0.13, 0.13)]),
        (2.25, [(P(-0.02, 0, 2.20), 0.13, 0.13), (P(0, 0.06, 2.70), 0.07, 0.07), (P(0, 0.2, 3.0), 0.008, 0.008)]),
    ]
    pai = raiz
    for n, (junta, aneis) in enumerate(gomos, start=1):
        pai = tubo("Gomo_%d" % n, aneis, lados=7, mat="Tentaculo", origem=P(0, 0, junta), pai=pai, torto=0.012, semente=n)
        # Espinhos escuros, dois por gomo, em lados alternados.
        for k in range(2):
            cima = junta + 0.25 + k * 0.3
            raio = 0.36 - cima * 0.1
            lado = 1 if (n + k) % 2 == 0 else -1
            tubo("Espinho", [(P(lado * raio * 0.8, 0, cima), 0.05, 0.05), (P(lado * (raio + 0.22), 0, cima + 0.12), 0.005, 0.005)], lados=4, mat="Escuro", pai=pai)
    return 3.0


os.makedirs(MODELOS, exist_ok=True)
for nome, fazer, centro in (("LampiaoDeMao", lampiao_de_mao, 0.0), ("PicaretaDeMao", picareta_de_mao, 0.3), ("Coracao", coracao, 0.5), ("Tentaculo", tentaculo, None)):
    altura = fazer()
    print("MODELO %s: %d triangulos" % (nome, g7.triangulos()))
    g7.exportar(os.path.join(MODELOS, nome + ".fbx"))
    if FOTOS:
        g7.foto(os.path.join(FOTOS, nome + ".png"), altura, centro=centro)
print("FIM")
