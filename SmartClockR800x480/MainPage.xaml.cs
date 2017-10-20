using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Media.Imaging;

namespace SmartClock
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer DispatcherClockTimer = new DispatcherTimer();
        private static bool broadcasted = false;
        TimeZoneInfo remoteTimeZone;        
            
        BitmapImage AuFlag = new BitmapImage(new Uri("ms-appx:///Assets/AU-Flag.png"));
        BitmapImage SgFlag = new BitmapImage(new Uri("ms-appx:///Assets/SG-Flag.png"));
        BitmapImage PhFlag = new BitmapImage(new Uri("ms-appx:///Assets/PH-Flag.png"));        

        public MainPage()
        {
            this.InitializeComponent();             

            // Timer setup for the Digital Clock
            DispatcherClockTimer.Interval = TimeSpan.FromSeconds(1);
            DispatcherClockTimer.Tick += DispatcherClockTimer_Tick;
            DispatcherClockTimer.Start();

            // Update Location TextBlock                       
            if (GetLocationByIPAddress() == null || GetLocationByIPAddress() == "")
            {
                this.SystemStatusTb.Text = "Unable to get location by IP Address!";
                this.LocalLocationLbl.Text = "Singapore, Singapore";
                Speak(
                    "I am having problem getting your location." +
                    " " +
                    "Please check your network.");
            }
            else
                this.LocalLocationLbl.Text = GetLocationByIPAddress();

            if (GetLocationByIPAddress().Contains("Singapore"))
            {
                LocalFlag.Source = SgFlag;
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                RemoteFlag.Source = AuFlag;
                Greetings.Text = "Alla mak! Late orready lah!";                
            }
            else if (GetLocationByIPAddress().Contains("Australia"))
            {
                LocalFlag.Source = AuFlag;
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                RemoteFlag.Source = PhFlag;
                Greetings.Text = "Ga day! Ow ya goin'";                
            }
            else if (GetLocationByIPAddress().Contains("Philippines"))
            {
                LocalFlag.Source = PhFlag;
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                RemoteFlag.Source = AuFlag;
                Greetings.Text = "Mabuhay!";                
            }
            else //default
            {
                LocalFlag.Source = PhFlag;
                remoteTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
                RemoteFlag.Source = AuFlag;
            }

            // Initialize Status TextBlock
            this.SystemStatusTb.Text = "";
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
            this.LocalTimeLbl.Text = DateTime.Now.ToString("h:mm");
            this.LocalDateLbl.Text =DateTime.Now.ToString("dddd, MMMM dd, yyyy");
            this.LocalTimeSecLbl.Text = DateTime.Now.ToString("ss");
            this.LocalTimeAMPMLbl.Text = (DateTime.Now.Hour >= 12) ? "PM" : "AM";

            if (DateTime.Now.Minute == 0 && broadcasted == false && DateTime.Now.Hour > 5)
            {
                if (DateTime.Now.Hour <= 12)
                    Speak("It's " + DateTime.Now.Hour + " o'clock!");
                else
                    Speak("It's " + (DateTime.Now.Hour - 12) + " o'clock!");
                broadcasted = true;
            }
            
            DateTime remoteTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, remoteTimeZone);
            this.RemoteTimeLbl.Text = Convert.ToString(remoteTime.ToString("h:mm"));
            this.RemoteDateLbl.Text = Convert.ToString(remoteTime.ToString("ddd, MMM dd"));

            if (DateTime.Now.Minute == 1)
                broadcasted = false;
        }

        /// <summary>
        /// Returns the private IP address of the device
        /// </summary>
        public IPAddress GetIPAddress()
        {
            List<string> IpAddress = new List<string>();
            var Hosts = Windows.Networking.Connectivity.NetworkInformation.GetHostNames().ToList();
            foreach (var Host in Hosts)
            {
                string IP = Host.DisplayName;
                IpAddress.Add(IP);
            }
            IPAddress address = IPAddress.Parse(IpAddress.Last());
            return address;
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
                this.SystemStatusTb.Text = "Exception: Get Public Address Error!";
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
                this.SystemStatusTb.Text = "Exception: Get Location By IP Address Error!";
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
    }
}
