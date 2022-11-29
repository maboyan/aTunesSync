# aTunesSync

## 概要

iPhoneからPixelに買い替えたはいいもののiTunesのデータをどうするか考えていなかったため作ったツール

## 使用前提

* iTunesで管理している楽曲が１つのフォルダにまとまっている  
* iTunes Library.xmlが出力させる設定になっている（後述）

## できること

* Windows上にある特定ディレクトリとPixel上の音楽ファイルをミラーリングします（親はWindows
* iTunesのプレイリストに基づいてm3u8ファイルを作成します

## iTunes Library.xml

iTunesのメニューで「編集」=>「環境設定」=>「詳細」を選択。  
iTunesライブラリXMLを他のアプリケーションと共有  
のチェックを「オン」にすることで出力されます。