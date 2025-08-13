# トラブルシューティングガイド

## 🚨 よくあるエラーと解決方法

### 1. "No suitable source mesh found"

**原因:** 重み転送のソースとなるメッシュ（頂点グループを持つメッシュ）が見つからない

**解決方法:**
```bash
# Blenderで手動確認
bin/blender workspace/input/your_model.fbx

# Blenderのコンソールで確認
import bpy
for obj in bpy.data.objects:
    if obj.type == 'MESH':
        print(f"{obj.name}: {len(obj.vertex_groups)} vertex groups")
```

**チェック項目:**
- ボディメッシュに頂点グループが設定されているか
- メッシュ名が予期した名前（"Bodybody", "Body"など）になっているか
- FBXエクスポート時に頂点グループが保持されているか

### 2. "No armature found"

**原因:** アーマチュア（ボーン構造）が見つからない

**解決方法:**
```bash
# アーマチュアの存在確認
import bpy
armatures = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE']
print(f"Found {len(armatures)} armature(s)")
for arm in armatures:
    print(f"  {arm.name}: {len(arm.data.bones)} bones")
```

**チェック項目:**
- FBXファイルにアーマチュアが含まれているか
- Blenderインポート時にアーマチュアが正しく読み込まれているか
- アーマチュアの名前が変更されていないか

### 3. "Weight transfer failed completely"

**原因:** データ転送処理でエラーが発生

**解決方法:**

1. **メッシュの整合性確認:**
```python
# 問題のあるメッシュを特定
import bpy
for obj in bpy.data.objects:
    if obj.type == 'MESH':
        mesh = obj.data
        print(f"{obj.name}:")
        print(f"  Vertices: {len(mesh.vertices)}")
        print(f"  Faces: {len(mesh.polygons)}")
        print(f"  Valid: {mesh.validate()}")
```

2. **手動でのデータ転送テスト:**
```python
# 手動でデータ転送をテスト
bpy.ops.object.select_all(action='DESELECT')
source = bpy.data.objects['Bodybody']
target = bpy.data.objects['衣服メッシュ名']

source.select_set(True)
target.select_set(True)
bpy.context.view_layer.objects.active = source

try:
    bpy.ops.object.data_transfer(data_type='VGROUP_WEIGHTS')
    print("Manual transfer successful")
except Exception as e:
    print(f"Manual transfer failed: {e}")
```

### 4. Unity で重みが表示されない

**原因:** Unityの設定またはFBXエクスポートの問題

**解決方法:**

1. **Unity Import Settings 確認:**
   - Model タブで "Import BlendShapes" を有効にする
   - Rig タブで Animation Type を "Humanoid" または "Generic" に設定
   - Materials タブで適切な設定を選択

2. **FBXファイルサイズ確認:**
```bash
# ファイルサイズが増加していることを確認
du -h workspace/input/original.fbx
du -h workspace/output/with_weights.fbx
# 重みデータが追加されるため、出力ファイルの方が大きくなるはず
```

3. **Blenderでの出力確認:**
```bash
# 出力ファイルをBlenderで開いて確認
bin/blender workspace/output/with_weights.fbx
# Weight Paint モードで重みが表示されることを確認
```

## 🔍 ログファイルの読み方

### ログレベルの理解

```
INFO  - 正常な処理状況
WARNING - 注意が必要だが処理は続行
ERROR - エラーが発生、処理が停止
```

### 重要なログメッセージ

```bash
# 成功パターン
✓ Success: 58 vertex groups transferred
✓ Object: 58 vertex groups + armature modifier

# 問題パターン
✗ Warning: No vertex groups transferred to [mesh_name]
✗ [mesh_name]: 0 vertex groups, armature modifier: False
```

### ログ分析コマンド

```bash
# エラーのみ表示
grep "ERROR" workspace/logs/weight_transfer_*.log

# 処理サマリー表示
grep -E "(Success|Failed|✓|✗)" workspace/logs/weight_transfer_*.log

# 特定メッシュの処理状況確認
grep "Object.001" workspace/logs/weight_transfer_*.log
```

## 🛠️ デバッグ用スクリプト

### メッシュ情報確認スクリプト

```python
#!/usr/bin/env python3
"""
mesh_analyzer.py - FBXファイルのメッシュ情報を分析
"""

import bpy
import sys

def analyze_fbx(filepath):
    # FBX読み込み
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()
    bpy.ops.import_scene.fbx(filepath=filepath)
    
    print(f"=== Analysis: {filepath} ===")
    
    # アーマチュア情報
    armatures = [obj for obj in bpy.data.objects if obj.type == 'ARMATURE']
    print(f"\nArmatures: {len(armatures)}")
    for arm in armatures:
        print(f"  {arm.name}: {len(arm.data.bones)} bones")
    
    # メッシュ情報
    meshes = [obj for obj in bpy.data.objects if obj.type == 'MESH']
    print(f"\nMeshes: {len(meshes)}")
    
    rigged_meshes = []
    unrigged_meshes = []
    
    for mesh in meshes:
        vg_count = len(mesh.vertex_groups)
        has_armature_mod = any(mod.type == 'ARMATURE' for mod in mesh.modifiers)
        
        info = f"  {mesh.name}: {vg_count} vertex groups, armature modifier: {has_armature_mod}"
        
        if vg_count > 0 and has_armature_mod:
            rigged_meshes.append(mesh.name)
            print(f"✓ {info}")
        elif vg_count > 0:
            print(f"⚠ {info} (has groups but no modifier)")
        else:
            unrigged_meshes.append(mesh.name)
            print(f"✗ {info}")
    
    print(f"\nSummary:")
    print(f"  Rigged meshes: {len(rigged_meshes)}")
    print(f"  Unrigged meshes: {len(unrigged_meshes)}")
    
    if rigged_meshes:
        print(f"  Source candidates: {rigged_meshes}")
    
    if unrigged_meshes:
        print(f"  Target candidates: {unrigged_meshes[:5]}{'...' if len(unrigged_meshes) > 5 else ''}")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        analyze_fbx(sys.argv[1])
    else:
        print("Usage: blender --background --python mesh_analyzer.py path/to/model.fbx")
```

### 使用方法

```bash
# メッシュ分析
bin/blender --background --python scripts/mesh_analyzer.py workspace/input/model.fbx

# 重み転送後の確認
bin/blender --background --python scripts/mesh_analyzer.py workspace/output/model_rigged.fbx
```

## 🐧 環境固有の問題

### WSL2 での問題

**問題:** パスの区切り文字やパーミッション

**解決方法:**
```bash
# Windowsパスの変換
wslpath "C:\\Users\\username\\model.fbx"

# パーミッション修正
chmod +x bin/transfer_weights.sh
chmod +x bin/blender
```

### メモリ不足

**問題:** 大きなFBXファイルの処理でメモリ不足

**解決方法:**
```bash
# メモリ使用量確認
free -h

# スワップファイル作成（必要に応じて）
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
```

## 📞 サポート情報

### 情報収集コマンド

問題報告時は以下の情報を含めてください：

```bash
# システム情報
uname -a
free -h
df -h

# Blenderバージョン
bin/blender --version

# ファイル情報
ls -la workspace/input/
ls -la workspace/output/
du -sh workspace/logs/

# 最新ログ
tail -50 workspace/logs/weight_transfer_*.log
```

### よくある質問

**Q: 処理にどのくらい時間がかかりますか？**
A: モデルの複雑さによりますが、通常は30秒〜2分程度です。

**Q: 元のFBXファイルは変更されますか？**
A: いいえ、元ファイルは変更されません。新しいファイルが出力されます。

**Q: Blender GUIで確認したい場合は？**
A: `bin/blender workspace/output/model_rigged.fbx` でGUI版Blenderが起動します。

**Q: 他の3Dソフトウェアで作成したFBXでも使用できますか？**
A: はい、Maya、3ds Max、Blenderなどで作成されたFBXに対応しています。