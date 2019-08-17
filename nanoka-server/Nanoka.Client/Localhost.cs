namespace Nanoka.Client
{
    // *.localhost.chiya.dev resolves to 127.0.0.1
    public static class Localhost
    {
        // client always runs on port 7230
        public const int Port = 7230;

        public static string Url(string sub = "nanoka") => $"https://{sub}.localhost.chiya.dev:{Port}";
    }
}