using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    [Description("Triggers a single haptic pulse on a controller.")]
    public class TriggerHapticPulse : Sink<DeviceState>
    {
        [Description("The axis on which the haptic pulse will be triggered.")]
        public int Axis { get; set; }

        [Range(0, 3999)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("The strength of the haptic pulse, measured in pulse width microseconds.")]
        public int Strength { get; set; }

        public override IObservable<DeviceState> Process(IObservable<DeviceState> source)
        {
            return source.Do(device => device.TriggerHapticPulse(Axis, Strength));
        }
    }
}
