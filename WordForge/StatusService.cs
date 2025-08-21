using System;
using System.Windows;

namespace WordForge;

public static class StatusService
{
    public static event Action<string>? StatusChanged;

    public static void Log(string message)
    {
        StatusChanged?.Invoke(message);
    }
}
