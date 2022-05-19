# Cauldron DCG

**自作のカード** で遊べるデジタルカードゲームです。
自分でカードを作ってサーバーを建てると、カードゲームを運営できます。

![cauldron](https://user-images.githubusercontent.com/12682383/120923543-158b8e80-c70a-11eb-8926-dcc117fac5c8.png)

## カードの作成

[こちらのツール](https://iuemon83.github.io/Cauldron.Editor/cardset)でブラウザから**オリジナルカード**の作成ができます。

![cauldron-tool](https://user-images.githubusercontent.com/12682383/120923636-98144e00-c70a-11eb-8f35-7a02b66a550b.png)

## 動作環境

### クライアント
* Windows

### サーバー

* .NET6.0+

## インストール

### クライアント

ソフトを[ダウンロード](https://cauldron.iuemon.xyz/downloads/cauldron_client.zip)する。
ダウンロードしたzipファイルを解凍する。

## カードの画像
解凍したフォルダの`Cauldron_Data\CardImages` にファイル名をカード名にした画像ファイルを入れる。

例：
```
「ゴブリン」カードの画像には、Cauldron_Data\CardImages\ゴブリン.png が使用される。
```

## ゲームの始め方

### クライアント

解凍したフォルダにある`Cauldron.exe` を起動する。

#### サーバーに接続する

1. タイトル画面の`Server host` 入力欄に、接続するサーバーのホスト名とポート番号を入力する。

例：
```
cauldron.iuemon.xyz:49336
```

2. `Player name` 入力欄にプレイヤー名を入力して、「Start」ボタンをクリックする。

### サーバー

解凍したフォルダにある`Cauldron.Server.exe` を実行する


# 自分でカードを作成する

1. [こちら](https://iuemon83.github.io/Cauldron.Editor/cardset)のカード作成ツールで、カードを作成する。
1. 作成したカードセットファイルを、サーバーの`CardSet` ディレクトリに保存する。
