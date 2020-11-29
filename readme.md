# 【ImageTrimmingTool β】

画像をボンッと食わせて、同じ領域だけトリミングしてバンッと返すバッチ処理ツール。

## 用途

動画から切り出したキャプチャ画像から余計な左右幅を消したりとか。  
WinShotとかで溜めたキャプチャ画像から任意の部分を切り出したりとか。

## このあと

- コンソールアプリ用ライブラリ適用。
- borderオプションを作りたい。（ついで）
    - borderオプションは作ったが、JPEG直保存だと画質劣化問題が発生した。
    - しかたないのでデフォルトのトリミング機能はPNG保存に変更（ついでに別ツールもPNGに変更）。
    - 改めてJPEG変換するコンバート機能をオプションとして追加。