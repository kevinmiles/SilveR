
$(function () {
    $("#Response").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#ResponseTransformation").kendoDropDownList({
        dataSource: theModel.transformationsList
    });

    $("#Treatment").kendoDropDownList({
        dataSource: theModel.availableVariablesAllowNull
    });

    $("#Significance").kendoDropDownList({
        dataSource: theModel.significancesList
    });
});