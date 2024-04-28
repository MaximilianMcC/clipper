using System.Diagnostics;

class Program
{
	public static void Main(string[] args)
	{
		// Check for if FFMPEG and FFPROBE is 
		// installed
		if (HasFfmpeg() == false) return;

		// Check for if there is actually a video
		// at the video path
		if (VideoExists(args) == false) return;

		// Set the video path, then run
		// the actual program
		VideoHandler.VideoPath = args[0];
		App.Run();
	}

	// Check for if FFMPEG and FFPROBE is installed
	// and added to path
	private static bool HasFfmpeg()
	{
		Process process = new Process();
		process.StartInfo = new ProcessStartInfo()
		{
			FileName = "cmd.exe",

			UseShellExecute = false,
			RedirectStandardOutput = true
		};

		// Check for if FFMPEG is installed
		process.StartInfo.Arguments = "/c where ffmpeg.exe";
		process.Start();
		process.WaitForExit();
		bool ffmpegFound = process.ExitCode == 0;

		// Check for if FFPROBE is installed
		process.StartInfo.Arguments = "/c where ffprobe.exe";
		process.Start();
		process.WaitForExit();
		bool ffprobeFound = process.ExitCode == 0;

		// Check for if both libraries were found
		if (ffmpegFound && ffprobeFound) return true;

		// Tell them to install the libraries
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("Could not find FFMPEG or FFPROBE libraries installed!");
		Console.ResetColor();
		Console.Write("Please install FFMPEG and FFPROBE from");
		Console.ForegroundColor = ConsoleColor.Blue;
		Console.Write(" https://github.com/BtbN/FFmpeg-Builds/releases ");
		Console.ResetColor();
		Console.WriteLine("then add both binaries to path.\n");

		// Don't continue with normal
		// program execution
		return false;
	}

	// Check for if the video actually exists
	private static bool VideoExists(string[] args)
	{
		// Check for if they actually gave us a path
		// and for if the file exists
		if (args.Length < 1 || !File.Exists(args[0]))
		{
			// Say that it doesn't exist
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Could not find file!");
			Console.ResetColor();
			Console.WriteLine("Please provide the path (relative or full) to the video you wish to edit as the first argument.");

			return false;
		}

		return true;
	}
}