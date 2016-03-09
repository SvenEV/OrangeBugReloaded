namespace OrangeBugReloaded.App
{
    public static class Mathf
    {
        public const float E = 2.7182818284590451f;
        public const float PI = 3.1415926535897931f;

        public static float Lerp(float min, float max, float t)
            => (1 - t) * min + t * max;

        public static float InverseLerp(float value, float min, float max)
            => (value - min) / (max - min);

        public static float Clamp(float value, float min, float max)
            => value < min ? min : (value > max ? max : value);

        public static float Clamp01(float value)
            => Clamp(value, 0, 1);

        public static float Abs(float value)
            => value < 0 ? -value : value;

        public static bool Within(float value, float min, float max)
            => value >= min && value <= max;
    }
}
