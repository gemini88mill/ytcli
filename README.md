# YouTube Audio Streamer (ytcli)

A simple command-line tool to download and convert YouTube videos to MP3 audio files using C# and .NET.

## Features

- 🎵 Download audio from any YouTube video
- 🔄 Automatic conversion to MP3 format
- 📁 Smart filename generation based on video title
- ✅ FFmpeg installation detection and guidance
- 🛡️ Error handling and user-friendly messages
- ⚡ High-quality audio extraction

## Prerequisites

### 1. .NET 9.0 Runtime

Make sure you have .NET 9.0 installed on your system.

**Download from:** https://dotnet.microsoft.com/download/dotnet/9.0

### 2. FFmpeg

FFmpeg is required for audio conversion. Choose one of the installation methods below:

#### Option A: Package Managers (Recommended)

**Chocolatey:**

```powershell
choco install ffmpeg
```

**Scoop:**

```powershell
scoop install ffmpeg
```

**Winget:**

```powershell
winget install FFmpeg
```

#### Option B: Manual Installation

1. Download the latest release from [FFmpeg Builds](https://github.com/BtbN/FFmpeg-Builds/releases)
2. Extract the zip file to a folder (e.g., `C:\ffmpeg`)
3. Add the `bin` folder to your system PATH:
   - Open System Properties → Environment Variables
   - Edit the PATH variable
   - Add `C:\ffmpeg\bin` (or your chosen path)
4. Restart your terminal/command prompt

#### Option C: Direct Download

- **Official Website:** https://ffmpeg.org/download.html

## Installation

1. **Clone or download this repository**

   ```bash
   git clone <repository-url>
   cd ytcli
   ```

2. **Restore NuGet packages**

   ```bash
   dotnet restore
   ```

3. **Build the application**
   ```bash
   dotnet build
   ```

## Usage

### Running the Application

```bash
dotnet run
```

The application will:

1. Check for FFmpeg installation
2. Prompt you to enter a YouTube URL
3. Display video information (title, duration)
4. Download and convert the audio to MP3
5. Save the file with a sanitized filename

### Example

```
YouTube Audio Streamer
====================
Checking for FFmpeg installation...
✅ FFmpeg found at: ffmpeg (in PATH)

Enter YouTube URL: https://www.youtube.com/watch?v=dQw4w9WgXcQ
Fetching video information...
Title: Rick Astley - Never Gonna Give You Up
Duration: 00:03:33
Audio format: webm
Audio bitrate: 128000
Downloading and converting audio to: Rick Astley - Never Gonna Give You Up.mp3
Audio saved as: Rick Astley - Never Gonna Give You Up.mp3
```

## Project Structure

```
ytcli/
├── Program.cs          # Main application logic
├── ytcli.csproj       # Project file with dependencies
├── README.md          # This file
└── bin/               # Compiled output
```

## Dependencies

- **YoutubeExplode** (6.5.4) - YouTube video metadata and stream extraction
- **FFMpegCore** (5.2.0) - Audio conversion and processing
- **.NET 9.0** - Runtime framework

## Troubleshooting

### FFmpeg Not Found

If you see "❌ FFmpeg not found!", follow the installation steps above. Make sure to:

- Add FFmpeg to your system PATH
- Restart your terminal after installation
- Verify installation by running `ffmpeg -version` in a new terminal

### Download Errors

- Ensure you have a stable internet connection
- Check that the YouTube URL is valid and accessible
- Some videos may have restrictions that prevent audio extraction

### Permission Errors

- Make sure you have write permissions in the current directory
- Try running as administrator if needed

## Technical Details

- **Audio Quality:** 128 kbps MP3
- **Supported Formats:** Any YouTube video with audio
- **Temporary Files:** Automatically cleaned up after conversion
- **Filename Sanitization:** Removes invalid characters for cross-platform compatibility

## License

This project is for educational purposes. Please respect YouTube's Terms of Service and copyright laws when using this tool.

## Contributing

Feel free to submit issues and enhancement requests!

## Disclaimer

This tool is for personal use only. Users are responsible for complying with YouTube's Terms of Service and applicable copyright laws.
