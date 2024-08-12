using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

class Window
{
	public static void Run()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.AlwaysRunWindow);
		Raylib.InitWindow(800, 600, "Clipper r5");
		rlImGui.Setup();
		
		Texture2D test = Raylib.LoadTexture("./assets/debug.png");

		while (!Raylib.WindowShouldClose())
		{
			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.White);
			rlImGui.Begin();

			ImGui.Text("Kia ora");
			ImGui.Image((nint)test.Id, new Vector2(test.Width, test.Height) / 3);

			rlImGui.End();
			Raylib.EndDrawing();
		}

		Raylib.UnloadTexture(test);

		rlImGui.Shutdown();
		Raylib.CloseWindow();
	}

	private static void InitImGui()
	{

	}

	private static void RenderImGui()
	{
		ImGui.Begin("kia ora");
		
		ImGui.End();
	}
}