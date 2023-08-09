// See https://aka.ms/new-console-template for more information
using ItbaToggle;
using Spectre.Console;

Toggler toggle = new Toggler();

try
{
    toggle.Run();
}
catch (Exception e)
{
    Console.WriteLine();
    AnsiConsole.MarkupLine("[red]Critical Error.[/]");
    AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
    Console.Write("Press any key to close...");
    Console.ReadKey();
}
