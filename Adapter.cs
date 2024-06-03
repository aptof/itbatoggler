namespace ItbaToggle;

public class Adapter
{
    public bool IsEnabled { get; set; }
    public bool IsConnected { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }

    public Adapter(string line)
    {
        List<string> props = line.Split("  ").ToList();
        props = props.Where(p => !String.IsNullOrEmpty(p)).ToList();
        if (props.Count != 4)
        {
            throw new ArgumentException("Invalid Argument Format");
        }
        IsEnabled = props[0].Trim() == "Enabled";
        IsConnected = props[1].Trim() == "Connected";
        Type = props[2].Trim();
        Name = props[3].Trim();
    }

    public void Toggle()
    {
        if (IsEnabled) Disable();
        else Enable();
    }

    public void Enable()
    {
        string command = "netsh interface set interface " + Name + " admin=enable";
        CommandRunner.Run(command);
    }

    public void Disable()
    {
        string command = "netsh interface set interface " + Name + " admin=disable";
        CommandRunner.Run(command);
    }
}