using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	public static string FfmpegPath { get; set; }
	public static string VideoPath { get; set; }

	public static bool VideoLoaded { get; set; }

	public static double Fps { get; private set; }
	public static int FrameCount { get; private set; }
	public static int Width { get; private set; }
	public static int Height { get; private set; }

	public static int CurrentFrame { get; set; }
	private static Texture2D[] frames;

	// Load in the video
	public static void LoadVideo()
	{
		// Extract all of the needed video information
		GetInformation();
		Console.WriteLine($"Extracted video information:\nwidth:\t{Width}\nheight:\t{Height}\nfps:\t{Fps}");

		// Get all of the frames.
		// This is done in another thread because
		// its super slow (no blocking!!)
		Thread loadFramesThread = new Thread(() => Ffmpeg.GetFrames());
		loadFramesThread.Start();
	}

	// Unload the video
	public static void UnloadVideo()
	{
		// Unload all of the frames
		foreach (Texture2D frame in frames) Raylib.UnloadTexture(frame);

		// TODO: Reset all variables

		// Say that we have finished unloading the video
		VideoLoaded = false;
	}

	private static void GetInformation()
	{
		// Get all of the video information as json
		string videoInfoJson = Ffmpeg.RunCommand(Ffmpeg.Service.Ffprobe, $"-i {VideoPath} -show_format -show_streams -print_format json -v error");
		JsonDocument json = JsonDocument.Parse(videoInfoJson);
		JsonElement root = json.RootElement;

		// Get the fps
		string fpsString = root.GetProperty("streams")[0].GetProperty("r_frame_rate").GetString();
		Fps = double.Parse(fpsString.Split("/")[0]);

		// Get the width and height
		Width = root.GetProperty("streams")[0].GetProperty("width").GetInt32();
		Height = root.GetProperty("streams")[0].GetProperty("height").GetInt32();

		// Get how many frame it has
		string frameCountString = root.GetProperty("streams")[0].GetProperty("nb_frames").GetString();
		FrameCount = int.Parse(frameCountString);

		// Get rid of the json stuff
		json.Dispose();
	}
}