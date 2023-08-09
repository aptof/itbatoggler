using Spectre.Console;
using System.Diagnostics;
using System.Security.Principal;
using System.Text.Json;

namespace ItbaToggle;

public class Toggler
{
    private readonly string _settingFile = "settings.txt";
    private readonly List<Adapter> _adapters = new();
    private Adapter? _itbaAdapter;
    private Adapter? _internetAdapter;

    public void Run()
    {
        IsAdministrator();
        PopulateAdapters();
        Intro();
        ReadSettings();
        ShowStatusOfAdapters();
        Toggle();
    }

    private void IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
        {
            throw new Exception("Please run as administrator.");
        }
    }

    private void PopulateAdapters()
    {
        string command = "netsh interface show interface";
        string result = CommandRunner.Run(command);
        List<string> resultLines = result.Split('\n').ToList();
        resultLines = resultLines.Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
        resultLines.RemoveRange(0, 2);
        foreach (string line in resultLines)
        {
            _adapters.Add(new Adapter(line));
        }
    }

    private void Intro()
    {
        AnsiConsole.Write(new FigletText("ITBA Toggler").Color(Color.Red));
        AnsiConsole.MarkupLine("[green]by Tushar Bakshi[/]");
        Console.WriteLine();
    }

    private void ReadSettings()
    {
        if (!File.Exists(_settingFile))
        {
            using (var fs = File.Create(_settingFile)) { }
            ChooseAdapters();
        }
        else
        {
            var lines = File.ReadAllLines(_settingFile);
            if (lines.Length != 2)
            {
                ChooseAdapters();
            }
            else
            {
                _itbaAdapter = _adapters.Find(_ => _.Name == lines[0]);
                _internetAdapter = _adapters.Find(_ => _.Name == lines[1]);
                if (_itbaAdapter == null || _internetAdapter == null)
                {
                    ChooseAdapters();
                }
            }
        }
    }

    private void ChooseAdapters()
    {
        List<string> choices = _adapters.Select(adapter => adapter.Name).ToList();
        choices.Sort();
        var itbaName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Please select [blue]ITBA Adapter[/] Name:\n[grey](Press 'up' and 'down' arrow to move and 'Enter' to select.)[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more adapters)[/]")
            .AddChoices(choices.ToArray())
        );
        _itbaAdapter = _adapters.Find(_ => _.Name == itbaName);
        AnsiConsole.MarkupLine($"[blue]ITBA Adapter[/]: [bold green]{itbaName}[/]");

        choices.Remove(itbaName);

        var internetName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Please select [blue]Internet Adapter[/] Name:\n[grey](Press 'up' and 'down' arrow to move and 'Enter' to select.)[/]")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more adapters)[/]")
            .AddChoices(choices.ToArray())
        );
        _internetAdapter = _adapters.Find(_ => _.Name == internetName);
        AnsiConsole.MarkupLine($"[blue]Internet Adapter[/]: [bold green]{internetName}[/]");

        var lines = new string[] { _itbaAdapter!.Name, _internetAdapter!.Name };
        File.WriteAllLines(_settingFile, lines);
    }

    private void ShowStatusOfAdapters()
    {
        Table table = new Table();
        table.AddColumn("Used For");
        table.AddColumn("Name");
        table.AddColumn("Status");
        table.AddColumn("Connection Status");
        table.AddRow("ITBA", _itbaAdapter!.Name, GetBoolMarkup(_itbaAdapter.IsEnabled, "Enabled", "Disabled"), GetBoolMarkup(_itbaAdapter.IsConnected, "Connected", "Not connected"));
        table.AddRow("Internet", _internetAdapter!.Name, GetBoolMarkup(_internetAdapter.IsEnabled, "Enabled", "Disabled"), GetBoolMarkup(_internetAdapter.IsConnected, "Connected", "Not connected"));
        AnsiConsole.MarkupLine("Current Status:");
        AnsiConsole.Write(table);
    }

    private string GetBoolMarkup(bool value, string trueText, string falseText)
    {
        if (value) return "[green]" + trueText + "[/]";
        else return "[red]" + falseText + "[/]";
    }

    // private void GetConfirmation()
    // {
    //     if (!AnsiConsole.Confirm("Do you want to proceed? [grey]Press enter to continue[/]"))
    //     {
    //         throw new Exception("Cancelled by user.");
    //     }
    // }

    private void Toggle()
    {
        Console.WriteLine();
        if (!_itbaAdapter!.IsEnabled && !_internetAdapter!.IsEnabled)
        {
            // None is working => ask user whats he want to enable
            AnsiConsole.MarkupLine("[red]None of the adapter is enabled![/]");
            string wantedToEnable = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("What feature do you want to [blue]enable[/]:\n[grey](Press 'up' and 'down' arrow to move and 'Enter' to select.)[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more adapters)[/]")
                .AddChoices(new[] { "Enable ITBA", "Enable Internet" })
            );
            if (wantedToEnable == "Enable ITBA")
            {
                EnableITBA();
            }
            else
            {
                EnableInternet();
            }
        }
        else if (_itbaAdapter!.IsEnabled && !_internetAdapter!.IsEnabled)
        {
            // ITBA is working => user trying to open internet
            EnableInternet();
        }
        else
        {
            // Internet is working => user trying to enable itba
            EnableITBA();
        }
    }

    private void EnableInternet()
    {
        AnsiConsole.MarkupLine("Please wait... I am trying to enable [green]Internet...[/]");
        Console.WriteLine("This will close automatically when finished.");
        _itbaAdapter!.Disable();
        _internetAdapter!.Enable();
    }
    private void EnableITBA()
    {
        AnsiConsole.MarkupLine("Please wait... I am trying to enable [green]ITBA...[/]");
        Console.WriteLine("This will close automatically when finished.");
        _itbaAdapter!.Enable();
        _internetAdapter!.Disable();
    }
}