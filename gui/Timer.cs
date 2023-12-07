using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chess.gui
{
    public class Timer
    {
        private int initialCentiSeconds;
        private int increment;
        private TextBlock timeDisplay;
        private DispatcherTimer timer;

        private Stopwatch stopwatch;

        public Timer(int startInSeconds, int incrementInSeconds, TextBlock timeDisplay)
        {
            initialCentiSeconds = startInSeconds * 100;
            increment = incrementInSeconds * 100;
            this.timeDisplay = timeDisplay;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += TickMethod;

            stopwatch = new Stopwatch();

            UpdateTimer();
        }

        private void TickMethod(object sender, EventArgs e)
        {
            if (initialCentiSeconds - (int)stopwatch.ElapsedMilliseconds / 10 > 0)
            {
                UpdateTimer();
            }
        }

        private void UpdateTimer()
        {
            int centiSecondsLeft = initialCentiSeconds - (int)stopwatch.ElapsedMilliseconds / 10;
            int secondsDisplay = centiSecondsLeft / 100;
            int centiSecondsDisplay = centiSecondsLeft - (secondsDisplay * 100);
            int minutesDisplay = secondsDisplay / 60;
            secondsDisplay -= minutesDisplay * 60;

            string minutesString = minutesDisplay.ToString();
            string secondsString = secondsDisplay.ToString();
            string centiSecondsString = centiSecondsDisplay.ToString();

            if (secondsString.Length == 1)
                secondsString = "0" + secondsString;

            if (centiSecondsString.Length == 1)
                centiSecondsString = "0" + centiSecondsString;

            string displayString = minutesString + ":" + secondsString + "." + centiSecondsString;
            timeDisplay.Dispatcher.Invoke(() =>
            {
                timeDisplay.Text = displayString;
            });
        }

        public void Stop()
        {
            stopwatch.Stop();
            timer.Stop();
            initialCentiSeconds += increment;
            UpdateTimer();
        }

        public void Start()
        {
            stopwatch.Start();
            timer.Start();
        }
    }
}
