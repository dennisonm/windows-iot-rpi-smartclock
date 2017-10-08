using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.SpeechSynthesis;

namespace SmartClock
{
    public sealed partial class MainPage : Page
    {
        // Dispatcher timer for the clock 
        private DispatcherTimer DispatcherClockTimer = new DispatcherTimer();

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
                this.SystemStatusTb.Text = "System Status: Error getting location by IP Address!";
                this.Lbl_Location.Text = "Singapore, Singapore";
                Speak(
                    "I am having problem getting your location." +
                    " " +
                    "Please check your network.");
            }
            else
                this.Lbl_Location.Text = GetLocationByIPAddress();

            // Initialize Status TextBlock
            this.SystemStatusTb.Text = "System Status: It's all good :)";
        }        

        /// <summary>
        /// Speech Synthesizer
        /// Text to Speech method (Speak)
        /// </summary>
        private async void Speak(string text)
        {
            // Dispose SpeechRecognizer 
            //await DisposeSpeechRecognizer();            

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

            // Play media
            //mediaElement.Play();

            // Stops and resets media 
            //mediaElement.Stop();  

            // Disposes the SpeechSynthesizer object and releases resources used 
            synth.Dispose();
        }

        /// <summary>
        /// Updates the current time and date display
        /// </summary>
        private void DispatcherClockTimer_Tick(object sender, object e)
        {
            if (!Global.pageWasReloaded)
            {                
                //Global.twelveHrBtnPreviousState = (TwelveHrBtn.IsChecked == true) ? true : false;
                //Global.twentyFourHrBtnPreviousState = (TwentyFourHrBtn.IsChecked == true) ? true : false;
            }
            else
            {
                TwentyFourHrBtn.IsChecked = Global.twentyFourHrBtnPreviousState;
                TwelveHrBtn.IsChecked = Global.twelveHrBtnPreviousState;
            }

            this.Lbl_Time.Text = (TwelveHrBtn.IsChecked == true) ? DateTime.Now.ToString("hh:mm tt") : DateTime.Now.ToString("HH:mm ddd");
            this.Lbl_Date.Text = (TwelveHrBtn.IsChecked == true) ? DateTime.Now.ToString("dddd, MMMM dd, yyyy") : DateTime.Now.ToString("MMMM dd, yyyy");

            if (DateTime.Now.Minute == 0 && Global.broadcasted == false && DateTime.Now.Hour > 5)
            {
                if (DateTime.Now.Hour <= 12)
                    Speak("It's " + DateTime.Now.Hour + " o'clock!");
                else
                    Speak("It's " + (DateTime.Now.Hour - 12) + " o'clock!");
                Global.broadcasted = true;
            }

            if (DateTime.Now.Minute == 1)
                Global.broadcasted = false;
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

        private async void TwelveHrBtn_Checked(object sender, RoutedEventArgs e)
        {
            if(Global.twelveHrBtnPreviousState == false)
                Speak("Changing time keeping convention to 12-hour clock.");
            Global.twelveHrBtnPreviousState = true;
            Global.twentyFourHrBtnPreviousState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Lbl_Time.Text = DateTime.Now.ToString("hh:mm tt"); // 12hr format
            });
            
        }

        private async void TwentyFourHrBtn_Checked(object sender, RoutedEventArgs e)
        {
            if(Global.twentyFourHrBtnPreviousState == false)
                Speak("Changing time keeping convention to 24-hour clock.");
            Global.twentyFourHrBtnPreviousState = true;
            Global.twelveHrBtnPreviousState = false;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Lbl_Time.Text = DateTime.Now.ToString("HH:mm ddd"); // 24hr format
            }); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            Speak("Rebooting the system.");
            Windows.System.ShutdownManager.BeginShutdown(Windows.System.ShutdownKind.Restart, TimeSpan.FromSeconds(1));		//Delay before restart after shutdown
        }

        /// <summary>
        /// 
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
