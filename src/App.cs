using System.Numerics;
using Raylib_cs;

class App
{
	private static Texture2D debugFrame;

	public static void Run()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

		// Raylib stuff
		// TODO: Switch library (this is NOT a game!!)
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
		Raylib.InitWindow(700, 500, "Clipper r2");
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
		// Load a video
		VideoHandler.LoadVideo();

		// debug
		debugFrame = VideoHandler.LoadFrameBatch(50);
	}

	private static void Update()
	{
		VideoPlayer.Update();
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Black);

		VideoPlayer.Render();

		Raylib.DrawTextureEx(debugFrame, Vector2.Zero, 0f, 2f, Color.White);

		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{

		// Close the window
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}