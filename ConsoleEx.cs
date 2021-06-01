using System;

namespace FileTime {
  
  public static class ConsoleEx {

    private static readonly object locker = new object();

    public static ConsoleColor BackColor = ConsoleColor.Black;
    public static ConsoleColor WarningColor = ConsoleColor.Yellow;
    public static ConsoleColor ErrorColor = ConsoleColor.Red;
    public static ConsoleColor InfoColor = ConsoleColor.White;
    public static ConsoleColor MessageColor = ConsoleColor.Green;
    public static ConsoleColor LogColor = ConsoleColor.Gray;

    public static void WriteLine(string message = null, ConsoleColor? color = null, ConsoleColor? backColor = null) {
      Write(message, color, backColor, true);
    }

    public static void WriteLog(string message = null, ConsoleColor? color = null, ConsoleColor? backColor = null) {
      lock (locker) {
        Write($"{DateTime.Now:hh\\:mm\\:ss} - ", LogColor);
        WriteLine(message, color ?? InfoColor, backColor);
      }
    }

    public static void WriteInfo(string message) {
        WriteLine(message, InfoColor);
    }

    public static void WriteMessage(string message) {
        WriteLine(message, MessageColor);
    }

    public static void WriteWarning(string message) {
        WriteLine(message, WarningColor);
    }

    public static void WriteError(Exception ex, string message = null, ConsoleColor? color = null, ConsoleColor? backColor = null) {
      lock (locker) {
        WriteLine(message ?? ex.Message, color ?? WarningColor, backColor);
        WriteLine(ex.StackTrace, color ?? ErrorColor, backColor);
      }
    }

    public static void Write(string message = null, ConsoleColor? color = null, ConsoleColor? backColor = null, bool newLine = false) {
      
      if (!Environment.UserInteractive) {
        // running as service
        return; //todo: add file log writing
      }

      lock (locker) {
        if (backColor.HasValue)
          Console.BackgroundColor = backColor.Value;
        if (color.HasValue)
          Console.ForegroundColor = color.Value;
        if (newLine)
          Console.WriteLine(message);
        else
          Console.Write(message);
        Console.ResetColor();
      }
    }
  }
}