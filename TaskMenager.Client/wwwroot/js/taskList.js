var dataTable;

$(document).ready(function () {
    //LoadTasksTest()
    loadDataTable();
});

function LoadTasksTest() {
    $.getJSON('\getall', function (data) {
        console.log(data);
    });
}

function DeletedTasksShowOrHide() {
    if ($("#showDeleted").prop('checked') == true) {
        $(".displayno").show();
    }
    else {
        $(".displayno").hide();
    }
}
function loadDataTable() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/Tasks/getall/",
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
                    return `<div class="text-center">
                        
                        <a href="/Tasks/TaskDetails?taskId=${data}" style='cursor:pointer;'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        &nbsp;
                        <a style='cursor:pointer;'
                            onclick=Delete('/Tasks/Delete?id='+${data})>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        </div>`;
                }, "width": "27%"
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
                type: "DELETE",
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