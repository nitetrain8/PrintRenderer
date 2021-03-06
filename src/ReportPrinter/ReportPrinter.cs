﻿using PrintRenderer;
using PrintRenderer.TableRenderer;
using System;
using System.Drawing;
using System.Data.OleDb;

namespace ReportPrinter
{

    internal class ReportFonts
    {
        public static Font TitleFont = new Font("Consolas", 18);
        public static Font MetaFont = new Font("Consolas", 6);
        public static Font ColumnHeaderFont = new Font("Consolas", 10);
        public static Font RecordFont = new Font("Consolas", 8);
    }

    internal class SingleCellRowRenderer : Row
    {
        private TextCell MyCell;
        public SingleCellRowRenderer(string text = "", Font font = null, Alignment alignment = Alignment.Left) : base()
        {
            Alignment = alignment;
            MyCell = new TextCell(text, font, alignment, 650);
            AddCell(MyCell);
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

    internal class RecordTableRow : Row
    {
        public RecordTableRow(Font font, params string[] headers) : base()
        {
            for (int i = 0; i < headers.Length; ++i)
            {
                var cell = new TextCell(headers[i], font, Alignment.Left);
                AddCell(cell);
            }
        }
    }

    internal class RecipeStepRecord
    {
        public TextCell StepNum;
        public TextCell Timestamp;
        public TextCell Step;
        public TextCell StepType;
        public TextCell NewRecipe;
        public CallbackRowRenderer Row;
        private bool ConfiguredForSteps = true;

        public RecipeStepRecord(CallbackRowRenderer row)
        {
            StepNum = new TextCell("", ReportFonts.RecordFont);
            Timestamp = new TextCell("", ReportFonts.RecordFont);
            Step = new TextCell("", ReportFonts.RecordFont);
            StepType = new TextCell("", ReportFonts.RecordFont);

            NewRecipe = new TextCell("", ReportFonts.RecordFont, Alignment.Center);
            NewRecipe.Padding.Top = 10;
            NewRecipe.Padding.Bottom = 10;

            Row = row;
            ConfigureForRecipeSteps();
        }
        public void SetWidths(int[] widths)
        {
            StepNum.Width = widths[0];
            Timestamp.Width = widths[1];
            Step.Width = widths[2];
            StepType.Width = widths[3];
            int w = 0;
            for (int i = 0; i < widths.Length; ++i)
            {
                w += widths[i];
            }
            NewRecipe.Width = w;
        }
        public void SetRecipeStepData(string num, string time, string step, string type)
        {
            if (!ConfiguredForSteps)
            {
                ConfigureForRecipeSteps();
            }

            StepNum.SetText(num);
            Timestamp.SetText(time);
            Step.SetText(step);
            StepType.SetText(type);
        }
        public void SetNewRecipeData(string recipe_name)
        {
            if (ConfiguredForSteps)
            {
                ConfigureForNewRecipe();
            }

            NewRecipe.SetText($"------------------- New Recipe: \"{recipe_name}\" -------------------");
        }

        private void ConfigureForNewRecipe()
        {
            Row.ClearCells();
            Row.AddCell(NewRecipe);
            ConfiguredForSteps = false;
        }
        private void ConfigureForRecipeSteps()
        {
            Row.ClearCells();
            Row.AddCell(StepNum);
            Row.AddCell(Timestamp);
            Row.AddCell(Step);
            Row.AddCell(StepType);
            ConfiguredForSteps = true;
        }

    }

    public class RecipeStepsReportPrinter
    {
        public SimpleDocumentRenderer Printer;

        // Report elements as helper classes
        internal ReportTitle _title;
        internal ReportMetadata _metadata;
        internal RecordTableRow _headers;
        internal RecipeStepRecord _record;
        internal CallbackRowRenderer _recordRow;

        public RecipeStepsReportPrinter(string printer_name = "")
        {
            _Init(printer_name);
        }
        private void _Init(string printer_name)
        {
            _Init(new SimpleDocumentRenderer(printer_name));
        }
        public void Print()
        {
            Printer.Print();
        }
        private void _Init(SimpleDocumentRenderer p)
        {
            Printer = p;

            int[] widths = new int[] { 75, 175, 300, 150 };

            _title = new ReportTitle("Recipe Steps Report");
            _metadata = new ReportMetadata("Batch name: \"test\"");
            _headers = new RecordTableRow(ReportFonts.ColumnHeaderFont, "#", "Timestamp", "Step", "Type");

            _recordRow = new CallbackRowRenderer(UpdateRow);
            _record = new RecipeStepRecord(_recordRow);

            p.AddRow(_title);
            p.AddRow(_metadata);
            p.AddRow(_headers);
            p.AddRow(_recordRow);

            _metadata.Margin.Top = 25;
            _metadata.Margin.Bottom = 25;

            _headers.SetWidths(widths);
            _record.SetWidths(widths);
        }

        private bool UpdateRow(CallbackRowRenderer row)
        {
            return Update();
        }
        public virtual bool Update()
        {
            throw new NotImplementedException();
        }
    }

    public class FullSQLRecipeStepsReport : RecipeStepsReportPrinter
    {
        OleDbConnection connection;
        OleDbCommand cmd;
        OleDbDataReader reader;
        object[] rowdata;
        int Index;
        public FullSQLRecipeStepsReport(int batch_id)
        {
            connection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;User ID=PBSShell;
Password=Gd4cV;Data Source=C:\Database\PBSBioreactorDatabase.mdb;Mode=ReadWrite;
Persist Security Info=False;Jet OLEDB:System database=C:\Database\System.mdw");
            connection.Open();
            var sql = $"SELECT TIME_STAMP,EVENT_DETAIL,ERROR_MESSAGE,SOURCE from EVENTS WHERE ((EVENT_TYPE=\"Recipe Generated\" OR ERROR_MESSAGE=\"Recipe started\")) ORDER BY TIME_STAMP Asc";
            cmd = new OleDbCommand(sql, connection);
            reader = cmd.ExecuteReader();
            rowdata = new object[4];
            Index = 0;
        }

        public void Close()
        {
            reader.Close();
            cmd.Dispose();
            connection.Close();
            connection.Dispose();
            reader = null;
            cmd = null;
            connection = null;
        }


        public override bool Update()
        {
            bool have_data = reader.Read();
            if (!have_data)
                return false;

            Index += 1;
            reader.GetValues(rowdata);

            var time = (DateTime)rowdata[0];
            var detail = (string)rowdata[1];
            var err = (string)rowdata[2];
            var source = (string)rowdata[3];

            // New batch: hla ips perf r01
            // Run recipe: sonosep-LoopRecipe
            if (err == "Recipe started") // new recipe start event
            {
                var name = detail.Substring("Run recipe: ".Length);
                _record.SetNewRecipeData(name);
            }
            else
            {
                _record.SetRecipeStepData(Index.ToString(), time.ToString(), detail, source);
            }
            return true;
        }
    }


    public class DummyRecipeStepsReport : RecipeStepsReportPrinter
    {
        string[][] data;
        int _index = 0;
        public DummyRecipeStepsReport()
        {
            data = new string[][]
            {
                new string[] {"foo", "bar", "baz" },
                new string[] {"bar", "foo", "baz" },
                new string[] {"baz", "bar", "foo" },
                new string[] {"bar", "baz", "foo" },
            };
            _index = 0;
        }

        public override bool Update()
        {
            if (_index >= data.Length)
            {
                return false;
            }

            var rowdata = data[_index++];
            _record.SetRecipeStepData(_index.ToString(), rowdata[0], rowdata[1], rowdata[2]);
            return true;
        }
    }
}
