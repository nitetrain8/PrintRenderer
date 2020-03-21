using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using PrintRenderer;
using PrintRenderer.TableRenderer;
using ADODB;
using System.Drawing;

namespace ReportPrinter
{

    internal class ReportFonts
    {
        public static Font TitleFont = new Font("Consolas", 18);
        public static Font MetaFont = new Font("Consolas", 6);
        public static Font ColumnHeaderFont = new Font("Consolas", 10);
        public static Font RecordFont = new Font("Consolas", 8);
    }

    internal class SingleCellRowRenderer : RowRenderer
    {
        private CellTextRenderer MyCell;
        public SingleCellRowRenderer(string text="", Font font=null, Alignment alignment=Alignment.Left) : base()
        {
            MyCell = new CellTextRenderer(text, font, alignment, 650);
            Cells.Add(MyCell);
        }
        public void SetText(string text, Font font)
        {
            MyCell.SetText(text, font);
        }
    }

    internal class ReportTitle : SingleCellRowRenderer
    {
        public ReportTitle(string title) : base(title, ReportFonts.TitleFont, Alignment.Center)
        {

        }
    }

    internal class ReportMetadata : SingleCellRowRenderer
    {
        public ReportMetadata(string data = "") : base(data, ReportFonts.MetaFont, Alignment.Left)
        {

        }
    }

    internal class RecordTableRow : RowRenderer
    {
        public RecordTableRow(Font font, params string[] headers) : base()
        {
            for (int i = 0; i < headers.Length; ++i)
            {
                var cell = new CellTextRenderer(headers[i], font, Alignment.Left);
                AddCell(cell);
            }
        }
        public void SetWidths(params int[] args)
        {
            if (args.Length != Cells.Count)
                throw new ArgumentException($"Number of widths provided does not match number of cells. ({args.Length} != {Cells.Count})");
            for (int i = 0; i < args.Length; ++i)
                Cells[i].Width = args[i];
        }
    }

    internal class RecipeStepRecord
    {
        CellTextRenderer StepNum;
        CellTextRenderer Timestamp;
        CellTextRenderer Step;
        CellTextRenderer StepType;

        public RecipeStepRecord()
        {
            StepNum = new CellTextRenderer("", ReportFonts.RecordFont);
            Timestamp = new CellTextRenderer("", ReportFonts.RecordFont);
            Step = new CellTextRenderer("", ReportFonts.RecordFont);
            StepType = new CellTextRenderer("", ReportFonts.RecordFont);
        }
        public void SetWidths(int[] widths)
        {
            StepNum.Width = widths[0];
            Timestamp.Width = widths[1];
            Step.Width = widths[2];
            StepType.Width = widths[3];
        }
        public void SetData(string num, string time, string step, string type)
        {
            StepNum.SetText(num);
            Timestamp.SetText(time);
            Step.SetText(step);
            StepType.SetText(type);
        }
        private bool UpdateRowData(CallbackRowRenderer row)
        {
            return false;
        }
        public void Connect(CallbackRowRenderer r)
        {
            r.AddCell(StepNum);
            r.AddCell(Timestamp);
            r.AddCell(Step);
            r.AddCell(StepType);
        }

    }

    public class RecipeStepsReportPrinter
    {
        public SimpleGridPrinter Printer;

        // Report elements as helper classes
        ReportTitle _title;
        ReportMetadata _metadata;
        RecordTableRow _headers;
        RecipeStepRecord _record;
        CallbackRowRenderer _recordRow;

        public RecipeStepsReportPrinter(string printer_name="")
        {
            _Init(printer_name);
        }
        private void _Init(string printer_name)
        {
            _Init(new SimpleGridPrinter(printer_name));
        }
        public void Print()
        {
            Printer.Print();
        }
        private void _Init(SimpleGridPrinter p)
        {
            Printer = p;

            int[] widths = new int[] { 50, 200, 350, 150 };

            _title = new ReportTitle("Recipe Steps Report");
            _metadata = new ReportMetadata("Batch name: \"test\"");
            _headers = new RecordTableRow(ReportFonts.ColumnHeaderFont, "#", "Timestamp", "Step", "Type");
            _recordRow = new CallbackRowRenderer(UpdateRow);

            _record = new RecipeStepRecord();
            _record.Connect(_recordRow);

            p.AddRow(_title);
            p.AddRow(_metadata);
            p.AddRow(_headers);
            p.AddRow(_recordRow);

            _headers.SetWidths(widths);
            _record.SetWidths(widths);

            data = new string[][]
            {
                new string[] {"foo", "bar", "baz" },
                new string[] {"bar", "foo", "baz" },
                new string[] {"baz", "bar", "foo" },
                new string[] {"bar", "baz", "foo" },
            };
            _index = 0;
        }
        string[][] data;
        int _index = 0;
        private bool UpdateRow(CallbackRowRenderer row)
        {
            if (_index >= data.Length)
                return false;

            var rowdata = data[_index++];
            _record.SetData(_index.ToString(), rowdata[0], rowdata[1], rowdata[2]);
            return true;
        }
        
    }
}
