# Etapas 126 a 128: os três inimigos da Galeria 7, modelados por script.
# Uso: blender.exe --background --python Blender/personagens.py -- <pasta para as fotos de conferência>
# Grava os modelos em Assets/Modelos3D. A Unity reimporta sozinha na próxima vez que abrir o projeto.
import os
import sys

AQUI = os.path.dirname(os.path.abspath(__file__))
sys.path.append(AQUI)

import g7
from g7 import P, tubo, caixa, bola, vazio

MODELOS = os.path.join(AQUI, "..", "Assets", "Modelos3D")
FOTOS = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else None


def rastejante():
    """Magro, de quatro patas compridas com o joelho acima das costas, como uma aranha. Olhos vermelhos."""
    g7.limpar()
    raiz = vazio("Raiz")
    tronco = tubo("Tronco", [
        (P(0, -0.62, 0.30), 0.015, 0.015),  # ponta do rabo
        (P(0, -0.40, 0.36), 0.06, 0.05),
        (P(0, -0.20, 0.42), 0.13, 0.11),    # quadril
        (P(0, 0.00, 0.40), 0.085, 0.075),   # cintura fina
        (P(0, 0.18, 0.45), 0.17, 0.15),     # costelas
        (P(0, 0.32, 0.43), 0.12, 0.11),
        (P(0, 0.42, 0.40), 0.07, 0.07),     # pescoço
    ], lados=8, mat="Rastejante", origem=P(0, 0, 0.40), pai=raiz, torto=0.008)
    for frente, base, altura in [(-0.28, 0.49, 0.10), (-0.06, 0.45, 0.12), (0.14, 0.57, 0.16), (0.30, 0.52, 0.11)]:
        tubo("Espinho", [(P(0, frente, base - 0.03), 0.03, 0.03), (P(0, frente - 0.04, base + altura), 0.004, 0.004)], lados=4, mat="Escuro", pai=tronco)

    cabeca = tubo("Cabeca", [
        (P(0, 0.42, 0.40), 0.07, 0.07),
        (P(0, 0.52, 0.43), 0.11, 0.09),
        (P(0, 0.66, 0.41), 0.09, 0.065),
        (P(0, 0.80, 0.385), 0.045, 0.035),  # focinho
    ], lados=8, mat="Rastejante", pai=raiz, torto=0.004)
    for x in (-0.075, 0.075):
        bola("Olho", P(x, 0.60, 0.475), (0.03, 0.03, 0.03), mat="Olhos", pai=cabeca)
    for x in (-0.03, 0.03):
        tubo("Presa", [(P(x, 0.77, 0.365), 0.012, 0.012), (P(x, 0.78, 0.30), 0.004, 0.004)], lados=4, mat="Pele", pai=cabeca)
    tubo("Queixo", [
        (P(0, 0.50, 0.345), 0.075, 0.03),
        (P(0, 0.64, 0.33), 0.065, 0.025),
        (P(0, 0.77, 0.335), 0.03, 0.015),
    ], lados=6, mat="Rastejante", pai=cabeca)

    # Na ordem em que alternam ao andar: 1 frente esquerda, 2 frente direita, 3 trás direita, 4 trás esquerda.
    for n, (lado, frente) in enumerate([(-1, 0.24), (1, 0.24), (1, -0.22), (-1, -0.22)], start=1):
        quadril = P(lado * 0.14, frente, 0.42)
        joelho = P(lado * 0.42, frente + 0.03, 0.57)
        pe = P(lado * 0.54, frente + 0.08, 0.0)
        perna = tubo("Perna_%d" % n, [(quadril, 0.05, 0.05), (joelho, 0.035, 0.035)], lados=5, mat="Rastejante", pai=raiz)
        meio = joelho.lerp(pe, 0.5) + P(lado * 0.04, 0, 0)
        tubo("Canela_%d" % n, [(joelho, 0.035, 0.035), (meio, 0.026, 0.026), (pe, 0.006, 0.006)], lados=5, mat="Rastejante", pai=perna)
    return 0.6


def ouvinte():
    """Alto, curvado, sem olhos, com orelhas enormes e braços que chegam aos joelhos."""
    g7.limpar()
    raiz = vazio("Raiz")
    tubo("Tronco", [
        (P(0, -0.02, 0.76), 0.10, 0.08),
        (P(0, 0.00, 0.84), 0.13, 0.10),   # quadril
        (P(0, 0.02, 0.98), 0.095, 0.075), # cintura
        (P(0, 0.07, 1.16), 0.14, 0.095),  # costelas
        (P(0, 0.15, 1.33), 0.17, 0.085),  # ombros
        (P(0, 0.23, 1.42), 0.06, 0.06),   # pescoço
    ], lados=8, mat="Ouvinte", origem=P(0, 0, 0.84), pai=raiz, torto=0.006)

    cabeca = tubo("Cabeca", [
        (P(0, 0.23, 1.42), 0.055, 0.055),
        (P(0, 0.28, 1.52), 0.10, 0.11),
        (P(0, 0.31, 1.64), 0.105, 0.12),
        (P(0, 0.29, 1.75), 0.07, 0.08),
        (P(0, 0.27, 1.80), 0.02, 0.02),
    ], lados=8, mat="Ouvinte", pai=raiz, torto=0.004)
    bola("Boca", P(0, 0.405, 1.535), (0.045, 0.025, 0.07), mat="Escuro", pai=cabeca)
    for lado in (-1, 1):
        tubo("Orelha", [
            (P(lado * 0.09, 0.27, 1.62), 0.02, 0.06),
            (P(lado * 0.20, 0.24, 1.72), 0.014, 0.13),
            (P(lado * 0.31, 0.20, 1.86), 0.006, 0.03),
        ], lados=4, mat="Ouvinte", pai=cabeca)

    for n, lado in enumerate((-1, 1), start=1):
        ombro = P(lado * 0.20, 0.17, 1.30)
        cotovelo = P(lado * 0.25, 0.12, 0.98)
        pulso = P(lado * 0.26, 0.24, 0.62)
        braco = tubo("Braco_%d" % n, [(ombro, 0.045, 0.045), (cotovelo, 0.035, 0.035)], lados=5, mat="Ouvinte", pai=raiz)
        antebraco = tubo("Antebraco_%d" % n, [(cotovelo, 0.035, 0.035), (pulso, 0.026, 0.026)], lados=5, mat="Ouvinte", pai=braco)
        for dedo in (-1, 0, 1):
            ponta = pulso + P(dedo * 0.035, 0.06 + 0.02 * abs(dedo), -0.22)
            tubo("Dedo", [(pulso, 0.014, 0.014), (ponta, 0.004, 0.004)], lados=4, mat="Ouvinte", pai=antebraco)

    for n, lado in enumerate((-1, 1), start=1):
        quadril = P(lado * 0.08, 0.0, 0.80)
        joelho = P(lado * 0.10, 0.09, 0.42)
        tornozelo = P(lado * 0.10, -0.02, 0.05)
        perna = tubo("Perna_%d" % n, [(quadril, 0.06, 0.06), (joelho, 0.045, 0.045)], lados=5, mat="Ouvinte", pai=raiz)
        canela = tubo("Canela_%d" % n, [(joelho, 0.045, 0.045), (tornozelo, 0.03, 0.03)], lados=5, mat="Ouvinte", pai=perna)
        caixa("Pe", P(lado * 0.10, 0.06, 0.025), (0.08, 0.22, 0.05), mat="Ouvinte", pai=canela)
    return 1.85


def mineiro_oco():
    """Um mineiro de 1983: macacão, botas, capacete com a lanterna apagada, picareta. No lugar do rosto, um buraco."""
    g7.limpar()
    raiz = vazio("Raiz")
    tronco = tubo("Tronco", [
        (P(0, 0.0, 0.90), 0.13, 0.10),
        (P(0, 0.0, 0.98), 0.17, 0.11),   # quadril
        (P(0, 0.0, 1.12), 0.155, 0.10),  # cintura
        (P(0, 0.01, 1.36), 0.21, 0.13),  # peito
        (P(0, 0.02, 1.52), 0.22, 0.12),  # ombros
        (P(0, 0.03, 1.58), 0.07, 0.07),  # gola
    ], lados=8, mat="Roupa", origem=P(0, 0, 0.98), pai=raiz)
    caixa("Cinto", P(0, 0.0, 1.06), (0.34, 0.23, 0.05), mat="Escuro", pai=tronco)

    cabeca = tubo("Cabeca", [
        (P(0, 0.03, 1.58), 0.055, 0.055),
        (P(0, 0.04, 1.66), 0.10, 0.11),
        (P(0, 0.04, 1.76), 0.105, 0.115),
        (P(0, 0.03, 1.84), 0.08, 0.09),
    ], lados=8, mat="Pele", pai=raiz)
    bola("RostoOco", P(0, 0.135, 1.715), (0.07, 0.03, 0.085), mat="Escuro", pai=cabeca)
    capacete = tubo("Capacete", [
        (P(0, 0.05, 1.795), 0.165, 0.185),  # aba
        (P(0, 0.05, 1.815), 0.125, 0.14),
        (P(0, 0.04, 1.90), 0.10, 0.115),
        (P(0, 0.04, 1.945), 0.03, 0.03),
    ], lados=8, mat="Metal", pai=cabeca)
    caixa("Lanterna", P(0, 0.18, 1.865), (0.07, 0.05, 0.07), mat="Escuro", pai=capacete)

    mao_direita = None
    for n, lado in enumerate((-1, 1), start=1):
        ombro = P(lado * 0.26, 0.02, 1.50)
        cotovelo = P(lado * 0.30, 0.02, 1.18)
        pulso = P(lado * 0.30, 0.10, 0.90)
        braco = tubo("Braco_%d" % n, [(ombro, 0.065, 0.065), (cotovelo, 0.05, 0.05)], lados=6, mat="Roupa", pai=raiz)
        antebraco = tubo("Antebraco_%d" % n, [(cotovelo, 0.05, 0.05), (pulso, 0.04, 0.04)], lados=6, mat="Roupa", pai=braco)
        caixa("Mao", pulso + P(0, 0.02, -0.05), (0.07, 0.09, 0.10), mat="Pele", pai=antebraco)
        if lado == 1:
            mao_direita = (antebraco, pulso + P(0, 0.02, -0.06))
    # A picareta dele, na mão direita: cabo apontando para a frente e para baixo, ferro atravessado na ponta.
    antebraco, mao = mao_direita
    ponta = mao + P(0, 0.62, -0.18)
    tubo("Cabo", [(mao + P(0, -0.12, 0.04), 0.02, 0.02), (ponta, 0.02, 0.02)], lados=5, mat="Madeira", pai=antebraco)
    tubo("Ferro", [(ponta + P(0, 0.02, 0.26), 0.008, 0.012), (ponta + P(0, 0.05, 0.0), 0.03, 0.035), (ponta + P(0, 0.02, -0.26), 0.008, 0.012)], lados=4, mat="Metal", pai=antebraco)

    for n, lado in enumerate((-1, 1), start=1):
        quadril = P(lado * 0.10, 0.0, 0.95)
        joelho = P(lado * 0.11, 0.03, 0.50)
        tornozelo = P(lado * 0.11, 0.0, 0.12)
        perna = tubo("Perna_%d" % n, [(quadril, 0.085, 0.085), (joelho, 0.07, 0.07)], lados=6, mat="Roupa", pai=raiz)
        canela = tubo("Canela_%d" % n, [(joelho, 0.07, 0.07), (tornozelo, 0.06, 0.06)], lados=6, mat="Roupa", pai=perna)
        caixa("Bota", P(lado * 0.11, 0.05, 0.06), (0.13, 0.28, 0.12), mat="Escuro", pai=canela)
    return 1.95


os.makedirs(MODELOS, exist_ok=True)
for nome, fazer in (("Rastejante", rastejante), ("Ouvinte", ouvinte), ("MineiroOco", mineiro_oco)):
    altura = fazer()
    print("MODELO %s: %d triangulos" % (nome, g7.triangulos()))
    g7.exportar(os.path.join(MODELOS, nome + ".fbx"))
    if FOTOS:
        g7.foto(os.path.join(FOTOS, nome + ".png"), altura)
print("FIM")
