using System;
using System.IO;

namespace SpendPoint
{
    public static class Logger
    {
        public static void WriteLog(string message, string directory)
        {
            if (!File.Exists(Path.Combine(directory, "Log.txt")))
                File.Create(Path.Combine(directory, "Log.txt")).Close();
            File.AppendAllText(Path.Combine(directory, "Log.txt"), Environment.NewLine + Environment.NewLine +
                message + Environment.NewLine +
                "Date of occurence : " + DateTime.Now + Environment.NewLine +
                "----------------------------------");
        }
    }
}