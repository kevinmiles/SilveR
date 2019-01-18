using SilveR.Helpers;
using SilveR.Models;
using SilveR.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text;

namespace SilveR.StatsModels
{
    public class OneWayANOVADatasetBasedInputsModel : AnalysisDataModelBase, IGraphSizeOptions
    {
        [CheckUsedOnceOnly]
        [Required]
        [DisplayName("Response")]
        public string Response { get; set; }

        [CheckUsedOnceOnly]
        public string Treatment { get; set; }

        [DisplayName("Significance level")]
        public string Significance { get; set; } = "0.05";

        public IEnumerable<string> SignificancesList
        {
            get { return new List<string>() { "0.1", "0.05", "0.01", "0.001" }; }
        }

        public PlottingRangeTypeOption PlottingRangeType { get; set; } = PlottingRangeTypeOption.SampleSize;

        [DisplayName("Sample size from")]
        public Nullable<int> SampleSizeFrom { get; set; } = 6;

        [DisplayName("Sample size to")]
        public Nullable<int> SampleSizeTo { get; set; } = 15;

        [DisplayName("Power from")]
        public Nullable<int> PowerFrom { get; set; } = 70;

        [DisplayName("Power to")]
        public Nullable<int> PowerTo { get; set; } = 90;

        [DisplayName("Graph title")]
        public string GraphTitle { get; set; }


        public OneWayANOVADatasetBasedInputsModel() : base("OneWayANOVADatasetBasedInputs") { }

        public OneWayANOVADatasetBasedInputsModel(IDataset dataset)
            : base(dataset, "OneWayANOVADatasetBasedInputs") { }

        public override ValidationInfo Validate()
        {
            OneWayANOVADatasetBasedInputsValidator meansComparisonValidator = new OneWayANOVADatasetBasedInputsValidator(this);
            return meansComparisonValidator.Validate();
        }

        public override string[] ExportData()
        {
            DataTable dtNew = DataTable.CopyForExport();

            //Get the response, treatment and covariate columns by removing all other columns from the new datatable
            foreach (string col in this.DataTable.GetVariableNames())
            {
                if (Response != col && Treatment != col)
                {
                    dtNew.Columns.Remove(col);
                }
            }

            //if the response is blank then remove that row
            dtNew.RemoveBlankRow(Response);


            string[] csvArray = dtNew.GetCSVArray();

            //fix any columns with illegal chars here (at the end)
            ArgumentFormatter argFormatter = new ArgumentFormatter();
            csvArray[0] = argFormatter.ConvertIllegalCharacters(csvArray[0]);

            return csvArray;
        }

        public override IEnumerable<Argument> GetArguments()
        {
            List<Argument> args = new List<Argument>();

            args.Add(ArgumentHelper.ArgumentFactory(nameof(Response), Response));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(Treatment), Treatment));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(Significance), Significance));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(PlottingRangeType), PlottingRangeType.ToString()));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(SampleSizeFrom), SampleSizeFrom));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(SampleSizeTo), SampleSizeTo));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(PowerFrom), PowerFrom));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(PowerTo), PowerTo));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(GraphTitle), GraphTitle));

            return args;
        }

        public override void LoadArguments(IEnumerable<Argument> arguments)
        {
            ArgumentHelper argHelper = new ArgumentHelper(arguments);

            this.Response = argHelper.LoadStringArgument(nameof(Response));
            this.Treatment = argHelper.LoadStringArgument(nameof(Treatment));
            this.Significance = argHelper.LoadStringArgument(nameof(Significance));
            this.PlottingRangeType = (PlottingRangeTypeOption)Enum.Parse(typeof(PlottingRangeTypeOption), argHelper.LoadStringArgument(nameof(PlottingRangeType)), true);
            this.SampleSizeFrom = argHelper.LoadNullableIntArgument(nameof(SampleSizeFrom));
            this.SampleSizeTo = argHelper.LoadNullableIntArgument(nameof(SampleSizeTo));
            this.PowerFrom = argHelper.LoadNullableIntArgument(nameof(PowerFrom));
            this.PowerTo = argHelper.LoadNullableIntArgument(nameof(PowerTo));
            this.GraphTitle = argHelper.LoadStringArgument(nameof(GraphTitle));
        }

        public override string GetCommandLineArguments()
        {
            ArgumentFormatter argFormatter = new ArgumentFormatter();
            StringBuilder arguments = new StringBuilder();

            arguments.Append(" " + "DatasetValues"); //4

            arguments.Append(" " + argFormatter.GetFormattedArgument(Response, true)); //5
            arguments.Append(" " + argFormatter.GetFormattedArgument(Treatment, true)); //6
            arguments.Append(" " + argFormatter.GetFormattedArgument(Significance, false)); //7

            if (PlottingRangeType == PlottingRangeTypeOption.SampleSize)
            {
                arguments.Append(" " + "SampleSize"); //8
                arguments.Append(" " + argFormatter.GetFormattedArgument(SampleSizeFrom)); //9
                arguments.Append(" " + argFormatter.GetFormattedArgument(SampleSizeTo)); //10
            }
            else
            {
                arguments.Append(" " + "PowerAxis"); //8
                arguments.Append(" " + argFormatter.GetFormattedArgument(PowerFrom)); //9
                arguments.Append(" " + argFormatter.GetFormattedArgument(PowerTo)); //10
            }

            arguments.Append(" " + argFormatter.GetFormattedArgument(GraphTitle, false)); //11

            return arguments.ToString().Trim();
        }
    }
}