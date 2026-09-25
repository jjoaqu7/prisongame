using System;
using UnityEngine;

namespace PrisonGame.Prototype
{
    // Shared world time. A solo presentation component supplies elapsed time and pause state.
    public sealed class PrisonClock : MonoBehaviour
    {
        public const double RealSecondsPerGameMinute = 5;
        public double TotalMinutes { get; private set; } = 8 * 60;
        public bool Paused { get; private set; } = true;
        public void SetPaused(bool paused) => Paused = paused;

        internal void Restore(double minutes) { TotalMinutes = minutes; Paused = true; }

        public void Advance(double realSeconds)
        {
            if (Paused || realSeconds <= 0 || double.IsNaN(realSeconds) || double.IsInfinity(realSeconds)) return;
            TotalMinutes += realSeconds / RealSecondsPerGameMinute;
        }

        public static string Format(double minutes)
        {
            long seconds = (long)Math.Floor(minutes * 60 + 0.000001);
            return "Day " + (seconds / 86400 + 1) + " " + ((seconds / 3600) % 24).ToString("00") + ":" +
                ((seconds / 60) % 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public static string Remaining(double minutes)
        {
            int seconds = (int)Math.Ceiling(Math.Max(0, minutes) * 60 - 0.000001);
            seconds = Math.Max(0, seconds);
            return (seconds / 60) + "m " + (seconds % 60).ToString("00") + "s";
        }
    }
}
