namespace Nanoka.Core
{
    public static class NanokaCore
    {
        // required for SQLite
        public static void Initialize() => SQLitePCL.Batteries_V2.Init();
    }
}