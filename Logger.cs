using Spectre.Console;

namespace ytCli;

/// <summary>
/// A logging utility class that uses Spectre.Console for styled console output
/// </summary>
public static class Logger
{
  /// <summary>
  /// Outputs an informational message with blue styling
  /// </summary>
  /// <param name="message">The message to display</param>
  public static void Info(string message)
  {
    AnsiConsole.MarkupLine($"[blue]ℹ[/] {message}");
  }

  /// <summary>
  /// Outputs an informational message with custom formatting
  /// </summary>
  /// <param name="message">The message to display</param>
  /// <param name="icon">Optional icon to display before the message</param>
  public static void Info(string message, string icon = "ℹ")
  {
    AnsiConsole.MarkupLine($"[blue]{icon}[/] {message}");
  }

  /// <summary>
  /// Outputs a warning message with yellow styling
  /// </summary>
  /// <param name="message">The message to display</param>
  public static void Warning(string message)
  {
    AnsiConsole.MarkupLine($"[yellow]⚠[/] {message}");
  }

  /// <summary>
  /// Outputs a warning message with custom formatting
  /// </summary>
  /// <param name="message">The message to display</param>
  /// <param name="icon">Optional icon to display before the message</param>
  public static void Warning(string message, string icon = "⚠")
  {
    AnsiConsole.MarkupLine($"[yellow]{icon}[/] {message}");
  }

  /// <summary>
  /// Outputs an error message with red styling
  /// </summary>
  /// <param name="message">The message to display</param>
  public static void Error(string message)
  {
    AnsiConsole.MarkupLine($"[red]❌[/] {message}");
  }

  /// <summary>
  /// Outputs an error message with custom formatting
  /// </summary>
  /// <param name="message">The message to display</param>
  /// <param name="icon">Optional icon to display before the message</param>
  public static void Error(string message, string icon = "❌")
  {
    AnsiConsole.MarkupLine($"[red]{icon}[/] {message}");
  }

  /// <summary>
  /// Outputs a success message with green styling
  /// </summary>
  /// <param name="message">The message to display</param>
  public static void Success(string message)
  {
    AnsiConsole.MarkupLine($"[green]✅[/] {message}");
  }

  /// <summary>
  /// Outputs a success message with custom formatting
  /// </summary>
  /// <param name="message">The message to display</param>
  /// <param name="icon">Optional icon to display before the message</param>
  public static void Success(string message, string icon = "✅")
  {
    AnsiConsole.MarkupLine($"[green]{icon}[/] {message}");
  }

  /// <summary>
  /// Handles exceptions with detailed error information and optional stack trace
  /// </summary>
  /// <param name="exception">The exception to handle</param>
  /// <param name="context">Optional context about where the exception occurred</param>
  /// <param name="showStackTrace">Whether to display the stack trace (default: false)</param>
  public static void HandleException(Exception exception, string? context = null, bool showStackTrace = false)
  {
    // Display the error message
    Error($"Exception occurred{(string.IsNullOrEmpty(context) ? "" : $" in {context}")}: {exception.Message}");

    // Show inner exception if present
    if (exception.InnerException != null)
    {
      Error($"Inner exception: {exception.InnerException.Message}");
    }

    // Show stack trace if requested
    if (showStackTrace)
    {
      AnsiConsole.WriteException(exception);
    }
  }

  /// <summary>
  /// Outputs a progress message with spinner animation
  /// </summary>
  /// <param name="message">The message to display</param>
  /// <param name="action">The action to perform while showing progress</param>
  public static async Task ShowProgressAsync(string message, Func<Task> action)
  {
    await AnsiConsole.Status()
        .StartAsync(message, async ctx =>
        {
          await action();
        });
  }

  /// <summary>
  /// Outputs a progress message with spinner animation and returns the result
  /// </summary>
  /// <typeparam name="T">The type of the result</typeparam>
  /// <param name="message">The message to display</param>
  /// <param name="action">The action to perform while showing progress</param>
  /// <returns>The result of the action</returns>
  public static async Task<T> ShowProgressAsync<T>(string message, Func<Task<T>> action)
  {
    return await AnsiConsole.Status()
        .StartAsync(message, async ctx =>
        {
          return await action();
        });
  }

  /// <summary>
  /// Creates a rule separator with optional title
  /// </summary>
  /// <param name="title">Optional title for the rule</param>
  public static void Rule(string? title = null)
  {
    if (string.IsNullOrEmpty(title))
    {
      AnsiConsole.Write(new Rule());
    }
    else
    {
      AnsiConsole.Write(new Rule(title));
    }
  }

  /// <summary>
  /// Outputs a header with styling
  /// </summary>
  /// <param name="title">The header title</param>
  public static void Header(string title)
  {
    AnsiConsole.Write(new FigletText(title)
        .Color(Color.Blue)
        .Centered());
  }

  /// <summary>
  /// Outputs a subheader with styling
  /// </summary>
  /// <param name="title">The subheader title</param>
  public static void SubHeader(string title)
  {
    AnsiConsole.MarkupLine($"[bold blue]{title}[/]");
    AnsiConsole.MarkupLine($"[dim]{new string('=', title.Length)}[/]");
  }

  /// <summary>
  /// Displays an async progress bar for music playback with song information
  /// </summary>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  /// <param name="totalDuration">Total duration in seconds</param>
  /// <param name="playbackTask">The task representing the playback process</param>
  /// <returns>Task representing the async progress operation</returns>
  public static async Task ShowPlaybackProgressAsync(string title, string author, double totalDuration, Task playbackTask)
  {
    await AnsiConsole.Progress()
      .AutoClear(false)
      .HideCompleted(false)
      .Columns(new ProgressColumn[]
      {
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new ElapsedTimeColumn(),
        new SpinnerColumn()
      })
      .StartAsync(async ctx =>
      {
        var task = ctx.AddTask($"[bold blue]{title}[/] - [bold green]{author}[/]");
        task.MaxValue = totalDuration;

        var startTime = DateTime.Now;

        while (!playbackTask.IsCompleted && !playbackTask.IsCanceled && !playbackTask.IsFaulted)
        {
          var elapsed = DateTime.Now - startTime;
          var elapsedSeconds = elapsed.TotalSeconds;

          // Update progress
          task.Value = Math.Min(elapsedSeconds, totalDuration);
          task.Description = $"[bold yellow]{FormatTime(elapsedSeconds)} / {FormatTime(totalDuration)}[/]";

          // Check if we've reached the end
          if (elapsedSeconds >= totalDuration && totalDuration > 0)
          {
            task.Value = totalDuration;
            break;
          }

          await Task.Delay(1000); // Update every second
        }

        // Ensure task is marked as complete
        task.Value = totalDuration;
        task.StopTask();
      });
  }

  /// <summary>
  /// Displays a progress bar for music playback with song information using AnsiConsole.Progress
  /// </summary>
  /// <param name="currentTime">Current playback time in seconds</param>
  /// <param name="totalDuration">Total duration in seconds</param>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  public static void ShowPlaybackProgress(double currentTime, double totalDuration, string title, string author)
  {
    // Calculate progress percentage
    var progress = totalDuration > 0 ? currentTime / totalDuration : 0;
    var progressPercentage = Math.Min(Math.Max(progress, 0), 1);

    // Format time strings
    var currentTimeStr = FormatTime(currentTime);
    var totalTimeStr = FormatTime(totalDuration);

    // Use AnsiConsole.Progress for better display
    AnsiConsole.Progress()
      .Start(ctx =>
      {
        var task = ctx.AddTask($"[bold blue]{title}[/] - [bold green]{author}[/]");
        task.MaxValue = 100;
        task.Value = progressPercentage * 100;
        task.Description = $"[bold yellow]{currentTimeStr} / {totalTimeStr}[/]";
      });
  }

  /// <summary>
  /// Displays a progress bar for music playback with song information (overloaded for TimeSpan)
  /// </summary>
  /// <param name="currentTime">Current playback time</param>
  /// <param name="totalDuration">Total duration</param>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  public static void ShowPlaybackProgress(TimeSpan currentTime, TimeSpan totalDuration, string title, string author)
  {
    ShowPlaybackProgress(currentTime.TotalSeconds, totalDuration.TotalSeconds, title, author);
  }

  /// <summary>
  /// Displays a simple progress bar without song information using AnsiConsole.Progress
  /// </summary>
  /// <param name="currentTime">Current playback time in seconds</param>
  /// <param name="totalDuration">Total duration in seconds</param>
  public static void ShowPlaybackProgress(double currentTime, double totalDuration)
  {
    // Calculate progress percentage
    var progress = totalDuration > 0 ? currentTime / totalDuration : 0;
    var progressPercentage = Math.Min(Math.Max(progress, 0), 1);

    // Format time strings
    var currentTimeStr = FormatTime(currentTime);
    var totalTimeStr = FormatTime(totalDuration);

    // Use AnsiConsole.Progress for better display
    AnsiConsole.Progress()
      .Start(ctx =>
      {
        var task = ctx.AddTask("[bold]Playback Progress[/]");
        task.MaxValue = 100;
        task.Value = progressPercentage * 100;
        task.Description = $"[bold yellow]{currentTimeStr} / {totalTimeStr}[/]";
      });
  }

  /// <summary>
  /// Displays a simple progress bar without song information (overloaded for TimeSpan)
  /// </summary>
  /// <param name="currentTime">Current playback time</param>
  /// <param name="totalDuration">Total duration</param>
  public static void ShowPlaybackProgress(TimeSpan currentTime, TimeSpan totalDuration)
  {
    ShowPlaybackProgress(currentTime.TotalSeconds, totalDuration.TotalSeconds);
  }

  /// <summary>
  /// Formats time in seconds to MM:SS or HH:MM:SS format
  /// </summary>
  /// <param name="seconds">Time in seconds</param>
  /// <returns>Formatted time string</returns>
  private static string FormatTime(double seconds)
  {
    var timeSpan = TimeSpan.FromSeconds(seconds);

    if (timeSpan.TotalHours >= 1)
    {
      return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
    else
    {
      return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
  }

  /// <summary>
  /// Displays song information in a formatted way
  /// </summary>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  /// <param name="duration">Song duration</param>
  public static void ShowSongInfo(string title, string author, TimeSpan duration)
  {
    AnsiConsole.MarkupLine($"[bold blue]Title:[/] {title}");
    AnsiConsole.MarkupLine($"[bold green]Author:[/] {author}");
    AnsiConsole.MarkupLine($"[bold yellow]Duration:[/] {FormatTime(duration.TotalSeconds)}");
  }

  /// <summary>
  /// Displays song information in a formatted way (overloaded for double duration)
  /// </summary>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  /// <param name="durationSeconds">Song duration in seconds</param>
  public static void ShowSongInfo(string title, string author, double durationSeconds)
  {
    ShowSongInfo(title, author, TimeSpan.FromSeconds(durationSeconds));
  }


  /// <summary>
  /// Displays a simple real-time progress update without using Progress context
  /// </summary>
  /// <param name="currentTime">Current playback time in seconds</param>
  /// <param name="totalDuration">Total duration in seconds</param>
  /// <param name="title">Song title</param>
  /// <param name="author">Song author/artist</param>
  public static void ShowRealTimeProgress(double currentTime, double totalDuration, string title, string author)
  {
    // Calculate progress percentage
    var progress = totalDuration > 0 ? currentTime / totalDuration : 0;
    var progressPercentage = Math.Min(Math.Max(progress, 0), 1);

    // Format time strings
    var currentTimeStr = FormatTime(currentTime);
    var totalTimeStr = FormatTime(totalDuration);

    // Create a simple progress bar
    const int barWidth = 30;
    var filledWidth = (int)(progressPercentage * barWidth);
    var emptyWidth = barWidth - filledWidth;

    // Build progress bar string
    var progressBar = "[" + new string('█', filledWidth) + new string('░', emptyWidth) + "]";
    var percentage = (int)(progressPercentage * 100);

    // Clear the current line and display progress
    Console.SetCursorPosition(0, Console.CursorTop);
    Console.Write(new string(' ', Console.WindowWidth - 1));
    Console.SetCursorPosition(0, Console.CursorTop);

    AnsiConsole.MarkupLine($"[bold blue]{title}[/] - [bold green]{author}[/]");
    AnsiConsole.MarkupLine($"[bold]{progressBar}[/] [bold yellow]{percentage}%[/]");
    AnsiConsole.MarkupLine($"[bold yellow]{currentTimeStr} / {totalTimeStr}[/]");
  }
}
