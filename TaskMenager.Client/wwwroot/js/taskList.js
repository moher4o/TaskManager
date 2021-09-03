﻿var dataTable;
var permisionType;
var empdirectorateId;
var empdepartmentId;
var empsectorId;
var userFullName;
var userId;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1); 


$(document).ready(function () {
    $('#showClosed').on('click', ClosedTasksShowOrHide);
    $('#showDeleted').on('click', DeletedTasksShowOrHide);
    loadDataTable(false, false);
    permisionType = $('#uid').val();
    empdirectorateId = $('#dirid').val();
    empdepartmentId = $('#depid').val();
    empsectorId = $('#secid').val();
    userFullName = $('#userFN').text();
    userId = $('#userid').val();
});

function LoadTasksTest() {
    $.getJSON('\getall', function (data) {
        console.log(data);
    });
}

function ClosedTasksShowOrHide() {
    if ($("#showClosed").prop('checked') == true) {
        $("#showDeleted").prop('checked', false); 
        dataTable.destroy();
        loadDataTable(true, false);
        
    }
    else {
        $("#showDeleted").prop('checked', false); 
        dataTable.destroy();
        loadDataTable(false, false);
        
    }
}
function DeletedTasksShowOrHide() {
    if ($("#showDeleted").prop('checked') == true) {
        $("#showClosed").prop('checked', false); 
        dataTable.destroy();
        loadDataTable(false, true);
        
    }
    else {
        $("#showClosed").prop('checked', false);
        dataTable.destroy();
        loadDataTable(false, false);
         
    }
}


function loadDataTable(getClosed, withDeleted) {
    var url = path + "getall?withClosed=" + getClosed + "&withDeleted=" + withDeleted;
     dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "taskName", "width": "40%" },
            { "data": "taskAssigner", "width": "14%" },
            { "data": "directorateName", "width": "10%" },
            { "data": "departmentName", "width": "12%" },
            { "data": "sectorName", "width": "8%" },
            { "data": "parentTaskId", "width": "3%" },
            { "data": "directorateId", "width": "3%" },
            { "data": "departmentId", "width": "3%" },
            { "data": "sectorId", "width": "3%" },
            { "data": "assignedExpertsCount", "width": "2%" },
            { "data": "status", "width": "7%" },
            { "data": "typeName", "width": "5%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (withDeleted || permisionType != 'SuperAdmin') {
                        return `<div  class="row">
                        
                        <a href="${path}TaskDetails?taskId=${row.id}" style='cursor:pointer; padding-left:15px; min-width:20%;' title='Информация' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        <a href="..\\Report\\TaskReportPeriod?taskId=${row.id}" style='cursor:pointer; padding-left:5px; min-width:20%;' ${((permisionType == "DirectorateAdmin" && (row.directorateId == empdirectorateId || row.assignerId == userId)) || (permisionType == "DepartmentAdmin" && (row.departmentId == empdepartmentId || row.assignerId == userId)) || (permisionType == "SectorAdmin" && (row.sectorId == empsectorId || row.assignerId == userId)) || (permisionType == "Employee" && row.assignerId == userId) || permisionType == 'SuperAdmin') ? "" : "hidden"} title='Отчет по задача'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                        <a style='cursor:pointer; min-width:15%;'
                            onclick=TotalDelete('${path}TotalDelete?taskId=${row.id}') title='Тотално Изтриване' ${(userFullName == "Ангел Иванов Вуков" && permisionType == 'SuperAdmin') ? "" : "hidden"}>
                            <img class="chatnotifications" src="../png/delete_total.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px; min-width:22%;'
                            onclick=CustomSearch('${row.id}') title='Подзадачи' ${(row.parentTaskId > 0 || row.typeId != 7) ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/child.png" />
                            <span class="notificationsTodayCountValue" ${row.childrenCount > 0 ? "" : "hidden"}>${row.childrenCount}</span>
                        </a>

                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${row.id}" style='cursor:pointer; padding-left:5px; min-width:22%;' title='Прикачени файлове' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications2" src="../png/files3.png" />
                            <span class="notificationsTodayCountValue" ${row.filesCount > 0 ? "" : "hidden"}>${row.filesCount}</span>
                        </a>

                        </div>`;
                    }
                    else {
                        return `<div class="row">
                        
                        <a href="${path}TaskDetails?taskId=${row.id}" style='cursor:pointer; padding-left:12px; min-width:15%;' title='Информация' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        
                        <a href="..\\Report\\TaskReportPeriod?taskId=${row.id}" style='cursor:pointer; padding-left:5px; min-width:15%;' ${(row.typeId == 8 )? "hidden" : ""} title='Отчет по задача'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                       
                        <a style='cursor:pointer; min-width:15%;'
                            onclick=Delete('${path}Delete?taskId=${row.id}') title='Изтриване' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        <a style='cursor:pointer; min-width:15%;'
                            onclick=TotalDelete('${path}TotalDelete?taskId=${row.id}') title='Тотално Изтриване' ${(userFullName == "Ангел Иванов Вуков" && permisionType == 'SuperAdmin') ? "" : "hidden"}>
                            <img class="chatnotifications" src="../png/delete_total.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:3px; min-width:20%;'
                            onclick=CustomSearch('${row.id}') title='Подзадачи' ${(row.parentTaskId > 0 || row.typeId != 7) ? "hidden" : ""}>
                            <img class="chatnotifications2" src="../png/child.png" />
                            <span class="notificationsTodayCountValue" ${row.childrenCount > 0 ? "" : "hidden"}>${row.childrenCount}</span>
                        </a>
                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${row.id}" style='cursor:pointer; padding-left:3px; min-width:20%;' title='Прикачени файлове' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications2" src="../png/files3.png" />
                            <span class="notificationsTodayCountValue" ${row.filesCount > 0 ? "" : "hidden"}>${row.filesCount}</span>
                        </a>
                        
                        </div>`;
                    }

                }, "width": "15%"
            }
         ],
         "columnDefs": [
             {
                 "targets": [3],
                 "visible": false
                 //"searchable": false
             },
             {
                 "targets": [4],
                 "visible": false
             },
             {
                 "targets": [5],
                 "visible": false
             },
             {
                 "targets": [6],
                 "visible": false
             },
             {
                 "targets": [7],
                 "visible": false
             },
             {
                 "targets": [8],
                 "visible": false
             },
             {
                 "targets": [9],
                 "visible": false
             },
         ],
         "order": [[10, 'asc']],
         "iDisplayLength": 25,
        "language": {
            "emptyTable": "Няма такива задачи!",
            "zeroRecords": "Няма такива задачи или недостатъчни права!"
        },
        "width": "100%"
     });
}

function TotalDelete(url) {
    swal({
        title: "Внимание",
        text: "Сигурни ли сте, че искате да изтриете задачата тотално???",
        icon: "warning",
        closeOnEsc: false,
        //showCancelButton: true,
        //buttons: true,
        //cancelButtonText: "Отказ",
        //confirmButtonColor: "#DD6B55",
        //confirmButtonText: "Изтриване",
        buttons: ["Отказ", "Тотално Изтриване!"],
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "Get",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

function Delete(url) {
    swal({
        title: "Потвърждение",
        text: "Сигурни ли сте, че искате да изтриете задачата?",
        icon: "warning",
        closeOnEsc: false,
        buttons: ["Отказ", "Изтриване!"],
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "Get",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

function CustomSearch(parentId) {
    //$('input[type=search]').val('').change();
    dataTable.destroy();
    loadDataTable(false, false);
    dataTable
        .columns(6)
        .search(parentId)
        .draw();
}

