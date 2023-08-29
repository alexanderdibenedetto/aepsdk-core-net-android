using Android.App;
using Android.Content.PM;
using Android.OS;
using Com.Adobe.Marketing.Mobile;

namespace test;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
    {
        base.OnCreate(savedInstanceState, persistentState);

        InitializeAdobeCore();
    }

    private void InitializeAdobeCore()
    {
        try
        {
            MobileCore.SetWrapperType(WrapperType.Xamarin);
            MobileCore.LogLevel = LoggingMode.Debug;
            MobileCore.Application = Application;

            Dictionary<string, Java.Lang.Object> configuration = new()
            {
                { "analytics.timezone", "UTC" },
                { "analytics.timezoneOffset", 0 },
                { "analytics.referrerTimeout", 5 },
                { "analytics.backdatePreviousSessionInfo", false },
                { "lifecycle.sessionTimeout", 300 }
            };

            MobileCore.UpdateConfiguration(configuration);

            MobileCore.RegisterExtensions(
                new List<Java.Lang.Class>
                {
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.AepIdentity)),
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.AepLifecycle)),
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.AepSignal))
                },
                new AdobeRegisterExtensionsCallback());
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to complete initialization of Adobe.{System.Environment.NewLine}{exception.Message}{exception.StackTrace}");
        }
    }

    private class AdobeRegisterExtensionsCallback : Java.Lang.Object, IAdobeCallback
    {
        public void Call(Java.Lang.Object p0)
        {
            System.Diagnostics.Debug.WriteLine($"Extension registrations complete.");

            // start the analytics collection lifecycle for the initial app open.
            MobileCore.LifecycleStart(null);
            System.Diagnostics.Debug.WriteLine($"Adobe initialized successfully.");
        }
    }
}

