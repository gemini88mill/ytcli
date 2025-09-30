using System.CommandLine;
using ytCli;

// Create the root command
var rootCommand = new RootCommand("YouTube Audio Streamer - Stream audio from YouTube videos");

// Create URL option
var urlOption = new Option<string?>("--url")
{
  Description = "YouTube video URL to stream"
};

// Create search option
var searchOption = new Option<string?>("--search", ["-s"])
{
  Description = "Search term to find a video to stream"
};

// Create verbose option
var verboseOption = new Option<bool>("--verbose")
{
  Description = "Show detailed information"
};

// Add options to root command
rootCommand.Add(urlOption);
rootCommand.Add(searchOption);
rootCommand.Add(verboseOption);

// Set the action
rootCommand.SetAction(async parseResult =>
{
  // Check if FFmpeg is installed
  if (!Helper.CheckFFmpegInstallation())
  {
    return 1;
  }

  YoutubeStream? youtubeStream = null;

  try
  {
    // Get parameter values from parse result
    var url = parseResult.GetValue(urlOption);
    var search = parseResult.GetValue(searchOption);
    var verbose = parseResult.GetValue(verboseOption);

    // Determine input method
    if (!string.IsNullOrEmpty(url))
    {
      youtubeStream = new YoutubeStream(url);
    }
    else if (!string.IsNullOrEmpty(search))
    {
      youtubeStream = new YoutubeStream(search, true);
    }
    else
    {
      Logger.Error("Invalid input");
      Logger.Info("Use --help for more information.");
      return 1;
    }

    var streamUrl = await youtubeStream.InitAndGetStreamUrl(verbose);

    // Play the audio using FFPlayHandler
    using var ffplayHandler = new FFPlayHandler();
    await ffplayHandler.PlayAsync(streamUrl,
      youtubeStream.Video?.Title,
      youtubeStream.Video?.Author?.ChannelTitle,
      youtubeStream.Video?.Duration);
    return 0;
  }
  catch (Exception ex)
  {
    Logger.HandleException(ex, "application");
    return 1;
  }
});

// Parse and execute the command
return await rootCommand.Parse(args).InvokeAsync();

