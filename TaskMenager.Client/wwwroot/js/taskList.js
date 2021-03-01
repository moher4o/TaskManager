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
            { "data": "taskName", "width": "50%" },
            { "data": "assignedExpertsCount", "width": "5%" },
            { "data": "status", "width": "12%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                        <a href="/Tasks/TaskDetails?taskId=${data}" class='btn btn-success text-white' style='cursor:pointer; width:90px;'>
                            Детайли
                        </a>
                        &nbsp;
                        <a class='btn btn-danger text-white' style='cursor:pointer; width:90px;'
                            onclick=Delete('/Tasks/Delete?id='+${data})>
                            Изтриване
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
        //showCancelButton: true,
        //buttons: true,
        //cancelButtonText: "Отказ",
        confirmButtonColor: "#DD6B55",
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