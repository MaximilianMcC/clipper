namespace clipper;

static class Program
{
	[STAThread]
	public static void Main()
	{
		// Run the form
		ApplicationConfiguration.Initialize();
		Application.Run(new Form1());
	}    
}