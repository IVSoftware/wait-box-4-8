The key to making this scheme behave is to make sure `MainForm.Handle` is the _first_ window created. Ordinarily, this happens when the form is shown. But in this case, we show the `WaitBox` (and its asynchronous `ProgressBar`) first. Here's how to pull this off:

1. Force the main for window creation using `_ = Handle;`
2. Override `SetVisibleCore` and prevent `MainForm` from becoming visible until we're ready for it.
3. Using `BeginInvoke`, post a message at the tail of the message queue to show the wait box.

___

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
    }
}
___
**WaitBox Example**

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
            labelProgress.Text = "Updating installation...";
            progressBar.Value = 5;
            await Task.Delay(1000);
            labelProgress.Text = "Loading avatars...";
            progressBar.Value = 25;
            await Task.Delay(1000);
            labelProgress.Text = "Fetching game history...";
            progressBar.Value = 50;
            await Task.Delay(1000);
            labelProgress.Text = "Initializing scene...";
            progressBar.Value = 75;
            await Task.Delay(1000);
            labelProgress.Text = "Success!";
            progressBar.Value = 100;
            await Task.Delay(1000);
            DialogResult= DialogResult.OK;
        }
    }
}
```