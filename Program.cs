using System;
using System.IO;
using System.Reflection;

namespace FileTime {
  
  internal class Program {

    private static readonly Random random = new Random();

    public static DateTime GetBuildTime(Version ver) {
      var buildTime = new DateTime(2000, 1, 1).AddDays(ver.Build).AddSeconds(ver.Revision * 2);
      if (TimeZone.IsDaylightSavingTime(DateTime.Now, TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year)))
        buildTime = buildTime.AddHours(1);
      return buildTime;
    }

    private static void ShowMainInfo(Assembly asm) {
      ConsoleEx.WriteLine();
      var ver = asm.GetName().Version;
      ConsoleEx.WriteInfo($"{asm.GetName().Name} Version: {ver.ToString(3)}; Build time: {GetBuildTime(ver):yyyy/MM/dd HH:mm:ss}");
    }

    private static void ShowHelpText(string appName) {
      
      ConsoleEx.WriteLine();
      ConsoleEx.WriteLine($"Usage: {appName} <path> [-c createdTime] [-w lastWriteTime] [-a lastAccessTime] [-t]");
      ConsoleEx.WriteLine();
      ConsoleEx.WriteLine("Where:");
      ConsoleEx.WriteLine("\t<path>: input file");

      ConsoleEx.WriteLine("\t-c: change file creation time");
      ConsoleEx.WriteLine("\t-w: change file last write time");
      ConsoleEx.WriteLine("\t-a: change file last access time");
      ConsoleEx.WriteLine("\t-t: add random time if only date provided");
      
      ConsoleEx.WriteLine();
      ConsoleEx.WriteLine($"Example: {appName} someFile.txt -c {DateTime.Now.AddYears(-1):s} -m  {DateTime.Now:s} ");
      ConsoleEx.WriteLine();
    }

    private static DateTime CheckTimePart(DateTime dateTime, bool addRandomTime) {
      if (dateTime.TimeOfDay == TimeSpan.Zero && addRandomTime) {
        dateTime = dateTime.AddSeconds(random.Next(1, 24 * 60 * 60) - 1);
      }
      return dateTime;
    }
    
    public static void Main(string[] args) {
      
      try {
        var asm = Assembly.GetExecutingAssembly();
        ShowMainInfo(asm);

        if (args.Length == 0) {
          ShowHelpText(asm.GetName().Name);
          return;
        }

        var filePath = args[0];
        if (!File.Exists(filePath)) {
          ConsoleEx.WriteWarning($"File not found: {filePath}");
        }

        var i = 1;
        var creationTime = DateTime.MinValue;
        var accessTime = DateTime.MinValue;
        var writeTime = DateTime.MinValue;
        var addRandomTime = false;
        
        while (i < args.Length) {

          var currentArg = args[i].ToLower().Trim();
          switch (currentArg) {
            case "-c":
              if (args.Length <= i + 1) {
                ConsoleEx.WriteWarning($"Missed '{currentArg}' parameter value.");
                return;
              }
              if (!DateTime.TryParse(args[i + 1], out creationTime)) {
                ConsoleEx.WriteWarning($"Invalid '{currentArg}' parameter value.");
                return;
              }
              i++;
              break;
            case "-w":
              if (args.Length <= (i + 1)) {
                ConsoleEx.WriteWarning($"Missed '{currentArg}' parameter value.");
                return;
              }
              if (!DateTime.TryParse(args[i + 1], out writeTime)) {
                ConsoleEx.WriteWarning($"Invalid '{currentArg}' parameter value.");
                return;
              }
              i++;
              break;
            case "-a":
              if (args.Length <= (i + 1)) {
                ConsoleEx.WriteWarning($"Missed '{currentArg}' parameter value.");
                return;
              }
              if (!DateTime.TryParse(args[i + 1], out accessTime)) {
                ConsoleEx.WriteWarning($"Invalid '{currentArg}' parameter value.");
                return;
              }
              i++;
              break;
            case "-t":
              addRandomTime = true;
              break;
          }
          i++;
        }

        if (creationTime == DateTime.MinValue && writeTime == DateTime.MinValue && accessTime == DateTime.MinValue) {
          //display current
          ConsoleEx.WriteMessage($"File '{filePath}' info:");
        }
        else {
          //change
          ConsoleEx.WriteMessage($"File '{filePath}' updated:");
          if (creationTime != DateTime.MinValue) {
            var time = CheckTimePart(creationTime, addRandomTime);
            File.SetCreationTime(filePath, time);
          }
          if (writeTime != DateTime.MinValue) {
            var time = CheckTimePart(writeTime, addRandomTime);
            File.SetLastWriteTime(filePath, time);
          }
          if (accessTime != DateTime.MinValue) {
            var time = CheckTimePart(accessTime, addRandomTime);
            File.SetLastAccessTime(filePath, time);
          }
        }
        ConsoleEx.WriteInfo($"\tCreated:  {File.GetCreationTime(filePath):s}");
        ConsoleEx.WriteInfo($"\tModified: {File.GetLastWriteTime(filePath):s}");
        ConsoleEx.WriteInfo($"\tAccessed: {File.GetLastAccessTime(filePath):s}");
      }
      catch (Exception ex) {
        ConsoleEx.WriteError(ex);
      }
    }
  }
}