using System.Numerics;
using Raylib_cs;

class App
{
	private static int debugIndex = 49;

	public static void Run()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

		// Raylib stuff
		// TODO: Switch library (this is NOT a game!!)
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
		Raylib.InitWindow(700, 500, "Clipper r2");
		// Raylib.SetTargetFPS(60);



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

		// !debug
		VideoHandler.LoadFrameBatch(50);
	}

	private static void Update()
	{
		VideoPlayer.Update();

		if (Raylib.IsKeyPressed(KeyboardKey.Right)) debugIndex++;
		if (Raylib.IsKeyPressed(KeyboardKey.Left)) debugIndex--;
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		VideoPlayer.Render();

		// !debug
		// Raylib.DrawTextureEx(VideoHandler.Frames[50], Vector2.Zero, 0f, 1f, Color.White);
		// Raylib.DrawTexture(VideoHandler.Frames[51], 0, 0, Color.White);
		Raylib.DrawTexture(VideoHandler.Frames[debugIndex], 0, 0, Color.White);
		Raylib.DrawText(debugIndex.ToString(), 500, 150, 30, Color.White);

		Raylib.DrawFPS(10, 10);
		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{

		// Close the window
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}