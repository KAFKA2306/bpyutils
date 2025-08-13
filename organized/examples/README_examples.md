# 使用例とサンプル

## 📖 基本的な使用例

### 例1: キャラクターモデルの重み転送

```bash
# 入力: character_model.fbx（ボディメッシュはリグ済み、衣服は未リグ）
# 出力: character_model_rigged.fbx（全メッシュがリグ済み）

./transfer_weights.sh ../workspace/input/character_model.fbx ../workspace/output/character_model_rigged.fbx
```

**処理内容:**
- ボディメッシュ（"Bodybody"）の58個のボーン重みを検出
- 42個の衣服メッシュ（Object, Pattern_*, Sleeves_*など）に重み転送
- 全メッシュにアーマチュアモディファイアを自動設定

### 例2: 複数ファイルの一括処理

```bash
# 複数のFBXファイルを一度に処理
for file in ../workspace/input/*.fbx; do
    filename=$(basename "$file" .fbx)
    ./transfer_weights.sh "$file" "../workspace/output/${filename}_rigged.fbx"
done
```

### 例3: カスタムパスでの実行

```bash
# 絶対パスを使用
./transfer_weights.sh /home/user/models/avatar.fbx /home/user/exports/avatar_unity.fbx

# 相対パスを使用
./transfer_weights.sh ../../Downloads/model.fbx ../../Unity_Project/Assets/model_rigged.fbx
```

## 🔍 実際の処理例: SUN_wear モデル

### 処理前の状態
```
入力ファイル: 2025-08-12-3_SUN_wear.fbx (2.3MB)

オブジェクト構成:
├── Armature.JamCommon (99ボーン)
├── Bodybody (58頂点グループ) ✅ リグ済み
├── Body (3頂点グループ) ✅ リグ済み  
└── 41個の衣服メッシュ (0頂点グループ) ❌ 未リグ
    ├── BindingPattern_46871
    ├── Body_Back, Body_Front_2～8
    ├── Object, Object.001～019
    ├── Pattern_*（8個）
    └── Sleeves_6, Sleeves_8
```

### 実行コマンド
```bash
./transfer_weights.sh ../workspace/input/2025-08-12-3_SUN_wear.fbx ../workspace/output/2025-08-12-3_SUN_wear_rigged.fbx
```

### 処理後の結果
```
出力ファイル: 2025-08-12-3_SUN_wear_rigged.fbx (6.7MB)

成功した処理:
✅ 41/41 個の衣服メッシュに重み転送完了
✅ 全メッシュ（43/43）が適切にリグ済み
✅ 各メッシュに58個の頂点グループを設定
✅ 全メッシュにアーマチュアモディファイア追加
✅ Unity向けFBX設定で出力

ファイルサイズ変化: 2.3MB → 6.7MB (+190%)
理由: 頂点グループデータの追加によるサイズ増加
```

## 🎮 Unity での確認方法

### 1. FBXインポート後の確認

```
Unity Project
├── Assets/
│   └── Models/
│       └── character_rigged.fbx
│           ├── 📦 Meshes (全メッシュが表示される)
│           ├── 🎭 Materials (マテリアル)
│           └── 🦴 Armature (ボーン構造)
```

### 2. Rigged状態の確認

1. **Inspector で確認:**
   - Animation Type: Humanoid または Generic
   - Avatar Definition: Create From This Model

2. **Scene View で確認:**
   - キャラクターを選択
   - Show Wireframe で骨格表示
   - 各衣服パーツにも骨格が表示されることを確認

3. **Animation で確認:**
   - テスト用アニメーションを適用
   - 衣服が体の動きに追従することを確認

## 🔧 トラブルシューティング例

### 例1: ログ出力の確認

```bash
# 処理実行
./transfer_weights.sh model.fbx model_rigged.fbx

# ログファイル確認
ls -la ../workspace/logs/
cat ../workspace/logs/weight_transfer_*.log | grep -E "(ERROR|WARNING|SUCCESS)"
```

### 例2: ファイルサイズによる確認

```bash
# ファイルサイズ比較
du -h ../workspace/input/model.fbx
du -h ../workspace/output/model_rigged.fbx

# サイズが大幅に増加していれば重みデータが正常に追加されている
```

### 例3: Blender での手動確認

```bash
# 出力ファイルをBlenderで開いて確認
../bin/blender ../workspace/output/model_rigged.fbx
```

Blender内で確認すべき点:
- 全メッシュに頂点グループが存在するか
- アーマチュアモディファイアが設定されているか
- Weight Paint モードで重みが正しく表示されるか

## 💡 高度な使用例

### カスタムスクリプトでの呼び出し

```python
import subprocess
import sys

def transfer_weights(input_fbx, output_fbx):
    """重み転送のPythonラッパー"""
    cmd = [
        "./transfer_weights.sh",
        input_fbx,
        output_fbx
    ]
    
    try:
        result = subprocess.run(cmd, check=True, capture_output=True, text=True)
        print("✅ Success:", result.stdout)
        return True
    except subprocess.CalledProcessError as e:
        print("❌ Error:", e.stderr)
        return False

# 使用例
success = transfer_weights(
    "../workspace/input/character.fbx",
    "../workspace/output/character_rigged.fbx"
)
```

### バッチ処理スクリプト

```bash
#!/bin/bash
# batch_process.sh

INPUT_DIR="../workspace/input"
OUTPUT_DIR="../workspace/output"

echo "=== Batch Weight Transfer ==="
echo "Input directory: $INPUT_DIR"
echo "Output directory: $OUTPUT_DIR"

count=0
for fbx_file in "$INPUT_DIR"/*.fbx; do
    if [ -f "$fbx_file" ]; then
        filename=$(basename "$fbx_file" .fbx)
        output_file="$OUTPUT_DIR/${filename}_rigged.fbx"
        
        echo ""
        echo "Processing [$((++count))]: $filename"
        
        if ./transfer_weights.sh "$fbx_file" "$output_file"; then
            echo "✅ Success: $output_file"
        else
            echo "❌ Failed: $fbx_file"
        fi
    fi
done

echo ""
echo "Batch processing complete: $count files processed"
```