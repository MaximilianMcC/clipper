using System.Diagnostics;
using System.Text.Json;

class VideoManager
{
	public static string Path { get; set; }

	// Video properties
	public static double Fps { get; private set;}
	public static int FrameCount { get; private set;}
	public static int Width { get; private set;}
	public static int Height { get; private set;}





	// Load the video
	public static void LoadVideo()
	{
		// Use FFPROBE to get all of the required
		// information about the video
		Process informationExtractionProcess = new Process();
		informationExtractionProcess.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffprobe.exe",
			Arguments = $"-i {Path} -show_streams -print_format json -v error",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		informationExtractionProcess.Start();
		
		// Get the output as a string
		string extractedInformationJson = informationExtractionProcess.StandardOutput.ReadToEnd();
		informationExtractionProcess.WaitForExit();

		// Parse the output to JSON so that the
		// values can be extracted and worked with
		// TODO: Make class and do with JsonSerializer object
		//? Using the first stream because thats normally video. Second stream is normally audio, and idk what the other steams are (irrelevant)
		JsonDocument extractedInformation = JsonDocument.Parse(extractedInformationJson);
		JsonElement root = extractedInformation.RootElement.GetProperty("streams")[0];

		// Get the FPS
		string fpsString = root.GetProperty("avg_frame_rate").GetString();
		Fps = double.Parse(fpsString.Split("/")[0]);

		// Get the total amount of frames
		string frameCountString = root.GetProperty("nb_frames").GetString();
		FrameCount = int.Parse(frameCountString);

		// Get the width/height
		Width = root.GetProperty("width").GetInt32();
		Height = root.GetProperty("height").GetInt32();
	}
}