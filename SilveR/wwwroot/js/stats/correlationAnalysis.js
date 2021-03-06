﻿
$(function () {

    $("#Responses").kendoMultiSelect({
        dataSource: theModel.availableVariables,
        value: theModel.responses
    });

    $("#Transformation").kendoDropDownList({
        dataSource: theModel.transformationsList
    });

    $("#FirstCatFactor").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#SecondCatFactor").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#ThirdCatFactor").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#FourthCatFactor").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#Method").kendoDropDownList({
        dataSource: theModel.methodsList
    });

    $("#Hypothesis").kendoDropDownList({
        dataSource: theModel.hypothesesList
    });

    $("#Significance").kendoDropDownList({
        dataSource: theModel.significancesList
    });
});