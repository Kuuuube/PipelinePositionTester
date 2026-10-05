using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace PipelinePositionTester;

[PluginName("Internal Filter")]
public class InternalFilter : IPositionedPipelineElement<IDeviceReport>
{
    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport? report)
    {
        if (report == null) return;

        if (report is ITabletReport tabletReport)
        {
            Console.WriteLine("I am Internal, reading pressure: " + tabletReport.Pressure.ToString() + ", Expected built-in PressureRewriteFilter rewrite: " + ExpectedRewrite(report).ToString() + " (MaxPressure: " + TabletReference?.Properties.Specifications.Pen.MaxPressure.ToString() + ", PressureThreshold: " + PressureThreshold.ToString() + "%)");
            if (RewritePressure > -1)
            {
                Console.WriteLine("I am Internal, rewriting pressure to: " + RewritePressure.ToString());
                tabletReport.Pressure = (uint)RewritePressure;
            }
        }
        Emit?.Invoke(report);
    }

    private uint ExpectedRewrite(IDeviceReport? report)
    {
        if (TabletReference == null)
        {
            Console.WriteLine("TabletReference not found, failed to calculate ExpectedRewrite");
            return 0;
        }

        var MaxPenPressure = TabletReference.Properties.Specifications.Pen.MaxPressure;
        var TipPressureThreshold = (float)PressureThreshold / 100;
        var EraserPressureThreshold = (float)PressureThreshold / 100;

        // ----
        if (report is ITabletReport tabletReport && tabletReport.Pressure != MaxPenPressure) {

            float activationThreshold = report is IEraserReport eraserReport && eraserReport.Eraser ? EraserPressureThreshold : TipPressureThreshold;

            float pressurePercent = tabletReport.Pressure / (float)MaxPenPressure;

            if (pressurePercent > activationThreshold)
            {
                // tabletReport.Pressure = (uint)(MaxPenPressure * ((pressurePercent - activationThreshold) / (1f - activationThreshold)));
                return (uint)(MaxPenPressure * ((pressurePercent - activationThreshold) / (1f - activationThreshold)));
            }
            else
                // tabletReport.Pressure = 0;
                return 0;
        }
        // ----

        Console.WriteLine("ExpectedRewrite fell through, this is expected only when tabletReport.Pressure == MaxPressure");
        return 0;
    }

    public PipelinePosition Position => PipelinePosition.Internal;

    [Property("Pressure Threshold"), DefaultPropertyValue(1u), Unit("%")]
    public uint PressureThreshold { set; get; }

    [Property("Rewrite Pressure"), DefaultPropertyValue(-1)]
    public int RewritePressure { set; get; }

    [TabletReference]
    public TabletReference? TabletReference { set; get; }
}
