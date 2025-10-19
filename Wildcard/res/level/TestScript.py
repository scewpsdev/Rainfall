import bpy

class TestPanel(bpy.types.Panel):
    bl_label = "Test Panel"
    bl_idname = "PT_TestPanel"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "TestPanel"
    
    def draw(self, context):
        layout = self.layout
        obj = context.object
        
        row = layout.row()
        row.label(text = "abc", icon = "CUBE")
        
        row = layout.row()
        row.label(text="Hello world!", icon='WORLD_DATA')
        
        row = layout.row()
        row.operator("wm.createlayer")
        
        row = layout.row()
        row.operator("wm.adjustlayerscale")
        
class CreateLayerOperator(bpy.types.Operator):
    bl_label = "Create Layer"
    bl_idname = "wm.createlayer"
    
    def execute(self, context):
        bpy.ops.mesh.primitive_plane_add(size=1, enter_editmode=True, align="WORLD", location=(0, 0, 0), scale=(1, 1, 1))
        #bpy.ops.transform.translate(value=(0.5, 0, 0.5), orient_type='GLOBAL', orient_matrix=((1, 0, 0), (0, 1, 0), (0, 0, 1)), orient_matrix_type='GLOBAL', constraint_axis=(True, False, False), mirror=True, use_proportional_edit=False, proportional_edit_falloff='SMOOTH', proportional_size=1, use_proportional_connected=False, use_proportional_projected=False, snap=False, snap_elements={'INCREMENT'}, use_snap_project=False, snap_target='CLOSEST', use_snap_self=True, use_snap_edit=True, use_snap_nonedit=True, use_snap_selectable=False)
        bpy.ops.transform.rotate(value=1.5708, orient_axis='X', orient_type='GLOBAL', orient_matrix=((1, 0, 0), (0, 1, 0), (0, 0, 1)), orient_matrix_type='GLOBAL', constraint_axis=(True, False, False), mirror=True, use_proportional_edit=False, proportional_edit_falloff='SMOOTH', proportional_size=1, use_proportional_connected=False, use_proportional_projected=False, snap=False, snap_elements={'INCREMENT'}, use_snap_project=False, snap_target='CLOSEST', use_snap_self=True, use_snap_edit=True, use_snap_nonedit=True, use_snap_selectable=False)
        bpy.ops.object.editmode_toggle()
        
        obj = bpy.context.active_object
        
        mat = bpy.data.materials.new(name = "ParallaxMaterial")
        obj.data.materials.append(mat)
        mat.use_nodes = True
        mat.blend_method = "CLIP"
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links
        
        bsdf = nodes.get("Principled BSDF")
        bsdf.inputs[9].default_value = 1.0
        
        texnode = nodes.new("ShaderNodeTexImage")
        texnode.location = (-300,300)
        texnode.interpolation = "Closest"
        
        links.new(texnode.outputs[0], bsdf.inputs[0])
        links.new(texnode.outputs[1], bsdf.inputs[21])
                        
        return {"FINISHED"}

class AdjustScaleOperator(bpy.types.Operator):
    bl_label = "Adjust Scale"
    bl_idname = "wm.adjustlayerscale"
    
    def execute(self, context):
        obj = bpy.context.active_object
        mat = obj.active_material
        
        layer = obj.location[1]
        scale = (layer + 10.0) / 10.0
        obj.scale = (scale, scale, scale)
        
        return {"FINISHED"}
                
def register():
    bpy.utils.register_class(TestPanel)
    bpy.utils.register_class(CreateLayerOperator)
    bpy.utils.register_class(AdjustScaleOperator)

def unregister():
    bpy.utils.unregister_class(TestPanel)
    bpy.utils.unregister_class(CreateLayerOperator)
    bpy.utils.unregister_class(AdjustScaleOperator)

if __name__ == "__main__":
    register()

