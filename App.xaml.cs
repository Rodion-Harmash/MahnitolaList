using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MahnitolaList
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var ci = CultureInfo.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;

            base.OnStartup(e);

            EventManager.RegisterClassHandler(
                typeof(Window),
                Window.LoadedEvent,
                new RoutedEventHandler((s, ev) =>
                {
                    var w = (Window)s;
                    if (w.Icon == null)
                    {
                        w.Icon = BitmapFrame.Create(
                            new Uri("pack://application:,,,/Resources/Icons/app.ico", UriKind.Absolute));
                    }
                }));
        }

        public static void SetCulture(string culture)
        {
            var ci = new CultureInfo(culture);
            Thread.CurrentThread.CurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
        }
    }
}
