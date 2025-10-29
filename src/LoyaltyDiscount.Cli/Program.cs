using System;
using System.Linq;
using System.Text.Json;
using Spectre.Console;
using LoyaltyDiscount;

static void PrintHeader()
{
    AnsiConsole.Write(new FigletText("Loyalty Discount").Centered());
    AnsiConsole.MarkupLine("[grey]No stacking · Highest wins · Tie: [bold]Loyalty > Coupon > BF > Regular[/] · Cap 25%[/]");
    AnsiConsole.WriteLine();
}

static void PrintHelp()
{
    var t = new Table().Border(TableBorder.Rounded);
    t.AddColumn("[cyan]Flag[/]");
    t.AddColumn("[cyan]Meaning[/]");
    t.AddRow("--loyalty", "Loyalty member (20%)");
    t.AddRow("--regular", "Regular customer (10%)");
    t.AddRow("--bf", "Black Friday (5%)");
    t.AddRow("--coupon 10|25", "Coupon percent");
    t.AddRow("--total <amount>", "Order total (default 100)");
    t.AddRow("--demo", "Run built-in PASS/FAIL demo suite");
    t.AddRow("--json", "Output JSON: { applied, pct, total, final }");
    t.AddRow("--help", "Show help");
    AnsiConsole.Write(t);

    AnsiConsole.MarkupLine("\nExamples:");
    AnsiConsole.MarkupLine("  [grey]--regular --coupon 25 --total 250[/]");
    AnsiConsole.MarkupLine("  [grey]--loyalty --total 1200[/]");
    AnsiConsole.MarkupLine("  [grey]--coupon 10 --bf --total 99.99[/]");
}

static (bool loyalty, bool regular, bool bf, CouponType coupon, decimal total, bool demo, bool json) ParseArgs(string[] args)
{
    bool loyalty = false, regular = false, bf = false, demo = false, json = false;
    CouponType coupon = CouponType.None;
    decimal total = 100m;

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--help": PrintHeader(); PrintHelp(); Environment.Exit(0); break;
            case "--demo": demo = true; break;
            case "--json": json = true; break;
            case "--loyalty": loyalty = true; break;
            case "--regular": regular = true; break;
            case "--bf": bf = true; break;
            case "--coupon":
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var c))
                {
                    coupon = c == 25 ? CouponType.TwentyFive :
                             c == 10 ? CouponType.Ten : CouponType.None;
                    i++;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid or missing value for --coupon (use 10 or 25).[/]");
                    Environment.Exit(1);
                }
                break;
            case "--total":
                if (i + 1 < args.Length && decimal.TryParse(args[i + 1], out var t)) { total = t; i++; }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid or missing value for --total.[/]");
                    Environment.Exit(1);
                }
                break;
        }
    }
    return (loyalty, regular, bf, coupon, total, demo, json);
}

static string PrettyPct(decimal p) => $"{p:P0}";

static void PrintInputSummary(decimal total, bool loyalty, bool regular, bool bf, CouponType coupon)
{
    var input = new Table().Border(TableBorder.Ascii);
    input.AddColumn("[yellow]Flag[/]");
    input.AddColumn("[yellow]Value[/]");
    input.AddRow("Total", $"[white]{total:0.00}[/]");
    input.AddRow("Loyalty", loyalty ? "[green]Yes[/]" : "[red]No[/]");
    input.AddRow("Regular", regular ? "[green]Yes[/]" : "[red]No[/]");
    input.AddRow("Black Friday", bf ? "[green]Yes[/]" : "[red]No[/]");
    input.AddRow("Coupon", coupon switch
    {
        CouponType.TwentyFive => "[green]25%[/]",
        CouponType.Ten => "[green]10%[/]",
        _ => "[red]None[/]"
    });
    AnsiConsole.Write(input);
}

static bool Scenario(string name, DiscountRequest req, DiscountKind expectedKind, decimal expectedPct)
{
    var d = DiscountSelector.Select(req);
    bool pass = d.Kind == expectedKind && d.Percentage == expectedPct;

    var table = new Table().Border(TableBorder.Rounded);
    table.AddColumn("[grey]Scenario[/]");
    table.AddColumn("[grey]Applied[/]");
    table.AddColumn("[grey]Expected[/]");
    table.AddRow(name, $"{d.Kind} ({PrettyPct(d.Percentage)})", $"{expectedKind} ({PrettyPct(expectedPct)})");
    AnsiConsole.Write(table);

    AnsiConsole.MarkupLine(pass ? "[green]PASS[/]\n" : "[red]FAIL[/]\n");
    return pass;
}

static void RunDemo()
{
    AnsiConsole.Write(new Rule("[yellow]Demo Suite[/]"));
    int pass = 0, total = 0;
    bool T(string n, DiscountRequest r, DiscountKind k, decimal p) { total++; if (Scenario(n, r, k, p)) pass++; return true; }

    T("Loyalty beats BF", new(100, true, false, true, CouponType.None), DiscountKind.Loyalty, 0.20m);
    T("Coupon25 > Loyalty", new(100, true, false, false, CouponType.TwentyFive), DiscountKind.Coupon25, 0.25m);
    T("Regular vs BF", new(100, false, true, true, CouponType.None), DiscountKind.Regular, 0.10m);
    T("Coupon10 tie 10%", new(100, false, true, false, CouponType.Ten), DiscountKind.Coupon10, 0.10m);
    T("BF only", new(100, false, false, true, CouponType.None), DiscountKind.BlackFriday, 0.05m);

    var panel = new Panel(new Markup($"[bold]Summary:[/] [green]{pass}[/] / {total} passed"))
        .Header("[white]RESULT[/]").Border(BoxBorder.Rounded);
    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();
}

PrintHeader();

var (loyalty, regular, bf, coupon, total, demo, json) = ParseArgs(args);

if (demo)
{
    RunDemo();
    return;
}

PrintInputSummary(total, loyalty, regular, bf, coupon);
AnsiConsole.WriteLine();

var req = new DiscountRequest(total, loyalty, regular, bf, coupon);
var decision = DiscountSelector.Select(req);
var final = DiscountSelector.ApplyFinalPrice(total, decision);

if (json)
{
    var payload = new
    {
        applied = decision.Kind.ToString(),
        pct = decision.Percentage,
        total = Math.Round(total, 2, MidpointRounding.AwayFromZero),
        final
    };
    Console.WriteLine(JsonSerializer.Serialize(payload));
    return;
}

var panelText = new Markup(
    $"[bold]Applied:[/] [dodgerblue1]{decision.Kind}[/]  " +
    $"[bold]Rate:[/] [green]{PrettyPct(decision.Percentage)}[/]\n" +
    $"[bold]Price:[/] {total:0.00} → [bold green]{final:0.00}[/]"
);
var panelOut = new Panel(panelText).Header("[white]RESULT[/]").Border(BoxBorder.Rounded);
AnsiConsole.Write(panelOut);

AnsiConsole.MarkupLine("\n[grey]Policy: No stacking · Highest wins · Cap 25% · Rounding: 2dp (AwayFromZero).[/]");
