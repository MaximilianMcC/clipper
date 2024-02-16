using Raylib_cs;

class App
{
	public static void Run()
	{
		// Raylib stuff
		// TODO: Switch library
		Raylib.InitWindow(700, 500, "Clipper r0");
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetTargetFPS(60);

		Start();
		while (!Raylib.WindowShouldClose())
		{
			Update();
			Render();
		}
		CleanUp();
	}

	private static void Start()
	{
		// Load the video
		// VideoManager.LoadVideo(VideoManager.VideoPath);

		// Ffmpeg.LoadVideo("./assets/video.mp4");

		string output = Ffmpeg.RunCommand(Ffmpeg.Service.Ffprobe, "-i ./assets/video.mp4 -show_format -show_streams -print_format json -v error");
		Console.WriteLine(output);

		//! test (clip to 3 seconds long)
		// Utils.RunFfmpegCommand("-t 3");
	}

	private static void Update()
	{
		
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		//! debug
		// Show some text to tell the use that
		// FFMPEG is busy doing stuff in background
		// if (Ffmpeg.Working) Raylib.DrawText("busy rn", 25, 25, 50, Color.White);

		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}