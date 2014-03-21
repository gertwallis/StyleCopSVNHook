using StyleCop;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TortoiseSVNStyleCop
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            int foundViolatons = 0;

            string[] filePaths = File.ReadAllLines(args[0]);
            string projectPath = GetRootPath(filePaths);
            string settingsPath = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, @"Settings.StyleCop");
            if (File.Exists(settingsPath))
            {
                settingsPath = null;
            }
            Console.Error.WriteLine("DEBUG: {0}", settingsPath);
            StyleCopConsole styleCopConsole = new StyleCopConsole(settingsPath, false, null, null, true);

            Configuration configuration = new Configuration(null);

            CodeProject project = new CodeProject(0, projectPath, configuration);

            foreach (string file in filePaths)
            {
                var loaded = styleCopConsole.Core.Environment.AddSourceCode(project, file, null);
            }

            List<Violation> violations = new List<Violation>();
            styleCopConsole.ViolationEncountered += ((sender, arguments) => violations.Add(arguments.Violation));

            List<string> output = new List<string>();
            styleCopConsole.OutputGenerated += ((sender, arguments) => output.Add(arguments.Output));

            styleCopConsole.Start(new[] { project }, true);

            foreach (string file in filePaths)
            {
                List<Violation> fileViolations = violations.FindAll(viol => viol.SourceCode.Path == file);

                if (fileViolations.Count > 0)
                {
                    foundViolatons = 1;
                    Console.Error.WriteLine("{0} - {1} violations.", fileViolations[0].SourceCode.Name, fileViolations.Count);
                    foreach (Violation violation in fileViolations)
                    {
                        Console.Error.WriteLine("      {0}: Line {1}-{2}", violation.Rule.CheckId, violation.Line, violation.Message);
                    }
                }
            }
            Environment.Exit(foundViolatons);
        }

        private static string GetRootPath(string[] filePaths)
        {
            if (filePaths.Length > 0)
            {
                string[] testAgainst = filePaths[0].Split('/');
                int noOfLevels = testAgainst.Length;
                foreach (string filePath in filePaths)
                {
                    string[] current = filePath.Split('/');
                    int level;
                    for (level = 0; level <= Math.Min(noOfLevels, current.Length) - 1; level++)
                    {
                        if (testAgainst[level] != current[level])
                        {
                            break;
                        }
                    }
                    noOfLevels = Math.Min(noOfLevels, level);
                }

                return (testAgainst.Take(noOfLevels).Aggregate((m, n) => m + "/" + n));
            }
            return string.Empty;
        }
    }
}