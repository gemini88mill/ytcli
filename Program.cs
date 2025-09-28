using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using FFMpegCore;
using FFMpegCore.Enums;

var yt = new YoutubeClient();

Console.WriteLine("YouTube Audio Streamer");
Console.WriteLine("====================");

// Check if FFmpeg is installed
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
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WinGet", "Packages", "Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe", "ffmpeg-6.1.1-full_build", "bin", "ffmpeg.exe")
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
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
  }
  else
  {
    Console.WriteLine($"✅ FFmpeg found at: {foundFfmpegPath}");
  }
}
catch (Exception ex)
{
  Console.WriteLine($"❌ Error checking FFmpeg: {ex.Message}");
  Console.WriteLine("Please ensure FFmpeg is properly installed and accessible.");
  Console.WriteLine("Press any key to exit...");
  Console.ReadKey();
  return;
}

// Get URL from user input
Console.Write("Enter YouTube URL: ");
var url = Console.ReadLine();

if (string.IsNullOrEmpty(url))
{
  Console.WriteLine("No URL provided. Exiting...");
  return;
}

try
{
  Console.WriteLine("Fetching video information...");

  // Get video info
  var video = await yt.Videos.GetAsync(url);
  Console.WriteLine($"Title: {video.Title}");
  Console.WriteLine($"Duration: {video.Duration}");

  // Get stream manifest
  var streamManifest = await yt.Videos.Streams.GetManifestAsync(url);

  // Get the best audio stream
  var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

  if (audioStreamInfo == null)
  {
    Console.WriteLine("No audio stream found for this video.");
    return;
  }

  Console.WriteLine($"Audio format: {audioStreamInfo.Container}");
  Console.WriteLine($"Audio bitrate: {audioStreamInfo.Bitrate}");

  Console.WriteLine("Starting audio stream...");
  Console.WriteLine("Press 'q' and Enter to stop playback at any time.");

  // Get the stream URL
  var streamUrl = audioStreamInfo.Url;

  Console.WriteLine("🎵 Now playing...");
  Console.WriteLine("Note: Audio will play through your default audio device");
  Console.WriteLine("Press 'q' to stop playback");

  // Use ffplay to stream and play the audio directly
  var ffplayProcess = new System.Diagnostics.Process
  {
    StartInfo = new System.Diagnostics.ProcessStartInfo
    {
      FileName = "ffplay",
      Arguments = $"-i \"{streamUrl}\" -nodisp -autoexit",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      RedirectStandardInput = true, // Enable input redirection
      CreateNoWindow = true
    }
  };

  // Start a background task to monitor for 'q' input
  var cancellationTokenSource = new CancellationTokenSource();
  var inputTask = Task.Run(async () =>
  {
    while (!cancellationTokenSource.Token.IsCancellationRequested)
    {
      if (Console.KeyAvailable)
      {
        var key = Console.ReadKey(true);
        if (key.KeyChar == 'q' || key.KeyChar == 'Q')
        {
          Console.WriteLine("\nStopping playback...");
          cancellationTokenSource.Cancel();

          // Send 'q' to ffplay to stop it gracefully
          try
          {
            if (!ffplayProcess.HasExited && ffplayProcess.StandardInput != null)
            {
              ffplayProcess.StandardInput.WriteLine("q");
              ffplayProcess.StandardInput.Flush();
            }
          }
          catch { }
          break;
        }
      }
      await Task.Delay(100);
    }
  }, cancellationTokenSource.Token);

  try
  {
    Console.WriteLine("Starting audio playback...");
    ffplayProcess.Start();

    // Wait for either the process to complete or user to press 'q'
    while (!ffplayProcess.HasExited && !cancellationTokenSource.Token.IsCancellationRequested)
    {
      await Task.Delay(100);
    }

    // If user pressed 'q', give ffplay a moment to stop gracefully
    if (cancellationTokenSource.Token.IsCancellationRequested)
    {
      Console.WriteLine("Waiting for ffplay to stop gracefully...");
      await Task.Delay(1000); // Wait 1 second for graceful shutdown

      // If still running, force kill
      if (!ffplayProcess.HasExited)
      {
        Console.WriteLine("Force stopping ffplay...");
        try
        {
          ffplayProcess.Kill(true);
          ffplayProcess.WaitForExit(2000);
        }
        catch (Exception killEx)
        {
          Console.WriteLine($"Warning: Could not stop ffplay: {killEx.Message}");
        }
      }
    }

    Console.WriteLine("Playback finished.");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"Error during playback: {ex.Message}");
  }
  finally
  {
    cancellationTokenSource.Cancel();

    // Final cleanup
    if (!ffplayProcess.HasExited)
    {
      try
      {
        ffplayProcess.Kill(true);
        ffplayProcess.WaitForExit(1000);
      }
      catch { }
    }

    try
    {
      ffplayProcess.Dispose();
    }
    catch { }
  }
}
catch (Exception ex)
{
  Console.WriteLine($"Error: {ex.Message}");
}
