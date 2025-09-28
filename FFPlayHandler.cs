using System.Diagnostics;

namespace ytCli;

public class FFPlayHandler : IDisposable
{
  private Process? _ffplayProcess;
  private CancellationTokenSource? _cancellationTokenSource;
  private Task? _inputTask;

  /// <summary>
  /// Starts playing audio from the provided stream URL
  /// </summary>
  /// <param name="streamUrl">The audio stream URL to play</param>
  /// <returns>Task representing the async operation</returns>
  public async Task PlayAsync(string streamUrl)
  {
    if (string.IsNullOrEmpty(streamUrl))
      throw new ArgumentException("Stream URL cannot be null or empty", nameof(streamUrl));

    Console.WriteLine("🎵 Now playing...");
    Console.WriteLine("Note: Audio will play through your default audio device");
    Console.WriteLine("Press 'q' to stop playback");

    // Create ffplay process
    _ffplayProcess = new Process
    {
      StartInfo = new ProcessStartInfo
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

    // Start input monitoring task
    _cancellationTokenSource = new CancellationTokenSource();
    _inputTask = Task.Run(async () =>
    {
      while (!_cancellationTokenSource.Token.IsCancellationRequested)
      {
        if (Console.KeyAvailable)
        {
          var key = Console.ReadKey(true);
          if (key.KeyChar == 'q' || key.KeyChar == 'Q')
          {
            Console.WriteLine("\nStopping playback...");
            _cancellationTokenSource.Cancel();

            // Send 'q' to ffplay to stop it gracefully
            try
            {
              if (!_ffplayProcess.HasExited && _ffplayProcess.StandardInput != null)
              {
                _ffplayProcess.StandardInput.WriteLine("q");
                _ffplayProcess.StandardInput.Flush();
              }
            }
            catch { }
            break;
          }
        }
        await Task.Delay(100);
      }
    }, _cancellationTokenSource.Token);

    try
    {
      Console.WriteLine("Starting audio playback...");
      _ffplayProcess.Start();

      // Wait for either the process to complete or user to press 'q'
      while (!_ffplayProcess.HasExited && !_cancellationTokenSource.Token.IsCancellationRequested)
      {
        await Task.Delay(100);
      }

      // If user pressed 'q', give ffplay a moment to stop gracefully
      if (_cancellationTokenSource.Token.IsCancellationRequested)
      {
        Console.WriteLine("Waiting for ffplay to stop gracefully...");
        await Task.Delay(1000); // Wait 1 second for graceful shutdown

        // If still running, force kill
        if (!_ffplayProcess.HasExited)
        {
          Console.WriteLine("Force stopping ffplay...");
          try
          {
            _ffplayProcess.Kill(true);
            _ffplayProcess.WaitForExit(2000);
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
      throw;
    }
    finally
    {
      await CleanupAsync();
    }
  }

  /// <summary>
  /// Stops the current playback
  /// </summary>
  public async Task StopAsync()
  {
    if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
    {
      _cancellationTokenSource.Cancel();
    }

    await CleanupAsync();
  }

  /// <summary>
  /// Checks if ffplay is currently running
  /// </summary>
  public bool IsPlaying => _ffplayProcess != null && !_ffplayProcess.HasExited;

  /// <summary>
  /// Gets the exit code of the ffplay process (if it has exited)
  /// </summary>
  public int? ExitCode => _ffplayProcess?.ExitCode;

  /// <summary>
  /// Cleans up resources
  /// </summary>
  private async Task CleanupAsync()
  {
    if (_cancellationTokenSource != null)
    {
      _cancellationTokenSource.Cancel();
    }

    // Final cleanup - ensure process is terminated
    if (_ffplayProcess != null && !_ffplayProcess.HasExited)
    {
      try
      {
        _ffplayProcess.Kill(true);
        _ffplayProcess.WaitForExit(1000);
      }
      catch (Exception cleanupEx)
      {
        Console.WriteLine($"Warning: Could not clean up ffplay process: {cleanupEx.Message}");
      }
    }

    // Dispose the process
    try
    {
      _ffplayProcess?.Dispose();
    }
    catch { }

    // Wait for input task to complete
    if (_inputTask != null)
    {
      try
      {
        await _inputTask;
      }
      catch { }
    }

    // Dispose cancellation token
    _cancellationTokenSource?.Dispose();
  }

  /// <summary>
  /// Disposes the FFPlayHandler and cleans up all resources
  /// </summary>
  public void Dispose()
  {
    CleanupAsync().Wait();
  }
}
