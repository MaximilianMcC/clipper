using System.Diagnostics;
using System.Text.Json;
using Raylib_cs;

class VideoHandler
{
	// I/O stuff
	public static string VideoPath { get; set; }

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

		// Load all frames in the background
		LoadFramesInBackground();

		// Load the audio
		//! LoadAllAudio();
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
			Arguments = $"-i {VideoPath} -show_streams -print_format json -v error",

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
		Console.WriteLine("Extracted information from " + VideoPath + ":");
		Console.WriteLine("Size:\t\t" + Width + "x" + Height);
		Console.WriteLine("Frames:\t\t" + FrameCount + " @ " + FrameRate.ToString("#.#") + "fps");
	}

	// TODO: Load audio to memory
	private static void LoadAllAudio()
	{
		// Get the temporary output path for saving the audio file
		//? temp name generation might be a bit dodgy but it works
		string temporaryDirectory = Path.GetTempPath();
		string temporaryName = $"clipper{DateTime.UtcNow.ToBinary()}.wav";
		string temporaryFile = Path.Combine(temporaryDirectory, temporaryName);

		// Make and run the FFMPEG command to extract the audio data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			// TODO: Use ogg (smaller and higher quality)
			FileName = "ffmpeg.exe",
			Arguments = $"-i {VideoPath} -vn -acodec libvorbis {temporaryFile}",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Load, then start/play the audio
		Audio = Raylib.LoadMusicStream(temporaryFile);
		Raylib.PlayMusicStream(Audio);

		// Delete the temporary file since it now
		// lives in memory when raylib loaded it
		File.Delete(temporaryFile);
	}

	public static void LoadFrameBatch(int frameIndex, int framesToLoad)
	{
		Console.WriteLine($"Loading {framesToLoad} frames from {frameIndex} - {frameIndex + framesToLoad}!");

		// Make and run the FFMPEG command to extract the video data
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "ffmpeg.exe",
			Arguments = $"-i {VideoPath} -vf select='between(n\\,{frameIndex}\\,{frameIndex + framesToLoad}\\)' -f image2pipe -vcodec rawvideo -pix_fmt rgb24 -",

			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true
		};
		process.Start();

		// Loop through all frames that we need to load
		for (int i = 0; i < framesToLoad; i++)
		{
			// If we have already loaded the frame then
			// skip the current frame
			if (Frames[i].Id != 0)
			{
				Console.WriteLine($"Frame {i} has already been loaded! Skipping.");
				continue;
			}

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

	// Bake a frame to a texture
	public static void GenerateFrame(byte[] frameData, int frameIndex)
	{
		// Convert byte array to a raylib color array
		Color[] pixels = new Color[Width * Height];
		for (int i = 0; i < (bytesPerFrame / 3); i++)
		{
			// Make the pixel from the raw bytes
			pixels[i] = new Color(
				frameData[i * 3 + 0],
				frameData[i * 3 + 1],
				frameData[i * 3 + 2],
				Byte.MaxValue
			);
		}

		// Open an unsafe context because
		// we're working with pointers
		unsafe
		{
			// Get a pointer to the colors/pixel array
			fixed (Color* pixelPointer = pixels)
			{
				// Create the image using the pointer to
				// add all of the data
				Image image = new Image
				{
					Width = Width,
					Height = Height,

					Format = PixelFormat.UncompressedR8G8B8A8,
					Data = pixelPointer,
					Mipmaps = 1,
				};

				// Convert the image to a texture for rendering,
				// and assign it to the frames array
				Texture2D texture = Raylib.LoadTextureFromImage(image);
				Frames[frameIndex] = texture;

				// TODO: Unload the image (might need to put in array and unload when program close maybe)
			}
		}
	}


	private static void LoadFramesInBackground()
	{
		// Define how many threads we are going to allocate
		// to loading in the video
		const int threads = 4;
		
		// Calculate how many frames we need to load on each 
		// thread and accounting for division remainder
		int framesPerThread = FrameCount / threads;
		int frameRemainder = FrameCount % threads;

		// Because threads run whenever all values need
		// to be gotten beforehand to avoid using the same
		// variables multiple times
		int[][] parameters = new int[threads][];
		for (int i = 0; i < threads; i++)
		{
			// Get the parameters
			int frameIndex = i * framesPerThread;
			int framesToLoad = framesPerThread;

			// Check for if we are on the last thread and
			// add the required division remainder thingy
			// to make sure all the frames are properly loaded
			if (i == threads - 1) framesToLoad += frameRemainder;

			// Add the parameters to the array
			parameters[i] = new int[] { frameIndex, framesToLoad };
		}

		// Now that all the variables have been gotten, and
		// in the correct order, we can make and run all
		// the background loader threads
		for (int i = 0; i < threads; i++)
		{
			// Grab a copy of the index because it
			// will change when initiating the lambda
			int currentIndex = i;

			// Make the thread, then run it
			Thread backgroundLoader = new Thread(() => {

				LoadFrameBatch(parameters[currentIndex][0], parameters[currentIndex][1]);
				Console.WriteLine($"Loaded frame {currentIndex + 1}/{threads}");
			});
			backgroundLoader.Start();
		}
	}

	public static void UnloadVideo()
	{
		// Get rid of the music
		Raylib.UnloadMusicStream(Audio);

		// Remove all frames
		foreach (Texture2D frame in Frames)
		{
			Raylib.UnloadTexture(frame);
		}
	}
}