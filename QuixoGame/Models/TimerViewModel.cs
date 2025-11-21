namespace Server.Models;

public class TimerViewModel
{
    public string Id { get; set; } = "timer";
    public int InitialSeconds { get; set; }
    public bool AutoStart { get; set; }
    public string JsVariableName { get; set; } = "timer";
    public string CssClass { get; set; } = "";
}


