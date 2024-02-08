class Program
{
	public static void Main(string[] args)
	{
		// Check for if they provided an FFMPEG location
		// TODO: Check for if there is actually ffmpeg stuff in the directory
		if (args.Length <= 0)
		{
			Console.WriteLine("You are missing the path to FFMPEG. Please download via the instructions in the GitHub repo.");
			return;
		}
		else if (args.Length <= 1)
		{
			Console.WriteLine("You are missing the path to the video you wish to open.");
			return;	
		}

		// Set the paths, then run the app
		VideoManager.FfmpegPath = args[0].Trim();
		VideoManager.VideoPath = args[1].Trim();
		App.Run();
	}
}