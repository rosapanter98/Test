using System;
using System.Collections.Generic;
using AvaloniaApplication3.Models;

namespace AvaloniaApplication3.Repositories
{
    public interface IToDoRepository
    {
        List<ToDoItem> LoadItems(string username);
        void SaveItems(string username, List<ToDoItem> items);
    }
}
