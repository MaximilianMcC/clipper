using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	// IO stuff
	public static string FfmpegPath { get; set; }
	public static string VideoPath { get; set; }

	// Loading debugging stuff
	public static int LoadedFrames { get; set; }
	public static bool VideoLoaded { get; set; }

	// Video properties
	public static double Fps { get; private set; }
	public static int FrameCount { get; private set; }
	public static int Width { get; private set; }
	public static int Height { get; private set; }

	// Frame crap
	public static int CurrentFrame { get; set; }
	private static Texture2D[] Frames;
	private static Color[][] frameColors;
	private static double showedLastFrame;

	// Playback state
	public static bool Paused { get; set; }

	// Load in the video
	public static void LoadVideo()
	{
		// Extract all of the needed video information
		GetInformation();
		Console.WriteLine($"Extracted video information:\nwidth:\t{Width}\nheight:\t{Height}\nfps:\t{Fps}");

		// Get all of the frames as their colors
		frameColors = Ffmpeg.GetFrames();
		Frames = new Texture2D[FrameCount];
		
		// Set the initial time for updating the frames
		showedLastFrame = Raylib.GetTime();
	}

	// Unload the video
	public static void UnloadVideo()
	{
		// Unload all of the frames
		foreach (Texture2D frame in Frames)
		{
			Raylib.UnloadTexture(frame);
			LoadedFrames--;
		}

		
		// TODO: Reset all variables


		// Say that we have finished unloading the video
		VideoLoaded = false;
	}

	// Get information about the video
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



	public static void Update()
	{	
		// Check for if we need the next frame
		double currentTime = Raylib.GetTime();
		double elapsedTime = currentTime - showedLastFrame;
		bool dueForNextFrame = elapsedTime >= (1d / Fps);

		// Update the current video frame if it's time
		if (dueForNextFrame == false) return;
		CurrentFrame++;

		// Load in the next frame so it doesn't need to be
		// loaded in at runtime when its needed
		// TODO: Could load in 2, or 3 instead of just 1
		//! Could break for higher frame rates
		Frames[CurrentFrame + 1] = Ffmpeg.GenerateFrame(frameColors[CurrentFrame]);
	}

	public static void Render()
	{
		// Draw the current video frame
	}
}