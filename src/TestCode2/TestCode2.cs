using CommandLine;
using PrintRenderer;
using PrintRenderer.TableRenderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;

namespace TestCode2
{

    internal class PrintOptions
    {
        [Option("print", Required = false, HelpText = "File to Print To", Default ="")]
        public string PrinterName { get; set; }

        [Option("print-file", Required = false, HelpText = "Print to file with the specified filename", Default ="")]
        public string PrintFileName { get; set; }

        [Value(0, MetaName = "[show printers]", Required = false, HelpText = "List available printers if no printer is specified")]
        public string ShowPrinters { get; set; }
    }

    internal class MyFonts
    {
        internal static readonly Font Consolas10 = new Font("Consolas", 10);
        internal static readonly Font Consolas6 = new Font("Consolas", 6);
        internal static readonly Font Consolas20 = new Font("Consolas", 20);
    }

    internal class TestPrinter
    {

        private static Func<string[]> MakeStepGetter()
        {
            Random rng = new Random();
            string[] step_types = new string[] { "Set", "Wait", "Wait Until" };
            string[] variables = new string[] { "AgPV(RPM)", "TempPV(C)", "DOO2FlowControllerRequestLimited(%)" };
            string[] ops = new string[] { "<", ">", "<=", ">=", "=", "!=" };
            string pick_one(string[] arr) => arr[rng.Next(0, arr.Length)];
            string step_type() => pick_one(step_types);
            string variable() => pick_one(variables);
            string op() => pick_one(ops);
            DateTime min_date = new DateTime(2018, 1, 1, 0, 0, 0);
            DateTime max_date = DateTime.Now;
            double diff = (max_date - min_date).TotalHours;
            string date()
            {
                return (min_date + new TimeSpan(rng.Next(0, (int)diff), 0, 0)).ToString();
            }

            string[] GetStep2()
            {
                string[] step = GetStep();
                string tmp = step[0];
                step[0] = step[1];
                step[1] = tmp;
                return step;
            }

            string[] GetStep()
            {
                string step = step_type();
                if (step == "Set")
                {
                    return new string[3] { $"Set {variable()} to {(rng.NextDouble() * 30).ToString("F2")}", date(), "Set" };
                }
                else if (step == "Wait")
                {
                    return new string[3] { $"Wait {rng.Next(10, 21)} seconds", date(), "Wait" };
                }
                else if (step == "Wait Until")
                {
                    return new string[3] { $"Wait Until {variable()} {op()} {(rng.NextDouble() * 30).ToString("F2")}", date(), "Wait Until" };
                }
                else
                {
                    throw new Exception("oops");
                }
            };
            return GetStep2;
        }

        internal static void PrintTestRecipeStepsReport(SimpleGridPrinter doc)
        {
            int[] widths = new int[] { 50, 200, 350, 150 };

            Func<string[]> GetStep2 = MakeStepGetter();

            int i = -1;
            int max_rows = 100;
            CallbackRowRenderer row = new CallbackRowRenderer((row_renderer) =>
            {
                CellTextRenderer cell(int idx) { return row_renderer.Cells[idx] as CellTextRenderer; }
                if (i++ >= max_rows)
                {
                    return false;
                }
                if (i == 0)
                {
                    cell(0).SetText("#", MyFonts.Consolas10);
                    cell(1).SetText("Timestamp", MyFonts.Consolas10);
                    cell(2).SetText("Step Text", MyFonts.Consolas10);
                    cell(3).SetText("Step Type", MyFonts.Consolas10);
                    return true;
                }

                var step = GetStep2();

                cell(0).SetText(i.ToString(), MyFonts.Consolas6);
                for (int j = 0; j < 3; ++j)
                {
                    cell(j + 1).SetText(step[j], MyFonts.Consolas6);
                }

                return true;
            });
            doc.AddRow(new CellTextRenderer("Recipe Steps Report", MyFonts.Consolas20, Alignment.Center, 0));
            doc.AddRow(new CellTextRenderer(" ", MyFonts.Consolas10)); // spacer
            doc.AddRow(row);

            for (int n = 0; n < 4; ++n)
            {
                row.AddCell(new CellTextRenderer("", MyFonts.Consolas6, Alignment.Left, widths[n]));
            }

            doc.Print();
        }
    }


    class TestCode2
    {
        static void print(string arg)
        {
            Console.WriteLine(arg);
        }

        static int Main(string[] args)
        {
            try
            {
                return Parser.Default.ParseArguments<PrintOptions>(args)
                        .MapResult(
                        (PrintOptions opts) => Handle(opts),
                        errs => 1
                        );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }

        private static int Handle(PrintOptions opts)
        {
            if (!String.IsNullOrWhiteSpace(opts.PrinterName))
            {
                return PrintFile(opts.PrinterName, opts.PrintFileName);
            }
            else
            {
                return ShowPrinters();
            }
        }

        private static int PrintFile(string PrinterName, string PrintFileName)
        {
            var doc = new SimpleGridPrinter(PrinterName);
            if (!String.IsNullOrWhiteSpace(PrintFileName))
            {
                doc.Document.PrinterSettings.PrintToFile = true;
                doc.Document.PrinterSettings.PrintFileName = PrintFileName;
            }
            TestPrinter.PrintTestRecipeStepsReport(doc);
            return 0;
        }

        static int ShowPrinters()
        {
            var printers = PrinterSettings.InstalledPrinters;
            if (printers.Count > 0)
            {
                print("Here are the available printers on your computer");
                foreach (string printer in printers)
                {
                    print($"  '{printer}'");
                    //ShowPrinterProperties(printer);
                }
            }
            else
            {
                print("Oh no! no printers found. Sucks for you lol!");
            }

            //ComparePrinters("Microsoft Print to PDF", "Xerox VersaLink C405 Biolab Bullpen");

            return 0;
        }
        #region PrinterProperties
        //private static void ComparePrinters(string p1, string p2)
        //{
        //    var props1 = GetPrinterProperties(p1);
        //    var props2 = GetPrinterProperties(p2);

        //    for (int i = 0; i < props1.Count; ++i)
        //    {
        //        var pd1 = props1[i];
        //        var pd2 = props2[i];
        //        if (pd1.Name != pd2.Name)
        //        {
        //            Console.WriteLine($"Name Mismatch: '{pd1.Name}' != '{pd2.Name}'");
        //            return;
        //        }
        //        Console.WriteLine($"{pd1.Name,-30} {pd1.Value,-30} {pd2.Value, -30}");
        //    }
        //}

        //private static List<PropertyData> GetPrinterProperties(string printer)
        //{
        //    List<PropertyData> properties = new List<PropertyData>();
        //    string query = $"SELECT * from Win32_Printer WHERE Name LIKE '{printer}'";
        //    using (var searcher = new ManagementObjectSearcher(query))
        //    using (var collection = searcher.Get())
        //    {
        //        foreach (var pobj in collection)
        //            foreach (var pd in pobj.Properties)
        //                properties.Add(pd);
        //    }
        //    return properties;
        //}

        //private static void ShowPrinterProperties(string PrinterName)
        //{
        //    string query = $"SELECT * from Win32_Printer WHERE Name LIKE '{PrinterName}'";
        //    using (var searcher = new ManagementObjectSearcher(query))
        //    using (var collection = searcher.Get())
        //    {
        //        try
        //        {
        //            foreach (ManagementObject printer in collection)
        //            {
        //                foreach (PropertyData p in printer.Properties)
        //                {
        //                    Console.WriteLine($"        {p.Name}: {p.Value}");
        //                    if (p.Name == "CapabilityDescriptions")
        //                        foreach (var descr in (string[])p.Value)
        //                            Console.WriteLine($"            {descr}");
        //                }
        //            }
        //        }
        //        catch (ManagementException ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}
        #endregion
    }
}
