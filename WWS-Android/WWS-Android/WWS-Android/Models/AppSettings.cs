using Xamarin.Essentials;

namespace WWS_Android.Models
{
    public static class AppSettings
    {
        public static string DbHost { get => Preferences.Get(nameof(DbHost), null); set => Preferences.Set(nameof(DbHost), value); }

        public static string DbUser { get => Preferences.Get(nameof(DbUser), null); set => Preferences.Set(nameof(DbUser), value); }

        public static string DbPassword { get => Preferences.Get(nameof(DbPassword), null); set => Preferences.Set(nameof(DbPassword), value); }
    }
}
