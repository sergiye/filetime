using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
      ConsoleEx.WriteLine("\t-fix: try to set date (and time if exists) from file name");
      
      ConsoleEx.WriteLine();
      ConsoleEx.WriteLine($"Example1: {appName} someFile.txt -c {DateTime.Now.AddYears(-1):s} -m  {DateTime.Now:s} ");
      ConsoleEx.WriteLine($"Example2: {appName} *.jpg -fix -t");
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

        if (string.IsNullOrEmpty(filePath)) {
          ConsoleEx.WriteWarning("File path is required.");
          ShowHelpText(asm.GetName().Name);
          return;
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
            case "-fix":
              creationTime = writeTime = accessTime = DateTime.MaxValue;
              break;
            case "-?":
            case "-h":
            case "/?":
              ShowHelpText(asm.GetName().Name);
              return;
          }
          i++;
        }

        var useFileMask = filePath.Contains("*") || filePath.Contains("!"); 
        
        if (useFileMask) {

          var workPath = Path.GetDirectoryName(filePath);
          if (string.IsNullOrEmpty(workPath))
            workPath = Path.GetDirectoryName(asm.Location);

          var fileMask = Path.GetFileName(filePath);

          var files = Directory.GetFiles(workPath, fileMask, SearchOption.TopDirectoryOnly).ToList();
          ConsoleEx.WriteMessage($"Found '{files.Count}' files...");
          if (files.Count <= 0) return;
          
          files.Sort();
          foreach (var f in files) {
            SetFileTime(f, creationTime, writeTime, accessTime, addRandomTime);
          }
        }
        else {
          if (File.Exists(filePath)) {
            SetFileTime(filePath, creationTime, writeTime, accessTime, addRandomTime);
          }
          else {
            ConsoleEx.WriteWarning($"File not found: {filePath}");
          }
        }
      }
      catch (Exception ex) {
        ConsoleEx.WriteError(ex);
      }
    }

    private static void SetFileTime(string filePath, DateTime creationTime, DateTime writeTime, DateTime accessTime,
      bool addRandomTime) {

      if (creationTime == DateTime.MaxValue && writeTime == DateTime.MaxValue && accessTime == DateTime.MaxValue) {

        //try to set from file name
        var fileName = Path.GetFileName(filePath);
        //2021-11-28_12-29-42 or 2021-11-28
        var dtFormats = new []{"yyyy-MM-dd_HH-mm-ss", "yyyy-MM-dd"};
        foreach (var dtFormat in dtFormats) {
          var datePart = fileName.Substring(0, Math.Min(fileName.Length, dtFormat.Length));
          if (!DateTime.TryParseExact(datePart, dtFormat, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces,
                out var dt)) continue;
          var time = CheckTimePart(dt, addRandomTime);
          File.SetCreationTime(filePath, time);
          File.SetLastWriteTime(filePath, time);
          File.SetLastAccessTime(filePath, time);
          break;
        }
      }
      else {
        //change
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

      ConsoleEx.WriteInfo($"\t{filePath}: Created {File.GetCreationTime(filePath):s}, Modified: {File.GetLastWriteTime(filePath):s}, Accessed: {File.GetLastAccessTime(filePath):s}");
    }
    
  }
}