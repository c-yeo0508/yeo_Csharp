# yeo_Csharp
C#で作りたいものを作ってみる

2024 08 22

フォームの大きさ調整によって碁石が表示されない問題解決  
new game機能追加  
  
2024 08 26  
  
checkWinメソッド追加  
WinGameメソッド追加（バグあり）

2024 09 03  

感想戦の機能を追加しようとしているがデバッグ実行中

夕会後  
先話した感想戦モードで碁石１個置くと勝利判定が表示されるバグ修正しました  

感想戦モードで置かれる碁石の色が全部白になってることと感想戦モードの勝利判定のダイアログにはcsvファイルが作成されないようにすることやダイアログ閉じた後に碁盤の反応がなくなる問題を明日修正

2024 09 04    
09：26  
感想戦モードで碁石がすべて白に表示される問題解決(381行目の "black"　→　" black"に修正）  
09:48  
感想戦モードが終わった場合もゲームがCSVファイルとして保存される問題修正  （352行目にif文を追加、現在の処理数とリストに保存されている処理数（碁石の番号）が一致したら感想戦を終了させる）  
感想戦モードが終わり、新しいゲームを始めた際に碁盤の反応がない問題修正（182行目にsaveFlag = false;で感想戦モードから抜ける）  
  
10:07  
csvファイルになんでblackとwhiteの前にスペース入ってんのかな思いながら原因探した結果17行目の列挙型の宣言が  
    enum STONE { none, black, white };  
になっていることを発見、スペース削除して試してみたら元のコードでもよく動けるようになった（修正前に作成されたcsvファイルを除く）  
  
連珠ルールの禁じ手である長連を追加  

2024 09 05  
禁じ手三々の実装…直線のみ対応  
  
2024 09 19  
十字パターンの三々で横に白石おいているときに、三々になってしまうバグ（縦はOK）  
  
2024 09 20  
十字パターンの三々対応済み、配列の順番修正