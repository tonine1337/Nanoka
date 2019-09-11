namespace Nanoka
{
    /// <summary>
    /// Simple range struct. Why doesn't .NET have this???
    /// </summary>
    public struct Range<T>
    {
        public readonly T Min;
        public readonly T Max;

        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public static implicit operator Range<T>((T min, T max) x) => new Range<T>(x.min, x.max);
    }
}
