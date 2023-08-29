using Android.App;
using Android.Content.PM;
using Android.OS;
using Com.Adobe.Marketing.Mobile;
using Java.Interop;

namespace test;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
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
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.Identity)),
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.Lifecycle)),
                    Java.Lang.Class.FromType(typeof(Com.Adobe.Marketing.Mobile.Signal))
                },
                new AdobeRegisterExtensionsCallback());
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Unable to complete initialization of Adobe.{System.Environment.NewLine}{exception.Message}{exception.StackTrace}");
        }
    }


    public void TrackState(string state, IDictionary<string, string> data)
    {
        MobileCore.TrackState(state, data);
    }

    public void TrackAction(string action, IDictionary<string, string> data)
    {
        MobileCore.TrackAction(action, data);
    }

    public async Task<string> GenerateVisitorUrl(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            GetUrlAdobeCallback callback = new();
            Identity.AppendVisitorInfoForURL(url, callback);
            return await callback.Completed() ?? url;
        }
        return url;
    }

    public async Task<string> GetUrlVariables()
    {
        GetUrlAdobeCallback callback = new();
        Identity.GetUrlVariables(callback);
        return await callback.Completed() ?? callback.UrlWithTracking;
    }

    private class GetUrlAdobeCallback : Java.Lang.Object, IAdobeCallback
    {
        private bool callCalled = false;

        public void Call(Java.Lang.Object urlWithTracking)
        {
            callCalled = true;

            if (urlWithTracking != null)
            {
                UrlWithTracking = Convert.ToString(urlWithTracking);
            }
        }

        internal async Task<string> Completed()
        {
            while (!callCalled)
            {
                await Task.Delay(100);
            }

            return UrlWithTracking;
        }

        public string UrlWithTracking { get; set; }
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