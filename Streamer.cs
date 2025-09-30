using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode.Search;

namespace ytCli;

public class Streamer
{
  private readonly bool _isSearchTerm;
  private readonly YoutubeClient _youtubeClient;

  /// <summary>
  /// Constructor for YouTube search term
  /// </summary>
  /// <param name="searchTerm">Search term to find the closest relevant video</param>
  /// <param name="isSearchTerm">Flag to indicate this is a search term (not a URL)</param>
  public Streamer(string searchTerm, bool isSearchTerm)
  {
    _isSearchTerm = isSearchTerm;

    _youtubeClient = new YoutubeClient();
    SearchTerm = searchTerm ?? throw new ArgumentNullException(nameof(searchTerm));
  }

  /// <summary>
  /// Default constructor
  /// </summary>
  public Streamer()
  {
    _youtubeClient = new YoutubeClient();
    SearchTerm = string.Empty;
  }

  /// <summary>
  /// Gets the video information
  /// </summary>
  public Video? Video { get; private set; }

  /// <summary>
  /// Gets the stream manifest
  /// </summary>
  private StreamManifest? StreamManifest { get; set; }

  /// <summary>
  /// Gets the search term used
  /// </summary>
  private string SearchTerm { get; }

  /// <summary>
  /// Initializes the video and stream manifest
  /// </summary>
  /// <returns>Task representing the async operation</returns>
  private async Task InitializeAsync()
  {
    try
    {
      string videoUrl;

      // If we have a search term, find the best matching video
      if (!string.IsNullOrEmpty(SearchTerm))
      {
        IAsyncEnumerable<VideoSearchResult>? searchResults;
        await Logger.ShowStatusAsync($"Searching for: {SearchTerm}", async () =>
          {
            searchResults = _youtubeClient.Search.GetVideosAsync(SearchTerm);
            VideoSearchResult? firstSearchResult = null;

            await foreach (var searchResult in searchResults)
            {
              firstSearchResult = searchResult;
              break; // Get the first search result and break
            }

            if (firstSearchResult == null)
            {
              throw new InvalidOperationException($"No videos found for search term: {SearchTerm}");
            }
            
            videoUrl = firstSearchResult.Url;
            
            // Get video information
            Video = await _youtubeClient.Videos.GetAsync(videoUrl);

            if (Video == null)
            {
              throw new InvalidOperationException("Could not retrieve video information");
            }

            // Get stream manifest
            StreamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);

            if (StreamManifest == null)
            {
              throw new InvalidOperationException("Could not retrieve stream manifest");
            }
            
            return Task.CompletedTask;
          },
          "Found video");
        // var searchResults = _youtubeClient.Search.GetVideosAsync(_searchTerm);
      }
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to initialize YouTube stream: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Gets the best audio stream from the manifest
  /// </summary>
  /// <returns>The best audio stream info, or null if none found</returns>
  private IStreamInfo GetBestAudioStream()
  {
    return StreamManifest == null
      ? throw new InvalidOperationException("Stream manifest not initialized. Call InitializeAsync() first.")
      : StreamManifest.GetAudioOnlyStreams()
        .GetWithHighestBitrate();
  }

  /// <summary>
  /// Gets all available audio streams
  /// </summary>
  /// <returns>Collection of audio stream infos</returns>
  private IEnumerable<IStreamInfo> GetAudioStreams()
  {
    return StreamManifest == null
      ? throw new InvalidOperationException("Stream manifest not initialized. Call InitializeAsync() first.")
      : StreamManifest.GetAudioOnlyStreams();
  }

  /// <summary>
  /// Gets the stream URL for the best audio stream
  /// </summary>
  /// <returns>The stream URL</returns>
  private string GetStreamUrl()
  {
    var audioStream = GetBestAudioStream();
    return audioStream == null ? throw new InvalidOperationException("No audio stream available") : audioStream.Url;
  }

  /// <summary>
  /// Displays available audio streams
  /// </summary>
  private void DisplayAudioStreams()
  {
    if (StreamManifest == null)
    {
      Console.WriteLine("Stream manifest not initialized. Call InitializeAsync() first.");
      return;
    }

    var audioStreams = GetAudioStreams().ToList();

    if (audioStreams.Count == 0)
    {
      Console.WriteLine("No audio streams available for this video.");
      return;
    }

    Console.WriteLine("Available audio streams:");
    Console.WriteLine("========================");

    foreach (var stream in audioStreams.OrderByDescending(s => s.Bitrate))
    {
      Console.WriteLine($"Format: {stream.Container}");
      Console.WriteLine($"Bitrate: {stream.Bitrate} bps");
      Console.WriteLine($"Size: {stream.Size}");
      Console.WriteLine("---");
    }
  }
  
  public async Task<string> InitAndGetStreamUrl(bool verbose = false)
  {
    await InitializeAsync();
    var streamUrl = GetStreamUrl();
    
    if (verbose)
    {
      DisplayAudioStreams();
    }

    return streamUrl;
  }
}
