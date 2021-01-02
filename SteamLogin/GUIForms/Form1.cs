using Newtonsoft.Json;
using SteamLogin.Classes;
using SuperSocket.SocketBase.Config;
using SuperWebSocket;
using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SteamLogin.Classes.WMI;

namespace SteamLogin
{
    public partial class Form1 : Form
    {
        private readonly string steamInstallPath = @"C:\Program Files (x86)\Steam\steam.exe";
        private readonly int listenPort = 2326;
        private readonly WMIManager wmiManager = new WMIManager();

        // Property for handling monitor count, when set updates smart plugs
        // this will only work for turning off the screens
        private int monitorCount;
        private int MonitorCount
        {
            get { return monitorCount; }
            set
            {
                monitorCount = Screen.AllScreens.Length;
                HandleMonitorCount();
            }
        }

        private readonly HWID hwid = new HWID();
        private readonly Cryptography crypt = new Cryptography();

        private static string key = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        /*      Form Load Event
         *      New instance of WebSocketServer created
         *      Listens on port 'listenPort'
         */
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!CheckSteamInstallPathExists()) throw new Exception("Steam file does not exist");
            key = hwid.GetUniqueID();

            var serverConfig = new ServerConfig()
            {
                Ip = "any",
                Port = listenPort,

            };

            WebSocketServer listener = new WebSocketServer();
            listener.Setup(new RootConfig(), serverConfig);
            listener.NewMessageReceived += Listener_NewMessageReceived;
            listener.Start();

            wmiManager.GetVirtualMachines();
        }

        // checks if steam actually is installed.
        // error thrown if false
        private bool CheckSteamInstallPathExists()
        {
            if (File.Exists(this.steamInstallPath)) return true;

            return false;
        }

        // Sends uPnP packet to smart plugs to physically turn off displays so no annoying noise is made
        private void HandleMonitorCount()
        {
            // do nothing (yet)
        }

        // event handler to receive login attempts
        private void Listener_NewMessageReceived(WebSocketSession session, string value)
        {
            MonitorCount = Screen.AllScreens.Length;
            PerformLogin(value);
        }

        // void 'PerformLogin' parses 'passwordDecoded' and passes it as argument to 'steamInstallPath' EXE
        private void PerformLogin(string loginDetails)
        {
            dynamic accountDetails;

            accountDetails = JsonConvert.DeserializeObject(loginDetails);

            var notification = new NotifyIcon()
            {
                Visible = true,
                Icon = SystemIcons.Information,
                BalloonTipTitle = "</SteamLogin>",
                BalloonTipText = $"Logging in as {accountDetails.username}",
            };

            notification.ShowBalloonTip(5000);
            RunProcess("taskkill.exe", "/im steam.exe -f");
            RunProcess(steamInstallPath, $"-login {accountDetails.username} {crypt.Decrypt(accountDetails.password.Value, key)}");
            Thread.Sleep(5000);
            notification.Dispose();
        }

        // starts process with arguments
        private void RunProcess(string process, string args)
        {
            ProcessStartInfo CMD = new ProcessStartInfo(process)
            {
                Arguments = args,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(CMD);
            Thread.Sleep(500);
        }

        // All the below just simply closes the application
        #region EXIT_APPLICATION
        private void QuitApplication()
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            QuitApplication();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QuitApplication();
        }
        #endregion

        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("http://adamdev/steam/");
        }

        // open screen manager
        private void ScreensToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScreenManager screenManager = new ScreenManager();
            screenManager.Show();
        }

        // List all instances in a message box
        private void ListMachinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string machinesFound = $"Virtual Machine Count: {this.wmiManager.instances.Count}\n";
            foreach (Instance instance in this.wmiManager.instances)
            {
                machinesFound += $"\n{instance.UUID}\t({instance.ElementName})";
            }

            Debug.Write(machinesFound);
            MessageBox.Show(machinesFound, $"Virtual Machine Count: {this.wmiManager.instances.Count}");
        }
    }
}