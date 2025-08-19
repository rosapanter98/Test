using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaApplication3.Utility
{
    public static class DbConfig
    {
        public static string GetDbPath() =>
            System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "quiz.db");

        public static string GetConnectionString() => $"Data Source={GetDbPath()}";

    }
}
