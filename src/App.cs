using Raylib_cs;

class App
{
	public static void Run()
	{
		// Raylib stuff
		// TODO: Switch library
		Raylib.InitWindow(700, 500, "Clipper v1");
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

		//! test (clip to 3 seconds long)
		Utils.RunFfmpegCommand("-t 3");
	}

	private static void Update()
	{

	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}