using Raylib_cs;

class App
{

	public static void Run()
	{
		// Raylib stuff
		// TODO: Switch library
		Raylib.InitWindow(700, 500, "Clipper r1");
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
		Raylib.SetTargetFPS(60);

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