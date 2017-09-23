using System;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SmartClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
       public Settings()
        {
            this.InitializeComponent();

            // Initialize Status TextBlock
            this.SystemStatusTb.Text = "Work in progress!";

            Speak("System settings page.");            
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
            mediaElement.Stop();

            // Disposes the SpeechSynthesizer object and releases resources used 
            synth.Dispose();
        }

        private async void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Frame.Navigate(typeof(MainPage)));
        }        
    }
}
