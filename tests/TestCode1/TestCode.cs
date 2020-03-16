using PrintRenderer;
using PrintRenderer.TableRenderer;
using System;
using System.Drawing;
using System.IO;

namespace TestCode1
{

    public class MyFonts
    {
        public static readonly Font Consolas10 = new Font("Consolas", 10);
        public static readonly Font Consolas6 = new Font("Consolas", 6);
    }

    internal class TestCode
    {
        private static void Main(string[] args)
        {
            Test1();
            Test2();
        }

        private static void Test2()
        {
            const string file = "testcode2.pdf";
            SimpleGridPrinter doc = CreatePDFPRinter(file);

            int[] widths = new int[] { 200, 400, 150 };

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
                var step = GetStep();
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
                    return new string[3] { $"Set {variable()} to {(rng.NextDouble() * 30).ToString("F2")}", date(), "Set"};
                }
                else if (step == "Wait")
                {
                    return new string[3] { $"Wait {rng.Next(10, 21)} seconds", date(), "Wait"};
                }
                else if (step == "Wait Until")
                {
                    return new string[3] { $"Wait Until {variable()} {op()} {(rng.NextDouble() * 30).ToString("F2")}", date(), "Wait Until"};
                }
                else
                {
                    throw new Exception("oops");
                }
            };
            for (int i = 0; i < 1000; ++i)
            {
                var row = new RowRenderer();
                string[] step = GetStep2();
                for (int j = 0; j < 3; ++j)
                    row.AddColumn(new ColumnTextRenderer(step[j], MyFonts.Consolas6, TextAlignments.Left, widths[j]));
                doc.AddRow(row);
            }
            doc.Print();
        }

        private static void Test1()
        {
            const string file = "testcode.pdf";
            SimpleGridPrinter doc = CreatePDFPRinter(file);

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

        private static SimpleGridPrinter CreatePDFPRinter(string file)
        {
            string pfn = Path.Combine(KnownFolders.Downloads, file);

            SimpleGridPrinter doc = new SimpleGridPrinter("Microsoft Print to PDF");
            doc.Document.PrinterSettings.PrintFileName = pfn;
            doc.Document.PrinterSettings.PrintToFile = true;
            return doc;
        }

        private static void AddRowWithText(SimpleGridPrinter printer, string text1, string text2, string text3)
        {
            RowRenderer row = new RowRenderer();
            printer.AddRow(row);

            ColumnTextRenderer c1 = new ColumnTextRenderer(text1, MyFonts.Consolas10);
            row.AddColumn(c1);
            ColumnTextRenderer c2 = row.AddTextColumn(text2, MyFonts.Consolas10);
            ColumnTextRenderer c3 = row.AddTextColumn(text3, MyFonts.Consolas10);

            c1.Width = 200;
            c2.Width = 200;
            c3.Width = 200;

        }
    }
}
