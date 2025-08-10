using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace MahnitolaList.Helpers
{
    public static class LocalizationService
    {
        public static string CurrentLanguage { get; private set; } = "uk";

        public static void SetLanguage(string lang)
        {
            if (string.Equals(CurrentLanguage, lang, StringComparison.OrdinalIgnoreCase)) return;

            var dictUri = new Uri($"/Resources/Lang.{lang}.xaml", UriKind.Relative);
            var dict = new ResourceDictionary { Source = dictUri };

            var appDicts = Application.Current.Resources.MergedDictionaries;
            var old = appDicts.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.StartsWith("/Resources/Lang.", StringComparison.OrdinalIgnoreCase));
            if (old != null) appDicts.Remove(old);
            appDicts.Add(dict);

            CurrentLanguage = lang;
        }
    }
}
