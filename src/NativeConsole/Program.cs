// See https://aka.ms/new-console-template for more information
using Lemon.Native.Winx64.Natives;

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine($"start param:{string.Join(' ',args)}");
        NativeClipboard.ClipboardIdle();
        if (args.Length > 0 ) 
        {
            try
            {
                switch (args[0].ToLower())
                {
                    case "gettext":
                        var text = NativeClipboard.GetText();
                        Console.WriteLine(text);
                        break;
                    case "settext":
                        if (args.Length > 1)
                        {
                            NativeClipboard.SetText(args[1]);
                            Console.WriteLine(0);
                        }
                        else
                        {
                            Console.WriteLine(1);
                        }
                        break;
                    case "clear":
                        if (NativeClipboard.TryClear())
                        {
                            Console.WriteLine(0);
                        }
                        else
                        {
                            Console.WriteLine(1);
                        }
                        break;
                    default:
                        Console.WriteLine(1);
                        break;
                }
            }
            catch 
            {
                Console.WriteLine(1);
            }
        }
    }
}
