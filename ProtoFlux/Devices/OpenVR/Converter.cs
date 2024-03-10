using Elements.Core;
using Valve.VR;

namespace OpenvrDataGetter
{
    internal static class Converter
    {
        public static unsafe double3 HmdVector3ToDobble3(HmdVector3d_t vec) => *(double3*)&vec;

        public static float4x4 HmdMatrix34ToFloat4x4(HmdMatrix34_t matrix) =>
            new(matrix.m0, matrix.m1, matrix.m2, matrix.m3,
                        matrix.m4, matrix.m5, matrix.m6, matrix.m7,
                        matrix.m8, matrix.m9, matrix.m10, matrix.m11,
                        0, 0, 0, 0);
    }
}
