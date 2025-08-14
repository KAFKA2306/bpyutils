WSL2環境でUnity Hubとエディターを導入する手順を紹介します。

## 1. Unity Hubのインストール

まずはUnityの公式リポジトリを追加してUnity Hubをインストールします：

```bash
# Unity公式リポジトリの追加
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null

# リポジトリの設定
sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

# Unity Hubのインストール
sudo apt update && sudo apt install -y unityhub
```

## 2. 必要な依存関係の追加

Unity HubをWSL2で動作させるために必要なパッケージをインストール：

```bash
sudo apt install -y xvfb libasound2 libgtk-3-0 libgconf-2-4 libnss3
```

## 3. D-Busサービスの起動

GUI アプリケーションに必要なD-Busを起動：

```bash
sudo service dbus start && export $(dbus-launch 2>/dev/null)
```

## 4. Unity エディターのインストール

コマンドラインから Unity 2022.3.22f1 をインストール：

```bash
unityhub --headless install --version 2022.3.22f1 --changeset b9e6963d9e67
```

# Google Chromeのインストール

Unity Hubのログインはブラウザを通じて行われるため、WSL2内にブラウザが必要です

```bash
wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | sudo apt-key add -
echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" | sudo tee /etc/apt/sources.list.d/google-chrome.list
sudo apt update
sudo apt install google-chrome-stable
```

## 5. バッチモードでの起動

WSL2環境ではGUIが不要な場合、バッチモードでUnityを起動できます：

```bash
# プロジェクトをバッチモードで起動
unity-editor -batchmode -nographics -projectPath /path/to/your/project
```

## 完了

これでWSL2環境でUnityが使用できるようになります。 