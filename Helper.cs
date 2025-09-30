namespace ytCli;

public static class Helper
{
    // Helper method to check FFmpeg installation
public static bool CheckFFmpegInstallation()
{
  Console.WriteLine("Checking for FFmpeg installation...");
  try
  {
    // Try to find FFmpeg in common locations
    var ffmpegPaths = new[]
    {
      "ffmpeg", // In PATH
      "ffmpeg.exe", // Windows executable
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ffmpeg", "bin", "ffmpeg.exe"),
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "ffmpeg", "bin", "ffmpeg.exe"),
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Microsoft",
        "WinGet",
        "Packages",
        "Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe",
        "ffmpeg-6.1.1-full_build",
        "bin",
        "ffmpeg.exe")
    };

    string? foundFfmpegPath = null;

    foreach (var path in ffmpegPaths)
    {
      try
      {
        if (path == "ffmpeg" || path == "ffmpeg.exe")
        {
          // Check if ffmpeg is in PATH
          var process = new System.Diagnostics.Process
          {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
              FileName = "ffmpeg",
              Arguments = "-version",
              RedirectStandardOutput = true,
              RedirectStandardError = true,
              UseShellExecute = false,
              CreateNoWindow = true
            }
          };

          if (process.Start())
          {
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
              foundFfmpegPath = "ffmpeg (in PATH)";
              break;
            }
          }
        }
        else if (File.Exists(path))
        {
          foundFfmpegPath = path;
          break;
        }
      }
      catch
      {
        // Continue checking other paths
      }
    }

    if (foundFfmpegPath == null)
    {
      Console.WriteLine("❌ FFmpeg not found!");
      Console.WriteLine();
      Console.WriteLine("FFmpeg is required for audio conversion. Please download and install it:");
      Console.WriteLine("📥 Download from: https://ffmpeg.org/download.html");
      Console.WriteLine();
      Console.WriteLine("Windows users:");
      Console.WriteLine("1. Download the latest release from https://github.com/BtbN/FFmpeg-Builds/releases");
      Console.WriteLine("2. Extract the zip file");
      Console.WriteLine("3. Add the 'bin' folder to your system PATH");
      Console.WriteLine("4. Restart your terminal/command prompt");
      Console.WriteLine();
      Console.WriteLine("Alternative: Install via package manager:");
      Console.WriteLine("• Chocolatey: choco install ffmpeg");
      Console.WriteLine("• Scoop: scoop install ffmpeg");
      Console.WriteLine("• Winget: winget install FFmpeg");
      return false;
    }
    else
    {
      Console.WriteLine($"✅ FFmpeg found at: {foundFfmpegPath}");
      return true;
    }
  }
  catch (Exception ex)
  {
    Console.WriteLine($"❌ Error checking FFmpeg: {ex.Message}");
    Console.WriteLine("Please ensure FFmpeg is properly installed and accessible.");
    return false;
  }
}

}