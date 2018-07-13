using Segmentation.Algorithms.Images;
using System;
using System.Threading;
using System.Windows.Forms;
using Segmentation.Algorithms.Screenshot;
using System.Runtime.InteropServices;
using botCore;
using System.IO;



namespace R6Bot
{

    public partial class SegmentsForm : Form
    {
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        public interface IWindowItem
        {
            bool UpdateEnabled { get; }
            void Update(BImage screenshot);
        }

        private ScreenshotSource _screenshotSource;
        public BImage Screenshot => _screenshotSource.Screenshot;
        public SubImageFinder SubFinder => new SubImageFinder(Screenshot);
        private int gameStarted, gameEnded, gameExited, operators, timeLimit;
        [DllImport("User32.dll", SetLastError = true)]
        public static extern int GetForegroundWindow();
        BImage screenshot;
        Thread tr, tr2;


        public class Dll
        {
            static Dll()
            {
                ExtractResourceToFile("R6Bot.botCore.dll", "botCore.dll");
                importDll("botCore.dll");

            }
            public static void ExtractResourceToFile(string resourceName, string filename)
            {

                if (!System.IO.File.Exists(filename))
                    using (System.IO.Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    using (System.IO.FileStream fs = new System.IO.FileStream(
                        Path.Combine(filename), System.IO.FileMode.Create))
                    {
                        byte[] b = new byte[s.Length];
                        s.Read(b, 0, b.Length);
                        fs.Write(b, 0, b.Length);
                    }
            }
            public static void importDll(string filename)
            {
                IntPtr h = LoadLibrary(filename);
                if (h == IntPtr.Zero)
                {
                    Exception e = new Exception();
                    throw new DllNotFoundException("Unable to load library", e);
                }
            }
            public static void deleteDll(string filename)
            {
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
                   
            }

        }


        public SegmentsForm()
        {
            new Dll();
            InitializeComponent();
            gameStarted = gameEnded = gameExited = 0;
            comboBox1.SelectedIndex = 2;
            comboBox2.SelectedIndex = 2;
            tr = new Thread(working);
            tr2 = new Thread(isF12);
            tr2.Start();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                tr.Start();
            else
            {
                tr.Abort();
                tr = new Thread(working);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            operators = comboBox1.SelectedIndex;
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            timeLimit = 3 * (comboBox2.SelectedIndex + 2);
        }
        private void SegmentsForm_Load(object sender, EventArgs e)
        {

        }
        private void SegmentsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(-1);
            Dll.deleteDll("botCore.dll");
        }

        void working()
        {
            System.Threading.Thread.Sleep(1000);
            var hWnd = (IntPtr)GetForegroundWindow();
            var bot = new Bot();

            while (true)
            {
                var lobbyChecker = 0;
                var checkLobbyRepeat = 0;
                var timeLimitCurr = timeLimit;
                var tryLimit = 5;
                while (!isInQueue(hWnd))
                {
                    bot.findGame();
                    tryLimit--;
                    System.Threading.Thread.Sleep(2000);
                    if (tryLimit == 0)
                    {
                        timeLimitCurr = 0;
                        break;
                    }
                }
                while (true)
                {
                    System.Threading.Thread.Sleep(20000);
                    if (isInMenuAFK(hWnd)) break;
                    if (!isInLobby(hWnd))
                    {
                        bot.click();  //antiAFK
                        lobbyChecker++;
                        checkLobbyRepeat = 0;
                    }
                    else
                    {
                        lobbyChecker = 0;
                        checkLobbyRepeat++;
                        if (checkLobbyRepeat < 2)
                        {
                            var randomInt = 0;
                            Random rand = new Random();
                            while (isInLobby(hWnd))
                            {
                                switch (operators)
                                {
                                    case 1:
                                        randomInt = rand.Next(1, 11);
                                        break;
                                    case 2:
                                        randomInt = rand.Next(1, 19);
                                        break;
                                }
                                bot.pickOperator(randomInt);
                            }
                            gameStarted++;
                            updateInfo();
                        }
                        else checkLobbyRepeat = 0;
                    }
                    if (lobbyChecker > timeLimitCurr)
                    {
                        if (isInMenu(hWnd) || isInQueue(hWnd))
                        {
                            bot.closeKickMessage();
                            bot.leaveQueue();
                            System.Threading.Thread.Sleep(3000);
                        }
                        else
                        {
                            bot.pressEsc();
                            System.Threading.Thread.Sleep(2000);
                            if (isInOperations(hWnd))
                                 bot.leftGameOperations();
                            else bot.leftGame();
                            gameExited++;
                            updateInfo();
                            System.Threading.Thread.Sleep(8000);
                        }
                        lobbyChecker = 0;
                        break;
                    }
                }
            }
        }

        void isF12()
        {
            while (true)
                if (!Bot.isRunning())
                {
                    checkBox1.Invoke((MethodInvoker)delegate {
                        checkBox1.Checked = !checkBox1.Checked;
                    });
                    System.Threading.Thread.Sleep(200);
                }

        }
        void updateInfo()
        {
            gameEnded = (gameStarted - gameExited > 0) ? gameStarted - gameExited : 0;

            label4.Invoke((MethodInvoker)delegate
            { label4.Text = gameStarted.ToString(); });

            label6.Invoke((MethodInvoker)delegate {
                label6.Text = gameExited.ToString();
            });
            label8.Invoke((MethodInvoker)delegate {
                label8.Text = gameEnded.ToString();
            });

            label10.Invoke((MethodInvoker)delegate
            { label10.Text = (gameEnded * 90).ToString(); }); //1 game ~90 points
            System.Threading.Thread.Sleep(8000);
        }

        bool isInLobby(IntPtr hWnd)
        {
            _screenshotSource = new ScreenshotSource(hWnd);
            screenshot = _screenshotSource.Screenshot;
            var color1 = screenshot.Get(Pixel.Create(278, 162));
            var color2 = screenshot.Get(Pixel.Create(278, 228));
            var color3 = screenshot.Get(Pixel.Create(285, 500));
            _screenshotSource.Dispose();
            return (color1 == color2 && color1 == color3)
                ? true : false;
        }
        bool isInQueue(IntPtr hWnd)
        {
            _screenshotSource = new ScreenshotSource(hWnd);
            screenshot = Screenshot;
            var color1 = screenshot.Get(Pixel.Create(455, 960));
            var color2 = screenshot.Get(Pixel.Create(615, 960));
            var color3 = screenshot.Get(Pixel.Create(500, 960));
            _screenshotSource.Dispose();
            return (color1 == color2 && color1 == color3)
                ? true : false;
        }
        bool isInMenu(IntPtr hWnd)
        {
            _screenshotSource = new ScreenshotSource(hWnd);
            screenshot = Screenshot;
            var color4 = screenshot.Get(Pixel.Create(123, 166));
            var color5 = screenshot.Get(Pixel.Create(123, 371));
            _screenshotSource.Dispose();
            return (color4 == color5)
                ? true : false;
        }
        bool isInMenuAFK(IntPtr hWnd)
        {
            _screenshotSource = new ScreenshotSource(hWnd);
            screenshot = Screenshot;
            var color4 = screenshot.Get(Pixel.Create(500, 560));
            var color5 = screenshot.Get(Pixel.Create(500, 600));
            _screenshotSource.Dispose();
            return (color4 == color5 && color4.R == 251)
                ? true : false;
        }
        bool isInOperations(IntPtr hWnd)
        {
            _screenshotSource = new ScreenshotSource(hWnd);
            screenshot = Screenshot;
            var color4 = screenshot.Get(Pixel.Create(540, 1500));
            var color5 = screenshot.Get(Pixel.Create(420, 1500));
            _screenshotSource.Dispose();
            return (color4 == color5)
                ? true : false;
        }
        
    }
        
}
