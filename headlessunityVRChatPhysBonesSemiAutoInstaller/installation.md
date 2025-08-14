# 1. Unity公開キーを追加
wget -qO - https://hub.unity3d.com/linux/keys/public | gpg --dearmor | sudo tee /usr/share/keyrings/Unity_Technologies_ApS.gpg > /dev/null

# 2. Unityリポジトリを追加（一行で実行）
sudo sh -c 'echo "deb [signed-by=/usr/share/keyrings/Unity_Technologies_ApS.gpg] https://hub.unity3d.com/linux/repos/deb stable main" > /etc/apt/sources.list.d/unityhub.list'

# 3. パッケージリストを更新
sudo apt update

# 4. Unity Hubをインストール
sudo apt-get install unityhub

# ALSAライブラリをインストール
sudo apt update
sudo apt install libasound2-dev libasound2

# GUI関連の追加パッケージ
sudo apt install libgtk-3-0 libgconf-2-4 libnss3 libxss1 libasound2 libxtst6 libatspi2.0-0 libdrm2 libxcomposite1 libxdamage1 libxrandr2 libgbm1 libxkbcommon0

# D-Busサービスを起動
sudo service dbus start

# D-Bus用の環境変数を設定
export DBUS_SESSION_BUS_ADDRESS="unix:path=/run/user/$(id -u)/bus"

# Unity 20223.22f1のインストール
unityhub --headless install --version 2022.3.22f1 --changeset b9e6963d9e67

# Unity Hubを起動
unityhub
