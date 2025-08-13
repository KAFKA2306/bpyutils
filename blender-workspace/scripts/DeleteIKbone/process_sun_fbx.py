import bpy
import os

def clear_scene():
    """シーンをクリアする"""
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)

def import_fbx_file(filepath):
    """FBXファイルをインポートする"""
    if not os.path.exists(filepath):
        print(f"ファイルが見つかりません: {filepath}")
        return False
    
    try:
        bpy.ops.import_scene.fbx(filepath=filepath)
        print(f"FBXファイルをインポートしました: {filepath}")
        return True
    except Exception as e:
        print(f"FBXインポートエラー: {e}")
        return False

def find_armature_object():
    """シーン内のアーマチュアオブジェクトを見つける"""
    for obj in bpy.context.scene.objects:
        if obj.type == 'ARMATURE':
            return obj
    return None

def get_ik_bone_names(armature_obj):
    """特定のIKボーンの名前リストを取得する"""
    if not armature_obj:
        return []
    
    ik_bone_names = []
    
    # アーマチュアオブジェクトをアクティブにする
    bpy.context.view_layer.objects.active = armature_obj
    
    # ポーズモードに切り替え
    bpy.ops.object.mode_set(mode='POSE')
    
    # 全てのボーンをチェック
    for bone_name, pose_bone in armature_obj.pose.bones.items():
        # IK制約を持つボーンを探す
        for constraint in pose_bone.constraints:
            if constraint.type == 'IK':
                ik_bone_names.append(bone_name)
                break
    
    return ik_bone_names

def get_bone_hierarchy(armature_obj, root_bone_name):
    """指定されたルートボーン以下の全てのボーン名を取得する"""
    if not armature_obj:
        return set()
    
    hierarchy_bones = set()
    armature = armature_obj.data
    
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

def delete_bones_outside_root(armature_obj, root_bone_name, keep_ik_bones=True):
    """ルート以下以外のボーンを削除する"""
    if not armature_obj:
        print("アーマチュアオブジェクトが見つかりません")
        return
    
    # アーマチュアオブジェクトをアクティブにする
    bpy.context.view_layer.objects.active = armature_obj
    
    # エディットモードに切り替え
    bpy.ops.object.mode_set(mode='EDIT')
    
    armature = armature_obj.data
    
    # 保持するボーンのセットを作成
    bones_to_keep = get_bone_hierarchy(armature_obj, root_bone_name)
    
    # IKボーンも保持する場合
    if keep_ik_bones:
        # オブジェクトモードに一時的に戻ってIKボーンを取得
        bpy.ops.object.mode_set(mode='OBJECT')
        ik_bones = set(get_ik_bone_names(armature_obj))
        bones_to_keep.update(ik_bones)
        # エディットモードに戻る
        bpy.ops.object.mode_set(mode='EDIT')
    
    # 削除するボーンのリスト
    bones_to_delete = []
    
    for bone in armature.edit_bones:
        if bone.name not in bones_to_keep:
            bones_to_delete.append(bone.name)
    
    print(f"保持されるボーン数: {len(bones_to_keep)}")
    print(f"削除対象ボーン数: {len(bones_to_delete)}")
    
    # ボーンを削除
    for bone_name in bones_to_delete:
        if bone_name in armature.edit_bones:
            armature.edit_bones.remove(armature.edit_bones[bone_name])
            print(f"削除されたボーン: {bone_name}")
    
    # オブジェクトモードに戻る
    bpy.ops.object.mode_set(mode='OBJECT')
    
    print(f"削除完了: {len(bones_to_delete)}個のボーンを削除しました")

def list_all_bones(armature_obj):
    """全てのボーン名をリスト表示する"""
    if not armature_obj:
        return
    
    print("\n=== 全ボーンリスト ===")
    for i, bone in enumerate(armature_obj.data.bones, 1):
        parent_name = bone.parent.name if bone.parent else "None"
        print(f"{i:3d}. {bone.name} (親: {parent_name})")

def find_root_bones(armature_obj):
    """ルートボーン（親のないボーン）を見つける"""
    if not armature_obj:
        return []
    
    root_bones = []
    for bone in armature_obj.data.bones:
        if bone.parent is None:
            root_bones.append(bone.name)
    
    return root_bones

# メイン処理
def main():
    fbx_path = "/mnt/wsl/SUN_v01.fbx"
    
    # シーンをクリア
    clear_scene()
    
    # FBXファイルをインポート
    if not import_fbx_file(fbx_path):
        return
    
    # アーマチュアオブジェクトを見つける
    armature_obj = find_armature_object()
    if not armature_obj:
        print("アーマチュアオブジェクトが見つかりませんでした")
        return
    
    print(f"アーマチュアオブジェクト発見: {armature_obj.name}")
    
    # 全ボーンをリスト表示
    list_all_bones(armature_obj)
    
    # ルートボーンを見つける
    root_bones = find_root_bones(armature_obj)
    print(f"\nルートボーン: {root_bones}")
    
    # IKボーンを見つける
    ik_bones = get_ik_bone_names(armature_obj)
    print(f"IKボーン: {ik_bones}")
    
    # 最初のルートボーンを基準にして他のボーンを削除
    if root_bones:
        root_bone = root_bones[0]  # 最初のルートボーンを使用
        print(f"\n{root_bone} 以下を保持し、他のボーンを削除します...")
        
        # 確認用に保持されるボーンを表示
        hierarchy = get_bone_hierarchy(armature_obj, root_bone)
        if ik_bones:
            hierarchy.update(ik_bones)
        print(f"保持されるボーン: {sorted(hierarchy)}")
        
        # 実際の削除処理（コメントアウトを外して実行）
        # delete_bones_outside_root(armature_obj, root_bone, keep_ik_bones=True)

if __name__ == "__main__":
    main()