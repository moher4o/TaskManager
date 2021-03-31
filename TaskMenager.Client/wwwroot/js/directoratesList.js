var dataTable;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);


$(document).ready(function () {
    $('#showDeleted').on('change', DeletedUsersShowOrHile);
    loadDataTable(false);
    //permisionType = $('#uil').val();

});

function DeletedUsersShowOrHile() {
    if ($("#showDeleted").prop('checked') == false) {
        dataTable.destroy();
        loadDataTable(false);
    }
    else {
        dataTable.destroy();
        loadDataTable(true);
    }
}

function loadDataTable(deleted) {
    var url = path + "getAllDirectorates?deleted=" + deleted;
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "90%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        return `<div>
                        <a href="${path}EditDirectorate?dirId=${row.id}" style='cursor:pointer; padding-left:10px;'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div>
                        <a href="${path}EditDirectorate?dirId=${row.id}" style='cursor:pointer; padding-left:10px;'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteDirectorate?dirId=${row.id}')>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>

                        </div>`;
                    }

                }, "width": "10%"
            }
        ],
        "language": {
            "emptyTable": "Няма създадени дирекции"
        },
        "width": "100%"
    });

}
function Delete(url) {
    swal({
        title: "Потвърждение",
        text: "Сигурни ли сте, че искате да изтриете дирекцията?",
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
                        loadDataTable(false);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
