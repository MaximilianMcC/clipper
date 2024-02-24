using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	// IO stuff
	public static string FfmpegPath { get; set; }
	public static string VideoPath { get; set; }

	// Loading debugging stuff
	public static int LoadedFrames { get; set; }
	public static bool ColorsLoaded { get; set; }
	public static bool FullyLoaded { get; set; }

	// Video properties
	public static double Fps { get; private set; }
	public static int TotalFrames { get; private set; }
	public static int Width { get; private set; }
	public static int Height { get; private set; }
	public static PixelFormat Format { get; private set; }

	// Frame crap
	public static int CurrentFrame { get; set; }
	private static Texture2D[] frames;
	private static Color[][] frameColors;
	private static double showedLastFrameTime;

	// Playback state
	public static bool Paused { get; set; } = false;
	public static bool Looped { get; set; } = true;

	// Load in the video
	public static void LoadVideo()
	{
		// Extract all of the needed video information
		GetInformation();
		Console.WriteLine($"Extracted video information:\nwidth:\t{Width}\nheight:\t{Height}\nfps:\t{Fps}\nPixel Format:\t{Format}\n");

		// Get all of the frames as their colors
		frameColors = Ffmpeg.GetFrames();
		frames = new Texture2D[TotalFrames];
		
		// Set the initial time for updating the frames
		// and generate the first frame
		showedLastFrameTime = Raylib.GetTime();
		frames[0] = Ffmpeg.GenerateFrame(frameColors[0]);
	}

	// Unload the video
	public static void UnloadVideo()
	{
		// Unload all of the frames
		foreach (Texture2D frame in frames)
		{
			Raylib.UnloadTexture(frame);
			LoadedFrames--;
		}

		
		// TODO: Reset all variables


		// Say that we have finished unloading the video
		ColorsLoaded = false;
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

		// Get how many frames it has
		string frameCountString = root.GetProperty("streams")[0].GetProperty("nb_frames").GetString();
		TotalFrames = int.Parse(frameCountString);

		// Get the pixel format
		string pixelFormatString = root.GetProperty("streams")[0].GetProperty("pix_fmt").GetString();
		switch (pixelFormatString)
		{
			case "yuv420p":
				Format = PixelFormat.YUV;
				break;
			
			case "rgb24":
				Format = PixelFormat.RGB;
				break;
			
			default:
				Format = PixelFormat.UNKNOWN;
				break;
		}

		// Get rid of the json stuff
		json.Dispose();
	}



	public static void Update()
	{	
		// Update the video
		PlayVideo();
	}

	public static void Render()
	{
		// Draw the current video frame
		Raylib.DrawTexture(frames[CurrentFrame], 0, 0, Color.White);

		// Progress bar settings
		// TODO: Put this at the top or in another class
		const float padding = 50f;
		const float padding2 = padding * 2;
		float width = Raylib.GetScreenWidth() - padding2;
		float height = 20f;
		float y = Raylib.GetScreenHeight() - padding;

		// Get the percentage that we are through the video
		//? percentage = (value / total) * 100
		float progressPercentage = ((float)CurrentFrame / (float)TotalFrames) * 100f;

		// Get the width that the progress bar should
		// be according to the progress percentage and
		// the width of the bar (1% is normally not 1px)
		float progressWidth = (progressPercentage * width) / 100f;

		// Draw the progress bar
		// TODO: Lerp it for quicker videos
		Raylib.DrawRectangleRec(new Rectangle(padding, y, width, height), Color.Gray);
		Raylib.DrawRectangleRec(new Rectangle(padding, y, progressWidth, height), Color.White);

		//! debug
		Raylib.DrawText($"{progressPercentage}%\n\n{CurrentFrame}/{TotalFrames}", 10, 150, 30, Color.White);
	}


	// Play/update the video
	private static void PlayVideo()
	{
		// Check for if we need the next frame
		double currentTime = Raylib.GetTime();
		double elapsedTime = currentTime - showedLastFrameTime;
		bool dueForNextFrame = elapsedTime >= (1d / Fps);

		// Don't do anything if we don't need to do anything
		if (dueForNextFrame == false) return;

		// Check for if the video has ended
		if (CurrentFrame >= (TotalFrames - 1))
		{
			if (Looped)
			{
				// Reset the video
				CurrentFrame = 0;
			}
			else
			{
				CurrentFrame = TotalFrames - 1;
				Paused = true;
				return;
			}
		}

		// Update the current frame
		CurrentFrame++;

		// Load in the next frame so it doesn't need to be
		// loaded in at runtime when its needed
		// TODO: Could load in 2, or 3 instead of just 1
		//! Could break for higher frame rates
		if (FullyLoaded == false)
		{
			// Load in the next frame
			if ((CurrentFrame + 1) >= TotalFrames) return;
			frames[CurrentFrame + 1] = Ffmpeg.GenerateFrame(frameColors[CurrentFrame]);
			LoadedFrames++;

			// Check for if we're fully loaded now
			if (LoadedFrames == TotalFrames) FullyLoaded = true;
		}
	}









	// TODO: Add more formats
	// TODO: Convert everything into a certain format
	public enum PixelFormat
	{
		YUV,
		RGB,
		UNKNOWN
	}
}