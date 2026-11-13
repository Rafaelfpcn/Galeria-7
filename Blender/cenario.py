# Etapas 137 a 139: objetos de 1983, itens e veios, modelados por script.
# Uso: blender.exe --background --python Blender/cenario.py -- <pasta para as fotos de conferência>
# Cada objeto sai em Assets/Modelos3D/Obj_<Nome>.fbx, com a origem no pé (os veios, no meio).
import os
import sys

AQUI = os.path.dirname(os.path.abspath(__file__))
sys.path.append(AQUI)

import g7
from g7 import P, tubo, caixa, bola, vazio

MODELOS = os.path.join(AQUI, "..", "Assets", "Modelos3D")
FOTOS = sys.argv[sys.argv.index("--") + 1] if "--" in sys.argv else None

g7.CORES.update({
    "Kit": (0.9, 0.9, 0.85), "Frasco": (1.0, 0.6, 0.2), "Sinal": (1.0, 0.2, 0.1), "Fita": (0.9, 0.85, 0.6),
    "Papel": (0.85, 0.8, 0.65), "Manto": (0.5, 0.08, 0.08), "Rocha": (0.45, 0.42, 0.38), "Minerio": (0.2, 0.9, 0.4),
    "Latao": (0.75, 0.58, 0.2), "Chama": (1.0, 0.8, 0.4),
})


def cilindro(nome, x, frente, base, raio, altura, mat, pai, lados=8, raio_de_cima=None):
    """Um cilindro em pé: (x, frente) é o meio, 'base' é a altura do pé."""
    topo = raio if raio_de_cima is None else raio_de_cima
    return tubo(nome, [(P(x, frente, base), raio, raio), (P(x, frente, base + altura), topo, topo)], lados=lados, mat=mat, pai=pai)


def alca(nome, x, frente, cima, largura, altura, mat, pai, deitada=False):
    """Um arco fino, de arame: a alça de um balde, de uma marmita, de uma caixa."""
    r = 0.006
    if deitada:  # o arco atravessa de frente para trás
        pontos = [P(x, frente - largura / 2, cima), P(x, frente - largura / 2, cima + altura), P(x, frente + largura / 2, cima + altura), P(x, frente + largura / 2, cima)]
    else:
        pontos = [P(x - largura / 2, frente, cima), P(x - largura / 2, frente, cima + altura), P(x + largura / 2, frente, cima + altura), P(x + largura / 2, frente, cima)]
    return tubo(nome, [(p, r, r) for p in pontos], lados=3, mat=mat, pai=pai)


def marmita(r):
    for i in range(3):  # três vasilhas empilhadas
        cilindro("Vasilha", 0, 0, i * 0.055, 0.07, 0.05, "Metal", r)
    cilindro("Tampa", 0, 0, 0.165, 0.073, 0.012, "Metal", r)
    alca("Alca", 0, 0, 0.0, 0.16, 0.2, "Metal", r)


def garrafa_termica(r):
    cilindro("Corpo", 0, 0, 0, 0.045, 0.2, "Manto", r)
    cilindro("Faixa", 0, 0, 0.2, 0.047, 0.02, "Metal", r)
    cilindro("Copo", 0, 0, 0.22, 0.04, 0.07, "Metal", r, raio_de_cima=0.034)


def radio(r):
    caixa("Corpo", P(0, 0, 0.08), (0.26, 0.08, 0.16), mat="Escuro", pai=r)
    caixa("Grade", P(-0.05, 0.042, 0.08), (0.13, 0.006, 0.12), mat="Metal", pai=r)
    caixa("Mostrador", P(0.075, 0.042, 0.11), (0.08, 0.006, 0.04), mat="Papel", pai=r)
    cilindro("Botao", 0.075, 0.045, 0.04, 0.018, 0.02, "Metal", r, lados=6)
    tubo("Antena", [(P(0.11, -0.02, 0.16), 0.004, 0.004), (P(0.2, -0.02, 0.5), 0.003, 0.003)], lados=3, mat="Metal", pai=r)
    alca("Alca", 0, 0, 0.16, 0.2, 0.04, "Escuro", r)


def gravador(r):
    caixa("Corpo", P(0, 0, 0.035), (0.32, 0.22, 0.07), mat="Metal", pai=r)
    caixa("Janela", P(0, 0.02, 0.072), (0.16, 0.09, 0.006), mat="Escuro", pai=r)
    caixa("AltoFalante", P(0, -0.07, 0.072), (0.26, 0.05, 0.006), mat="Escuro", pai=r)
    for i in range(5):
        caixa("Tecla", P(-0.1 + i * 0.05, 0.09, 0.078), (0.04, 0.035, 0.018), mat="Fita", pai=r)
    alca("Alca", 0, -0.115, 0.02, 0.18, 0.03, "Escuro", r)


def caneca(r):
    cilindro("Corpo", 0, 0, 0, 0.04, 0.09, "Metal", r)
    tubo("Asa", [(P(0.04, 0, 0.07), 0.007, 0.007), (P(0.07, 0, 0.06), 0.007, 0.007), (P(0.07, 0, 0.03), 0.007, 0.007), (P(0.04, 0, 0.02), 0.007, 0.007)], lados=3, mat="Metal", pai=r)


def lata_de_oleo(r):
    cilindro("Corpo", 0, 0, 0, 0.09, 0.2, "Metal", r)
    cilindro("Ombro", 0, 0, 0.2, 0.09, 0.05, "Metal", r, raio_de_cima=0.03)
    tubo("Bico", [(P(0, 0, 0.24), 0.02, 0.02), (P(0, 0.12, 0.34), 0.008, 0.008)], lados=5, mat="Metal", pai=r)
    alca("Alca", 0, -0.05, 0.2, 0.09, 0.07, "Metal", r, deitada=True)


def lampiao_apagado(r):
    cilindro("Fundo", 0, 0, 0, 0.055, 0.04, "Metal", r)
    tubo("Vidro", [(P(0, 0, 0.04), 0.03, 0.03), (P(0, 0, 0.08), 0.043, 0.043), (P(0, 0, 0.13), 0.043, 0.043), (P(0, 0, 0.16), 0.028, 0.028)], lados=8, mat="Escuro", pai=r)
    cilindro("Tampa", 0, 0, 0.16, 0.048, 0.04, "Metal", r, raio_de_cima=0.012)
    alca("Alca", 0, 0, 0.16, 0.1, 0.1, "Metal", r)


def pa(r):
    tubo("Cabo", [(P(0, 0, 0.3), 0.018, 0.018), (P(0, 0, 1.3), 0.018, 0.018)], lados=5, mat="Madeira", pai=r)
    caixa("Punho", P(0, 0, 1.31), (0.14, 0.03, 0.03), mat="Madeira", pai=r)
    tubo("Lamina", [(P(0, 0, 0.0), 0.06, 0.006), (P(0, 0.01, 0.12), 0.11, 0.012), (P(0, 0, 0.32), 0.1, 0.012)], lados=6, mat="Metal", pai=r)


def alavanca(r):
    tubo("Barra", [(P(0, 0, 0.0), 0.004, 0.012), (P(0, 0, 0.12), 0.014, 0.014), (P(0, 0, 1.7), 0.014, 0.014), (P(0, 0.03, 1.8), 0.008, 0.014)], lados=5, mat="Metal", pai=r)


def balde(r):
    cilindro("Corpo", 0, 0, 0, 0.12, 0.26, "Metal", r, raio_de_cima=0.16)
    cilindro("Borda", 0, 0, 0.26, 0.165, 0.012, "Metal", r)
    alca("Alca", 0, 0, 0.26, 0.32, 0.16, "Metal", r)


def caixa_de_ferramentas(r):
    caixa("Corpo", P(0, 0, 0.09), (0.45, 0.2, 0.18), mat="Metal", pai=r)
    caixa("Tampa", P(0, 0, 0.19), (0.46, 0.21, 0.03), mat="Manto", pai=r)
    caixa("Fecho", P(0, 0.105, 0.15), (0.05, 0.01, 0.05), mat="Latao", pai=r)
    alca("Alca", 0, 0, 0.205, 0.16, 0.04, "Metal", r)


def bota(r):
    caixa("Pe", P(0, 0.04, 0.05), (0.11, 0.28, 0.1), mat="Escuro", pai=r)
    tubo("Cano", [(P(0, -0.05, 0.08), 0.055, 0.06), (P(0, -0.06, 0.3), 0.06, 0.065)], lados=6, mat="Escuro", pai=r)
    caixa("Sola", P(0, 0.04, 0.01), (0.12, 0.3, 0.02), mat="Madeira", pai=r)


def banquinho(r):
    cilindro("Assento", 0, 0, 0.4, 0.18, 0.04, "Madeira", r)
    for x, frente in ((0.12, 0.1), (-0.12, 0.1), (0, -0.14)):
        tubo("Perna", [(P(x * 1.4, frente * 1.4, 0), 0.02, 0.02), (P(x, frente, 0.4), 0.025, 0.025)], lados=4, mat="Madeira", pai=r)


def picareta(r):
    tubo("Cabo", [(P(0, 0, 0), 0.021, 0.023), (P(0, 0, 0.45), 0.017, 0.019), (P(0, 0, 0.92), 0.021, 0.023)], lados=6, mat="Madeira", pai=r)
    tubo("Ferro", [(P(-0.31, 0, 0.78), 0.008, 0.005), (P(-0.16, 0, 0.858), 0.022, 0.017), (P(0, 0, 0.88), 0.036, 0.03), (P(0.16, 0, 0.858), 0.022, 0.017), (P(0.31, 0, 0.78), 0.008, 0.005)], lados=4, mat="Metal", pai=r)


def barril(r):
    tubo("Corpo", [(P(0, 0, 0), 0.26, 0.26), (P(0, 0, 0.25), 0.31, 0.31), (P(0, 0, 0.65), 0.31, 0.31), (P(0, 0, 0.9), 0.26, 0.26)], lados=10, mat="Madeira", pai=r)
    for cima in (0.12, 0.45, 0.78):
        raio = 0.3 if cima != 0.45 else 0.318
        cilindro("Aro", 0, 0, cima - 0.02, raio, 0.04, "Metal", r, lados=10)


def kit_medico(r):
    caixa("Caixa", P(0, 0, 0.11), (0.36, 0.22, 0.22), mat="Kit", pai=r)
    for largura, altura in ((0.14, 0.045), (0.045, 0.14)):  # a cruz vermelha, na frente e na tampa
        caixa("Cruz", P(0, 0.112, 0.11), (largura, 0.008, altura), mat="Sinal", pai=r)
        caixa("Cruz", P(0, 0, 0.222), (largura, altura, 0.008), mat="Sinal", pai=r)
    alca("Alca", 0, -0.06, 0.22, 0.12, 0.035, "Metal", r)


def frasco_de_oleo(r):
    tubo("Corpo", [(P(0, 0, 0), 0.06, 0.06), (P(0, 0, 0.03), 0.075, 0.075), (P(0, 0, 0.17), 0.075, 0.075), (P(0, 0, 0.22), 0.03, 0.03), (P(0, 0, 0.28), 0.028, 0.028)], lados=8, mat="Frasco", pai=r)
    cilindro("Rolha", 0, 0, 0.28, 0.03, 0.035, "Madeira", r, lados=6)
    caixa("Rotulo", P(0, 0.074, 0.1), (0.08, 0.008, 0.07), mat="Papel", pai=r)


def sinalizador(r):
    cilindro("Tubo", 0, 0, 0, 0.026, 0.28, "Sinal", r, lados=6)
    cilindro("Tampa", 0, 0, 0.28, 0.03, 0.05, "Escuro", r, lados=6)
    cilindro("Pe", 0, 0, 0, 0.03, 0.02, "Escuro", r, lados=6)


def fita_cassete(r):
    caixa("Caixa", P(0, 0, 0.02), (0.26, 0.17, 0.04), mat="Fita", pai=r)
    caixa("Etiqueta", P(0, 0.035, 0.041), (0.2, 0.06, 0.004), mat="Papel", pai=r)
    for x in (-0.06, 0.06):
        cilindro("Rolo", x, -0.015, 0.04, 0.028, 0.006, "Escuro", r)
    caixa("Janela", P(0, -0.015, 0.041), (0.06, 0.035, 0.005), mat="Escuro", pai=r)


def veio(forma):
    """A massa de rocha com pepitas e cristais de minério. Três formatos: 1 redondo, 2 deitado e comprido, 3 alto e estreito."""
    def fazer(r):
        if forma == 1:
            aneis = [(P(0, 0, -0.6), 0.2, 0.2), (P(0.05, 0, -0.3), 0.55, 0.5), (P(0, 0, 0.1), 0.65, 0.55), (P(-0.05, 0, 0.45), 0.45, 0.4), (P(0, 0, 0.65), 0.12, 0.12)]
            brotos = [(0.3, 0.45, 0.2), (-0.35, 0.4, -0.1), (0.1, 0.5, -0.35), (-0.1, 0.42, 0.45), (0.45, 0.3, -0.25), (-0.5, 0.25, 0.25)]
        elif forma == 2:
            aneis = [(P(-0.95, 0, -0.1), 0.12, 0.15), (P(-0.5, 0, 0.0), 0.4, 0.42), (P(0, 0, -0.05), 0.5, 0.45), (P(0.5, 0, 0.05), 0.38, 0.4), (P(0.95, 0, 0.1), 0.1, 0.12)]
            brotos = [(-0.6, 0.38, 0.1), (-0.25, 0.42, -0.2), (0.15, 0.45, 0.15), (0.5, 0.36, -0.05), (0.75, 0.25, 0.15), (-0.8, 0.2, -0.05)]
        else:
            aneis = [(P(0.05, 0, -0.95), 0.12, 0.12), (P(0, 0, -0.5), 0.36, 0.38), (P(-0.05, 0, 0.0), 0.42, 0.4), (P(0.05, 0, 0.5), 0.34, 0.36), (P(0, 0, 0.95), 0.1, 0.1)]
            brotos = [(0.1, 0.36, -0.6), (-0.15, 0.4, -0.25), (0.2, 0.4, 0.1), (-0.1, 0.36, 0.45), (0.1, 0.25, 0.75), (-0.2, 0.3, -0.8)]
        tubo("Rocha", aneis, lados=7, mat="Rocha", origem=P(0, 0, 0), pai=r, torto=0.05, semente=forma)
        for i, (x, frente, cima) in enumerate(brotos):
            if i % 2 == 0:  # cristal: uma ponta saindo para a frente
                ponta = P(x * 1.25, frente + 0.3, cima * 1.2 + 0.08)
                tubo("Cristal", [(P(x, frente - 0.12, cima), 0.09, 0.09), (P(x, frente, cima) + (ponta - P(x, frente, cima)) * 0.5, 0.07, 0.07), (ponta, 0.006, 0.006)], lados=5, mat="Minerio", pai=r)
            else:  # pepita: um caroço encravado
                bola("Pepita", P(x, frente - 0.02, cima), (0.13, 0.09, 0.12), mat="Minerio", pai=r)
    return fazer


OBJETOS = [
    ("Marmita", marmita), ("GarrafaTermica", garrafa_termica), ("Radio", radio), ("Gravador", gravador), ("Caneca", caneca),
    ("LataDeOleo", lata_de_oleo), ("LampiaoApagado", lampiao_apagado), ("Pa", pa), ("Alavanca", alavanca), ("Balde", balde),
    ("CaixaDeFerramentas", caixa_de_ferramentas), ("Bota", bota), ("Banquinho", banquinho), ("Picareta", picareta), ("Barril", barril),
    ("KitMedico", kit_medico), ("FrascoDeOleo", frasco_de_oleo), ("Sinalizador", sinalizador), ("FitaCassete", fita_cassete),
    ("Veio1", veio(1)), ("Veio2", veio(2)), ("Veio3", veio(3)),
]

os.makedirs(MODELOS, exist_ok=True)
for nome, fazer in OBJETOS:
    g7.limpar()
    raiz = vazio("Raiz")
    fazer(raiz)
    print("MODELO Obj_%s: %d triangulos" % (nome, g7.triangulos()))
    g7.exportar(os.path.join(MODELOS, "Obj_" + nome + ".fbx"))
    if FOTOS and nome in ("Radio", "Gravador", "KitMedico", "Veio1", "Barril", "Marmita"):
        g7.foto(os.path.join(FOTOS, "Obj_" + nome + ".png"), 0.4 if not nome.startswith("Veio") and nome != "Barril" else 1.2, centro=0.1 if nome != "Barril" else 0.45)
print("FIM")
