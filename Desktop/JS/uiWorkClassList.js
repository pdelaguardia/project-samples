/// <reference path="_references.js" />


function updateClassList() {
    var data = $("#schoolClassId").val();
    
    if (data != "0") {
        $.ajax({
            url: '/ClassView/StudentsInClass',
            type: 'get',
            data: { id: data },
            beforeSend: function() {
                LoadingAction();
            },
            success: function(data, status) {
                $("#updateArea").html(data);
            },
            error: function(xhr, desc, err) {
                $("#updateArea").html(err);

            }
        });
    } else {
        $("#updateArea").html("");
    }
}

function updateMonthList() {
    var data = $("#termName").val();
    SmallLoadingAction();

    if (data != "-1") {

        $.getJSON('/ClassView/MonthsInTerm?termId=' + data, function (allData) {
            //clear the month list
            var end = document.getElementById("monthName");
            while (end.options.length > 0) {
                end.remove(0);
            }

            end.add(new Option("Select Month", "-1"), null);
            var option = null;

            for (var i = 0; i < allData.length; i++) {
                option = new Option(allData[i].month, allData[i].id);
                option.setAttribute("data-val-sessions", allData[i].sessions);
                end.add(option, null);
            }
            SmallLoadingActionHide();
        });

    } else {
        var end = document.getElementById("monthName");
        while (end.options.length > 0) {
            end.remove(0);
        }

        end.add(new Option("Select term first", "-1"), null);
        SmallLoadingActionHide();
    }
}

function updateClassListAttendance() {
    var groupId = $("#className").val();
    var monthId = $("#monthName").val();
    var monthMaxSessions = $("#monthName").attr('data-val-sessions');


    if (groupId != "-1" && monthId != "-1") {
        $.ajax({
            url: '/ClassView/ClassAttendance',
            type: 'get',
            data: { groupId: groupId, monthId:monthId },
            beforeSend: function () {
                LoadingAction();
            },
            success: function (data, status) {
                $("#updateArea").html(data);
            },
            error: function (xhr, desc, err) {
                $("#updateArea").html(err);

            }
        });
    } else {
        $("#updateArea").html("");
    }
}

function updateClassListSubjects() {
    var data = $("#schoolClassId").val();
    if ($("#schoolClassId").val() == "0") $("#schoolClassId").addClass("empty");
    else $("#schoolClassId").removeClass("empty").children('.placeholder').remove();

    if (data != "0") {
        $.ajax({
            url: '/ClassView/ClassListSubjectsView',
            type: 'get',
            data: { id: data },
            beforeSend: function () {
                LoadingAction();
            },
            success: function (data, status) {
                $("#updateArea").html(data);

            },
            error: function (xhr, desc, err) {
                $("#updateArea").html(err);

            }
        });
    } else {
        $("#updateArea").html("");
    }
}


function ToggleAllCheckboxes(element) {
    var checkedStatus = $(element).prop('checked');
    var id = $(element).val();

    $("#studentSubjectDataTable tbody tr td input:checkbox").each(function () {
        var checkboxvalue = $(this).val().split("|", 2);
        if (checkboxvalue[1] == id) {
            this.checked = checkedStatus;
        }

    });
}

function CheckToggle(element)
{
    var checkedStatus = $(element).prop('checked');
    var id = $(element).val().split("|", 2);
    var allChecked = true;

    $("#studentSubjectDataTable tbody tr td input:checkbox").each(function () {
        var checkboxvalue = $(this).val().split("|", 2);
        if (checkboxvalue[1] == id[1]) {
            if (this.checked == false)
            {
                allChecked = false;
            }
        }
    });



    if (allChecked == false) {
        $("#studentSubjectDataTable tbody tr td input:checkbox[value='" + id[1] + "']").attr('checked', false);
    }
    else
    {
        $("#studentSubjectDataTable tbody tr td input:checkbox[value='" + id[1] + "']").prop('checked', true);
    }

}



