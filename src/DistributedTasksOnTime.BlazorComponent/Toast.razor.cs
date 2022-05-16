namespace DistributedTasksOnTime.BlazorComponent;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public partial class Toast : ComponentBase
{
    [Parameter]
    public int DurationInSecond { get; set; } = 5;

    System.Timers.Timer _countdown;

    string Heading { get; set; }
    MarkupString Message { get; set; }
    bool IsVisible { get; set; }
    string BackgroundCssClass { get; set; }
    string IconCssClass { get; set; }

    public void Show(string message, ToastLevel level)
    {
        if (level != ToastLevel.Error)
		{
            StartCountdown();
        }
        BuildToastSettings(level, message);
        IsVisible = true;
        StateHasChanged();
    }


    void StartCountdown()
    {
        SetCountdown();

        if (_countdown.Enabled)
        {
            _countdown.Stop();
        }
        _countdown.Start();
    }

    void SetCountdown()
    {
        if (_countdown == null)
        {
            _countdown = new System.Timers.Timer(DurationInSecond * 1000);
            _countdown.Elapsed += HideToast;
            _countdown.AutoReset = false;
        }
    }

    async void HideToast(object source, System.Timers.ElapsedEventArgs args)
    {
        IsVisible = false;
        await InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

	void CloseToast()
	{
        IsVisible = false;
        StateHasChanged();
    }

    void BuildToastSettings(ToastLevel level, string message)
    {
        switch (level)
        {
            case ToastLevel.Info:
                BackgroundCssClass = $"bg-info";
                IconCssClass = "info";
                Heading = "Info";
                break;
            case ToastLevel.Success:
                BackgroundCssClass = $"bg-success";
                IconCssClass = "check";
                Heading = "Succès";
                break;
            case ToastLevel.Warning:
                BackgroundCssClass = $"bg-warning";
                IconCssClass = "exclamation";
                Heading = "Attention";
                break;
            case ToastLevel.Error:
                BackgroundCssClass = "bg-danger";
                IconCssClass = "times";
                Heading = "Erreur";
                break;
        }

		Message = new MarkupString(message.Replace("\r\n", "<br/>"));
	}

    public void Dispose()
    {
        _countdown?.Dispose();
    }
}
