using UnityEngine;

namespace HTJ21
{
    public static class MathExt
    {
        public static float Square(float value)
        {
            return value * value;
        }
        public static float Remap(float value, float inputStart, float inputEnd, float outputStart, float outputEnd)
        {
            return (value - inputStart) / (inputEnd - inputStart) * (outputEnd - outputStart) + outputStart;
        }
        public static float Sign(float value)
        {
            return value < 0 ? -1 : 1;
        }
    }
}
