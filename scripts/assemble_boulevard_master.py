import bpy
import os
import math
from mathutils import Vector, Euler

models_dir = r"C:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Models"
master_blend = os.path.join(models_dir, "NeoEclipse_Boulevard_Segment_01.blend")
output_fbx = os.path.join(models_dir, "NeoEclipse_Boulevard_Segment_01.fbx")
preview_img = r"C:\Users\asakura\.gemini\antigravity\brain\c7ee7e60-d313-4624-b38b-8301e3292962\preview_boulevard_assembled.png"

print(f"Loading master blend: {master_blend}")
bpy.ops.wm.open_mainfile(filepath=master_blend)

scene = bpy.context.scene

def get_or_create_collection(name, parent_col=None):
    if name in bpy.data.collections:
        col = bpy.data.collections[name]
    else:
        col = bpy.data.collections.new(name)
        if parent_col:
            parent_col.children.link(col)
        else:
            scene.collection.children.link(col)
    return col

col_visuals = get_or_create_collection("Environment_Visuals")
col_props = get_or_create_collection("Street_Props", col_visuals)
col_colliders = get_or_create_collection("Colliders_UCX")
col_lighting = get_or_create_collection("Lighting_Cyberpunk")

for obj in list(scene.objects):
    if obj.name.startswith("UCX_"):
        obj.display_type = 'WIRE'
        for c in obj.users_collection:
            c.objects.unlink(obj)
        col_colliders.objects.link(obj)
    elif obj.type == 'MESH':
        if col_visuals not in obj.users_collection:
            for c in list(obj.users_collection):
                c.objects.unlink(obj)
            col_visuals.objects.link(obj)

def append_assets_from_blend(blend_file, target_col, offset_loc=(0, 0, 0), rot_z=0.0):
    filepath = os.path.join(models_dir, blend_file)
    if not os.path.exists(filepath):
        print(f"Warning: {filepath} not found!")
        return []
    
    with bpy.data.libraries.load(filepath, link=False) as (data_from, data_to):
        data_to.objects = [name for name in data_from.objects]
    
    imported = []
    for obj in data_to.objects:
        if obj is None: continue
        imported.append(obj)
        if obj.name.startswith("UCX_"):
            obj.display_type = 'WIRE'
            col_colliders.objects.link(obj)
        else:
            target_col.objects.link(obj)
        
        obj.location = obj.location + Vector(offset_loc)
        if rot_z != 0.0:
            obj.rotation_euler.z += rot_z
    print(f"Appended {len(imported)} objects from {blend_file} at offset {offset_loc}")
    return imported

# 1. Terrace Set at Cyber Cafe (Sidewalk Left, near Cafe facade)
append_assets_from_blend("WiredCafe_TerraceSet.blend", col_props, offset_loc=(-3.7, -4.0, 0.15))

# 2. Cyber Ramen Bar (Sidewalk Left)
append_assets_from_blend("CyberRamen_CounterBar.blend", col_props, offset_loc=(-3.65, -0.5, 0.15), rot_z=math.radians(180))

# 3. Tactical Cover Ballistic Barrier (Sidewalk Right)
append_assets_from_blend("TacticalCover_BallisticBarrier.blend", col_props, offset_loc=(3.6, -2.5, 0.15))

# 4. Cyber Supply Chest (Sidewalk Right)
append_assets_from_blend("CyberChest_Supply3D.blend", col_props, offset_loc=(3.6, 2.0, 0.15))

# 5. Data Well Core (Street / Extraction point)
append_assets_from_blend("DataWell_Core3D.blend", col_props, offset_loc=(0.0, -4.5, 0.01))

print("=== Setting up Cyberpunk Lighting ===")

def create_light(name, ltype, location, color, energy, spot_size=None):
    light_data = bpy.data.lights.new(name=f"{name}_Data", type=ltype)
    light_data.color = color
    light_data.energy = energy
    if spot_size and ltype == 'SPOT':
        light_data.spot_size = spot_size
        light_data.spot_blend = 0.5
    light_obj = bpy.data.objects.new(name=name, object_data=light_data)
    light_obj.location = location
    col_lighting.objects.link(light_obj)
    return light_obj

# Street spotlights (pointing straight down at the wet asphalt)
spot1 = create_light("StreetSpot_L1", 'SPOT', (-2.8, -4.0, 5.0), (0.1, 0.9, 1.0), 350.0, spot_size=math.radians(70))
spot1.rotation_euler = (0, 0, 0)

spot2 = create_light("StreetSpot_L2", 'SPOT', (-2.8, 2.0, 5.0), (0.1, 0.9, 1.0), 350.0, spot_size=math.radians(70))
spot2.rotation_euler = (0, 0, 0)

spot3 = create_light("StreetSpot_R1", 'SPOT', (2.8, -4.0, 5.0), (0.2, 0.8, 1.0), 350.0, spot_size=math.radians(70))
spot3.rotation_euler = (0, 0, 0)

spot4 = create_light("StreetSpot_R2", 'SPOT', (2.8, 2.0, 5.0), (0.2, 0.8, 1.0), 350.0, spot_size=math.radians(70))
spot4.rotation_euler = (0, 0, 0)

# Neon Ambient Point Lights
create_light("NeonGlow_RadioChipset", 'POINT', (0.0, 6.8, 6.0), (1.0, 0.05, 0.65), 500.0)
create_light("NeonGlow_Skybridge_Under", 'POINT', (0.0, 5.25, 3.5), (0.0, 1.0, 0.85), 250.0)
create_light("AmbientGlow_RamenStall", 'POINT', (-3.65, -0.5, 1.5), (1.0, 0.35, 0.05), 120.0)
create_light("AmbientGlow_CafeTerrace", 'POINT', (-3.7, -4.0, 1.5), (0.2, 0.7, 1.0), 90.0)
create_light("AmbientGlow_DataWell", 'POINT', (0.0, -4.5, 0.8), (0.0, 1.0, 0.7), 80.0)

# Global night sky ambience
if scene.world is None:
    scene.world = bpy.data.worlds.new("World_CyberpunkNight")
scene.world.use_nodes = True
bg_node = scene.world.node_tree.nodes.get("Background")
if bg_node:
    bg_node.inputs['Color'].default_value = (0.015, 0.025, 0.06, 1.0)
    bg_node.inputs['Strength'].default_value = 0.45

print("=== Setting up Camera ===")
cam_data = bpy.data.cameras.new("Cam_VR_Player_EyeLevel")
cam_data.lens = 24  # Wide 24mm VR field of view
cam_data.clip_start = 0.1
cam_data.clip_end = 250.0
cam_obj = bpy.data.objects.new("Cam_VR_Player_EyeLevel", cam_data)
cam_obj.location = (0.0, -6.2, 1.65)  # 1.65m human eye level on street
cam_obj.rotation_euler = (math.radians(82), 0.0, 0.0)  # Looking down boulevard slightly up
col_lighting.objects.link(cam_obj)
scene.camera = cam_obj

# Shading & Render Setup
try:
    scene.render.engine = 'BLENDER_EEVEE_NEXT'
except Exception:
    scene.render.engine = 'BLENDER_WORKBENCH'
scene.render.resolution_x = 1920
scene.render.resolution_y = 1080
scene.render.filepath = preview_img
scene.render.image_settings.file_format = 'PNG'

print("=== Rendering Preview ===")
try:
    bpy.ops.render.render(write_still=True)
    print(f"Render successfully saved to: {preview_img}")
except Exception as e:
    print(f"EEVEE render failed ({e}), falling back to workbench/opengl...")
    scene.render.engine = 'BLENDER_WORKBENCH'
    bpy.ops.render.render(write_still=True)

print("=== Saving Master Blend ===")
bpy.ops.wm.save_mainfile(filepath=master_blend)
print(f"Saved blend to {master_blend}")

print("=== Exporting FBX for Unity ===")
bpy.ops.export_scene.fbx(
    filepath=output_fbx,
    check_existing=False,
    use_selection=False,
    global_scale=1.0,
    apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_ALL',
    bake_space_transform=False,
    object_types={'MESH'},
    use_mesh_modifiers=True,
    mesh_smooth_type='FACE',
    add_leaf_bones=False,
    axis_forward='-Z',
    axis_up='Y'
)
print(f"Exported FBX to {output_fbx}")
print("=== BOULEVARD MASTER ASSEMBLY COMPLETE ===")
