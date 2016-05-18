using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.VR
{
    static class ObjReader
    {
        class VertexAttribute : List<float>
        {
            public int ElementSize;
        }

        struct Index
        {
            public int V;
            public int VT;
            public int VN;

            public static Index Create(string face)
            {
                var index = new Index();
                var values = face.Split('/');
                for (int i = 0; i < values.Length; i++)
                {
                    int value;
                    if (string.IsNullOrEmpty(values[i])) continue;
                    if (!int.TryParse(values[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    {
                        throw new InvalidOperationException(string.Format(
                            "Invalid face specification: {0}.",
                            values[i]));
                    }

                    if (i == 0) index.V = value;
                    if (i == 1) index.VT = value;
                    if (i == 2) index.VN = value;
                }

                return index;
            }
        }

        static void ParseValues(ref VertexAttribute buffer, string[] values)
        {
            if (buffer == null)
            {
                buffer = new VertexAttribute();
                buffer.ElementSize = values.Length - 1;
            }
            else if (buffer.ElementSize != values.Length - 1)
            {
                throw new InvalidOperationException("Invalid vertex specification. Vertex attributes must all have the same size.");
            }

            for (int i = 1; i < values.Length; i++)
            {
                float value;
                if (!float.TryParse(values[i], NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    throw new InvalidOperationException(string.Format(
                        "Invalid vertex specification: {0}.",
                        values[i]));
                }

                buffer.Add(value);
            }
        }

        static void AddVertexAttribute(VertexAttribute buffer, int index, List<float> vertices)
        {
            if (buffer == null && index <= 0) return;
            else if (buffer == null || index <= 0)
            {
                throw new InvalidOperationException("Invalid face specification. Specified vertex attribute does not exist.");
            }

            var offset = (index - 1) * buffer.ElementSize;
            for (int i = offset; i < offset + buffer.ElementSize; i++)
            {
                vertices.Add(buffer[i]);
            }
        }

        static int GetElementSize(VertexAttribute buffer)
        {
            return buffer == null ? 0 : buffer.ElementSize;
        }

        static int PushAttribArray(VertexAttribute buffer, int index, int stride, int offset)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(
                index, buffer.ElementSize,
                VertexAttribPointerType.Float,
                false, stride, offset);
            return offset + buffer.ElementSize * BlittableValueType<float>.Stride;
        }

        internal static Mat ReadObject(string fileName, float[] instanceData, int instanceCount)
        {
            var faceLength = 0;
            ushort vertexCount = 0;
            VertexAttribute position = null;
            VertexAttribute texCoord = null;
            VertexAttribute normals = null;
            var vertices = new List<float>();
            var indices = new List<ushort>();
            var indexMap = new Dictionary<Index, ushort>();
            foreach (var line in File.ReadAllLines(fileName))
            {
                var values = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0) continue;
                switch (values[0])
                {
                    case "v":
                        ParseValues(ref position, values);
                        break;
                    case "vt":
                        ParseValues(ref texCoord, values);
                        break;
                    case "vn":
                        ParseValues(ref normals, values);
                        break;
                    case "f":
                        var length = values.Length - 1;
                        if (faceLength == 0) faceLength = length;
                        else if (faceLength != length)
                        {
                            throw new InvalidOperationException("Invalid face specification. All faces must have the same number of vertices.");
                        }

                        for (int i = 1; i < values.Length; i++)
                        {
                            ushort index;
                            var face = Index.Create(values[i]);
                            if (!indexMap.TryGetValue(face, out index))
                            {
                                AddVertexAttribute(position, face.V, vertices);
                                if (texCoord != null)
                                {
                                    AddVertexAttribute(texCoord, face.VT, vertices);
                                }
                                AddVertexAttribute(normals, face.VN, vertices);
                                index = vertexCount++;
                                indexMap.Add(face, index);
                            }

                            indices.Add(index);
                        }
                        break;
                    default:
                        continue;
                }
            }

            var attrib = 0;
            var offset = 0;
            var stride = GetElementSize(position) + GetElementSize(texCoord) + GetElementSize(normals);

            // We're assuming for now we have positions and normals
            var instanceDataStride = instanceData.Length / instanceCount;
            var indexStride = stride + instanceDataStride;
            var totalStride = indices.Count * indexStride;
            var data = new float[totalStride * instanceCount];

            // For every instance
            for (int i = 0; i < instanceCount; i++)
            {
                // For every vertex in each triangle
                for (int v = 0; v < indices.Count; v++)
                {
                    // Add vertex data
                    for (int c = 0; c < stride; c++)
                    {
                        data[i * totalStride + v * indexStride + c] = vertices[indices[v] * stride + c];
                    }

                    // Add instance data
                    for (int k = 0; k < instanceDataStride; k++)
                    {
                        data[i * totalStride + v * indexStride + stride + k] = instanceData[i * instanceDataStride + k];
                    }
                }
            }

            return Mat.FromArray(data).Reshape(0, data.Length / indexStride);
        }
    }
}
