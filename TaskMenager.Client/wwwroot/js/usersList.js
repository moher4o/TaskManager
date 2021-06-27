var dataTable;
var permisionType;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);


$(document).ready(function () {
    $('#showDeleted').on('change', DeletedUsersShowOrHile);
    $('#showNotActive').on('change', NotActiveUsersShowOrHile);
    loadDataTable(false, false);
    permisionType = $('#uil').val();

});

function NotActiveUsersShowOrHile() {
    if ($("#showNotActive").prop('checked') == false) {
        $("#showDeleted").prop('checked', false);
        dataTable.destroy();
        loadDataTable(false, false);
    }
    else {
        $("#showDeleted").prop('checked', false);
        dataTable.destroy();
        loadDataTable(false, true);
    }
}

function DeletedUsersShowOrHile() {
    if ($("#showDeleted").prop('checked') == false) {
        $("#showNotActive").prop('checked', false);
        dataTable.destroy();
        loadDataTable(false, false);
    }
    else {
        $("#showNotActive").prop('checked', false);
        dataTable.destroy();
        loadDataTable(true, false);
    }
}

function loadDataTable(deleted, notActivated) {
    var url = path + "getAllUsers?deleted=" + deleted + "&notActivated=" + notActivated;
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "directorateName", "width": "23%" },
            { "data": "departmentName", "width": "25%" },
            { "data": "sectorName", "width": "18%" },
            { "data": "fullName", "width": "18%" },
            { "data": "telephoneNumber", "width": "3%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        return `<div>
                        <a style='cursor:pointer;'
                            onclick=InfoUser(${row.id}) title='Информация'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        <a href="${path}EditUser?userId=${row.id}" style='cursor:pointer; padding-left:5px;' ${row.status == "editable" ? "" : "hidden"} title='Редакция'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a href="..\\Report\\SetPersonalPeriodDate?userId=${row.id}" style='cursor:pointer;' ${row.status == "editable" ? "" : "hidden"} title='Отчет за период на ${row.fullName}'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                        </div>`;
                    }
                    else if (notActivated) {
                        return `<div>
                        <a style='cursor:pointer;'
                            onclick=InfoUser(${row.id}) title='Информация'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                        <a href="${path}EditUser?userId=${row.id}" style='cursor:pointer; padding-left:5px;' title='Редакция'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div>
                        <a style='cursor:pointer;'
                            onclick=InfoUser(${row.id}) title='Информация'>
                            <img class="chatnotifications" src="../png/info2.png" />
                        </a>
                         <a href="${path}EditUser?userId=${row.id}" style='cursor:pointer; padding-left:5px;' ${row.status == "editable" ? "" : "hidden"} title='Редакция'>
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a href='mailto:${row.email}' target="_top" style='cursor:pointer; padding-left:5px;' ${row.email == null ? "hidden" : ""} title='Изпращане на email'>
                            <img class="chatnotifications" src="../png/email.png" />
                        </a>
                        <a href="..\\Report\\SetPersonalPeriodDate?userId=${row.id}" style='cursor:pointer;' ${row.status == "editable" ? "" : "hidden"} title='Отчет за период на ${row.fullName}'>
                            <img class="chatnotifications" src="../png/report.png" />
                        </a>
                        </div>`;
                    }

                }, "width": "13%"
            }
        ],
        "iDisplayLength": 25,
        "language": {
            "emptyTable": "Няма потребители отговарящи на условието"
        },
        "width": "100%"
    });

}


