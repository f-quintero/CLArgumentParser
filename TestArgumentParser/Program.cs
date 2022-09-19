using CLArgumentParser;
namespace TestArgumentParser
{
   internal class Program
   {
      static void Main(string[] args)
      {
         // The arguments of this program should be:
         // First, they are case sensitive
         // Argument names must start with a slash /
         // There are two valued arguments: /input and /output
         // The value of each argument will be separated with an equal sign
         // There are two switches: /S and /F
         ArgumentParser argumentParser = new ArgumentParser("argcharname:/ argcharvalue:= CaseSensitive args:*input:value output:value F S");
         try
         {
            argumentParser.Parse(args);
            argumentParser.SetDescription("input", "input json file");
            argumentParser.SetDescription("F", "Force");
            argumentParser.SetDescription("S", "Save output to default file");
            argumentParser.ShowUsage("program.exe");
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            Console.WriteLine("\nUSAGE:\n");
            argumentParser.ShowUsage(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return;
         }

         Console.WriteLine("Executed as: {0}", Environment.CommandLine);

         Console.WriteLine("Argument /input value: {0}", argumentParser.GetArgumentValue("input"));
         Console.WriteLine("Argument /output value: {0}", argumentParser.GetArgumentValue("output"));
         Console.WriteLine("Argument /F value: {0}", argumentParser.GetSwitch("F"));
         Console.WriteLine("Argument /F value: {0}", argumentParser.GetSwitch("S"));

         Console.Write("End.");
         Console.ReadLine();
      }
   }
}