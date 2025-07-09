using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.Repositories
{
    public class JsonToDoRepository : IToDoRepository
    {
        private readonly string _baseDir;

        public JsonToDoRepository(string? baseDirectory = null)
        {
            _baseDir = baseDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AvaloniaApplication3", "Users");
        }

        public List<ToDoItem> LoadItems(string username)
        {
            var filePath = GetUserFilePath(username);

            if (!File.Exists(filePath))
                return new List<ToDoItem>();

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<ToDoItem>();

                return JsonSerializer.Deserialize<List<ToDoItem>>(json) ?? new List<ToDoItem>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load items for {username}: {ex}");
                return new List<ToDoItem>();
            }
        }

        public void SaveItems(string username, List<ToDoItem> items)
        {
            try
            {
                string filePath = GetUserFilePath(username);
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to save items for {username}: {ex}");
            }
        }

        private string GetUserFilePath(string username)
        {
            string safeName = username.ToLowerInvariant();
            return Path.Combine(_baseDir, $"{safeName}.json");
        }
    }
}
