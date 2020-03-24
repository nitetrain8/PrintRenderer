using PrintRenderer;
using PrintRenderer.TableRenderer;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Data;
using ADODB;

namespace TestCode1
{

    public class MyFonts
    {
        public static readonly Font Consolas10 = new Font("Consolas", 10);
        public static readonly Font Consolas6 = new Font("Consolas", 6);
        public static readonly Font Consolas20 = new Font("Consolas", 20);
    }

    internal class TestCode1
    {

        public static int Main(string[] args)
        {
            Test4();

            return 0;
        }

        private static void Test4()
        {
            Console.WriteLine("nothing to see here....");
            var printer = new ReportPrinter.FullSQLRecipeStepsReport(2);
            //var printer = new ReportPrinter.DummyRecipeStepsReport();
            printer.Printer.Document.PrinterSettings.PrinterName = "Microsoft Print to PDF";
            printer.Printer.Document.PrinterSettings.PrintToFile = true;
            printer.Printer.Document.PrinterSettings.PrintFileName = Path.Combine(KnownFolders.Downloads, "hmm.pdf");
            Console.WriteLine("Begin printing...");
            printer.Print();
            Console.WriteLine("yay!");
        }

        private static void Main1(string[] args)
        {
            //Test1();
            //Test2();
            const string file = "testcode2.pdf";
            SimpleDocumentRenderer doc = CreatePDFPRinter(file);
            Test3(doc);
           
            //var doc = new SimpleGridPrinter("Xerox VersaLink C405 Biolab Bullpen");
            //Test3(doc);
        }

        private static void Test3(SimpleDocumentRenderer doc)
        {


            int[] widths = new int[] { 50, 200, 350, 150 };

            Func<string[]> GetStep2 = MakeStepGetter();

            int i = -1;
            int max_rows = 100;
            CallbackRowRenderer row = new CallbackRowRenderer((row_renderer) =>
            {
                TextCell cell(int idx) { return row_renderer.Cells[idx] as TextCell; }
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
                    cell(j+1).SetText(step[j], MyFonts.Consolas6);
                return true;
            });
            doc.AddRow(new TextCell("Recipe Steps Report", MyFonts.Consolas20, Alignment.Center, 0));
            doc.AddRow(new TextCell(" ", MyFonts.Consolas10)); // spacer
            doc.AddRow(row);

            for (int n = 0; n < 4; ++n)
                row.AddCell(new TextCell("", MyFonts.Consolas6, Alignment.Left, widths[n]));
            doc.Print();
        }

        private static void Test2()
        {
            const string file = "testcode2.pdf";
            SimpleDocumentRenderer doc = CreatePDFPRinter(file);

            int[] widths = new int[] { 50, 200, 350, 150 };

            Func<string[]> GetStep2 = MakeStepGetter();
            for (int i = 0; i < 200; ++i)
            {
                Row row = new Row();
                string[] step = GetStep2();
                row.AddCell(new TextCell(i.ToString(), MyFonts.Consolas6, Alignment.Left, widths[0]));
                for (int j = 0; j < 3; ++j)
                {
                    row.AddCell(new TextCell(step[j], MyFonts.Consolas6, Alignment.Left, widths[j + 1]));
                }

                doc.AddRow(row);
            }
            doc.Print();
        }

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

        private static void Test1()
        {
            const string file = "testcode.pdf";
            SimpleDocumentRenderer doc = CreatePDFPRinter(file);

            string lorem_text = File.ReadAllText(Path.Combine(KnownFolders.Downloads, "lorem2.txt"));
            string[] lorem_words = lorem_text.Split(' ');
            int index = 0;
            string lorem() => lorem_words[index++ % lorem_words.Length];
            string lorem2() => lorem() + " " + lorem();

            for (int i = 0; i < 3; ++i)
            {
                AddRowWithText(doc, lorem2(), lorem2(), lorem2());
            }
            doc.Print();
        }

        private static SimpleDocumentRenderer CreatePDFPRinter(string file)
        {
            string pfn = Path.Combine(KnownFolders.Downloads, file);

            SimpleDocumentRenderer doc = new SimpleDocumentRenderer("Microsoft Print to PDF");
            doc.Document.PrinterSettings.PrintFileName = pfn;
            doc.Document.PrinterSettings.PrintToFile = true;
            return doc;
        }

        private static void AddRowWithText(SimpleDocumentRenderer printer, string text1, string text2, string text3)
        {
            Row row = new Row();
            printer.AddRow(row);

            TextCell c1 = new TextCell(text1, MyFonts.Consolas10);
            row.AddCell(c1);
            TextCell c2 = row.AddTextCell(text2, MyFonts.Consolas10);
            TextCell c3 = row.AddTextCell(text3, MyFonts.Consolas10);

            c1.Width = 200;
            c2.Width = 200;
            c3.Width = 200;

        }
    }
}
