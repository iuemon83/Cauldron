# Cauldron DCG

**自作のカード** で遊べるデジタルカードゲームです。

![cauldron](https://user-images.githubusercontent.com/12682383/120923543-158b8e80-c70a-11eb-8926-dcc117fac5c8.png)

## カードの作成

ブラウザからオリジナルカードの作成ができます。

![cauldron-tool](https://user-images.githubusercontent.com/12682383/120923636-98144e00-c70a-11eb-8f35-7a02b66a550b.png)


# Cauldron.Web.Server

```sh
cd Cauldron.Web.Server
dotnet publish -c Release -o release
docker build . -t cauldronweb_server
docker run --rm -p 5000:80 --name cauldronweb_server cauldronweb_server
```
