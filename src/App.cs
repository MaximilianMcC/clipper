using Raylib_cs;

class App
{

	public static void Run()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

		// Raylib stuff
		// TODO: Switch library (this is NOT a game!!)
		Raylib.InitWindow(700, 500, "Clipper r1");
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
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
		// TODO: Count how long it takes to load video
		Console.WriteLine("Loading video");
		VideoManager.LoadVideo();

		Console.WriteLine(@$"
		Video loaded:
		{VideoManager.Width} x {VideoManager.Height}
		at {VideoManager.Fps}fps with a total of {VideoManager.FrameCount} frames.
		");
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
		// Close the window
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}