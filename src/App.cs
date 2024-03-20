using System.Numerics;
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
		testFrame = VideoManager.LoadFrame(0);
	}

	private static void Update()
	{
		// Toggle debug mode
		if (Raylib.IsKeyPressed(KeyboardKey.F3) || Raylib.IsKeyPressed(KeyboardKey.Grave)) debugMode = !debugMode;


		// Update the video
		VideoManager.UpdateVideo();
	}

	private static void Render()
	{
		Raylib.BeginDrawing();
		Raylib.ClearBackground(Color.Magenta);

				// Draw the frmae upside down because opengl is stupid
		// they draw everything upside down
		Rectangle source = new Rectangle(0, 0, testFrame.Width, -testFrame.Height);
		Rectangle destination = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
		Raylib.DrawTexturePro(testFrame, source, destination, Vector2.Zero, 0f, Color.White);

		
		
		// Render the video
		VideoManager.RenderVideo();



		// Debug stuff
		if (debugMode)
		{
			Vector2 size = Raylib.MeasureTextEx(Raylib.GetFontDefault(), VideoManager.Diagnostics(), 35f, (25f / 10f));
			Raylib.DrawRectangle(0, 0, (int)size.Y, (int)size.X / 2, new Color(0, 0, 0, 128));
			Raylib.DrawText(VideoManager.Diagnostics(), 10, 10, 35, Color.White);
		}



		Raylib.EndDrawing();
	}

	private static void CleanUp()
	{
		// Unload the video
		VideoManager.UnloadVideo();

		// Close the window
		//! Make sure to do this last.
		Raylib.CloseWindow();
	}
}