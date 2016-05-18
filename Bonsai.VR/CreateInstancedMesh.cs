using OpenCV.Net;
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
    public class CreateInstancedMesh : Transform<Mat, Mat>
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("OBJ Files (*.obj)|*.obj")]
        [Description("The name of the model file.")]
        public string FileName { get; set; }

        public int InstanceDataStride { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var instanceData = new float[input.Rows * input.Cols];
                using (var instanceHeader = Mat.CreateMatHeader(instanceData, input.Rows, input.Cols, input.Depth, 1))
                {
                    CV.Convert(input, instanceHeader);
                }

                var instanceCount = instanceData.Length / InstanceDataStride;
                return ObjReader.ReadObject(FileName, instanceData, instanceCount);
            });
        }
    }
}
