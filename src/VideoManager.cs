using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	public static string FfmpegPath { get; set; }
	public static string VideoPath { get; set; }

	public static int CurrentFrame { get; set; }
	private static Texture2D[] frames;
	private static double fps;

	// Load in the video
	//? path is VideoPath from main method
	public static void LoadVideo()
	{
		// Get all of the video information as json
		string videoInfoJson = Ffmpeg.RunCommand(Ffmpeg.Service.Ffprobe, $"-i {VideoPath} -show_format -show_streams -print_format json -v error");
		JsonDocument json = JsonDocument.Parse(videoInfoJson);
		JsonElement root = json.RootElement;

		// Get the fps
		string fpsString = root.GetProperty("streams")[0].GetProperty("r_frame_rate").GetString();
		double fps = double.Parse(fpsString.Split("/")[0]);

		// Get the width and height
		int width = root.GetProperty("streams")[0].GetProperty("width").GetInt32();
		int height = root.GetProperty("streams")[0].GetProperty("height").GetInt32();

		// Get rid of the json stuff
		json.Dispose();


		// Log all the stuff
		// TODO: remove
		Console.WriteLine($"Extracted video information:\nwidth:\t{width}\nheight:\t{height}\nfps:\t{fps}");
	}
}