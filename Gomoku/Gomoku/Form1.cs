using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace Gomoku
{
    enum STONE { none, black, white };

    public partial class Form1 : Form
    {
        int margin = 40;　//親メニューとの間隔
        int distance = 28;　//線と線の間の距離
        int stoneSize = 27; //碁石の大きさ
        int stoneCnt = 1; //碁石の順番をカウント
        

        List<Save> lstSave = new List<Save> ();
        int sequence = 0;
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
            this.BackColor = Color.BlanchedAlmond;

            pen = new Pen(Color.Black);
            bBrush = new SolidBrush(Color.Black);
            wBrush = new SolidBrush(Color.White);

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
                    if (goban[x, y] == STONE.black)
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
                    else if (goban[x, y] == STONE.white)
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
                Console.Write(saveFlag);
                postMortem();
                return;
            }

            //マウスクリックの座標を設定
            int x = (e.X - margin + distance / 2) / distance;
            int y = (e.Y - margin + distance / 2) / distance;


            //マウスのクリックで石を置く
            //if (goban[x, y] != STONE.none)
            //    return;

            //石の重複を防止
            if (x < 0 || x >= 19 || y < 0 || y >= 19 || goban[x, y] != STONE.none)
                return;

            Rectangle r = new Rectangle(
                margin + distance * x - (stoneSize / 2),
                margin + distance * y - (stoneSize / 2),
                stoneSize, stoneSize);

            string imagePath;
            if (flag == false)
            {
                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "kuro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(stoneCnt, Brushes.White, r);
                lstSave.Add(new Save(x, y, STONE.black, stoneCnt++));
                flag = true;
                goban[x, y] = STONE.black;
            }
            else
            {
                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "shiro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(stoneCnt, Brushes.Black, r);
                lstSave.Add(new Save(x, y, STONE.white, stoneCnt++));
                flag = false;
                goban[x, y] = STONE.white;
            }
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
            goban = new STONE[19, 19];
            //明日確認
            stoneCnt = 1;
            lstSave.Clear();

            //for (int x = 0; x < 19; x++)
            //    for (int y = 0; y < 19; y++)
            //goban[x, ] = STONE.none;

            //碁盤を再描画
            this.Refresh();

            if (goban[0, 1] == STONE.none)
            {
                //MessageBox.Show("noneです！");
            }
        }


        //ゲームのデータを保存する
        private void saveGame()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string folderPath = Path.Combine(baseDirectory + "Post mortem");

            //フォルダーが存在しない場合
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".csv";
            string fileData = Path.Combine(folderPath, fileName);


            FileStream fileStream = new FileStream(fileData, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.Default);


            //要素を追加
            foreach (Save item in lstSave)
            {
                streamWriter.WriteLine("{0},{1},{2},{3}", item.X, item.Y, item.stone, item.seq);
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

            if (goban[x,y] == STONE.black)
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
            //stoneCnt++;
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

        private void Undo()
        {
            if (lstSave.Count == 0)
            {
                MessageBox.Show("石をおいてください。");
                return;
            }

            // 最近のSave要素を取得、リストから削除
            Save lastSave = lstSave.Last();
            lstSave.Remove(lastSave);

            // 碁盤リセット、初期化
            goban[lastSave.X, lastSave.Y] = STONE.none;
            this.Refresh();

            // 碁石のカウント確認
            stoneCnt = lastSave.seq;

            // プレイヤーの順番更新
            flag = !flag;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void postMortem()
        {
            Console.WriteLine("Sequence before drawing: " + sequence);
            Console.WriteLine("lstSave count before drawing: " + lstSave.Count);

            if (sequence < lstSave.Count)
            {
                Console.WriteLine("Drawing stone with sequence: " + sequence);
                drawAStone(lstSave[sequence++]);
            }
            else
            {
                Console.WriteLine("No more stones to draw. Sequence: " + sequence + ", Count: " + lstSave.Count);
            }
        }

        private void drawAStone(Save item)
        {
            int x = item.X;
            int y = item.Y;
            STONE s = item.stone;
            int seq = item.seq;

            Rectangle r = new Rectangle(
                margin + distance * x - stoneSize / 2,
                margin + distance * y - stoneSize / 2,
                stoneSize, stoneSize);

            string imagePath;
            if (s == STONE.black)
            {
                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "kuro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(seq, Brushes.White, r);
                goban[x, y] = STONE.black;
            }
            else
            {
                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goishi", "shiro.png");
                Bitmap bmp = new Bitmap(imagePath);
                g.DrawImage(bmp, r);
                ShowCount(seq, Brushes.Black, r);
                goban[x, y] = STONE.white;
            }
            checkWin(x, y);
        }

        private void postMortemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setGame();
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Post mortem");
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.InitialDirectory = folderPath;
            openfile.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            openfile.FilterIndex = 1;
            openfile.RestoreDirectory = true;

            if (openfile.ShowDialog() == DialogResult.OK)
            {
                string fileName = openfile.FileName;

                try
                {
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        string line = "";
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] items = line.Split(',');

                            Console.WriteLine("x: " + items[0]);
                            Console.WriteLine("y: " + items[1]);
                            Console.WriteLine("stone: " + items[2]);
                            Console.WriteLine("seq: " + items[3]);
                            Save sav = new Save(int.Parse(items[0]), int.Parse(items[1]),
                                items[2] == "black" ? STONE.black : STONE.white, int.Parse(items[3]));
                            lstSave.Add(sav);
                        }
                        sr.Close();
                    }
                    Console.WriteLine("lstSave count: " + lstSave.Count);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                saveFlag = true;
                sequence = 0;
                Console.WriteLine("Sequence after reset: " + sequence);
            }
        }

        private void checkWin(int x, int y)
        {

            int cnt = 1;

            //右方向
            for (int i = x + 1; i < 19; i++)
                if (goban[i, y] == goban[x, y])
                    cnt++;
                else
                    break;

            //左方向
            for (int i = x - 1; i >= 0; i--)
                if (goban[i, y] == goban[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                WinGame(x, y);
                return;
            }

            cnt = 1;

            //上方向
            for (int j = y - 1; j >= 0; j--)
                if (goban[x, j] == goban[x, y])
                    cnt++;
                else
                    break;

            //下方向
            for (int j = y + 1; j < 19; j++)
                if (goban[x, j] == goban[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                WinGame(x, y);
                return;
            }

            cnt = 1;

            //左上方向
            for (int i = x - 1, j = y -1; i >= 0 && j >= 0; i-- , j--)
                    if (goban[i, j] == goban[x, y])
                        cnt++;
                    else
                        break;

            //右下
            for (int i = x + 1, j = y + 1; i < 19 && j < 19; i++, j++)
                if (goban[i, j] == goban[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                WinGame(x, y);
                return;
            }


            cnt = 1;

            //右上

            for (int i = x + 1, j = y - 1; i < 19 && j >= 0; i++, j--)
                if (goban[i, j] == goban[x, y])
                    cnt++;
                else
                    break;

            //左下

            for (int i = x - 1, j = y + 1; i >= 0 && j < 19; i--, j++)
                if (goban[i, j] == goban[x, y])
                    cnt++;
                else
                    break;

            if (cnt >= 5)
            {
                WinGame(x, y);
                return;
            }

        }
    }
}