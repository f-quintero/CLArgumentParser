using System.Text.RegularExpressions;

namespace CLArgumentParser
{
   internal class Argument
   {
      public string Name { get; set; }

      public bool Mandatory { get; set; }

      public bool HasValue { get; set; }

      public string Value { get; set; }

      public string Description { get; set; }
   }

   /// <summary>
   /// Argument parser
   /// </summary>
   public class ArgumentParser
   {
      /// <summary>
      /// The pattern describes the way arguments can be interpteted
      /// Especification
      /// CaseSensitive: indicates the parameters will be case sensitive, case insensitive if not especified
      /// ArgCharName: it's the prefix for each named argument, by default hyphen - ; example ArgCharName:/
      /// ArgCharValue: it's the character thar identify the asignation, by fefault colon :  example  ArgCharValue:=
      /// Args: it's the pattern to extract argument values, it is a list separated by spaces. Each element of the list has the form: [*]argname[:value]
      /// Each argname can be prefixed with an asterisk wich means it is mandatory
      /// The fixed siffix text ":value" is optional and means that a value is expected after the argument name
      /// If the element doesn't have the suffix ":value" it will be a boolean parameter, like a switch
      /// Only the argnames suffixed with ":value" can be optional
      /// CaseSensitive, ArgCharName and ArgCharValue ar optional and must be specified before Args
      /// Full example:
      ///   argcharname:/ argcharvalue:= args:*input:value output:value F S
      ///  
      /// In this example the valid arguments are:
      ///   input: is mandatory and expects a value after the argument name
      ///   output: is optional and must have a value after the argument is especified
      ///   F: it's a switch
      ///   S: it's a switch
      /// </summary>
      
      string pattern;
      bool caseSensitive = false;
      string argCharName = "-";
      string argCharValue = ":";

      List<Argument> _arguments = new List<Argument>();

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="pattern">Argument pattern</param>
      public ArgumentParser(string pattern)
      {
         this.pattern = pattern;
         BreakDownPattern();
      }

      private void BreakDownPattern()
      {
         string argumentLists = "";
         Regex rePattern = new Regex(@"((?<CaseSensitive>CaseSensitive)|(?<p2>ArgCharName:(?<ArgCharName>.))|(?<p3>ArgCharValue:(?<ArgCharValue>.))|(?<p4>args:(?<args>.*)))",
            RegexOptions.IgnoreCase);
         
         MatchCollection breakDowns = rePattern.Matches(pattern);

         caseSensitive = false;
         foreach (Match breakDown in breakDowns)
         {
            if (breakDown.Groups.ContainsKey("CaseSensitive") && breakDown.Groups["CaseSensitive"].Value != "")
            {
               caseSensitive = true;
            }

            if (breakDown.Groups.ContainsKey("ArgCharName"))
            {
               if (breakDown.Groups["ArgCharName"].Value != "")
                  argCharName = breakDown.Groups["ArgCharName"].Value;
            }

            if (breakDown.Groups.ContainsKey("ArgCharValue"))
            {
               if (breakDown.Groups["ArgCharValue"].Value != "")
                  argCharValue = breakDown.Groups["ArgCharValue"].Value;
            }

            if (breakDown.Groups.ContainsKey("args"))
            {
               if (breakDown.Groups["args"].Value != "")
                  argumentLists = breakDown.Groups["args"].Value;
            }
         }

         if (string.IsNullOrEmpty(argumentLists))
         {
            throw new Exception("You must include at least the args specification");
         }

         // now analize the list
         var arguments = argumentLists.Split(' ');
         for(int i=0; i<arguments.Length; i++)
         {
            arguments[i] = arguments[i].Trim();

            bool mandatory = false;
            bool hasValue = false;
            if (arguments[i].StartsWith("*"))
            {
               mandatory = true;
               arguments[i] = arguments[i].Substring(1);
            }

            if (arguments[i].ToLower().EndsWith(":value"))
            {
               hasValue = true;
               arguments[i] = arguments[i].Substring(0, arguments[i].IndexOf(":"));
            }
            else
            {
               hasValue = false;
            }

            if (mandatory && !hasValue)
            {
               throw new Exception("Only valued arguments can be mandatories");
            }

            arguments[i] = argCharName + arguments[i];

            if (!caseSensitive)
               arguments[i] = arguments[i].ToLower();

            _arguments.Add(
               new Argument() 
               { 
                 Name = arguments[i].Trim(),
                 Mandatory = mandatory,
                 HasValue = hasValue,
                 Value = !hasValue ? "true" : "",
                 Description = ""
               });
         }
      }

      /// <summary>
      /// Parse actual arguments using the pattern
      /// </summary>
      /// <param name="args">List of arguments</param>
      /// <exception cref="Exception">An exception is thrown if a mandatory parameter is misson</exception>
      public void Parse(string[] args)
      {
         for (int i=0; i < args.Length; i++)
         {
            string argName = "";
            string argValue = "";

            int p = args[i].IndexOf(argCharValue);

            if (p >= 0)
            {
               argName = args[i].Substring(0, p);
               argValue = args[i].Substring(p + 1);
            }
            else
            {
               argName = args[i];
            }

            if (!caseSensitive)
               argName = argName.ToLower();

            var argumentSpecification = _arguments.Where(x => x.Name == argName).FirstOrDefault();

            if (argumentSpecification != null)
            {
               if (argumentSpecification.HasValue)
               {
                  argumentSpecification.Value = argValue;
               }
               else
               {
                  argumentSpecification.Value = "true";
               }
            }
         }

         if (_arguments.FindAll(a => a.HasValue && a.Mandatory && a.Value == "").Count > 0)
         {
            throw new Exception("There are missing mandatory parameters");
         }
      }

      /// <summary>
      /// Get the value of a switch argument
      /// </summary>
      /// <param name="argumentName">Argument name</param>
      /// <returns>True if the argument was parsed, False if not</returns>
      /// <exception cref="Exception">An exception is thrown if the requested argument does not exists</exception>
      public bool GetSwitch(string argumentName)
      {
         if (!argumentName.StartsWith(argCharName))
         {
            argumentName = argCharName + argumentName;
         }

         if (!caseSensitive)
            argumentName = argumentName.ToLower();

         var arg = _arguments.FindAll(a => a.Name == argumentName).FirstOrDefault();

         if (arg != null)
         {
            return (arg.Value == "true");
         }
         else
         {
            throw new Exception(String.Format("Argument {0} has not been defined", argumentName));
         }
      }

      /// <summary>
      /// Get the parsed value of the argument
      /// </summary>
      /// <param name="argumentName">Argument name</param>
      /// <returns>The string value of the argument</returns>
      /// <exception cref="Exception">An exception is thrown if the requested argument does not exists</exception>
      public string GetArgumentValue(string argumentName)
      {
         if (!argumentName.StartsWith(argCharName))
         {
            argumentName = argCharName + argumentName;
         }

         if (!caseSensitive)
            argumentName = argumentName.ToLower();

         var arg = _arguments.FindAll(a => a.Name == argumentName).FirstOrDefault();

         if (arg != null)
         {
            return arg.Value;
         }
         else
         {
            throw new Exception(String.Format("Argument {0} has not been defined", argumentName));
         }
      }

      /// <summary>
      /// Set the optional description of an agument
      /// </summary>
      /// <param name="argumentName">Argument name</param>
      /// <param name="description">Argument description</param>
      public void SetDescription(string argumentName, string description)
      {
         if (!this.caseSensitive)
            argumentName = argumentName.ToLower();

         if (!argumentName.StartsWith(this.argCharName))
            argumentName = this.argCharName + argumentName;
         
         var arg = this._arguments.FindAll(a => a.Name == argumentName).FirstOrDefault();

         if (arg != null)
         {
            arg.Description = description.Trim();
         }
      }

      /// <summary>
      /// Show the program usage
      /// </summary>
      /// <param name="command">Program name to be shown in the usage text</param>
      public void ShowUsage(string command)
      {
         string usage = command + " ";
         string mandatories = "";

         foreach(var arg in _arguments)
         {
            if (arg.HasValue)
               usage += string.Format("{0}{1}samplevalue ", arg.Name, argCharValue);
            else
               usage += string.Format("{0} ", arg.Name);

            if (arg.Mandatory)
            {
               mandatories += string.Format("{0}, ", arg.Name);
            }

            mandatories = mandatories.Trim(new char[]{ ' ', ','}) + " ";
         }

         Console.WriteLine("{0}\n", usage);

         if (!string.IsNullOrEmpty(mandatories))
         {
            Console.WriteLine("The following arguments are mandatories: {0}\n", mandatories);
         }

         if (caseSensitive)
         {
            Console.WriteLine("Argument names are case sensitive\n");
         }

         var argDecriptions = _arguments.Where(a => a.Description != "").ToList();
         if (argDecriptions.Count() > 0)
         {
            Console.WriteLine("Arguments:");
            foreach (var arg in argDecriptions)
            {
               Console.WriteLine("{0} : {1}", arg.Name, arg.Description);
            }
            Console.WriteLine();
         }
      }
   }
}