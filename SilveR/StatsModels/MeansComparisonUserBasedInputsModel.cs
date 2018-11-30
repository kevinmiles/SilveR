using SilveR.Helpers;
using SilveR.Models;
using SilveR.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SilveR.StatsModels
{
    public enum ChangeTypeOption { Percent = 0, Absolute = 1 }
    public enum PlottingRangeTypeOption { SampleSize = 0, Power = 1 }
    public enum DeviationType { StandardDeviation = 0, Variance =1 }

    public class MeansComparisonUserBasedInputsModel : AnalysisModelBase, IMeansComparisonOutputOptions
    {
        //[ValidateGroupMean]
        [Range(0, double.MaxValue)]
        [DisplayName("Group mean")]
        public Nullable<decimal> GroupMean { get; set; }

        public DeviationType DeviationType { get; set; } = DeviationType.StandardDeviation;

        //[ValidateStandardDeviation]
        [Range(0, double.MaxValue)]
        [DisplayName("Standard deviation")]
        public Nullable<decimal> StandardDeviation { get; set; }

        //[ValidateVariance]
        [Range(0, double.MaxValue)]
        [DisplayName("Variance")]
        public Nullable<decimal> Variance { get; set; }

        [DisplayName("Significance level")]
        public string Significance { get; set; } = "0.05";

        public IEnumerable<string> SignificancesList
        {
            get { return new List<string>() { "0.1", "0.05", "0.01", "0.001" }; }
        }

        public ChangeTypeOption ChangeType { get; set; } = ChangeTypeOption.Percent;

        [ValidatePercentChanges]
        [DisplayName("Percent changes")]
        public string PercentChange { get; set; }

        [ValidateAbsoluteChanges]
        [DisplayName("Absolute changes")]
        public string AbsoluteChange { get; set; }
        
        public PlottingRangeTypeOption PlottingRangeType { get; set; } = PlottingRangeTypeOption.SampleSize;

        [ValidateSampleSizeFrom]
        [DisplayName("Sample size from")]
        public string SampleSizeFrom { get; set; } = "6";

        [ValidateSampleSizeTo]
        [DisplayName("Sample size to")]
        public string SampleSizeTo { get; set; } = "15";

        [ValidateCustomFrom]
        [DisplayName("Power from")]
        public string PowerFrom { get; set; } = "70";

        [ValidateCustomTo]
        [DisplayName("Power to")]
        public string PowerTo { get; set; } = "90";

        [DisplayName("Graph title")]
        public string GraphTitle { get; set; }


        public MeansComparisonUserBasedInputsModel() : base("MeansComparisonUserBasedInputs") { }

        public override ValidationInfo Validate()
        {
            MeansComparisonUserBasedInputsValidator meansComparisonValidator = new MeansComparisonUserBasedInputsValidator(this);
            return meansComparisonValidator.Validate();
        }

        public override IEnumerable<Argument> GetArguments()
        {
            List<Argument> args = new List<Argument>();

            args.Add(ArgumentHelper.ArgumentFactory(nameof(GroupMean), GroupMean));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(DeviationType), DeviationType.ToString()));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(StandardDeviation), StandardDeviation));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(Variance), Variance));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(Significance), Significance));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(ChangeType), ChangeType.ToString()));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(PercentChange), PercentChange));
            args.Add(ArgumentHelper.ArgumentFactory(nameof(AbsoluteChange), AbsoluteChange));
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

            this.GroupMean = argHelper.LoadNullableDecimalArgument(nameof(GroupMean));
            this.DeviationType = (DeviationType)Enum.Parse(typeof(DeviationType), argHelper.LoadStringArgument(nameof(DeviationType)), true);
            this.StandardDeviation = argHelper.LoadNullableDecimalArgument(nameof(StandardDeviation));
            this.Variance = argHelper.LoadNullableDecimalArgument(nameof(Variance));
            this.Significance = argHelper.LoadStringArgument(nameof(Significance));
            this.ChangeType = (ChangeTypeOption)Enum.Parse(typeof(ChangeTypeOption), argHelper.LoadStringArgument(nameof(ChangeType)), true);
            this.PercentChange = argHelper.LoadStringArgument(nameof(PercentChange));
            this.AbsoluteChange = argHelper.LoadStringArgument(nameof(AbsoluteChange));
            this.PlottingRangeType = (PlottingRangeTypeOption)Enum.Parse(typeof(PlottingRangeTypeOption), argHelper.LoadStringArgument(nameof(PlottingRangeType)), true);
            this.SampleSizeFrom = argHelper.LoadStringArgument(nameof(SampleSizeFrom));
            this.SampleSizeTo = argHelper.LoadStringArgument(nameof(SampleSizeTo));
            this.PowerFrom = argHelper.LoadStringArgument(nameof(PowerFrom));
            this.PowerTo = argHelper.LoadStringArgument(nameof(PowerTo));
            this.GraphTitle = argHelper.LoadStringArgument(nameof(GraphTitle));
        }

        public override string GetCommandLineArguments()
        {
            ArgumentFormatter argFormatter = new ArgumentFormatter();
            StringBuilder arguments = new StringBuilder();

            arguments.Append(" " + "UserValues"); //4

            arguments.Append(" " + argFormatter.GetFormattedArgument(GroupMean.ToString(), false)); //5

            if (DeviationType == DeviationType.StandardDeviation)
            {
                arguments.Append(" " + "StandardDeviation"); //6
                arguments.Append(" " + argFormatter.GetFormattedArgument(StandardDeviation.ToString(), false)); //7
            }
            else
            {
                arguments.Append(" " + "Variance"); //6
                arguments.Append(" " + argFormatter.GetFormattedArgument(Variance.ToString(), false)); //7
            }

            arguments.Append(" " + argFormatter.GetFormattedArgument(Significance, false)); //8

            if (ChangeType == ChangeTypeOption.Percent)
            {
                arguments.Append(" " + "Percent"); //9
                arguments.Append(" " + argFormatter.GetFormattedArgument(PercentChange, false)); //10
            }
            else
            {
                arguments.Append(" " + "Absolute"); //9
                arguments.Append(" " + argFormatter.GetFormattedArgument(AbsoluteChange, false)); //10
            }

            if (PlottingRangeType == PlottingRangeTypeOption.SampleSize)
            {
                arguments.Append(" " + "SampleSize"); //11
                arguments.Append(" " + argFormatter.GetFormattedArgument(SampleSizeFrom, false)); //12
                arguments.Append(" " + argFormatter.GetFormattedArgument(SampleSizeTo, false)); //13
            }
            else
            {
                arguments.Append(" " + "PowerAxis"); //11
                arguments.Append(" " + argFormatter.GetFormattedArgument(PowerFrom, false)); //12
                arguments.Append(" " + argFormatter.GetFormattedArgument(PowerTo, false)); //13
            }

            arguments.Append(" " + argFormatter.GetFormattedArgument(GraphTitle, false)); //14

            return arguments.ToString().Trim();
        }
    }
}