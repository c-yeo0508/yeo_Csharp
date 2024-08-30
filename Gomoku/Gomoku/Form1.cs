using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gomoku
{
    enum Color { none, black, white };

    public partial class Form1 : Form
    {
        int margin = 40;　//親メニューとの間隔
        int distance = 28;　//線と線の間の距離
        int stoneSize = 27; //碁石の大きさ
        int stoneCnt = 0; //碁石の順番をカウント
        

        List<Save> lstSave = new List<Save> ();
        bool saveFlag = false;
        bool showCountFlag = false;

        Graphics g;
        Pen pen;
        Brush wBrush, bBrush;
        Font font = new Font("Gothic", 10);

        public Form1()
        {
            InitializeComponent();

            this.Text = "Gomoku";
            this.BackColor = System.Drawing.Color.BlanchedAlmond;

            pen = new Pen(System.Drawing.Color.Black);
            bBrush = new SolidBrush(System.Drawing.Color.Black);
            wBrush = new SolidBrush(System.Drawing.Color.White);

            this.ClientSize = new Size(2 * margin + 18 * distance,
                2 * margin + 18 * distance + menuStrip1.Height);
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawBoard();
            DrawStone();
        }
        private void DrawBoard()
        {
            //panel1にGraphicsオブジェクト作成
            g = panel1.CreateGraphics();

            //縦19
            for (int i = 0; i < 19; i++)
            {
                g.DrawLine(pen, new Point(margin + i * distance, margin),
                    new Point(margin + i * distance, margin + 18 * distance));
            }

            //横19
            for (int i = 0; i < 19; i++)
            {
                g.DrawLine(pen, new Point(margin, margin + i * distance),
                    new Point(margin + 18 * distance, margin + i * distance));
            }
        }

        //２次元配列に保存されている碁石を呼び出す
        private void DrawStone()
        {
            string imagePath;

            for (int x = 0; x < 19; x++)
                for (int y = 0; y < 19; y++)
                    if (goban[x, y] == Color.black)
                    {
                        imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "kuro.png");
                        Bitmap bmp = new Bitmap(imagePath);
                        g.DrawImage(bmp, margin + x * distance - stoneSize / 2,
                            margin + y * distance - stoneSize / 2, stoneSize, stoneSize);

                        ShowCount(GetStoneCount(x,y), Brushes.White, new Rectangle(
                            margin + x * distance - (stoneSize / 2),
                            margin + y * distance - (stoneSize / 2),
                            stoneSize, stoneSize));
                    }
                    else if (goban[x, y] == Color.white)
                    {
                        imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "shiro.png");
                        Bitmap bmp = new Bitmap(imagePath);
                        g.DrawImage(bmp, margin + x * distance - stoneSize / 2,
                            margin + y * distance - stoneSize / 2, stoneSize, stoneSize);

                        ShowCount(GetStoneCount(x, y), Brushes.Black, new Rectangle(
                            margin + x * distance - (stoneSize / 2),
                            margin + y * distance - (stoneSize / 2),
                            stoneSize, stoneSize));
                    }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

            

            if (saveFlag == true)
            {
                saveGame();
                return;
            }

            //マウスクリックの座標を設定
            int x = (e.X - margin + distance / 2) / distance;
            int y = (e.Y - margin + distance / 2) / distance;


            //マウスのクリックで石を置く
            //if (goban[x, y] != Color.none)
            //    return;

            //石の重複を防止
            if (x < 0 || x >= 19 || y < 0 || y >= 19 || goban[x, y] != Color.none)
                return;

            Rectangle r = new Rectangle(
                margin + distance * x - (stoneSize / 2),
                margin + distance * y - (stoneSize / 2),
                stoneSize, stoneSize);

            string imagePath = AppDomain.CurrentDomain.BaseDirectory;
            stoneCnt++;
            // 黒が置かれた場合
            if (flag == false)
            {
                // 碁石の情報を生成
                var stone = new Save(x, y, Color.black, stoneCnt);
                imagePath = Path.Combine(imagePath, "goishi", "kuro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(stoneCnt, Brushes.White, r);
                lstSave.Add(stone);
                flag = true;
                goban[x, y] = stone.color;
            }
            // 白が置かれた場合
            else
            {
                var stone = new Save(x, y, Color.white, stoneCnt);
                imagePath = Path.Combine(imagePath,"goishi", "shiro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(stoneCnt, Brushes.Black, r);
                lstSave.Add(stone);
                flag = false;
                goban[x, y] = stone.color;
            }
            // 勝利判定
            checkWin(x, y);
        }
        private int GetStoneCount(int x, int y)
        {
            foreach (var item in lstSave)
            {
                if (item.X == x && item.Y == y)
                {
                    return item.seq;
                }
            }
            return 0;
        }

        //碁盤をリセット
        private void setGame()
        {
            //碁石の状態を初期化
            flag = false;
            goban = new Color[19, 19];
            //明日確認
            stoneCnt = 0;
            lstSave.Clear();

            //for (int x = 0; x < 19; x++)
            //    for (int y = 0; y < 19; y++)
            //goban[x, ] = Color.none;

            //碁盤を再描画
            this.Refresh();

            if (goban[0, 1] == Color.none)
            {
                //MessageBox.Show("noneです！");
            }
        }


        //ゲームのデータを保存する
        private void saveGame()
        {

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Post mortem");
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HHmm") + ".csv";
            string fileData = Path.Combine(folderPath, fileName);
            FileStream fileStream = new FileStream(fileData, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Default);


            //要素を追加
            foreach (Save item in lstSave)
            {
                streamWriter.WriteLine("{0}, {1}, {2}, {3}", item.X, item.Y, item.color, item.seq);
            }
            streamWriter.Close();
            fileStream.Close();
        }
        //メニューから新しいゲームを始める
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setGame();
        }


        //ゲーム終了処理
        private void WinGame(int x, int y)
        {
            saveGame();

            DialogResult end;

            if (goban[x,y] == Color.black)
            {
                end = MessageBox.Show("黒の勝ちです!\n新しいゲームを始めますか？","ゲーム終了",MessageBoxButtons.YesNo);
            }
            else
            {
                end = MessageBox.Show("白の勝ちです!\n新しいゲームを始めますか？","ゲーム終了",MessageBoxButtons.YesNo);
            }

            if (end == DialogResult.Yes)
                setGame();
            else if (end == DialogResult.No)
                this.Close();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void showCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showCountFlag = !showCountFlag;
            DrawStone();
        }

        private void ShowCount(int v, Brush color, Rectangle r)
        {
            if (showCountFlag == true)
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                g.DrawString(v.ToString(), font, color, r, stringFormat);
            }
        }

        private void checkWin(int x, int y)
        {

            int horizontalCnt = 1;
            int virticalCnt = 1;
            int leftDiagonalCnt = 1;
            int rightDiagonallyCnt = 1;

            //右方向
            for (int i = x + 1; i < 19; i++)
                if (goban[i, y] == goban[x, y])
                    horizontalCnt++;
                else
                    break;

            //左方向
            for (int i = x - 1; i >= 0; i--)
                if (goban[i, y] == goban[x, y])
                    horizontalCnt++;
                else
                    break;

            //if (horizontalCnt >= 5)
            //{
            //    WinGame(x, y);
            //    return;
            //}


            //上方向
            for (int j = y - 1; j >= 0; j--)
                if (goban[x, j] == goban[x, y])
                    virticalCnt++;
                else
                    break;

            //下方向
            for (int j = y + 1; j < 19; j++)
                if (goban[x, j] == goban[x, y])
                    virticalCnt++;
                else
                    break;

            //if (virticalCnt >= 5)
            //{
            //    WinGame(x, y);
            //    return;
            //}

            //左上方向
            for (int i = x - 1, j = y -1; i >= 0 && j >= 0; i-- , j--)
                    if (goban[i, j] == goban[x, y])
                    leftDiagonalCnt++;
                    else
                        break;

            //右下
            for (int i = x + 1, j = y + 1; i < 19 && j < 19; i++, j++)
                if (goban[i, j] == goban[x, y])
                    leftDiagonalCnt++;
                else
                    break;

            //if (leftDiagonalCnt >= 5)
            //{
            //    WinGame(x, y);
            //    return;
            //}

            //右上

            for (int i = x + 1, j = y - 1; i < 19 && j >= 0; i++, j--)
                if (goban[i, j] == goban[x, y])
                    rightDiagonallyCnt++;
                else
                    break;

            //左下

            for (int i = x - 1, j = y + 1; i >= 0 && j < 19; i--, j++)
                if (goban[i, j] == goban[x, y])
                    rightDiagonallyCnt++;
                else
                    break;

            // 5個並んだ場合は勝利と判定
            if (rightDiagonallyCnt >= 5 || leftDiagonalCnt >= 5 || virticalCnt >= 5 || horizontalCnt >= 5)
            {
                WinGame(x, y);
                return;
            }

        }
    }
}