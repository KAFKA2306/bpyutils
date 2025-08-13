Skirt PB Installer (Headless Unity Edition)

***

目的
----

VRChat アバターのスカート用 VRCPhysBone（PB）設定を  
GUI なし（`-batchmode -nographics`）の Unity ランタイム／CI 上で自動化する  
CLI ツールを作成する。

環境
----

* Unity 2022.3 LTS 以降（Windows / Linux 共通）  
* 起動オプション: `-batchmode -nographics`  
* VRCSDK3-AVATAR（PhysBone API 利用）  
* .NET Standard 2.1 相当（Unity 内部ランタイム）

入出力
------

1. 入力  
   * シーン or Prefab 内に含まれるスケルトン階層  
   * コマンドライン引数  
     - `-skirtRoot ` (省略可)  
     - `-angle ` (default 45)  
     - `-innerAngle ` (default 10)  
     - `-boneRegex ` (省略時: `\.\d+`)  
     - `-hierarchyOut ` (default `hierarchy_dump.txt`)
2. 出力  
   * 階層ダンプ TXT (パス/名前を 1 行 1 ノードで保存)  
   * 標準ログ（Unity log）  
   * 終了コード   
     0 = 成功 / 1 = -skirtRoot 未指定 / 2 = 指定ノードなし /  
     3 = PhysBone 不足 / 4 = その他エラー

機能要件
--------

FR-1 階層ダンプ  
* アクティブシーンをロードし、`Scene.GetRootGameObjects()` から DFS で  
  `fullPath` を `hierarchyOut` に出力する。

FR-2 Skirt Root 指定の受付  
* `-skirtRoot` が無い場合は FR-1 のみ実行して ExitCode 1。  
* ある場合は `GameObject.Find()` で該当 Transform を取得。

FR-3 ボーン列挙  
* `boneRegex` で Skirt Root 直下の **直系の子** をフィルタ。  
  該当なし → ExitCode 2。

FR-4 PhysBone 検証  
* 各ボーンに `VRCPhysBone` が付いているか確認。  
  1 つでも欠ければ ExitCode 3。

FR-5 PhysBone 設定  
* 対象ボーンに対し下記プロパティを一括変更 / 追加入。  
  ```
  limitType      = Hinge
  maxAngleX      = 
  limitRotation.x=  - 
  limitRotation.y= roll   // 計算式下記
  ```
* roll = atan2(childPos.z, childPos.x) * Rad2Deg + 90  
  ただし childPos は Root から最深 Leaf までのベクトル。

FR-6 アセット保存  
* `EditorUtility.SetDirty()` と `AssetDatabase.SaveAssets()` で変更を永続化。  
  Player ビルド時は不要。

非機能要件
----------

NFR-1 ヘッドレス互換  
* GraphicsDeviceType.Null でも動作する API のみを使用する。

NFR-2 依存最小化  
* 追加 DLL は VRCSDK3 以外導入しない。  
* ソースは単一 C# ファイル (`SkirtPBHeadless.cs`) で完結させる。

NFR-3 スクリプトサイズ  
* 500 行以内、外部 JSON 等は使わない。

NFR-4 ログ  
* `Debug.Log` / `Debug.LogError` で人が読めるメッセージを出力。  
* Hierarchy構造を出力する
* PhysBoneがついたかどうかあとからチェックする別スクリプトを使って出力する
* 重大エラー時は `Application.Quit()` を必ず呼ぶ。


ファイル構成
-------------

```
Assets/
 └─ Editor/
     └─ SkirtPBHeadless.cs   // メイン実装
requirements.md             // 本ドキュメント
```


/mnt/wsl/bpyutils-backup - コピー/headlessunityVRChatPhysBonesSemiAutoInstallerで作業する
