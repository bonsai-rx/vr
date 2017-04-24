using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    public class GetButtonPressed : Transform<ControllerState, bool>
    {
        public VRButtonId Button { get; set; }

        public override IObservable<bool> Process(IObservable<ControllerState> source)
        {
            return source.Select(input => input.GetButtonPressed(Button));
        }
    }
}
