﻿using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Chess.gui
{
    public class Timer
    {
        private int initialCentiSeconds;
        private readonly int increment;
        private TextBlock timeDisplay;
        private DispatcherTimer timer;
        public ChessBoard ChessBoard { set; get; }

        private Stopwatch stopwatch;

        private uint color;

        private bool isBlocked = false;

        public Timer(TimerOptions timerOption, TextBlock timeDisplay, uint color)
        {
            this.color = color;
            if (timerOption.Option == TimerOptions.Options.NoTime)
            {
                isBlocked = true;
                return;
            }

            int startInSeconds = timerOption.start;
            int incrementInSeconds = timerOption.increment;
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
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            int centiSecondsLeft = initialCentiSeconds - ((int)stopwatch.ElapsedMilliseconds / 10);
            if (centiSecondsLeft <= 0)
            {
                isBlocked = true;
                centiSecondsLeft = 0;
            }
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

            if (isBlocked)
            {
                Stop();
                if (color == Logic.Piece.WHITE)
                {
                    ChessBoard.Finish("Black won!", "White ran out of time.");
                }
                if (color == Logic.Piece.BLACK)
                {
                    ChessBoard.Finish("White won!", "Black ran out of time.");
                }
            }
        }

        public void Stop()
        {
            if (isBlocked)
            {
                return;
            }
            initialCentiSeconds += increment;
            UpdateTimer();
            stopwatch.Stop();
            timer.Stop();
            
        }

        public void Start()
        {
            if (!isBlocked)
            {
                stopwatch.Start();
                timer.Start();
            }
        }

        public int GetTimeLeft()
        {
            return 10 * initialCentiSeconds - (int)stopwatch.ElapsedMilliseconds;
        }
    }
}
