using System.Diagnostics;
using System.Text.Json;
using Raylib_cs;

class VideoManager
{
	public static string Path { get; set; }

	// Diagnostics stuff
	public static bool Loading { get; private set; }

	// Video properties
	public static double Fps { get; private set; }
	public static int FrameCount { get; private set; }
	public static int Width { get; private set; }
	public static int Height { get; private set; }

	// Frame stuff
	private static byte[][] RawFrames;
	private static Texture2D?[] Frames;

	// Playback stuff
	public static bool Paused { get; set; }



	//? https://chat.openai.com/share/8d5271d7-c12c-4c8d-acd9-5dfefa305dbc
	//? https://www.youtube.com/watch?v=32PPzwPjDZ8	




	// Load the video
	// TODO: Make non-blocking (multi-thread)
	public static void LoadVideo()
	{
		Loading = true;

		// Use FFPROBE to get all of the required
		// information about the video
		Console.WriteLine("Extracting information...");
		Thread getInformationThread = new Thread(() => GetInformation());
		getInformationThread.Start();
		getInformationThread.Join();

		// Split the video into sections of
		// frames that contain their YUV data
		Console.WriteLine("Splitting frames...");
		Thread splitFramesThread = new Thread(() => SplitFrames());
		splitFramesThread.Start();
		splitFramesThread.Join();

		// Make a place to store all of the frames
		//? null means the current frame hasn't been loaded yet
		Frames = new Texture2D?[FrameCount];

		Console.WriteLine("Done!");
		Loading = false;
	}

	// Get video properties and whatnot
	private static void GetInformation()
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffprobe.exe",
			Arguments = $"-i {Path} -show_streams -print_format json -v error",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		process.Start();
		
		// Get the output as a string
		string extractedJson = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Parse the output to JSON so that the
		// values can be extracted and worked with
		// TODO: Make class and do with JsonSerializer object
		//? Using the first stream because thats normally video. Second stream is normally audio, and idk what the other steams are (irrelevant)
		JsonDocument extractedInformation = JsonDocument.Parse(extractedJson);
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

	// Split the video into frames
	// TODO: Only split a third or something then if we get close to reaching the end of split frames load in the other third, then again untill the whole thing loaded.
	//! idk if these optimizations are really necessary because the clips are gonna be short they gonna load super fast
	// TODO: First read the entire byte stream, then once done split it. Don't split as its being read
	private static void SplitFrames()
	{
		// Figure out how many bytes one frame takes up
		// TODO: Actually do that calculation if doing the read entire then split method
		//? I got 3 from the formula (totalBytes / totalFrames) / (width * height)
		//? and I did it for multiple videos and got 3 as a result for all of them
		int bytesPerPixel = 3;
		int bytesPerFrame = (Width * Height) * bytesPerPixel;


		// Store all of the raw frame data
		int frameIndex = 0;
		RawFrames = new byte[FrameCount][];


		// Use FFMPEG to get the entire in
		// a massive raw byte array
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			//? idk why need to times by 2
			FileName = "ffmpeg.exe",
			Arguments = $"-i {Path} -vf fps={Fps * 2} -f image2pipe -vcodec rawvideo -",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};
		process.Start();


		// Pipe the command output so that we can
		// use it
		using (Stream stream = process.StandardOutput.BaseStream)
		{
			// Store all of the incoming bytes
			//? 1024 is a standard size (1kb)
			byte[] buffer = new byte[1024];

			// Keep track of how many bytes
			// we have read for the current frame
			int totalBytesRead = 0;
			byte[] frameBuffer = new byte[bytesPerFrame];

			// Continuously pipe data from the video
			while (true)
			{
				// Get how many bytes we have read
				// and add the read bytes to the frame buffer.
				// And save the data that doesn't fit in the
				// current frame for next frame
				int bytesRead = stream.Read(frameBuffer, totalBytesRead, bytesPerFrame - totalBytesRead);
				totalBytesRead += bytesRead;

				// Check for if we have reached the
				// end of the stream (no more data)
				if (bytesRead == 0) break;

				// Check for if we have read enough
				// bytes to make up one entire frame
				// TODO: Use guard clause
				//? Not using guard clause here because its better for readability even thought we 3 indents in now
				if (totalBytesRead == bytesPerFrame)
				{
					// Add the data to the raw frame
					// data array so it can be processed
					// later when its ready to be drawn
					RawFrames[frameIndex] = frameBuffer;
					frameIndex++;

					// Reset the total bytes for the
					// next frame
					totalBytesRead = 0;
				}
			}
		}
	}



	public static void UpdateVideo()
	{
		// Check for if it's paused
		if (Paused) return;

		// TODO: Check for if we're due for the next frame
	}



	public static void RenderVideo()
	{


		
	}


	// Convert all of the YUV values to
	// RGB values, then bake them into a 
	// render texture so the frame can be
	// drawn.
	// TODO: Get luminance
	public static void LoadFrame(int frameIndex)
	{
		// Get the data we're working with, and
		// where we gonna save it
		byte[] rawData = RawFrames[frameIndex];
		byte[] yuvData = new byte[Width * Height];

		// Extract the chrominance data indices
		int chrominanceStartIndex = Width * Height;
		int chrominanceLength = (Width * Height) / 4;

		// Blue starts just after the luminance values,
		// and red starts just after the blue values
		int blueIndex = chrominanceLength;
		int redIndex = chrominanceStartIndex + chrominanceLength;

		// Loop over every chrominance value
		int index = 0;
		for (int i = 0; i < (rawData.Length - chrominanceStartIndex); i++)
		{
			// Check for if we are looking at the red
			// or the blue chrominance values
			byte chrominance;
			if (i % 2 == 0)
			{
				// Get the blue chrominance value
				chrominance = rawData[blueIndex];
				blueIndex++;
			}
			else
			{
				chrominance = rawData[redIndex];
				redIndex++;
			}

			// Add the chrominance to the array
			// om the x axis
			yuvData[index] = chrominance;
			yuvData[index + 1] = chrominance;

			// Add the chrominance value to the array
			// on the y axis if there is enough room
			if (index / Width < Height - 1)
			{
				yuvData[index + Width] = chrominance;
				yuvData[index + Width + 1] = chrominance;
			}

			// Increase the index for the next pixels
			index += 2;
		}
	}
}