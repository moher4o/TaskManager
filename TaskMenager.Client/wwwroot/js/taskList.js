var dataTable;
var permisionType;
var empdirectorateId;
var empdepartmentId;
var empsectorId;
var userFullName;
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
});

function LoadTasksTest() {
    $.getJSON('\getall', function (data) {
        console.log(data);
    });
}

//function DeletedTasksShowOrHideOld() {
//    if ($("#showClosed").prop('checked') == true && $("#showDeleted").prop('checked') == true) {
//        dataTable.destroy();
//        loadDataTable(true, true);
//    }
//    else if ($("#showClosed").prop('checked') == true && $("#showDeleted").prop('checked') == false){
//        dataTable.destroy();
//        loadDataTable(true, false);
//    }
//    else if ($("#showClosed").prop('checked') == false && $("#showDeleted").prop('checked') ==true) {
//        dataTable.destroy();
//        loadDataTable(false, true);
//    }
//    else if ($("#showClosed").prop('checked') == false && $("#showDeleted").prop('checked') == false) {
//        dataTable.destroy();
//        loadDataTable(false, false);
//    }


//}

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
            //{ "data": "taskName", "width": "41%" },
            { "data": "taskName", "width": "40%" },
            { "data": "taskAssigner", "width": "14%" },
            { "data": "directorateName", "width": "10%" },
            { "data": "departmentName", "width": "12%" },
            { "data": "sectorName", "width": "8%" },
            { "data": "parentTaskId", "width": "3%" },
            { "data": "directorateId", "width": "3%" },
            { "data": "departmentId", "width": "3%" },
            { "data": "sectorId", "width": "3%" },
            { "data": "assignedExpertsCount", "width": "3%" },
            //{ "data": "status", "width": "12%" },
            { "data": "status", "width": "7%" },
            //{ "data": "typeName", "width": "14%" },
            { "data": "typeName", "width": "7%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (withDeleted || permisionType != 'SuperAdmin') {
                        return `<div>
                        
                        <a href="${path}TaskDetails?taskId=${row.id}" style='cursor:pointer;' title='Информация' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${row.id}" style='cursor:pointer;' title='Прикачени файлове' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications3" src="../png/files.png" />
                        </a>
                        <a href="..\\Report\\TaskReportPeriod?taskId=${row.id}" style='cursor:pointer;' ${((row.typeId == 8) || (row.assignedExpertsCount == 0) || ((permisionType == "DirectorateAdmin") && (row.directorateId != empdirectorateId)) || ((permisionType == "DepartmentAdmin") && (row.departmentId != empdepartmentId)) || ((permisionType == "SectorAdmin") && (row.sectorId != empsectorId)) || (permisionType == "Employee")) && (row.taskAssigner != userFullName) ? "hidden" : ""} title='Отчет по задача'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=CustomSearch('${row.id}') title='Подзадачи' ${(row.parentTaskId > 0 || row.typeId == 8) ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/child.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div>
                        
                        <a href="${path}TaskDetails?taskId=${row.id}" style='cursor:pointer;' title='Информация' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}Delete?taskId=${row.id}') title='Изтриване' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${row.id}" style='cursor:pointer;' title='Прикачени файлове' ${row.typeId == 8 ? "hidden" : ""}>
                            <img class="chatnotifications3" src="../png/files.png" />
                        </a>
                        <a href="..\\Report\\TaskReportPeriod?taskId=${row.id}" style='cursor:pointer;' ${(row.assignedExpertsCount == 0 || row.typeId == 8 )? "hidden" : ""} title='Отчет по задача'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=CustomSearch('${row.id}') title='Подзадачи' ${(row.parentTaskId > 0 || row.typeId == 8) ? "hidden" : ""}>
                            <img class="chatnotifications" src="../png/child.png" />
                        </a>


                        </div>`;
                    }

                }, "width": "12%"
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
            "emptyTable": "Няма такива задачи"
        },
        "width": "100%"
     });
}

function Delete(url) {
    swal({
        title: "Потвърждение",
        text: "Сигурни ли сте, че искате да изтриете задачата?",
        icon: "warning",
        closeOnEsc: false,
        //showCancelButton: true,
        //buttons: true,
        //cancelButtonText: "Отказ",
        //confirmButtonColor: "#DD6B55",
        //confirmButtonText: "Изтриване",
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
        dataTable
            .columns(6)
            .search(parentId)
            .draw();
}

//"data": "id",
//    "render": function (data) {
//        if (withDeleted || permisionType != 'SuperAdmin') {
//            return `<div>
                        
//                        <a href="${path}TaskDetails?taskId=${data}" style='cursor:pointer;' title='Информация'>
//                            <img class="chatnotifications" src="../png/info2.png" />
//                        </a>
//                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${data}" style='cursor:pointer;' title='Прикачени файлове'>
//                            <img class="chatnotifications3" src="../png/files.png" />
//                        </a>

//                        </div>`;
//        }
//        else {
//            return `<div>
                        
//                        <a href="${path}TaskDetails?taskId=${data}" style='cursor:pointer;' title='Информация'>
//                            <img class="chatnotifications" src="../png/info2.png" />
//                        </a>
                        
//                        <a style='cursor:pointer; padding-left:5px;'
//                            onclick=Delete('${path}Delete?taskId=${data}') title='Изтриване'>
//                            <img class="chatnotifications" src="../png/delete2.png" />
//                        </a>
//                        <a href="..\\TasksFiles\\TaskFilesList?taskId=${data}" style='cursor:pointer;' title='Прикачени файлове'>
//                            <img class="chatnotifications3" src="../png/files.png" />
//                        </a>

//                        </div>`;
//        }

//    }, "width": "11%"
//            }
