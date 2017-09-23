using System;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;

namespace SmartClock
{
    public class Global
    {
        public static bool pageWasReloaded = false;
        public static bool enableSRBtnPreviousState = false;
        public static bool twentyFourHrBtnPreviousState = false;
        public static bool twelveHrBtnPreviousState = true;
        public static bool broadcasted = false;        
    }
}
