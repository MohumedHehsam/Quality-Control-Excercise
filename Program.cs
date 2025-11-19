
using System.Collections.Generic;
using System.Drawing;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;
using Console1;
using ScottPlot;
internal class Program
{
    static Dictionary<string, List<double>> QCTable = JsonSerializer.Deserialize<Dictionary<string, List<double>>>(File.ReadAllText("QCTable.json"));
    private static void Main(string[] args)
    {
        List<XParR> pointsXR = JsonSerializer.Deserialize<List<XParR>>(File.ReadAllText("inputXR.json"));
        List<XParR> pointsXS = JsonSerializer.Deserialize<List<XParR>>(File.ReadAllText("inputXS.json"));


        System.Console.WriteLine("input number of samples in a subGroup");
        int n = int.Parse(Console.ReadLine());
        XBarS(pointsXR,n);
        

    }

    static void XBarS(List<XParR> points,int n)
    {
        double xBarBar, sBar;

        (xBarBar, sBar) = Avg(points);

        double UCL = xBarBar + sBar * QCTable["A3"][n - 1];
        double LCL = xBarBar - sBar * QCTable["A3"][n - 1];
        double UCLS = sBar * QCTable["B4"][n - 1];
        double LCLS = sBar * QCTable["B3"][n - 1];


        var newPoints = points;
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.xBar).ToList(), UCL, LCL, xBarBar, "X-bar chart", 1);
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.rangeR).ToList(), UCLS, LCLS, sBar, "S chart", 1);


        if (newPoints.All(x => x.xBar >= LCL && x.xBar <= UCL && x.rangeR >= LCLS && x.rangeR <= UCLS))
        {
            System.Console.WriteLine("operation was stable ");
        System.Console.WriteLine("programme finished !");
            return;
        }
        else
            System.Console.WriteLine("operation is unstable continuing using revision rule :-");

        newPoints = points.Where(x => x.xBar >= LCL && x.xBar <= UCL && x.rangeR >= LCLS && x.rangeR <= UCLS).ToList();

        (xBarBar, sBar) = Avg(newPoints);
        var segma = sBar / QCTable["c4"][n - 1];
        UCL = xBarBar + QCTable["A"][n - 1] * segma;
        LCL = xBarBar - QCTable["A"][n - 1] * segma;
        UCLS =QCTable["B6"][n - 1] * segma;
        LCLS =QCTable["B5"][n - 1] * segma;
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.xBar).ToList(), UCL, LCL, xBarBar, "X-bar chart", 2);
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.rangeR).ToList(), UCLS, LCLS, sBar, "S chart", 2);

        System.Console.WriteLine("programme finished !");
    }
    static void XBarR(List<XParR> points,int n)
    {
        double xBarBar, rBar;

        (xBarBar, rBar) = Avg(points);

        double UCL = xBarBar + rBar * QCTable["A2"][n - 1];
        double LCL = xBarBar - rBar * QCTable["A2"][n - 1];
        double UCLR = rBar * QCTable["D4"][n - 1];
        double LCLR = rBar * QCTable["D3"][n - 1];


        var newPoints = points;
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.xBar).ToList(), UCL, LCL, xBarBar, "X-bar chart", 1);
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.rangeR).ToList(), UCLR, LCLR, rBar, "R chart", 1);


        if (newPoints.All(x => x.xBar >= LCL && x.xBar <= UCL && x.rangeR >= LCLR && x.rangeR <= UCLR))
        {
            System.Console.WriteLine("operation was stable ");
        System.Console.WriteLine("programme finished !");
            return;
        }
        else
            System.Console.WriteLine("operation is unstable continuing using revision rule :-");

        newPoints = points.Where(x => x.xBar >= LCL && x.xBar <= UCL && x.rangeR >= LCLR && x.rangeR <= UCLR).ToList();

        (xBarBar, rBar) = Avg(newPoints);
        var segma = rBar / QCTable["d2"][n - 1];
        UCL = xBarBar + QCTable["A"][n - 1] * segma;
        LCL = xBarBar - QCTable["A"][n - 1] * segma;
        UCLR =QCTable["D2"][n - 1] * segma;
        LCLR =QCTable["D1"][n - 1] * segma;
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.xBar).ToList(), UCL, LCL, xBarBar, "X-bar chart", 2);
        Draw(newPoints.Select(x => x.subgroupNumber).ToList(), newPoints.Select(x => x.rangeR).ToList(), UCLR, LCLR, rBar, "R chart", 2);

        System.Console.WriteLine("programme finished !");
    }

    static (double xBarBar, double rBar) Avg(List<XParR> points)
    {
        double xBarBar = 0, rBar = 0;
        foreach (var item in points)
        {
            xBarBar += item.xBar;
            rBar += item.rangeR;
        }
        xBarBar /= points.Count;
        rBar /= points.Count;
        return (xBarBar, rBar);
    }

    static void Draw(List<int> subgroups, List<double> values, double UCL, double LCL, double mean, string tableName, int iteration)
    {
        Console.WriteLine("calculating  xPar & R graph ...");
        ScottPlot.Plot plot = new();
        ScottPlot.Plot r = new();

        plot.Add.Scatter(subgroups, values);
        plot.XLabel("subgroup no.");
        plot.YLabel("value");
        plot.Title(tableName);

        var centerLine = plot.Add.HorizontalLine(mean);
        centerLine.Color = ScottPlot.Color.FromHex("#000000"); // Black

        // --- Upper Control Limit (UCL) ---
        var uclLine = plot.Add.HorizontalLine(UCL);
        uclLine.Color = ScottPlot.Color.FromHex("#FF0000"); // Red

        // --- Lower Control Limit (LCL) ---
        var lclLine = plot.Add.HorizontalLine(LCL);
        lclLine.Color = ScottPlot.Color.FromHex("#FF0000"); // Red

        // Add a legend to explain the lines
        plot.ShowLegend();

        // 2. Save the Plot
        
        Directory.CreateDirectory($"{tableName}");
        plot.SavePng($"{tableName}/{iteration}", 600, 400);

    }
}