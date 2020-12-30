﻿using System;
using System.Threading.Tasks;
using WallpaperSetter.Library;
using static System.Console;

namespace WallpaperSetter.Console
{
    internal class Program
    {
        private static int _timeInMilliseconds;
        private static string _tag;
        private static Wallpaper.Style _style;

        public static async Task Main(string[] args)
        {
            Clear();

            SetMinutesUntilNextWallpaper();
            SetInstagramTag();
            SetWallpaperStyle();

            await RunConsoleApp();

            WriteLine("== PROGRAM ENDED ==");
            ReadKey();
        }

        private static void SetMinutesUntilNextWallpaper()
        {
            Write("Minutes until next Wallpaper: ");
            var inputTimeInMinutes = ReadLine()?.Trim() ?? string.Empty;

            if (!int.TryParse(inputTimeInMinutes, out var timeInMinutes) || !IsPositive(timeInMinutes))
                throw new InvalidAmountOfTimeException($"{inputTimeInMinutes} is not a valid positive number!");

            SetTimeInMillisecondsFromMinutes(timeInMinutes);
        }

        private static void SetInstagramTag()
        {
            Write("Instagram Tag: ");
            _tag = ReadLine()?.Trim() ?? string.Empty;

            if (!IsPositive(_tag.Length))
                throw new InvalidInstagramTagException("Input Instagram tag was empty!");
        }

        private static void SetWallpaperStyle()
        {
            Write("Wallpaper style (Tiled, Centered, Stretched): ");
            var inputStyle = ReadLine()?.Trim() ?? string.Empty;

            if (!Enum.TryParse(inputStyle, out _style))
                throw new InvalidWallpaperStyleException($"{inputStyle} is not a valid wallpaper style!");
        }

        private static async Task RunConsoleApp()
        {
            var app = new ConsoleApp(_timeInMilliseconds, _tag, _style);
            await app.Run();
        }

        private static bool IsPositive(int n)
        {
            return n >= 0;
        }

        private static void SetTimeInMillisecondsFromMinutes(in int timeInMinutes)
        {
            _timeInMilliseconds = timeInMinutes * 1000 * 60;
        }
    }

    internal class InvalidAmountOfTimeException : ArgumentException
    {
        public InvalidAmountOfTimeException(string message)
            : base(message)
        {
        }
    }

    internal class InvalidInstagramTagException : ArgumentException
    {
        public InvalidInstagramTagException(string message)
            : base(message)
        {
        }
    }

    internal class InvalidWallpaperStyleException : ArgumentException
    {
        public InvalidWallpaperStyleException(string message)
            : base(message)
        {
        }
    }
}