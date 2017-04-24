using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    [Serializable]
    public class VRException : Exception
    {
        public VRException()
        {
        }

        public VRException(string message)
            : base(message)
        {
        }

        public VRException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected VRException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
