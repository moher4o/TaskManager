﻿var dataTable;
var permisionType;

$(document).ready(function () {
    $('#showClosed').on('change', DeletedTasksShowOrHide);
    $('#showDeleted').on('change', DeletedTasksShowOrHide);
    loadDataTable(false, false);
    permisionType = $('#uid').val();
 
});

function LoadTasksTest() {
    $.getJSON('\getall', function (data) {
        console.log(data);
    });
}

function DeletedTasksShowOrHide() {
    if ($("#showClosed").prop('checked') == true && $("#showDeleted").prop('checked') == true) {
        dataTable.destroy();
        loadDataTable(true, true);
    }
    else if ($("#showClosed").prop('checked') == true && $("#showDeleted").prop('checked') == false){
        dataTable.destroy();
        loadDataTable(true, false);
    }
    else if ($("#showClosed").prop('checked') == false && $("#showDeleted").prop('checked') ==true) {
        dataTable.destroy();
        loadDataTable(false, true);
    }
    else if ($("#showClosed").prop('checked') == false && $("#showDeleted").prop('checked') == false) {
        dataTable.destroy();
        loadDataTable(false, false);
    }


}
function loadDataTable(getClosed,withDeleted) {
    var url = "/Tasks/getall?withClosed=" + getClosed +"&withDeleted="+withDeleted;
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "6%" },
            { "data": "taskName", "width": "60%" },
            { "data": "assignedExpertsCount", "width": "5%" },
            { "data": "status", "width": "12%" },
            {
                "data": "id",
                "render": function (data) {
                    if (withDeleted || permisionType != 'SuperAdmin') {
                        return `<div class="text-center">
                        
                        <a href="/Tasks/TaskDetails?taskId=${data}" style='cursor:pointer;'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div class="text-center">
                        
                        <a href="/Tasks/TaskDetails?taskId=${data}" style='cursor:pointer;'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        &nbsp;
                        <a style='cursor:pointer;'
                            onclick=Delete('/Tasks/Delete?taskId=${data}')>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        </div>`;
                    }

                }, "width": "17%"
            }
        ],
        "language": {
            "emptyTable": "Няма задачи"
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

//function TaskInfo(url) {
//    var swal_html = '<div class="panel" style="background:aliceblue;font-weight:bold"><div class="panel-heading panel-info text-center btn-info"> <b>Import Status</b> </div> <div class="panel-body"><div class="text-center"><b><p style="font-weight:bold">Total number of not inserted  rows : add data</p><p style="font-weight:bold">Row numbers:Add data</p></b></div></div></div>';
//    swal({ title: "Good Job!", content: (swal_html) });
//}

//function loadDataTable() {
//    dataTable = $('#DT_load').DataTable({
//        "ajax": {
//            "url": "/Tasks/getall/",
//            "type": "GET",
//            "datatype": "json"
//        },
//        "columns": [
//            { "data": "id", "width": "6%" },
//            { "data": "taskName", "width": "60%" },
//            { "data": "assignedExpertsCount", "width": "5%" },
//            { "data": "status", "width": "12%" },
//            {
//                "data": "id",
//                "render": function (data) {
//                    return `<div class="text-center">
                        
//                        <a href="/Tasks/TaskDetails?taskId=${data}" style='cursor:pointer;'>
//                            <img class="chatnotifications" src="../png/info2.png" />
//                        </a>
//                        &nbsp;
//                        <a style='cursor:pointer;'
//                            onclick=Delete('/Tasks/Delete?id='+${data})>
//                            <img class="chatnotifications" src="../png/delete2.png" />
//                        </a>
//                        </div>`;
//                }, "width": "27%"
//            }
//        ],
//        "language": {
//            "emptyTable": "Няма задачи"
//        },
//        "width": "100%"
//    });
//}