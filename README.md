<p align="center"><img src="kpeg/Resources/logo.png" alt="drawing" width="200"/></p>

# kpeg

**kpeg** is a WPF application built with **.NET Framework 4.7.2** that acts as a graphical wrapper for `yt-dlp` and `ffmpeg`. The goal is to simplify media downloading and processing by providing an easy-to-use interface.

> ⚠️ This project is in **early (inactive) development**. Currently, only the `yt-dlp` wrapper functionality is implemented. `ffmpeg` is planned to be used for video conversion.

## Features

- ✅ Downloading videos from various sources using `yt-dlp`
- ✅ High-quality conversion of downloaded videos to common formats 
- 🚧 Convenient conversion of videos between formats without downloads

## Requirements

Ensure the following executables are available in the Resources directory or system path:

- [yt-dlp](https://github.com/yt-dlp/yt-dlp)
- [ffmpeg](https://ffmpeg.org/)
- [PhantomJS](https://phantomjs.org/)

## Getting Started

### Prerequisites

- Windows with [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) or later installed
- `yt-dlp.exe`, `ffmpeg.exe`, and `phantomjs.exe` in the Resources folder as the application (or in the system PATH)

### Build Instructions

1. Clone the repository:

   ```bash
   git clone https://github.com/salvijusbieliauskas/kpeg.git
   ```

2. Open the solution in a C# IDE of your choice.

3. Restore any NuGet packages if required.

4. Build and run the project.

## Usage

1. Launch the application.
2. Paste a supported media URL.
3. Configure the options to your liking.
4. Click "Download" to use `yt-dlp` for downloading the content.

## License

[MIT License](LICENSE)

## Disclaimer

This tool is intended for personal use only. Please ensure that you comply with the terms of service of any platform you use it with.

