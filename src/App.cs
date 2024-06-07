using System.Numerics;
using Raylib_cs;

class App
{

	public static void Run()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		// Raylib.SetTraceLogLevel(TraceLogLevel.All);

		// Raylib stuff
		// TODO: Switch library (this is NOT a game!!)
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
		Raylib.SetConfigFlags(ConfigFlags.AlwaysRunWindow);
		Raylib.InitWindow(700, 500, "Clipper r3");
		Raylib.InitAudioDevice();

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



		Raylib.DrawFPS(10, 10);
		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{

		// Close the window
		//! Make sure to do this last.
		Raylib.CloseAudioDevice();
		Raylib.CloseWindow();
	}
}