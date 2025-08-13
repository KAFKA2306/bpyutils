import bpy
import os

def process_sun_fbx():
    """SUN_v01.fbxを処理してRoot以下以外のボーンを削除する"""
    
    # FBXファイルをインポート
    fbx_path = "/mnt/wsl/SUN_v01.fbx"
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=fbx_path)
    
    # アーマチュアオブジェクトを見つける
    armature_obj = None
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            armature_obj = obj
            break
    
    if not armature_obj:
        print("アーマチュアが見つかりませんでした")
        return
    
    # アーマチュアをアクティブにする
    bpy.context.view_layer.objects.active = armature_obj
    bpy.ops.object.mode_set(mode='EDIT')
    
    armature = armature_obj.data
    
    # Root以下の階層を取得
    root_hierarchy = set()
    
    def get_children_recursive(bone_name):
        root_hierarchy.add(bone_name)
        if bone_name in armature.edit_bones:
            bone = armature.edit_bones[bone_name]
            for child in bone.children:
                get_children_recursive(child.name)
    
    # "Root"ボーンから開始
    if "Root" in armature.edit_bones:
        get_children_recursive("Root")
    
    print(f"Root階層のボーン数: {len(root_hierarchy)}")
    print("Root階層:", sorted(root_hierarchy))
    
    # 削除対象ボーンを特定
    bones_to_delete = []
    for bone in armature.edit_bones:
        if bone.name not in root_hierarchy:
            bones_to_delete.append(bone.name)
    
    print(f"\n削除対象ボーン数: {len(bones_to_delete)}")
    print("削除対象:", bones_to_delete)
    
    # 実際に削除を実行
    for bone_name in bones_to_delete:
        if bone_name in armature.edit_bones:
            armature.edit_bones.remove(armature.edit_bones[bone_name])
            print(f"削除: {bone_name}")
    
    # オブジェクトモードに戻る
    bpy.ops.object.mode_set(mode='OBJECT')
    
    # 結果を保存
    output_path = "/mnt/wsl/SUN_v01_cleaned.blend"
    bpy.ops.wm.save_as_mainfile(filepath=output_path)
    print(f"\n処理完了！結果を保存: {output_path}")
    print(f"削除されたボーン数: {len(bones_to_delete)}")
    print(f"残ったボーン数: {len(root_hierarchy)}")

if __name__ == "__main__":
    process_sun_fbx()