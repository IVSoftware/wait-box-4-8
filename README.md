The key to making this scheme behave is to make sure `MainForm.Handle` is the _first_ window created. Ordinarily, the `Handle` is created when the form is shown. But in this case, we want to show the `WaitBox` (and its asynchronous `ProgressBar`) first. Here's one way to make this work:

1. Force the main for window creation using `_ = Handle;`
2. Override `SetVisibleCore` and prevent `MainForm` from becoming visible until we're ready for it.
3. Using `BeginInvoke`, post a message at the tail of the message queue to show the wait box.

___
```
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
```
___
**WaitBox Minimal Example**

This demo uses the https://catfact.ninja API as a stand-in for _"**an async method that connects to a service and retrieves some data**"_. The received "facts" are used to populate the data source of a `DataGridView`.

[![wait box and main view][1]][1]

___

```
public partial class WaitBox : Form
{
    public WaitBox()
    {
        InitializeComponent();
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
    }
    protected async override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            labelProgress.Text = "Connecting to service...";
            progressBar.Value = 5;

            // Includes some cosmetic delay for demo purposes
            for (int i = 0; i < 10; i++)
            {
                labelProgress.Text = await GetCatFactAsync();
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                progressBar.Value = Math.Min(100, progressBar.Value + 5);
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                progressBar.Value = Math.Min(100, progressBar.Value + 5);
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
```


  [1]: https://i.sstatic.net/bmvo9olU.png