using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    [Description("Tests whether the specified controller button is pressed.")]
    public class GetButtonPressed : Transform<TrackerState, bool>
    {
        [Description("The index of the controller button to check.")]
        public VRButtonId Button { get; set; }

        public override IObservable<bool> Process(IObservable<TrackerState> source)
        {
            return source.Select(input => input.GetButtonPressed(Button));
        }
    }
}
