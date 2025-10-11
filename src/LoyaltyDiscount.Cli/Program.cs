using System;
using Spectre.Console;
using LoyaltyDiscount;

static void PrintHeader()
{
    AnsiConsole.Write(
        new FigletText("Loyalty Discount")
            .Centered()
    );
    AnsiConsole.MarkupLine("[grey]No stacking · Highest wins · Tie: [bold]Loyalty > Coupon > BF > Regular[/][/]");
    AnsiConsole.WriteLine();
}

static void PrintHelp()
{
    var t = new Table().Border(TableBorder.Rounded);
    t.AddColumn("[cyan]Option[/]");
    t.AddColumn("[cyan]Meaning[/]");
    t.AddRow("--loyalty", "Loyalty member (20%)");
    t.AddRow("--regular", "Regular customer (10%)");
    t.AddRow("--bf", "Black Friday (5%)");
    t.AddRow("--coupon 10|25", "Coupon percent");
    t.AddRow("--total <amount>", "Order total (default 100)");
    t.AddRow("--help", "Show help");
    AnsiConsole.Write(t);
    AnsiConsole.MarkupLine("\nExamples:");
    AnsiConsole.MarkupLine("  [grey]--regular --coupon 25 --total 250[/]");
    AnsiConsole.MarkupLine("  [grey]--loyalty --total 1200[/]");
    AnsiConsole.MarkupLine("  [grey]--coupon 10 --bf --total 99.99[/]");
}

static (bool, bool, bool, CouponType, decimal) ParseArgs(string[] args)
{
    bool loyalty = false, regular = false, bf = false;
    CouponType coupon = CouponType.None;
    decimal total = 100m;

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--help": PrintHeader(); PrintHelp(); Environment.Exit(0); break;
            case "--loyalty": loyalty = true; break;
            case "--regular": regular = true; break;
            case "--bf": bf = true; break;
            case "--coupon":
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var c))
                { coupon = c == 25 ? CouponType.TwentyFive : c == 10 ? CouponType.Ten : CouponType.None; i++; }
                break;
            case "--total":
                if (i + 1 < args.Length && decimal.TryParse(args[i + 1], out var t)) { total = t; i++; }
                break;
        }
    }
    return (loyalty, regular, bf, coupon, total);
}

static string PrettyPct(decimal p) => $"{p:P0}";

PrintHeader();

var (loyalty, regular, bf, coupon, total) = ParseArgs(args);

// Input summary
var input = new Table().Border(TableBorder.Ascii);
input.AddColumn("[yellow]Flag[/]");
input.AddColumn("[yellow]Value[/]");
input.AddRow("Total", $"[white]{total:0.00}[/]");
input.AddRow("Loyalty", loyalty ? "[green]Yes[/]" : "[red]No[/]");
input.AddRow("Regular", regular ? "[green]Yes[/]" : "[red]No[/]");
input.AddRow("Black Friday", bf ? "[green]Yes[/]" : "[red]No[/]");
input.AddRow("Coupon", coupon switch { CouponType.TwentyFive => "[green]25%[/]", CouponType.Ten => "[green]10%[/]", _ => "[red]None[/]" });
AnsiConsole.Write(input);
AnsiConsole.WriteLine();

// Decision
var req = new DiscountRequest(total, loyalty, regular, bf, coupon);
var decision = DiscountSelector.Select(req);
var final = DiscountSelector.ApplyFinalPrice(total, decision);

// Result panel
var panelText = new Markup(
    $"[bold]Applied:[/] [dodgerblue1]{decision.Kind}[/]  " +
    $"[bold]Rate:[/] [green]{PrettyPct(decision.Percentage)}[/]\n" +
    $"[bold]Price:[/] {total:0.00} → [bold green]{final:0.00}[/]"
);
var panel = new Panel(panelText).Header("[white]RESULT[/]").Border(BoxBorder.Rounded);
AnsiConsole.Write(panel);

// Footer note
AnsiConsole.MarkupLine("\n[grey]Policy: No stacking · Highest wins · Max 25% · Rounding: 2dp (AwayFromZero).[/]");
