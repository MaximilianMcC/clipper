using Raylib_cs;

class VideoManager
{
	public static string FfmpegPath { get; set; }
	public static string VideoPath { get; set; }

	public static int CurrentFrame { get; set; }
	private static Texture2D[] frames;
	private static double fps;

	public static void LoadVideo(string filepath)
	{
		
	}
}