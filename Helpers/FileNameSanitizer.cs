using System.Text;
using System.Text.RegularExpressions;

namespace MahnitolaList.Helpers
{
    public static class FileNameSanitizer
    {
        public static string Transliterate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var map = new (string cyr, string lat)[]
            {
                ("Є","Ye"),("Ї","Yi"),("Й","Y"),("Ю","Yu"),("Я","Ya"),
                ("є","ie"),("ї","i"),("й","i"),("ю","iu"),("я","ia"),
                ("А","A"),("Б","B"),("В","V"),("Г","H"),("Ґ","G"),("Д","D"),("Е","E"),("Ж","Zh"),("З","Z"),("И","Y"),
                ("І","I"),("К","K"),("Л","L"),("М","M"),("Н","N"),("О","O"),("П","P"),("Р","R"),("С","S"),("Т","T"),
                ("У","U"),("Ф","F"),("Х","Kh"),("Ц","Ts"),("Ч","Ch"),("Ш","Sh"),("Щ","Shch"),("Ь",""),("Ъ",""),
                ("а","a"),("б","b"),("в","v"),("г","h"),("ґ","g"),("д","d"),("е","e"),("ж","zh"),("з","z"),("и","y"),
                ("і","i"),("к","k"),("л","l"),("м","m"),("н","n"),("о","o"),("п","p"),("р","r"),("с","s"),("т","t"),
                ("у","u"),("ф","f"),("х","kh"),("ц","ts"),("ч","ch"),("ш","sh"),("щ","shch"),("ь",""),("ъ",""),

                ("Ё","Yo"),("ё","yo"),("Э","E"),("э","e"),("Ы","Y"),("ы","y"),
            };

            var sb = new StringBuilder(input);
            foreach (var (cyr, lat) in map)
                sb.Replace(cyr, lat);
            return sb.ToString();
        }

        public static string CleanFileName(string name)
        {
            var t = Transliterate(name);

            t = Regex.Replace(t, @"[^A-Za-z0-9 \(\)\.,\-]", "");

            t = Regex.Replace(t, @"\s{2,}", " ").Trim();

            return string.IsNullOrWhiteSpace(t) ? "file" : t;
        }

        public static string SuggestOutputName(string originalName)
        {
            return CleanFileName(originalName);
        }
    }
}
