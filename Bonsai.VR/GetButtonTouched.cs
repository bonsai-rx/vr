using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    [Description("Tests whether the touchpad on the specified controller is touched.")]
    public class GetButtonTouched : Transform<ControllerState, bool>
    {
        [Description("The index of the touchpad button to check.")]
        public VRButtonId Button { get; set; }

        public override IObservable<bool> Process(IObservable<ControllerState> source)
        {
            return source.Select(input => input.GetButtonTouched(Button));
        }
    }
}
