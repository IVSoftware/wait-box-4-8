using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class WaitBox : Form
    {
        public WaitBox()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MarqueeAnimationSpeed = 50; 
        }
        protected async override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                labelProgress.Text = "Connecting to service...";
                // Includes some cosmetic delay for demo purposes
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    labelProgress.Text = await GetCatFactAsync();
                }
                DialogResult = DialogResult.OK;
            }
        }
        HttpClient _httpClient = new HttpClient();
        private string _nextPageUrl = "https://catfact.ninja/facts?limit=1";

        private  async Task<string> GetCatFactAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_nextPageUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonData = await response.Content.ReadAsStringAsync();
                    var catFacts = JsonConvert.DeserializeObject<ResponseParser>(jsonData);
                    if (catFacts?.Data != null && catFacts.Data.Count > 0)
                    {
                        _nextPageUrl = $"{catFacts.Next_Page_Url}&limit=1";
                        ResponseReceived?.Invoke(this, catFacts.Data[0]);
                        return catFacts.Data[0].Fact;
                    }
                }
            }
            catch (Exception ex) {  Debug.WriteLine($"Error: {ex.Message}"); }
            return null;
        }
        public event EventHandler<ReceivedHttpResponseEventArgs> ResponseReceived;
    }
    class ResponseParser
    {
        [JsonProperty("data")]
        public List<ReceivedHttpResponseEventArgs> Data { get; set; }

        [JsonProperty("next_page_url")]
        public string Next_Page_Url { get; set; }
    }
    public class ReceivedHttpResponseEventArgs : EventArgs
    {
        [JsonProperty("fact")]
        public string Fact { get; set; }
    }
}
