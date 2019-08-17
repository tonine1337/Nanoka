using System.IO;

namespace Nanoka.Controllers.Recaptcha
{
    public static class RecaptchaForm
    {
        static readonly string _form;

        static RecaptchaForm()
        {
            var assembly = typeof(RecaptchaForm).Assembly;

            using (var stream = assembly.GetManifestResourceStream($"{typeof(RecaptchaForm).Namespace}.form.html"))
            using (var reader = new StreamReader(stream))
                _form = reader.ReadToEnd();
        }

        public static string GetForm(string recaptchaSiteKey) => _form.Replace("{{site-key}}", recaptchaSiteKey);
    }
}