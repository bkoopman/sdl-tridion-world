using System;
using System.Drawing;
using System.Web.Configuration;
using System.Windows.Forms;

namespace LocalStsService
{
    public class SysTrayApp : Form
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.Run(new SysTrayApp());
        }

        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenu _trayMenu;

        public SysTrayApp()
        {
            // Create a simple tray menu with two items.
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("Info", OnInfo);
            _trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon and add context menu. 
            _trayIcon = new NotifyIcon
                {
                    Text = @"Local Developer STS",
                    Icon = new Icon(Properties.Resources.Lock, 22, 22),
                    ContextMenu = _trayMenu,
                    Visible = true
                };

            Console.WriteLine(@"Start Service class");
            new StsService();
        }

        private static void OnInfo(object sender, EventArgs e)
        {
            string frmstring = string.Format("Local Developer STS is running on port: {0} ", WebConfigurationManager.AppSettings["IssuerName"]);
            MessageBox.Show(frmstring, @" Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private static void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
