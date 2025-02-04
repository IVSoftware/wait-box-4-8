using app_with_login;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace click_anywhere_event_4_8
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _ = Handle;
            BeginInvoke(new Action(() => execSplashFlow()));
        }
        protected override void SetVisibleCore(bool value) =>
            base.SetVisibleCore(value && _initialized);

        bool _initialized = false;

        private void execSplashFlow()
        {
            using (var splash = new WaitBox())
            {
                splash.ShowDialog();
            }
            _initialized = true;
            WindowState = FormWindowState.Normal;
            Show();
        }
    }
}
