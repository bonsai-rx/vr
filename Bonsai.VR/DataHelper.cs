using OpenTK;
using Valve.VR;

namespace Bonsai.VR
{
    static class DataHelper
    {
        public static void ToMatrix4(ref HmdMatrix44_t matrix, out Matrix4 result)
        {
            result = new Matrix4(
                matrix.m0,
                matrix.m4,
                matrix.m8,
                matrix.m12,
                matrix.m1,
                matrix.m5,
                matrix.m9,
                matrix.m13,
                matrix.m2,
                matrix.m6,
                matrix.m10,
                matrix.m14,
                matrix.m3,
                matrix.m7,
                matrix.m11,
                matrix.m15
            );
        }

        public static void ToMatrix4(ref HmdMatrix34_t matrix, out Matrix4 result)
        {
            result = new Matrix4(
                matrix.m0,
                matrix.m4,
                matrix.m8,
                0,
                matrix.m1,
                matrix.m5,
                matrix.m9,
                0,
                matrix.m2,
                matrix.m6,
                matrix.m10,
                0,
                matrix.m3,
                matrix.m7,
                matrix.m11,
                1
            );
        }

        public static void ToVector3(ref HmdVector3_t vector, out Vector3 result)
        {
            result.X = vector.v0;
            result.Y = vector.v1;
            result.Z = vector.v2;
        }

        public static void ToVector2(ref VRControllerAxis_t axis, out Vector2 result)
        {
            result.X = axis.x;
            result.Y = axis.y;
        }
    }
}
