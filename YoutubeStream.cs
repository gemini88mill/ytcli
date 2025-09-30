using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using YoutubeExplode.Search;

namespace ytCli;

public class YoutubeStream
{
  private readonly YoutubeClient _youtubeClient;
  private readonly string _url;
  private readonly string _searchTerm;
  private Video? _video;
  private StreamManifest? _streamManifest;

  /// <summary>
  /// Constructor for direct YouTube URL
  /// </summary>
  /// <param name="url">Direct YouTube URL (e.g., https://www.youtube.com/watch?v=VIDEO_ID)</param>
  public YoutubeStream(string url)
  {
    _youtubeClient = new YoutubeClient();
    _url = url ?? throw new ArgumentNullException(nameof(url));
    _searchTerm = string.Empty;
  }

  /// <summary>
  /// Constructor for YouTube search term
  /// </summary>
  /// <param name="searchTerm">Search term to find the closest relevant video</param>
  /// <param name="isSearchTerm">Flag to indicate this is a search term (not a URL)</param>
  public YoutubeStream(string searchTerm, bool isSearchTerm)
  {
    if (!isSearchTerm)
      throw new ArgumentException("When using search term constructor, isSearchTerm must be true", nameof(isSearchTerm));

    _youtubeClient = new YoutubeClient();
    _searchTerm = searchTerm ?? throw new ArgumentNullException(nameof(searchTerm));
    _url = string.Empty;
  }

  /// <summary>
  /// Default constructor
  /// </summary>
  public YoutubeStream()
  {
    _youtubeClient = new YoutubeClient();
    _url = string.Empty;
    _searchTerm = string.Empty;
  }

  /// <summary>
  /// Gets the video information
  /// </summary>
  public Video? Video => _video;

  /// <summary>
  /// Gets the stream manifest
  /// </summary>
  public StreamManifest? StreamManifest => _streamManifest;

  /// <summary>
  /// Gets the URL being used (either direct URL or resolved from search)
  /// </summary>
  public string Url => _url;

  /// <summary>
  /// Gets the search term used
  /// </summary>
  public string SearchTerm => _searchTerm;

  /// <summary>
  /// Initializes the video and stream manifest
  /// </summary>
  /// <returns>Task representing the async operation</returns>
  public async Task InitializeAsync()
  {
    try
    {
      string videoUrl = _url;

      // If we have a search term, find the best matching video
      if (!string.IsNullOrEmpty(_searchTerm))
      {
        Console.WriteLine($"Searching for: {_searchTerm}");

        var searchResults = _youtubeClient.Search.GetVideosAsync(_searchTerm);
        VideoSearchResult? firstSearchResult = null;

        await foreach (var searchResult in searchResults)
        {
          firstSearchResult = searchResult;
          break; // Get the first search result and break
        }

        if (firstSearchResult == null)
        {
          throw new InvalidOperationException($"No videos found for search term: {_searchTerm}");
        }

        videoUrl = firstSearchResult.Url;
        Console.WriteLine($"Found video: {firstSearchResult.Title}");
      }

      // Get video information
      _video = await _youtubeClient.Videos.GetAsync(videoUrl);

      if (_video == null)
      {
        throw new InvalidOperationException("Could not retrieve video information");
      }

      // Get stream manifest
      _streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(videoUrl);

      if (_streamManifest == null)
      {
        throw new InvalidOperationException("Could not retrieve stream manifest");
      }

      Console.WriteLine($"Video: {_video.Title}");
      Console.WriteLine($"Duration: {_video.Duration}");
      Console.WriteLine($"Author: {_video.Author}");
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
  public IStreamInfo? GetBestAudioStream()
  {
    if (_streamManifest == null)
    {
      throw new InvalidOperationException("Stream manifest not initialized. Call InitializeAsync() first.");
    }

    return _streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
  }

  /// <summary>
  /// Gets all available audio streams
  /// </summary>
  /// <returns>Collection of audio stream infos</returns>
  public IEnumerable<IStreamInfo> GetAudioStreams()
  {
    if (_streamManifest == null)
    {
      throw new InvalidOperationException("Stream manifest not initialized. Call InitializeAsync() first.");
    }

    return _streamManifest.GetAudioOnlyStreams();
  }

  /// <summary>
  /// Gets the stream URL for the best audio stream
  /// </summary>
  /// <returns>The stream URL</returns>
  public string GetStreamUrl()
  {
    var audioStream = GetBestAudioStream();
    if (audioStream == null)
    {
      throw new InvalidOperationException("No audio stream available");
    }

    return audioStream.Url;
  }

  /// <summary>
  /// Displays available audio streams
  /// </summary>
  public void DisplayAudioStreams()
  {
    if (_streamManifest == null)
    {
      Console.WriteLine("Stream manifest not initialized. Call InitializeAsync() first.");
      return;
    }

    var audioStreams = GetAudioStreams().ToList();

    if (!audioStreams.Any())
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
