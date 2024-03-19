using Raylib_cs;

class App
{

	private static bool debugMode = true;
	private static Texture2D testFrame;


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
		VideoManager.LoadVideo();
		Console.WriteLine("loaded video (sweet as!!)");

		

		//! temp
		testFrame = VideoManager.LoadFrame(10);
	}

	private static void Update()
	{
		// Toggle debug mode
		if (Raylib.IsKeyPressed(KeyboardKey.F3) || Raylib.IsKeyPressed(KeyboardKey.Grave)) debugMode = !debugMode;


		Raylib.DrawTexture(testFrame, 0, 0, Color.White);


		// Update the video
		if (VideoManager.Loading == false)
		{
			VideoManager.UpdateVideo();
		}
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		
		
		// Render the video
		if (VideoManager.Loading == false)
		{
			VideoManager.RenderVideo();
		}



		// Debug stuff
		if (debugMode)
		{
			Raylib.DrawText("Loading: " + VideoManager.Loading, 10, 10, 30, Color.Black);
		}



		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		// Close the window
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}