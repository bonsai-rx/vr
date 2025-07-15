using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace Bonsai.VR
{
    [Description("Extracts a sequence of video frames from the latest tracked camera state.")]
    public class GetVideoStreamFrameBuffer : Combinator<TrackedCameraState, TrackedCameraFrame>
    {
        public override IObservable<TrackedCameraFrame> Process(IObservable<TrackedCameraState> source)
        {
            return source
                .Where(input => input.FrameExposureTime > 0)
                .DistinctUntilChanged(input => input.FrameSequence)
                .Select(input =>
                {
                    var header = new CameraVideoStreamFrameHeader_t();
                    var image = new IplImage(new Size((int)input.Width, (int)input.Height), IplDepth.U8, (int)input.BytesPerPixel);
                    OpenVR.TrackedCamera.GetVideoStreamFrameBuffer(
                        input.Handle,
                        input.FrameType,
                        image.ImageData,
                        (uint)(image.WidthStep * image.Height),
                        ref header,
                        TrackedCameraState.HeaderSize);

                    var result = new TrackedCameraFrame();
                    result.DeviceIndex = input.DeviceIndex;
                    result.UpdateFrameHeader(ref header);
                    result.FrameType = input.FrameType;
                    result.Handle = input.Handle;
                    result.Image = image;
                    return result;
                });
        }
    }
}
