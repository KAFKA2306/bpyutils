import bpy

def get_ik_bone_names():
    """
    特定のIKボーンの名前リストを取得する関数
    
    Returns:
        list: IKボーンの名前のリスト
    """
    ik_bone_names = []
    
    # アクティブなオブジェクトがアーマチュアかチェック
    if bpy.context.active_object and bpy.context.active_object.type == 'ARMATURE':
        armature = bpy.context.active_object.data
        
        # 全てのボーンをチェック
        for bone in armature.bones:
            # IK制約を持つボーンを探す
            if bpy.context.active_object.pose.bones[bone.name].constraints:
                for constraint in bpy.context.active_object.pose.bones[bone.name].constraints:
                    if constraint.type == 'IK':
                        ik_bone_names.append(bone.name)
                        break
    
    return ik_bone_names

def get_root_bone_hierarchy(root_bone_name):
    """
    指定されたルートボーン以下の全てのボーン名を取得する関数
    
    Args:
        root_bone_name (str): ルートボーンの名前
    
    Returns:
        set: ルートボーン以下の全てのボーン名のセット
    """
    hierarchy_bones = set()
    
    if bpy.context.active_object and bpy.context.active_object.type == 'ARMATURE':
        armature = bpy.context.active_object.data
        
        def get_children_recursive(bone_name):
            """再帰的に子ボーンを取得"""
            hierarchy_bones.add(bone_name)
            
            if bone_name in armature.bones:
                bone = armature.bones[bone_name]
                for child in bone.children:
                    get_children_recursive(child.name)
        
        # ルートボーンから開始
        if root_bone_name in armature.bones:
            get_children_recursive(root_bone_name)
    
    return hierarchy_bones

def delete_bones_outside_root(root_bone_name, keep_ik_bones=True):
    """
    ルート以下以外の別のボーンを削除する関数
    
    Args:
        root_bone_name (str): 保持するルートボーンの名前
        keep_ik_bones (bool): IKボーンも保持するかどうか
    """
    if not bpy.context.active_object or bpy.context.active_object.type != 'ARMATURE':
        print("アクティブオブジェクトがアーマチュアではありません")
        return
    
    # エディットモードに切り替え
    bpy.ops.object.mode_set(mode='EDIT')
    
    armature = bpy.context.active_object.data
    
    # 保持するボーンのセットを作成
    bones_to_keep = get_root_bone_hierarchy(root_bone_name)
    
    # IKボーンも保持する場合
    if keep_ik_bones:
        ik_bones = set(get_ik_bone_names())
        bones_to_keep.update(ik_bones)
    
    # 削除するボーンのリスト
    bones_to_delete = []
    
    for bone in armature.edit_bones:
        if bone.name not in bones_to_keep:
            bones_to_delete.append(bone.name)
    
    # ボーンを削除
    for bone_name in bones_to_delete:
        if bone_name in armature.edit_bones:
            armature.edit_bones.remove(armature.edit_bones[bone_name])
            print(f"削除されたボーン: {bone_name}")
    
    # オブジェクトモードに戻る
    bpy.ops.object.mode_set(mode='OBJECT')
    
    print(f"削除完了: {len(bones_to_delete)}個のボーンを削除しました")

# 使用例
if __name__ == "__main__":
    # IKボーンの名前リストを取得
    ik_bones = get_ik_bone_names()
    print("IKボーン:", ik_bones)
    
    # 例: "Root" ボーン以下を保持し、他のボーンを削除
    # delete_bones_outside_root("Root", keep_ik_bones=True)