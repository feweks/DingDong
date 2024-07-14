
namespace Dong;

class Entry
{
    public static void Main(string[] args)
    {
        if (!args.Contains("-server"))
        {
            Client.Program.Initialize();
            Client.Program.Run();
        }
    }
}