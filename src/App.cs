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

		//! having no logging speeds everything up heaps
		Raylib.SetTraceLogLevel(TraceLogLevel.Error);

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
		VideoManager.LoadVideo();
	}

	private static void Update()
	{
		
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		//! debug
		if (VideoManager.VideoLoaded) Raylib.DrawText("done loa", 25, 25, 50, Color.White);
		else Raylib.DrawText("loading rn (im so exctied)", 25, 25, 50, Color.White);

		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		VideoManager.UnloadVideo();

		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}