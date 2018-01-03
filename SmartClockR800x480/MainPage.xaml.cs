using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.Media.SpeechSynthesis;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Newtonsoft.Json;

namespace SmartClock
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer DispatcherClockTimer = new DispatcherTimer();
        private DispatcherTimer DispatcherWeatherTimer = new DispatcherTimer();
        
        private static bool broadcasted = false;        

        string appId = "7c1ae704f9dfc739df4b5aa95de5cb53";
        double lat = -33.812023;
        double lon = 151.173050;
        int outsideTemp, minOutsideTemp, maxOutsideTemp, humidity;
        string weatherDescription, city, country;
        int sunrise, sunset;

        BitmapImage thumbsUp = new BitmapImage(new Uri("ms-appx:///Assets/ThumbsUp-Icon.png"));
        BitmapImage thumbsDown = new BitmapImage(new Uri("ms-appx:///Assets/ThumbsDown-Icon.png"));

        public MainPage()
        {
            this.InitializeComponent();             

            // Timer setup for the Digital Clock
            DispatcherClockTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherClockTimer.Tick += DispatcherClockTimer_Tick;
            DispatcherClockTimer.Start();

            // Timer setup for the Weather
            DispatcherWeatherTimer.Interval = TimeSpan.FromMinutes(5);
            DispatcherWeatherTimer.Tick += DispatcherWeatherTimer_Tick;
            DispatcherWeatherTimer.Start();

            // Initialize Status TextBlock
            getWeather(lat, lon, appId);
        }        

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void Speak(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {                
                _Speak(text);                
            });            
        }

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void _Speak(string text)
        {
            // Initialize a new instance of the SpeechSynthesizer
            SpeechSynthesizer synth = new SpeechSynthesizer();

            // Generate the audio stream from plain text
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(text);
			
			// Instantiate a new instance of MediaElement
			// Represents an object that renders audio and video to the display
			MediaElement mediaElement = new MediaElement();

            // Send the stream to the media object
            mediaElement.SetSource(stream, stream.ContentType);

            // Disposes the SpeechSynthesizer object and releases resources used 
            synth.Dispose();
        }

        /// <summary>
        /// Updates the current time and date display
        /// </summary>
        private void DispatcherClockTimer_Tick(object sender, object e)
        {
            this.LocalTimeLbl.Text = DateTime.Now.ToString("hh:mm");
            this.LocalDateLbl.Text =DateTime.Now.ToString("dddd, MMMM dd");
            this.LocalTimeSecLbl.Text = DateTime.Now.ToString("ss");
            this.LocalTimeAMPMLbl.Text = (DateTime.Now.Hour >= 12) ? "PM" : "AM";

            if (DateTime.Now.Minute == 0)
            {
                if (DateTime.Now.Hour > 5 && broadcasted == false)
                {
                    if (DateTime.Now.Hour <= 12)
                        Speak("It's " + DateTime.Now.Hour + " o'clock!");
                    else
                        Speak("It's " + (DateTime.Now.Hour - 12) + " o'clock!");
                    broadcasted = true;
                }
                // Automatically reboot at 1:00AM
                if(DateTime.Now.Hour == 1)
                    Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));		//Delay before restart after shutdown
            }
            
            if(DateTime.Now.Minute == 5)
            {
                if (DateTime.Now.Hour > 5 && broadcasted == false)
                {
                    Speak("Weather conditions: " + outsideTemp + "°C with " + weatherDescription);
                    broadcasted = true;
                }
            }

            if (DateTime.Now.Minute == 1 || DateTime.Now.Minute == 6)
            {
                broadcasted = false;
            }                
        }

        /// <summary>
        /// Get weather info from openweathermap.org
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherWeatherTimer_Tick(object sender, object e)
        {
            getWeather(lat, lon, appId);
        }

        /// <summary>
        /// Returns the private IP address of the device
        /// </summary>
        /// <param name="hostNameType"></param>
        /// <returns></returns>
        public static string GetLocalIp(HostNameType hostNameType = HostNameType.Ipv4)
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .FirstOrDefault(
                        hn =>
                            hn.Type == hostNameType &&
                            hn.IPInformation?.NetworkAdapter != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }

        /// <summary>
        /// Displays the device's local IP address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetMyLocalIP_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog myPrivateIPAdd = new ContentDialog()
            {
                Title = "My Local IP Address",
                Content = GetLocalIp().ToString(),
                CloseButtonText = "Close"
            };

            await myPrivateIPAdd.ShowAsync();
        }

        /// <summary>
        /// Returns the public IP address of the device 
        /// </summary>
        public string GetPublicIPAddress()
        {
            string ip = String.Empty;
            try
            {
                string uri = "http://whatismyip.bitsflipper.com/";
                
                using (var client = new HttpClient())
                {
                    var result = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    ip = result.Split(':')[1].Split('<')[0];
                }
            }
            catch(Exception)
            {
                this.systemStatusTb.Text = "Exception: Get Public Address Error!";
            }            
            return ip;            
        }

        /// <summary>
        /// Returns the location (city and country) of the device
        /// </summary>
        public string GetLocationByIPAddress()
        {
            string location = String.Empty;
            try
            {
                string uri = "http://whatismyip.bitsflipper.com/location.php";
                using (var client = new HttpClient())
                {
                    var result = client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                    location = result.Split(':')[1].Split('<')[0];
                }
            }
            catch(Exception)
            {
                this.systemStatusTb.Text = "Exception: Get Location By IP Address Error!";
            }            
            return location.Trim();
        } 

        /// <summary>
        /// Shuts down, then restarts the device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Speak("Rebooting the system.");
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));		//Delay before restart after shutdown
        }

        /// <summary>
        /// Shuts down the device without restarting it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            Speak("Shutting down.");
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Shutdown, TimeSpan.FromSeconds(1));		//Delay is not relevant to shutdown
        }

        /// <summary>
        /// About the app
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        private async void About_Click(object sender, RoutedEventArgs e)
        {
            var assyVersion = typeof(App).GetTypeInfo().Assembly.GetName().Version;

            ContentDialog about = new ContentDialog()
            {
                Title = "About Project Leo",
                Content = "The Smart Clock Prototype\n" + "Developed by Myron Richard Dennison\n" + "Version: " + assyVersion.ToString(),
                CloseButtonText = "Close"
            };

            await about.ShowAsync();
        }

        /// <summary>
        /// Launch Bluetooth Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LaunchBluetoothSettings_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"ms-settings:bluetooth");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // Launched
            }
            else
            {
                //Launch failed
                this.systemStatusTb.Text = "Launch failed!";
            }
        }

        /// <summary>
        /// Launch Date and Time Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LaunchDateTimeSettings_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"ms-settings:dateandtime");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // Launched
            }
            else
            {
                // Launch failed
                this.systemStatusTb.Text = "Launch failed!";
            }
        }

        /// <summary>
        /// Launch Wi-Fi Settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LaunchWiFiSettings_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(@"ms-settings:network-wifi");

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // Launched
            }
            else
            {
                // Launch failed
                this.systemStatusTb.Text = "Launch failed!";
            }
        }

        /// <summary>
        /// Poll weather info from openweathermap.org
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="appId"></param>
        private async void getWeather(double lat, double lon, string appId)
        {
            try
            {
                RootObject myWeather = await OpenWeatherMapProxy.GetWeather(lat, lon, appId);
                city = myWeather.name;
                country = myWeather.sys.country;
                sunrise = myWeather.sys.sunrise;                
                sunset = myWeather.sys.sunset;
                outsideTemp = ((int)myWeather.main.temp);
                humidity = myWeather.main.humidity;
                minOutsideTemp = ((int)myWeather.main.temp_min);
                maxOutsideTemp = ((int)myWeather.main.temp_max);
                weatherDescription = myWeather.weather[0].description;
                string icon = String.Format("http://openweathermap.org/img/w/{0}.png", myWeather.weather[0].icon);

                if (outsideTemp < 25)
                    this.outsideTempLbl.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Green);
                else if(outsideTemp >= 25 && outsideTemp < 30)
                    this.outsideTempLbl.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Yellow);
                else if(outsideTemp >= 30 && outsideTemp < 35)
                    this.outsideTempLbl.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Orange);
                else
                    this.outsideTempLbl.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.Red);

                this.systemStatusTb.Text = "Last Updated: " + DateTime.Now;
                //this.systemStatusTb.Text = "Time of data calculation: " + UnixTimeStampToDateTime(myWeather.dt).ToString("hh:mm:ss tt");
                this.systemStatusIcon.Source = thumbsUp;
                this.locationLbl.Text = city + ", " + country;
                this.outsideTempLbl.Text = outsideTemp.ToString() + "°C";
                this.outsideHumLbl.Text = humidity.ToString() + "%";
                this.outsideMinTempLbl.Text = minOutsideTemp.ToString() + "°C";
                this.outsideMaxTempLbl.Text = maxOutsideTemp.ToString() + "°C";
                this.sunriseLbl.Text = UnixTimeStampToDateTime(sunrise).ToString("hh:mm:ss tt");
                this.sunsetLbl.Text = UnixTimeStampToDateTime(sunset).ToString("hh:mm:ss tt");
                this.weatherDescLbl.Text = weatherDescription;                
                this.weatherIcon.Source = new BitmapImage(new Uri(icon, UriKind.Absolute));
            }
            catch //(Exception e)
            {
                //this.systemStatusTb.Text = "Exception: OpenWeatherMapProxy Error!";
                //var dialog = new MessageDialog(e.ToString());
                //await dialog.ShowAsync();
                this.systemStatusIcon.Source = thumbsDown;
            }
        }

        /// <summary>
        /// Test Text To Speech for Time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testTimeTTS_Click(object sender, RoutedEventArgs e)
        {
            string AMPM = (DateTime.Now.Hour >= 12) ? "PM" : "AM";
            if (DateTime.Now.Hour <= 12 && DateTime.Now.Minute == 0)
                Speak("It's " + DateTime.Now.Hour + AMPM);
            else if(DateTime.Now.Hour <= 12 && DateTime.Now.Minute != 0)
                Speak("It's " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + AMPM);
            else if(DateTime.Now.Hour > 12)
            {
                if(DateTime.Now.Minute == 0)
                    Speak("It's " + (DateTime.Now.Hour - 12) + AMPM);
                else
                    Speak("It's " + (DateTime.Now.Hour - 12) + ":" + DateTime.Now.Minute + AMPM);
            }            
        }

        /// <summary>
        /// Test TTS for Weather
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testWeatherTTS_Click(object sender, RoutedEventArgs e)
        {
            Speak("Weather conditions: " + outsideTemp + "°C with " + weatherDescription);
        }

        /// <summary>
        /// Convert unix timestamp to datetime format
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
