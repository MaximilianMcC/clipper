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
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

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
		VideoManager.Update();
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		VideoManager.Render();

		// Show if the video is loaded or not
		Raylib.DrawText($"Loaded {VideoManager.LoadedFrames}/{VideoManager.FrameCount}", 10, 10, 30, Color.White);
		Raylib.DrawText($"Video loaded: {VideoManager.VideoLoaded}", 10, 40, 30, Color.White);



		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		VideoManager.UnloadVideo();

		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}