# Marvelous Designer → Unity 透過設定自動変換スクリプトの要件定義

## **1. プロジェクト概要**

### **目的**
Marvelous DesignerからエクスポートしたFBXファイルをUnityに読み込む際、透過設定が失われStandard Shaderに変換される問題を自動解決するHeadless Unityスクリプトを開発する。

### **対象範囲**
- FBXファイル内のマテリアル自動検出
- Standard Shader → lilToon Shaderへの一括変換
- 透過設定の自動復元・適用

## **2. 機能要件**

### **必須機能**
- **FBX自動スキャン**: プロジェクト内の全FBXファイルを検索・識別
- **マテリアル抽出**: FBXファイルからマテリアルを抽出
- **シェーダー変換**: Standard Shader → lilToon Shaderへの変換
- **透過設定復元**: アルファチャンネル検出と透過モード自動設定
- **プロパティマッピング**: メインテクスチャ、ノーマルマップ、カラー値の移行
- **バックアップ機能**: 変換前マテリアルの自動バックアップ作成
- **ログ出力**: 処理結果の詳細ログ出力

### **推奨機能**
- **設定ファイル対応**: JSON形式の設定ファイルによるカスタマイズ
- **プログレスバー**: 処理進捗の視覚化
- **エラーハンドリング**: 変換失敗時の詳細エラー情報
- **フィルタリング**: 特定フォルダ・ファイル名での処理対象限定
- **レポート出力**: 変換結果の統計情報をCSV/JSON出力

## **3. 技術要件**

### **開発環境**
- **Unity バージョン**: 2022.3 LTS以上
- **プラットフォーム**: Windows/Mac対応
- **言語**: C# (.NET Framework 4.8)
- **実行方式**: Unity Editor拡張 + Headless Unity対応

### **依存関係**
- **lilToon シェーダー**: 事前にプロジェクトにインポート済み
- **Unity Editor API**: AssetDatabase, EditorUtility使用
- **System.IO**: ファイル操作用

### **パフォーマンス要件**
- **処理速度**: 100FBXファイル/10分以内
- **メモリ使用量**: 2GB以下
- **バッチ実行**: 無人実行可能

## **4. 入力・出力仕様**

### **入力**
```json
{
  "targetFolder": "Assets/Models/",
  "enableTransparency": true,
  "createBackups": true,
  "targetShader": "lilToon",
  "alphaThreshold": 0.5,
  "logLevel": "Info"
}
```

### **出力**
```json
{
  "processedFiles": 25,
  "convertedMaterials": 47,
  "transparencyApplied": 18,
  "errors": [],
  "processingTime": "00:02:34"
}
```

## **5. プロパティマッピング仕様**

| Standard Shader | lilToon | 設定値 |
|----------------|---------|--------|
| _MainTex | _MainTex | そのまま移行 |
| _BumpMap | _BumpMap | そのまま移行 |
| _Color | _Color | そのまま移行 |
| _Mode (Transparent) | _TransparentMode | 2 (Normal) |
| Rendering Queue | renderQueue | 3000 |

## **6. エラーハンドリング要件**

### **必須対応エラー**
- lilToonシェーダーが見つからない場合
- FBXファイルの読み込み失敗
- マテリアルの書き込み権限エラー
- テクスチャファイルの欠損

### **エラー時動作**
- 処理続行（エラーのあるファイルをスキップ）
- エラーログの詳細記録
- 処理完了後のエラーサマリ表示

## **7. 実行方式**

### **Headless Unity実行**
```bash
Unity.exe -batchmode -quit -projectPath "ProjectPath" 
-executeMethod FBXMaterialConverter.ConvertAllFBXMaterials 
-logFile "conversion.log"
```

### **Unity Editor内実行**
メニューバー: `Tools/Convert FBX Materials to lilToon`

## **8. 成功基準**

### **定量的指標**
- **変換成功率**: 95%以上
- **透過設定復元率**: 90%以上
- **処理時間**: 100FBX/10分以内
- **エラー率**: 5%以下

### **定性的指標**
- Unityプロジェクトが破損しない
- 変換後のマテリアルが正しく表示される
- バックアップから復元可能
- ログが判読可能

## **9. 制約事項**

### **技術的制約**
- lilToonシェーダーがプロジェクトに存在すること
- Unity 2022.3 LTS以上での動作保証
- Headless Unity実行には十分なシステムリソース必要

### **業務的制約**
- 変換前の必須バックアップ作成
- 大量処理時のUnityライセンス制限考慮
- 既存ワークフローへの影響最小化

***

**Claude Codeへの依頼内容**: 上記要件定義に基づき、完全動作するC#スクリプト（エディタ拡張 + 設定ファイル + バッチ実行対応）を実装してください。特に透過設定の自動検出・適用ロジックとエラーハンドリングに重点を置いて開発をお願いします。


use this directory as working directory
mkdir first to smart use
ultrathink to plan 
