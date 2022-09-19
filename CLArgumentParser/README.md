# Command-line-argument-parser

This is a simple command line argument parser that pretend be configurated with just one string pattern.

The pattern describes the way arguments shuuld be interpteted

**Especification:**

The string pattern has four parts: CaseSensitive, ArgCharName, ArgCharValue, args.

And have this form:  [CaseSensitive] [ArgCharName:c] [ArgCharValue:c] args:<argument list specification>

**Just args is mandatory**

**CaseSensitive**: optional. Indicates the parameters will be case sensitive, case insensitive if not especified.

**ArgCharName**: optional. It's the prefix for each named argument, by default hyphen - ; example ArgCharName:/

**ArgCharValue**: optional. It's the character thar identify the asignation, by fefault colon :  example  ArgCharValue:=

**Args**: it's the pattern to extract argument values, it is a list separated by spaces. Each element of the list has the form: [*]argname[:value]

- Each argname can be prefixed with an asterisk wich means it is mandatory.
- The fixed siffix text ":value" is optional and means that a value is expected after the argument name.
- If the element doesn't have the suffix ":value" it will be a boolean parameter, it's like a switch.
- Only the argnames suffixed with ":value" can be optional.

CaseSensitive, ArgCharName and ArgCharValue are optionals, and Args must be specified.

>Full example1:
  argcharname:/ argcharvalue:= args:*input:value output:value F S
 
In this example the valid arguments are:
  
- input: is mandatory and expects a value after the argument name
- output: is optional and must have a value after the argument is especified
- F: it's a switch
- S: it's a switch

>Full example2:
  argcharname:/ argcharvalue:= casesensitive args:*input:value output:value F S f
 
In this example the arguments are case sensitive the valid arguments are:
- input: is mandatory and expects a value after the argument name
- output: is optional and must have a value after the argument is especified
- F: it's a switch
- S: it's a switch
- f: it's a switch

Using the class:
```
static void Main(string[] args)
      {
         // The arguments of this program should be:
         // First, they are case sensitive
         // Argument names must start with a slash /
         // There are two valued arguments: /input and /output
         // The value of each argument will be separated with an equal sign
         // There are two switches: /S and /F
         
         // Configure parameters in a single line:
         ArgumentParser argumentParser = new ArgumentParser("argcharname:/ argcharvalue:= CaseSensitive args:*input:value output:value F S");
         try
         {
            argumentParser.Parse(args);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            Console.WriteLine("\nUSAGE:\n");
            
            // The class have a useful function that shows the list of parameters in console.
            
            argumentParser.ShowUsage(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return;
         }

         Console.WriteLine("Executed as: {0}", Environment.CommandLine);
         
         // And using the functions GetArgumentValue and GetSwitch you can get the argument values

         Console.WriteLine("Argument /input value: {0}", argumentParser.GetArgumentValue("input"));
         Console.WriteLine("Argument /output value: {0}", argumentParser.GetArgumentValue("output"));
         Console.WriteLine("Argument /F value: {0}", argumentParser.GetSwitch("F"));
         Console.WriteLine("Argument /F value: {0}", argumentParser.GetSwitch("S"));

         Console.Write("End.");
         Console.ReadLine();
      }
   }
```

For instance, if you run:
```
program.exe  /input="archivo datos.in" /output=archivo.out /F /S
```
  
The result will be:
```
Argument /input value: archivo datos.in
Argument /output value: archivo.out
Argument /F value: True
Argument /F value: True
```

Hope you enjoy it.
