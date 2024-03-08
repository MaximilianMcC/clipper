class Program
{
	public static void Main(string[] args)
	{
		// Check for if they provided an FFMPEG location
		// TODO: Check for if there is actually ffmpeg stuff in the directory
		if (args.Length <= 0)
		{
			Console.WriteLine("You are missing the path to the video you wish to open.");
			return;
		}

		// Set the video path
		// then run the app
		VideoManager.VideoPath = args[0].Trim();
		App.Run();
	}
}