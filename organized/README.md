# FBX Weight Transfer Tool for Unity

このツールは、3Dキャラクターモデルの衣服や装飾品にボディメッシュの重み付け（ウェイト）を自動転送し、Unity用に最適化されたFBXファイルを出力します。

## 🎯 主な機能

- **自動ウェイト転送**: ボディメッシュから衣服メッシュへの重み自動転送
- **Unityファフレンドリー**: Unity向けに最適化されたFBX出力設定
- **バッチ処理**: 複数の衣服メッシュを一度に処理
- **詳細ログ**: トラブルシューティング用の詳細なログ出力
- **Kiseruベース**: Kiseruアドオンの技術を活用

## 📁 ディレクトリ構造

```
organized/
├── bin/                          # 実行ファイル
│   ├── blender -> Blender 4.0.2  # Blender実行ファイル（シンボリックリンク）
│   └── transfer_weights.sh       # 簡単実行スクリプト
├── scripts/                      # Pythonスクリプト
│   ├── fbx_weight_transfer.py    # メインの重み転送スクリプト
│   └── Kiseru/                   # Kiseruアドオン（元のスクリプト）
├── examples/                     # サンプル
│   └── README_examples.md        # 使用例
├── docs/                         # ドキュメント
│   └── troubleshooting.md        # トラブルシューティング
└── workspace/                    # ワークスペース
    ├── input/                    # 入力ファイル置き場
    ├── output/                   # 出力ファイル置き場
    └── logs/                     # ログファイル
```

## 🚀 クイックスタート

### 1. 簡単な使用方法（推奨）

```bash
# organized/bin/ ディレクトリに移動
cd organized/bin/

# 重み転送を実行
./transfer_weights.sh ../workspace/input/your_model.fbx ../workspace/output/your_model_rigged.fbx
```

### 2. 直接Blenderを使用する方法

```bash
# organized/ ディレクトリから実行
bin/blender --background --python scripts/fbx_weight_transfer.py -- workspace/input/model.fbx workspace/output/model_with_weights.fbx
```

## 📋 必要な条件

### 入力FBXファイルの要件

- **最低1つのリグ済みメッシュ** - 通常は"Bodybody"や"Body"という名前のボディメッシュ
- **アーマチュア（ボーン構造）** - キャラクターのスケルトン
- **頂点グループ** - ボディメッシュにボーンに対応した頂点グループが設定済み
- **重み付けが必要なメッシュ** - 衣服や装飾品など、重みが設定されていないメッシュ

### システム要件

- **Linux環境** (WSL2推奨)
- **Blender 4.0.2** (同梱済み)
- **Python 3.10+** (Blenderに同梱)

## 🔧 動作原理

1. **入力FBX読み込み**: 3Dモデルデータを解析
2. **ソースメッシュ検出**: 最も多くの頂点グループを持つメッシュを自動検出
3. **ターゲットメッシュ検出**: 重みが設定されていないメッシュを自動検出
4. **ウェイト転送**: Blenderのdata_transferオペレーターを使用
5. **アーマチュア設定**: 自動でアーマチュアの親子関係とモディファイアを設定
6. **Unity向けエクスポート**: 最適化された設定でFBX出力

## 📊 処理前後の比較

| 項目 | 処理前 | 処理後 |
|------|---------|---------|
| ボディメッシュ | ✅ 58個の頂点グループ | ✅ 58個の頂点グループ |
| 衣服メッシュ | ❌ 0個の頂点グループ | ✅ 58個の頂点グループ |
| アーマチュアモディファイア | ❌ 無し | ✅ 全メッシュに設定 |
| Unityでの表示 | ❌ 重みなし | ✅ 完全な重み付き |

## 🐛 よくある問題

### エラー: "No suitable source mesh found"
- ボディメッシュに頂点グループが設定されているか確認
- メッシュ名が"Bodybody"や"Body"になっているか確認

### エラー: "No armature found"
- FBXファイルにアーマチュア（ボーン）が含まれているか確認
- Blenderでインポート時にアーマチュアが正しく読み込まれているか確認

### Unity で重みが表示されない
- FBXファイルサイズが増加しているか確認（重みデータが追加されるため）
- Unity の Import Settings で "Import BlendShapes" が有効か確認

## 📝 ログファイル

詳細なログは `workspace/logs/` に自動保存されます：

```
workspace/logs/weight_transfer_20250813_123456.log
```

## 🤝 サポート

問題が発生した場合は、以下を確認してください：

1. **ログファイル** - `workspace/logs/` の最新ログを確認
2. **入力ファイル** - FBXファイルがBlenderで正しく開けるか確認
3. **システム** - Linux環境（WSL2推奨）で実行しているか確認

## 📄 ライセンス

MIT License

## 👨‍💻 開発情報

- **開発**: Generated with Claude Code
- **バージョン**: 1.0.0
- **更新日**: 2025-08-13
- **対応Blender**: 4.0.2+