# Etapas 133 a 136: converte peças do pacote "Free Mine Assets" (CC0, ver CREDITOS-ARTE.md) para o jogo.
# Uso: blender.exe --background --python Blender/pacote.py -- <mine_parts.blend> [foto de conferência .png]
# Cada peça escolhida sai em Assets/Modelos3D/Kit_<Nome>.fbx: no tamanho certo em metros, com a origem no pé (ou no
# topo, se for de pendurar), faces chapadas e os materiais com os nomes que o jogo usa. As texturas de madeira e de
# ferro do pacote saem reduzidas a 128 pixels em Assets/Modelos3D/Texturas.
import os
import sys

import bpy
from mathutils import Vector

AQUI = os.path.dirname(os.path.abspath(__file__))
ARGUMENTOS = sys.argv[sys.argv.index("--") + 1:]
PACOTE = ARGUMENTOS[0]
FOTO = ARGUMENTOS[1] if len(ARGUMENTOS) > 1 else None
MODELOS = os.path.join(AQUI, "..", "Assets", "Modelos3D")

# Material do pacote -> material do jogo. A rocha usa a rocha do jogo, para mudar de cor com o trecho.
MATERIAIS = {"wood": "KitMadeira", "metal": "KitFerro", "rock": "Rocha", "stalags": "Rocha", "light": "Chama", "cable": "Escuro"}
CORES = {"KitMadeira": (0.5, 0.33, 0.18), "KitFerro": (0.3, 0.3, 0.32), "Rocha": (0.55, 0.52, 0.48), "Chama": (1.0, 0.8, 0.4), "Escuro": (0.03, 0.03, 0.03)}

# (nome no jogo, objeto do pacote, medida, alvo em metros, origem)
# medida "x", "y" ou "z": a peça inteira é ampliada até esse lado ter o alvo.
# medida "xz": largura e altura esticadas separadamente (alvo = (largura, altura)); serve para as molduras de porta.
# medida "viga": alvo = (fator da grossura, altura).
PECAS = [
    ("Portal1", "Wooden_15", "xz", (2.9, 3.0), "base"),
    ("Escora", "Wooden_05", "z", 3.0, "base"),  # um poste só, com a travessa em cima: serve de escora caída
    ("Portal2", "Wooden_09", "xz", (2.9, 3.0), "base"),
    ("Portal3", "Wooden_14", "xz", (2.9, 3.0), "base"),
    ("Viga", "Wooden_17", "viga", (0.3, 3.0), "base"),
    ("Tabuas", "Wooden_33", "y", 2.2, "base"),
    ("Pedra1", "rock_1_low", "x", 0.8, "base"),
    ("Pedra2", "rock_3_low", "x", 0.55, "base"),
    ("Pedra3", "rock_5_low", "x", 1.0, "base"),
    ("Pedra4", "rock_7_low", "x", 0.4, "base"),
    ("Pedras1", "rocks_2_low", "x", 1.8, "base"),
    ("Pedras2", "rocks_3_low", "y", 1.5, "base"),
    ("Estalagmite1", "stalag_2_low", "z", 1.3, "base"),
    ("Estalagmite2", "stalag_5_low", "z", 1.6, "base"),
    ("Estalagmite3", "stalag_7_low", "z", 1.0, "base"),
    ("Estalagmites", "stalags_6_low", "z", 1.9, "base"),
    ("Carrinho", "minecart", "y", 1.6, "base"),
    ("Lampiao", "Lamp_01", "z", 0.9, "topo"),
    ("Arandela", "Lamp_03", "x", 0.4, "centro"),
]

bpy.ops.wm.open_mainfile(filepath=PACOTE)


def material(nome):
    mat = bpy.data.materials.get(nome)
    if mat is None:
        mat = bpy.data.materials.new(nome)
        cor = CORES.get(nome, (0.5, 0.5, 0.5))
        mat.diffuse_color = (cor[0], cor[1], cor[2], 1.0)
    return mat


def converter(nome, origem_nome, medida, alvo, origem):
    velho = bpy.data.objects[origem_nome]
    malha = velho.data.copy()
    malha.transform(velho.matrix_world)
    menor = Vector((min(v.co.x for v in malha.vertices), min(v.co.y for v in malha.vertices), min(v.co.z for v in malha.vertices)))
    maior = Vector((max(v.co.x for v in malha.vertices), max(v.co.y for v in malha.vertices), max(v.co.z for v in malha.vertices)))
    tamanho = maior - menor
    if medida == "xz":
        escala = Vector((alvo[0] / tamanho.x, alvo[0] / tamanho.x, alvo[1] / tamanho.z))
    elif medida == "viga":
        escala = Vector((alvo[0], alvo[0], alvo[1] / tamanho.z))
    else:
        fator = alvo / tamanho["xyz".index(medida)]
        escala = Vector((fator, fator, fator))
    meio = (menor + maior) / 2
    pe = menor.z if origem == "base" else maior.z if origem == "topo" else meio.z
    for v in malha.vertices:
        v.co = Vector(((v.co.x - meio.x) * escala.x, (v.co.y - meio.y) * escala.y, (v.co.z - pe) * escala.z))
    for face in malha.polygons:
        face.use_smooth = False
    for i, velho_material in enumerate(malha.materials):
        base = velho_material.name.split(".")[0] if velho_material else ""
        malha.materials[i] = material(MATERIAIS.get(base, "Escuro"))
    malha.update()
    novo = bpy.data.objects.new("Kit_" + nome, malha)
    bpy.context.scene.collection.objects.link(novo)
    for obj in bpy.context.view_layer.objects:
        obj.select_set(obj == novo)
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(MODELOS, "Kit_" + nome + ".fbx"), use_selection=True, object_types={"MESH"},
        apply_scale_options="FBX_SCALE_UNITS", axis_forward="Y", axis_up="Z", bake_space_transform=False,
        mesh_smooth_type="FACE", bake_anim=False)
    triangulos = sum(len(p.vertices) - 2 for p in malha.polygons)
    print("PECA Kit_%s: %d triangulos, %.2f x %.2f x %.2f m" % (nome, triangulos, tamanho.x * escala.x, tamanho.y * escala.y, tamanho.z * escala.z))
    return novo, tamanho.x * escala.x


os.makedirs(os.path.join(MODELOS, "Texturas"), exist_ok=True)
prontos = []
for peca in PECAS:
    prontos.append(converter(*peca))

# As duas texturas do pacote, reduzidas: o jogo usa pixels grandes.
pasta = os.path.dirname(PACOTE)
for arquivo, nome in (("wood1.png", "KitMadeira"), ("iron_diffuse.png", "KitFerro")):
    achado = None
    for raiz, _, arquivos in os.walk(pasta):
        if arquivo in arquivos:
            achado = os.path.join(raiz, arquivo)
    if achado is None:
        print("AVISO: nao achei " + arquivo)
        continue
    imagem = bpy.data.images.load(achado)
    imagem.scale(128, 128)
    imagem.filepath_raw = os.path.join(MODELOS, "Texturas", nome + ".png")
    imagem.file_format = "PNG"
    imagem.save()
    print("TEXTURA " + nome + ".png 128 x 128")

if FOTO:
    # Todas as peças em fila, para conferir de uma vez.
    x = 0.0
    for novo, largura in prontos:
        novo.location = (x + largura / 2, 0, 1.0 if novo.name in ("Kit_Lampiao", "Kit_Arandela") else 0)
        x += largura + 0.5
    cena = bpy.context.scene
    camera = bpy.data.objects.new("Camera", bpy.data.cameras.new("Camera"))
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = x * 1.05
    camera.location = Vector((x / 2, -30, 12))
    camera.rotation_euler = (Vector((x / 2, 0, 1.2)) - camera.location).to_track_quat("-Z", "Y").to_euler()
    cena.collection.objects.link(camera)
    cena.camera = camera
    sol = bpy.data.objects.new("Sol", bpy.data.lights.new("Sol", "SUN"))
    sol.data.energy = 4.0
    sol.rotation_euler = (0.9, 0.1, 0.6)
    cena.collection.objects.link(sol)
    mundo = bpy.data.worlds.new("Mundo")
    cena.world = mundo
    try:
        mundo.use_nodes = True
        mundo.node_tree.nodes["Background"].inputs[0].default_value = (0.3, 0.3, 0.33, 1.0)
    except Exception:
        pass
    for mat in bpy.data.materials:
        try:
            mat.use_nodes = True
            mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = mat.diffuse_color
        except Exception:
            pass
    for obj in bpy.data.objects:
        if obj.type == "MESH" and not obj.name.startswith("Kit_"):
            obj.hide_render = True
    cena.render.engine = "CYCLES"
    cena.cycles.samples = 16
    cena.cycles.device = "CPU"
    cena.render.resolution_x = 1800
    cena.render.resolution_y = 520
    cena.render.resolution_percentage = 100
    cena.render.filepath = FOTO
    bpy.ops.render.render(write_still=True)
print("FIM")
