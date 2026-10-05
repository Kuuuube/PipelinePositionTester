using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace PipelinePositionTester;

[PluginName("PreTransform Filter")]
public class PreTransformFilter : IPositionedPipelineElement<IDeviceReport>
{
    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport? report)
    {
        if (report == null) return;

        if (report is ITabletReport tabletReport)
        {
            Console.WriteLine("I am PreTransform, reading pressure: " + tabletReport.Pressure.ToString());
            if (RewritePressure > -1)
            {
                Console.WriteLine("I am PreTransform, rewriting pressure to: " + RewritePressure.ToString());
                tabletReport.Pressure = (uint)RewritePressure;
            }
        }
        Emit?.Invoke(report);
    }

    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Rewrite Pressure"), DefaultPropertyValue(-1)]
    public int RewritePressure { set; get; }
}
