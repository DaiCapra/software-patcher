using System;

namespace SoftwarePatcher.Diagnostics
{
    public class Logger
    {
        public void Log(string text)
        {
            var time = DateTime.Now.ToShortTimeString();
            var s = $"{time}\t {text}";
            Console.WriteLine(s);
        }

        public void Error(string text)
        {
            Log($"Error: {text}");
        }

        public void Error(Exception exception)
        {
            Log($"Error: {exception.Message}");
        }
    }
}