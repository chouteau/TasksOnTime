namespace DistributedTasksOnTime.BlazorComponent;

public partial class ConfirmDialog : ComponentBase
{
    [Parameter]
    public string Title { get; set; }

    [Parameter]
    public EventCallback<object> Accept { get; set; }

    public object Tag { get; set; }

    string body;
    string modalDisplay = "none;";
    string modalClass = "";

    public void ShowDialog(string message)
    {
        modalDisplay = "block;";
        modalClass = "show";
        body = message;
        StateHasChanged();
    }

    async Task Confirm(bool confirm)
    {
        modalDisplay = "none";
        modalClass = "";
        if (confirm)
        {
            await Accept.InvokeAsync(Tag);
        }
    }
}
