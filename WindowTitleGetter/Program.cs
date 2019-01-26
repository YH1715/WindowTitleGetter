using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;


namespace WindowTitleGetter
{
    class Program
    {
        //Windowtitel取得用
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        static Encoding sjisEnc = Encoding.GetEncoding("Shift_JIS");

        //OpenDestktop使用用
        [DllImport("user32", EntryPoint = "OpenDesktopA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern Int32 OpenDesktop(string lpszDesktop, Int32 dwFlags, bool fInherit, Int32 dwDesiredAccess);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern Int32 CloseDesktop(Int32 hDesktop);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern Int32 SwitchDesktop(Int32 hDesktop);

        //Win32APIを使用した画面キャプチャ用
        private const int SRCCOPY = 13369376;
        private const int CAPTUREBLT = 1073741824;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hDestDC,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hSrcDC,
            int xSrc,
            int ySrc,
            int dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);


        static void Main(string[] args)
        {
            GetWindowTitle();
        }

        static void GetWindowTitle()
        {
            //OpenDestktop使用による、ロック状態の判別　※画面ロックされているとGetScreenShot()が失敗するので、それを回避
            const int DESKTOP_SWITCHDESKTOP = 256;
            int hwnd = -1;
            int rtn = -1;
            hwnd = OpenDesktop("Default", 0, false, DESKTOP_SWITCHDESKTOP);

            bool end = false;
            while (!end)
            {

                if (hwnd != 0)
                {
                    rtn = SwitchDesktop(hwnd);
                    if (rtn == 0)
                    {
                        // Locked
                        WindowTitleWriter();
                        

                    }
                    else
                    {
                        // Not locked
                        WindowTitleWriter();
                        GetScreenShot2();
                    }
                }

                Thread.Sleep(600000);

            }

            

            

            //Console.WriteLine("何かキーを押すと開始します");
            // Console.ReadLine();
            Console.WriteLine("終了するには何かキーを押して下さい");
            Console.ReadLine();


        }


        /// <summary>
        /// プライマリスクリーンのタイトル文字列を取得する
        /// </summary>
        public static void WindowTitleWriter()
        {
            DateTime dt = System.DateTime.Now;

            using (StreamWriter writer = new StreamWriter(dt.ToString("yyyyMMdd") + ".txt", true, sjisEnc))
            {

                StringBuilder sb = new StringBuilder(1000);  // 1000は取得するテキストの最大文字数
                GetWindowText(GetForegroundWindow(), sb, 1000);
                Console.Write(DateTime.Now + "  ");
                Console.WriteLine(sb);
                String outputText = DateTime.Now + "  " + sb;
                writer.WriteLine(outputText);
            }

        }

        //public static void GetScreenShot()
        //{
        //    //Bitmapの作成
        //    Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
        //        Screen.PrimaryScreen.Bounds.Height);
        //    //Console.WriteLine("Width" + Screen.PrimaryScreen.Bounds.Width + ", Height" + Screen.PrimaryScreen.Bounds.Height);
        //    //Graphicsの作成
        //    Graphics g = Graphics.FromImage(bmp);
        //    //画面全体をコピーする
        //    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp.Size);
        //    //解放
        //    g.Dispose();

        //    //スクリーンショットファイルを保存
        //    DateTime dt = System.DateTime.Now;
        //    DirectoryUtils.SafeCreateDirectory(Directory.GetCurrentDirectory() + "/" + dt.ToString("yyyyMMdd"));
        //    bmp.Save(@"./" + dt.ToString("yyyyMMdd") + "/" + dt.ToString("yyyyMMddhhmm") + ".jpg", ImageFormat.Jpeg);
        //}

        /// <summary>
        /// 画面をキャプチャし保存する
        /// </summary>
        public static void GetScreenShot2()
        {
            //プライマリモニタのデバイスコンテキストを取得
            IntPtr disDC = GetDC(IntPtr.Zero);
            //Bitmapの作成
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
            //Graphicsの作成
            Graphics g = Graphics.FromImage(bmp);
            //Graphicsのデバイスコンテキストを取得
            IntPtr hDC = g.GetHdc();
            //Bitmapに画像をコピーする
            BitBlt(hDC, 0, 0, bmp.Width, bmp.Height,
                disDC, 0, 0, SRCCOPY);
            //解放
            g.ReleaseHdc(hDC);
            g.Dispose();
            ReleaseDC(IntPtr.Zero, disDC);

            //スクリーンショットファイルを保存
            DateTime dt = System.DateTime.Now;
            DirectoryUtils.SafeCreateDirectory(Directory.GetCurrentDirectory() + "/" + dt.ToString("yyyyMMdd"));
            bmp.Save(@"./" + dt.ToString("yyyyMMdd") + "/" + dt.ToString("yyyyMMddhhmm") + ".jpg", ImageFormat.Jpeg);
        }

    }

    
}
