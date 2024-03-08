class Program
{
	public static void Main(string[] args)
	{
		// Check for if they provided a video location
		// TODO: Check for if there is actually a video in the directory
		if (args.Length <= 0)
		{
			Console.WriteLine("You are missing the path to the video you wish to open.");
			return;
		}

		// TODO: Check for if they actually have FFMPEG installed
		

		// Set the video path
		// then run the app
		VideoManager.Path = args[0].Trim();
		App.Run();
	}
}