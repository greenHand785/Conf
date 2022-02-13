using System;
using CommandLine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace SilentOrbit.ProtocolBuffers
{
    /// <summary>
    /// Options set using Command Line arguments
    /// </summary>
    public class Options
    {
        /// <summary>
        /// Convert message/class and field/propery names to CamelCase
        /// </summary>
        [Option("preserve-names", HelpText = "Keep names as written in .proto, otherwise class and field names by default are converted to CamelCase")]
        public bool PreserveNames { get; set; }

        /// <summary>
        /// If false, an error will occur.
        /// </summary>
        [Option("fix-nameclash", HelpText = "If a property name is the same as its class name or any subclass the property will be renamed. if the name clash occurs and this flag is not set, an error will occur and the code generation is aborted.")]
        public bool FixNameclash { get; set; }

        /// <summary>
        /// Generated code indent using tabs
        /// </summary>
        [Option('t', "use-tabs", HelpText = "If set generated code will use tabs rather than 4 spaces.")]
        public bool UseTabs { get; set; }

        [Value(0, Required = true)]
        public IEnumerable<string> InputProto { get; set; }

        /// <summary>
        /// Path to the generated cs files
        /// </summary>
        [Option('o', "output", Required = false, HelpText = "Path to the generated .cs file.")]
        public string OutputPath { get; set; }

        /// <summary>
        /// Use experimental stack per message type
        /// </summary>
        [Option("experimental-message-stack", HelpText = "Assign the name of the stack implementatino to use for each message type, included options are ThreadSafeStack, ThreadUnsafeStack, ConcurrentBagStack or the full namespace to your own implementation.")]
        public string ExperimentalStack { get; set; }

        /// <summary>
        /// If set default constructors will be generated for each message
        /// </summary>
        [Option("ctor", HelpText = "Generate constructors with default values.")]
        public bool GenerateDefaultConstructors { get; set; }

        /// <summary>
        /// Generate nullable primitives for optional fields
        /// </summary>
        [Option("nullable", HelpText = "Generate nullable primitives for optional fields")]
        public bool Nullable { get; set; }

        /// <summary>
        /// Exclude code that require .NET 4
        /// </summary>
        [Option("net2", HelpText = "Exclude code that require .NET 4")]
        public bool Net2 { get; set; }

        /// <summary>
        /// De/serialize DateTime as DateTimeKind.Utc
        /// </summary>
        [Option("utc", HelpText = "De/serialize DateTime as DateTimeKind.Utc")]
        public bool Utc { get; set; }

        /// <summary>
        /// Add the [Serializable] attribute to generated classes
        /// </summary>
        [Option("serializable-attr", HelpText = "Add the [Serializable] attribute to generated classes")]
        public bool SerializableAttributes { get; set; }

        /// <summary>
        /// Skip serializing properties having the default value.
        /// </summary>
        [Option("skip-default", HelpText = "Skip serializing properties having the default value")]
        public bool SkipSerializeDefault { get; set; }

        /// <summary>
        /// Do not output ProtocolParser.cs
        /// </summary>
        [Option("no-protocol-parser", HelpText = "Do not output ProtocolParser.cs")]
        public bool NoProtocolParser { get; set; }

        /// <summary>
        /// Don't generate code from imported .proto files.
        /// </summary>
        [Option("no-generated-imports", HelpText = "Do not generate files for imported protos.")]
        public bool NoGenerateImported { get; set; }

        public static void TryParse(string[] args, Action<Options> onSuccess)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(options =>
            {
                bool error = false;

                //Do any extra option checking/cleanup here
                var inputs = new List<string>(options.InputProto);
                options.InputProto = inputs;
                for (int n = 0; n < inputs.Count; n++)
                {
                    inputs[n] = Path.GetFullPath(inputs[n]);
                    if (File.Exists(inputs[n]) == false)
                    {
                        Console.Error.WriteLine("File not found: " + inputs[n]);
                        error = true;
                    }
                }

                //Backwards compatibility
                string firstPathCs = inputs[0];
                firstPathCs = Path.Combine(
                    Path.GetDirectoryName(firstPathCs),
                    Path.GetFileNameWithoutExtension(firstPathCs)) + ".cs";

                if (options.OutputPath == null)
                {
                    //Use first .proto as base for output
                    options.OutputPath = firstPathCs;
                    Console.Error.WriteLine("Warning: Please use the new syntax: --output \"" + options.OutputPath + "\"");
                }
                //If output is a directory then the first input filename will be used.
                if (options.OutputPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || Directory.Exists(options.OutputPath))
                {
                    Directory.CreateDirectory(options.OutputPath);
                    options.OutputPath = Path.Combine(options.OutputPath, Path.GetFileName(firstPathCs));
                }
                options.OutputPath = Path.GetFullPath(options.OutputPath);

                if (options.ExperimentalStack != null && !options.ExperimentalStack.Contains("."))
                    options.ExperimentalStack = "global::SilentOrbit.ProtocolBuffers." + options.ExperimentalStack;

                if (!error)
                {
                    onSuccess(options);
                }
            });
        }
    }
}
