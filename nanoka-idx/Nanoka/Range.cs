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
    }
}