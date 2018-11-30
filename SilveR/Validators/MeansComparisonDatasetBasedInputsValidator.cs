﻿using SilveR.StatsModels;
using System;
using System.Collections.Generic;

namespace SilveR.Validators
{
    public class MeansComparisonDatasetBasedInputsValidator : ValidatorBase
    {
        private readonly MeansComparisonDatasetBasedInputsModel mcVariables;

        public MeansComparisonDatasetBasedInputsValidator(MeansComparisonDatasetBasedInputsModel mc)
            : base(mc.DataTable)
        {
            mcVariables = mc;
        }

        public override ValidationInfo Validate()
        {
            //go through all the column names, if any are numeric then stop the analysis
            List<string> allVars = new List<string>();
            allVars.Add(mcVariables.Treatment);
            allVars.Add(mcVariables.Response);

            if (!CheckColumnNames(allVars))
                return ValidationInfo;

            if (!CheckFactorsHaveLevels(mcVariables.Treatment))
                return ValidationInfo;

            //Check that the response does not contain non-numeric 
            if (!CheckIsNumeric(mcVariables.Response))
            {
                ValidationInfo.AddErrorMessage("The response variable selected (" + mcVariables.Response + ") contains non-numerical data that cannot be processed. Please check raw data and make sure the data was entered correctly.");
                return ValidationInfo;
            }

            if (!String.IsNullOrEmpty(mcVariables.Treatment))
            {
                if (!CheckResponsesPerLevel(mcVariables.Treatment, mcVariables.Response, ReflectionExtensions.GetPropertyDisplayName<MeansComparisonDatasetBasedInputsModel>(i => i.Treatment)))
                    return ValidationInfo;
            }

            if (!String.IsNullOrEmpty(mcVariables.Treatment) && !String.IsNullOrEmpty(mcVariables.Response)) //if a treat and response is selected...
            {
                //Check that the number of responses for each level is at least 2
                Dictionary<string, int> levelResponses = ResponsesPerLevel(mcVariables.Treatment, mcVariables.Response);
                foreach (KeyValuePair<string, int> level in levelResponses)
                {
                    if (level.Value < 2)
                    {
                        //ValidationInfo.AddErrorMessage("There is no replication in one or more of the levels of the treatment factor. Please select another factor.");
                        ValidationInfo.AddErrorMessage("There is no replication in one or more of the levels of the treatment factor (" + mcVariables.Treatment + ").  Please amend the dataset prior to running the analysis.");
                        return ValidationInfo;
                    }
                }

                //check response and doses contain values
                if (!CheckFactorAndResponseNotBlank(mcVariables.Treatment, mcVariables.Response, "treatment factor")) return ValidationInfo;
            }
            else if (String.IsNullOrEmpty(mcVariables.Treatment) && !String.IsNullOrEmpty(mcVariables.Response))
            //if only a response selected (doing absolute change) then check that more than 1 value is in the dataset!
            {
                if (CountResponses(mcVariables.Response) == 1)
                {
                    ValidationInfo.AddErrorMessage("The response selected (" + mcVariables.Response + ") contains only 1 value. Please select another factor.");
                    return ValidationInfo;
                }
            }

            //if get here then no errors so return true
            return ValidationInfo;
        }
    }
}