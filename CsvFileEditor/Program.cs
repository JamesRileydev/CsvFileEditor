using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;

namespace CsvFileEditor
{
    public class Program
    {
        private class Options
        {
            [Option("input-file", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option("output-file", Required = true, HelpText = "Output file destination.")]
            public string OutputFile { get; set; }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptionsAndReturnExitCode)
                .WithNotParsed(HandleParseError);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Console.WriteLine("Failed to parse command line arguments.");

            foreach (var err in errs)
            {
                Console.WriteLine(err.ToString());
            }
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            Console.WriteLine("Input: " + opts.InputFile + ", Output: " + opts.OutputFile);

            var records = File.ReadLines(opts.InputFile);

            Console.WriteLine("Read in " + records.Count() + " records.");

            var newRecords = new List<string>(new[] { "AccountNumber,LoanId,FirstName,LastName,AmountDue,DateDue,SSN" });

            var re = new Regex(@"(\d{6}),(\d{3}),""(\w+), (\w+)"",""?([\d,]+)""?,(\d{8}),(\d{4})", RegexOptions.Compiled);

            foreach (var record in records)
            {
                var match = re.Match(record);

                if (match.Success)
                {
                    var accountNumber = match.Groups[1].Value;
                    var loanId = match.Groups[2].Value;
                    var name = match.Groups[4].Value + " " + match.Groups[3].Value;
                    var firstName = match.Groups[4].Value;
                    var amountDue = "$" + int.Parse(match.Groups[5].Value.Replace(",", "")) / 100.0m + ".00";
                    var dueDate = match.Groups[6].Value;
                    var ssn = match.Groups[7].Value;

                    var newRecord = string.Join(",", accountNumber, loanId, name, amountDue, dueDate, ssn);

                    newRecords.Add(newRecord);
                }
            }

            File.WriteAllLines(opts.OutputFile, newRecords);
        }
    }
}

