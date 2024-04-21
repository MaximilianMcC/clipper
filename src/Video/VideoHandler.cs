using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Raylib_cs;

class VideoHandler
{
	// I/O stuff
	public static string Path { get; set; }

	// Video information
	public static int Width { get; private set; }
	public static int Height { get; private set; }
	public static double FrameRate { get; private set; } 
	public static int FrameCount { get; private set; }

	// Important stuff
	public static Texture2D[] Frames { get; private set; }
	public static byte[][] RawFrames { get; private set; }
	public static Music Audio { get; private set; }

	// Frame stuff
	private static int bytesPerPixel;
	private static int bytesPerFrame;
	private static RenderTexture2D renderTexture;

	public static void LoadVideo()
	{
		// Get video info
		GetVideoInformation();

		// Store all of the frames
		RawFrames = new byte[FrameCount][];
		Frames = new Texture2D[FrameCount];

		// Make a render texture for baking to
		renderTexture = Raylib.LoadRenderTexture(Width, Height);

		// Immediately make a new thread and begin to
		// load in every single frame in the video so
		// that it should be ready when the time comes
		// to use it. If the frame isn't ready then it
		// can be loaded using LoadFrameBatch()
		Thread backgroundLoader = new Thread(() => {
			LoadFrameBatch(0, FrameCount);
			Console.WriteLine("Background loader finished loading everything.");
		});
		backgroundLoader.Start();

		// Load the audio
		LoadAllAudio();
	}

	private static void GetVideoInformation()
	{
		// Make a new process to run the FFMPEG command
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			// This command shows all of the stream information
			// in JSON format
			FileName = "ffprobe.exe",
			Arguments = $"-i {Path} -show_streams -print_format json -v error",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true
		};

		// Get the output from the command
		process.Start();
		string processOutput = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		// Parse it to a JSON document so we
		// can parse it without a class like
		// how it can be done in languages
		// like JavaScript and Python
		JsonDocument json = JsonDocument.Parse(processOutput);
		JsonElement videoStream = json.RootElement.GetProperty("streams")[0];

		// Get the width and height
		Width = videoStream.GetProperty("width").GetInt32();
		Height = videoStream.GetProperty("height").GetInt32();

		// Get the number of frames
		string frameCountString = videoStream.GetProperty("nb_frames").GetString();
		FrameCount = int.Parse(frameCountString);

		// Get the fps
		//? fps is both values divided by each other
		string frameRateString = videoStream.GetProperty("avg_frame_rate").GetString();
		string[] equation = frameRateString.Split("/");
		FrameRate = double.Parse(equation[0]) / double.Parse(equation[1]);

		// Figure out how many bytes in a pixel,
		// and how many bytes in a frame
		bytesPerPixel = 3; //? R, G, B (24 bit)
		bytesPerFrame = (Width * Height) * bytesPerPixel;

		// Print out all the extracted information
		// TODO: Don't do this in production
		// TODO: Put in ToString()
		// TODO: Don't do
		Console.WriteLine("Extracted information from " + Path + ":");
		Console.WriteLine("Size:\t\t" + Width + "x" + Height);
		Console.WriteLine("Frames:\t\t" + FrameCount + " @ " + FrameRate.ToString("#.#") + "fps");
	}

	/*
	private static void LoadAllAudio()
	{
		// Make and run the FFMPEG command to extract the audio data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			// TODO: Use ogg (smaller and higher quality)
			FileName = "ffmpeg.exe",
			Arguments = $"-i {Path} -vn -acodec libvorbis -f wav -",
			// Arguments = $"-i {Path} -f wav -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		//? idk if theres an easy way to calculate the size of the audio so doing like this
		// Open a memory stream to read the bytes
		byte[] audioBytes;
		using (MemoryStream stream = new MemoryStream())
		{
			// Get all the data
			process.StandardOutput.BaseStream.CopyTo(stream);
			audioBytes = stream.ToArray();
		}

		audioBytes = File.ReadAllBytes(Path);

		// Load, then give back the audio as Raylib music
		Console.WriteLine("Loading music rn");
		Audio = Raylib.LoadMusicStreamFromMemory(".wav", audioBytes);
		Raylib.PlayMusicStream(Audio);
		Console.WriteLine("music loaded!!");
	}
	*/

	private static void LoadAllAudio()
{
    // Make and run the FFMPEG command to extract the audio data
    Process ffmpegProcess = new Process();
    ffmpegProcess.StartInfo = new ProcessStartInfo()
    {
        FileName = "ffmpeg.exe",
        Arguments = $"-i {Path} -vn -acodec pcm_s16le -ar 44100 -ac 2 -f wav -",
        UseShellExecute = false,
        RedirectStandardError = true,
        RedirectStandardOutput = true
    };
    ffmpegProcess.Start();

    // Open a memory stream to read the bytes
    using (MemoryStream audioMemoryStream = new MemoryStream())
    {
        // Get all the data
        ffmpegProcess.StandardOutput.BaseStream.CopyTo(audioMemoryStream);

        // Load the audio data from memory directly into Raylib
        audioMemoryStream.Seek(0, SeekOrigin.Begin);
        Console.WriteLine("Loading music...");
        Audio = Raylib.LoadMusicStreamFromMemory(".wav", audioMemoryStream.ToArray());
        Raylib.PlayMusicStream(Audio);
        Console.WriteLine("Music loaded!!");
    }
}


	public static void LoadFrameBatch(int frameIndex, int framesToLoad)
	{
		// Make and run the FFMPEG command to extract the video data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffmpeg.exe",
			Arguments = $"-i {Path} -vf select='between(n\\,{frameIndex}\\,{frameIndex + framesToLoad}\\)' -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Loop through all frames that we need to load
		for (int i = 0; i < framesToLoad; i++)
		{
			// Store all of the data for the current frame
			byte[] frameBuffer = new byte[bytesPerFrame];
			int totalBytesRead = 0;

			// Keep reading stuff until we have read a complete frame
			while (totalBytesRead != bytesPerFrame)
			{
				// Read the incoming bytes
				int bytesRemaining = bytesPerFrame - totalBytesRead;
				int bytesRead = process.StandardOutput.BaseStream.Read(frameBuffer, totalBytesRead, bytesRemaining);
				totalBytesRead += bytesRead;

				// Check for if we've collected enough
				// bytes to make a whole frame
				if (totalBytesRead == bytesPerFrame)
				{
					// Save the frames raw data
					RawFrames[frameIndex + i] = frameBuffer;
					break;
				}
			}
		}

		// TODO: Wait for exit
		//! process isn't ending for some reason
		//! Process will still be running in background
		// TODO: Figure out if the process is actually still running in background
		// process.WaitForExit();
	}

	// Bake a texture to a texture
	//? Because OpenGL stink its drawn upside down so when rendering it needs to be flipped
	// TODO: Load image from memory
	public static void GenerateFrame(byte[] frameData, int frameIndex)
	{
		// Make a new render texture (debug)
		renderTexture = Raylib.LoadRenderTexture(Width, Height);

		// Loop through every pixel in the frame and draw it
		Raylib.BeginTextureMode(renderTexture);
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				// Get the color of the pixel
				//? 3 because 3 bytes (r, g, b)
				//? Apparently doing maths faster than using variable
				int pixelIndex = ((y * Width) + x) * 3;
				Color pixel = new Color(frameData[pixelIndex], frameData[pixelIndex + 1], frameData[pixelIndex + 2], byte.MaxValue);

				// Draw the pixel
				Raylib.DrawPixel(x, y, pixel);
			}
		}
		// Raylib.ClearBackground(Color.Green);
		Raylib.EndTextureMode();

		// Get the frame, then return it
		Frames[frameIndex] = renderTexture.Texture;
	}

	// TODO: If get this working its quicker than render texture
	/*
	private static Texture2D GenerateFrame(byte[] frameData)
	{
		Image frameImage = Raylib.LoadImageFromMemory("rgb24", frameData);
		Texture2D frame = Raylib.LoadTextureFromImage(frameImage);
		Raylib.UnloadImage(frameImage);

		return frame;
	}
	*/
}