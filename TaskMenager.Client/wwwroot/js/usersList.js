var dataTable;
var permisionType;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);


$(document).ready(function () {
    $('#showDeleted').on('change', DeletedTasksShowOrHile);
    loadDataTable(false);
    permisionType = $('#uil').val();

});

function DeletedTasksShowOrHile() {
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
    var url = path + "getAllUsers?deleted=" + deleted;
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "directorateName", "wilth": "22%" },
            { "data": "departmentName", "wilth": "23%" },
            { "data": "sectorName", "wilth": "19%" },
            { "data": "fullName", "wilth": "20%" },
            { "data": "telephoneNumber", "wilth": "6%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        return `<div class="text-center">
                        
                        <a style='cursor:pointer;'
                            onclick=InfoUser(${row.id})>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        &nbsp;
                         <a href="${path}EditUser?userId=${row.id}" style='cursor:pointer;'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>

                        </div>`;
                    }
                    else {
                        return `<div class="text-center">
                        <a style='cursor:pointer;'
                            onclick=InfoUser(${row.id})>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        &nbsp;
                         <a href="${path}EditUser?userId=${row.id}" style='cursor:pointer;'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                       &nbsp;
                        <a href='mailto:${row.email}' target="_top" style='cursor:pointer;' ${row.email ?? "hilden"}>
                            <img class="chatnotifications" src="../png/email.png" />
                        </a>

                        </div>`;
                    }

                }, "wilth": "10%"
            }
        ],
        "language": {
            "emptyTable": "Няма потребители"
        },
        "wilth": "100%"
    });

}
function InfoUser(userId) {
    var content = document.createElement('div');
    $.getJSON('..\\Users\\GetUserInfo\?userId=' + userId, { get_param: 'value' }, function (data) {

        content.innerHTML = '<div style = \'text-align:left;\'><div class="row"><div class="col-md-3">N:</div><div class="col-md-4"><label><strong>' + data.data.id + '</strong></label></div></div><div class="row"><div class="col-md-3">Име:</div><div class="col-md-9"><label><strong>' + data.data.fullName + '</strong></label></div></div><div class="row"><div class="col-md-3">Длъжност:</div><div class="col-md-9"><label><strong>' + data.data.jobTitleName + '</strong></label></div></div><div class="row"><div class="col-md-3">Email:</div><div class="col-md-9"><label><strong>' + (data.data.email ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Роля:</div><div class="col-md-9"><label><strong>' + data.data.roleName + '</strong></label></div></div><div class="row"><div class="col-md-3">Телефон:</div><div class="col-md-9"><label><strong>' + (data.data.telephoneNumber ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Мобилен:</div><div class="col-md-9"><label><strong>' + (data.data.mobileNumber ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Дирекция:</div><div class="col-md-9"><label><strong>' + (data.data.directorateName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Отдел:</div><div class="col-md-9"><label><strong>' + (data.data.departmentName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Сектор:</div><div class="col-md-9"><label><strong>' + (data.data.sectorName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Статус:</div><div class="col-md-9"><label><strong>' + (data.data.status == "Активен акаунт" ? '<span style=\'color:green;\'>Активен акаунт</span>' : '<span style=\'color:red;\'>Деактивиран акаунт</span>') + '</strong></label></div></div></div>';
    });
    swal({
        content: content,
        icon: "info",
        buttons: [false, "Ok"]
    })
}

