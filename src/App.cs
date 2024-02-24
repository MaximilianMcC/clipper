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
		if (Raylib.IsKeyPressed(KeyboardKey.Space)) VideoManager.Paused = !VideoManager.Paused;
		if (Raylib.IsKeyPressed(KeyboardKey.L)) VideoManager.Looped = !VideoManager.Looped;

		if (VideoManager.Paused == false) VideoManager.Update();
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

		VideoManager.Render();

		// Show if the video is loaded or not
		Raylib.DrawText($"Colors loaded: {VideoManager.ColorsLoaded}", 10, 40, 30, Color.White);
		Raylib.DrawText($"Loaded {VideoManager.LoadedFrames}/{VideoManager.TotalFrames}", 10, 10, 30, Color.White);
		Raylib.DrawText($"Fully loaded: {VideoManager.FullyLoaded}", 10, 80, 30, Color.White);

		Raylib.DrawText($"Paused: {VideoManager.Paused}", 10, 250, 30, Color.White);
		Raylib.DrawText($"Looped: {VideoManager.Looped}", 10, 290, 30, Color.White);



		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		VideoManager.UnloadVideo();

		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}