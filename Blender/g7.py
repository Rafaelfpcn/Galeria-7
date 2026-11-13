# Biblioteca de modelagem da Galeria 7 para o Blender.
# Os modelos são feitos de peças soltas (tronco, cabeça, cada perna...), sem esqueleto: cada peça gira em torno
# da própria origem, e na Unity o script AnimacaoDeInimigo gira as peças. É como os jogos de PS1 faziam.
#
# Regras (ver ARTE.md): 1 unidade = 1 metro, o personagem olha para +Y (que vira o +Z, a frente, na Unity), Z é para cima,
# faces chapadas, e nenhum objeto é girado: toda forma já nasce na posição final. Isso garante que o modelo
# chegue à Unity com tamanho e eixos certos.
import math
import random

import bpy
import bmesh
from mathutils import Vector

METROS_POR_TEXTURA = 2.0  # a textura se repete a cada 2 m, como nas paredes do jogo

# Cor de cada material na foto de conferência. Na Unity, o material de mesmo nome substitui este.
CORES = {
    "Rastejante": (0.23, 0.26, 0.2),
    "Ouvinte": (0.75, 0.72, 0.65),
    "Roupa": (0.2, 0.22, 0.3),
    "Pele": (0.5, 0.46, 0.42),
    "Escuro": (0.02, 0.02, 0.02),
    "Olhos": (1.0, 0.1, 0.05),
    "Metal": (0.25, 0.25, 0.27),
    "Madeira": (0.45, 0.3, 0.18),
    "Coracao": (0.25, 0.03, 0.05),
    "Cristal": (0.3, 1.0, 0.5),
}

_origens = {}  # nome do objeto -> onde fica a origem dele no mundo


def limpar():
    """Começa um arquivo vazio."""
    bpy.ops.wm.read_factory_settings(use_empty=True)
    _origens.clear()


def P(x, frente, cima):
    """Um ponto dito do jeito do jogo: x para o lado, 'frente' para onde o personagem olha, 'cima' é a altura."""
    return Vector((x, frente, cima))


def material(nome):
    mat = bpy.data.materials.get(nome)
    if mat is None:
        mat = bpy.data.materials.new(nome)
        cor = CORES.get(nome, (0.5, 0.5, 0.5))
        mat.diffuse_color = (cor[0], cor[1], cor[2], 1.0)
    return mat


def _objeto(nome, bm, mat, origem, pai):
    for v in bm.verts:
        v.co -= origem
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    malha = bpy.data.meshes.new(nome)
    bm.to_mesh(malha)
    bm.free()
    malha.materials.append(material(mat))
    obj = bpy.data.objects.new(nome, malha)
    bpy.context.scene.collection.objects.link(obj)
    _prender(obj, origem, pai)
    return obj


def _prender(obj, origem, pai):
    _origens[obj.name] = origem.copy()
    if pai is None:
        obj.location = origem
    else:
        obj.parent = pai
        obj.location = origem - _origens[pai.name]


def vazio(nome, origem=None, pai=None):
    """Um objeto sem forma, que serve de raiz ou de eixo."""
    obj = bpy.data.objects.new(nome, None)
    bpy.context.scene.collection.objects.link(obj)
    _prender(obj, origem if origem is not None else Vector((0, 0, 0)), pai)
    return obj


def tubo(nome, aneis, lados=6, mat="Pele", origem=None, pai=None, torto=0.0, semente=1):
    """Uma forma que passa por vários anéis: serve para tronco, cabeça, braço, perna, chifre, orelha.

    aneis: lista de (centro, raio de lado, raio de espessura). A forma segue os centros, e cada anel é uma
    elipse atravessada no caminho. Raios pequenos fazem ponta. A origem do objeto é o primeiro anel,
    a não ser que 'origem' diga outra coisa: é em torno dela que a peça gira na animação.
    """
    if origem is None:
        origem = aneis[0][0].copy()
    sorteio = random.Random(semente)
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    fileiras = []
    comprimento = 0.0
    for i, (centro, raio_a, raio_b) in enumerate(aneis):
        antes = aneis[max(i - 1, 0)][0]
        depois = aneis[min(i + 1, len(aneis) - 1)][0]
        direcao = (depois - antes).normalized()
        apoio = Vector((0, 0, 1)) if abs(direcao.z) < 0.9 else Vector((0, -1, 0))
        lado = direcao.cross(apoio).normalized()
        espessura = lado.cross(direcao).normalized()
        if i > 0:
            comprimento += (centro - aneis[i - 1][0]).length
        anel = []
        for k in range(lados):
            angulo = 2 * math.pi * (k + 0.5) / lados
            ponto = centro + lado * max(raio_a, 0.004) * math.cos(angulo) + espessura * max(raio_b, 0.004) * math.sin(angulo)
            if torto:
                ponto += Vector((sorteio.uniform(-1, 1), sorteio.uniform(-1, 1), sorteio.uniform(-1, 1))) * torto
            anel.append(bm.verts.new(ponto))
        perimetro = 2 * math.pi * max(raio_a, raio_b)
        fileiras.append((anel, comprimento, perimetro))

    for i in range(len(fileiras) - 1):
        (a, va, pa), (b, vb, pb) = fileiras[i], fileiras[i + 1]
        perimetro = max(pa, pb)
        for k in range(lados):
            k2 = (k + 1) % lados
            face = bm.faces.new((a[k], a[k2], b[k2], b[k]))
            cantos = ((k, va), (k + 1, va), (k + 1, vb), (k, vb))
            for laco, (coluna, v) in zip(face.loops, cantos):
                laco[uv].uv = (coluna / lados * perimetro / METROS_POR_TEXTURA, v / METROS_POR_TEXTURA)
    for anel in (fileiras[0][0], fileiras[-1][0]):
        tampa = bm.faces.new(anel)
        for laco in tampa.loops:
            laco[uv].uv = (laco.vert.co.x / METROS_POR_TEXTURA, laco.vert.co.y / METROS_POR_TEXTURA)
    return _objeto(nome, bm, mat, origem, pai)


def caixa(nome, centro, tamanho, mat="Metal", origem=None, pai=None):
    """Um bloco. 'tamanho' é (largura, comprimento para a frente, altura)."""
    bm = bmesh.new()
    uv = bm.loops.layers.uv.new("UVMap")
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * tamanho[0], v.co.y * tamanho[1], v.co.z * tamanho[2])) + centro
    for face in bm.faces:
        n = face.normal
        for laco in face.loops:
            c = laco.vert.co
            if abs(n.z) > 0.5:
                ponto = (c.x, c.y)
            elif abs(n.y) > 0.5:
                ponto = (c.x, c.z)
            else:
                ponto = (c.y, c.z)
            laco[uv].uv = (ponto[0] / METROS_POR_TEXTURA, ponto[1] / METROS_POR_TEXTURA)
    return _objeto(nome, bm, mat, origem if origem is not None else centro.copy(), pai)


def bola(nome, centro, raios, mat="Olhos", pai=None):
    """Uma bolinha de 20 faces, para olhos e detalhes. 'raios' é (lado, frente, altura)."""
    bm = bmesh.new()
    bm.loops.layers.uv.new("UVMap")
    bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * raios[0], v.co.y * raios[1], v.co.z * raios[2])) + centro
    return _objeto(nome, bm, mat, centro.copy(), pai)


def triangulos():
    return sum(len(p.vertices) - 2 for o in bpy.data.objects if o.type == "MESH" for p in o.data.polygons)


def exportar(caminho):
    """Grava tudo em FBX. Quem acerta os eixos é a Unity, com a opção Bake Axis Conversion ligada na importação
    (o script Montar faz isso). A opção do Blender que faz essa conversão na exportação tira do lugar as peças
    que são filhas de outras, como as canelas."""
    for obj in bpy.data.objects:
        obj.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=caminho,
        use_selection=True,
        object_types={"MESH", "EMPTY"},
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="Y",  # eixos do próprio Blender: o arquivo sai sem nenhum giro, e a Unity converte
        axis_up="Z",
        bake_space_transform=False,
        mesh_smooth_type="FACE",
        bake_anim=False,
    )


def foto(caminho, altura, largura=1200, alto=900, centro=None):
    """Tira uma foto do modelo, de frente e um pouco de lado, para conferir a forma.
    'centro' é a altura para onde a câmera olha; sem ele, olha para o meio de um modelo que pisa no chão."""
    cena = bpy.context.scene
    alvo = Vector((0, 0, altura * 0.5 if centro is None else centro))
    distancia = max(altura, 0.4) * 2.6
    camera = bpy.data.objects.new("Camera", bpy.data.cameras.new("Camera"))
    camera.location = alvo + Vector((-distancia * 0.55, distancia * 0.8, distancia * 0.25))
    camera.rotation_euler = (alvo - camera.location).to_track_quat("-Z", "Y").to_euler()
    cena.collection.objects.link(camera)
    cena.camera = camera
    sol = bpy.data.objects.new("Sol", bpy.data.lights.new("Sol", "SUN"))
    sol.data.energy = 4.0
    sol.rotation_euler = (0.9, 0.1, 0.9)
    cena.collection.objects.link(sol)
    mundo = bpy.data.worlds.new("Mundo")
    cena.world = mundo
    try:
        mundo.use_nodes = True
        mundo.node_tree.nodes["Background"].inputs[0].default_value = (0.35, 0.35, 0.38, 1.0)
    except Exception:
        pass
    for mat in bpy.data.materials:
        try:
            mat.use_nodes = True
            mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = mat.diffuse_color
        except Exception:
            pass
    cena.render.engine = "CYCLES"
    cena.cycles.samples = 24
    cena.cycles.device = "CPU"
    cena.render.resolution_x = largura
    cena.render.resolution_y = alto
    cena.render.resolution_percentage = 100
    cena.render.filepath = caminho
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(camera)
    bpy.data.objects.remove(sol)
