using app_with_login;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace wait_box_4_8
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _ = Handle;
            BeginInvoke(new Action(() => ConnectToService()));
            // Setup the DataGridView
            Load += (sender, e) => dataGridView.DataSource = Responses;
        }
        protected override void SetVisibleCore(bool value) =>
            base.SetVisibleCore(value && _initialized);
        bool _initialized = false;

        IList Responses { get; } = new BindingList<ReceivedHttpResponseEventArgs>();
        private void ConnectToService()
        {
            using (var waitBox = new WaitBox())
            {
                waitBox.ResponseReceived += (sender, e) => Responses.Add(e);
                waitBox.ShowDialog();
            }
            _initialized = true;
            Show();
        }
    }
}
