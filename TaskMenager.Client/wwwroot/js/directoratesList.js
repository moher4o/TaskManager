﻿var dataTable;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);
let placeholderElement = $('#modal-placeholder');
let newdirplaseholder = $('#newdir-placeholder');

$(document).ready(function () {
    $('#showDeleted').on('change', DeletedUsersShowOrHile);
    loadDataTable(false);
    ModalAction();
    NewDirModalAction();
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
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "85%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        return `<div>
                        <a style='cursor:pointer; padding-left:10px;' onclick=Restore('${path}RestoreDirectorate?dirId=${row.id}') title='Възстановяване'>
                            <img class="chatnotifications" src="../png/restore-icon.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' data-url='${path}RenameDirectorate' data-dirid='${row.id}' data-dirname='${row.name}' onclick=DirModalShow('${path}RenameDirectorate?dirId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteDirectorate?dirId=${row.id}') title='Изтриване'>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>

                        </div>`;
                    }

                }, "width": "10%"
            }
        ],
        "language": {
            "emptyTable": "Няма дирекции отговарящи на условието"
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
                        dataTable.destroy();
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

function Restore(url) {
    swal({
        title: "Потвърждение",
        text: "Сигурни ли сте, че искате да възстановите дирекцията?",
        icon: "warning",
        closeOnEsc: false,
        buttons: ["Отказ", "Възстановяване!"],
        dangerMode: true
    }).then((willRestore) => {
        if (willRestore) {
            $.ajax({
                type: "Get",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.destroy();
                        loadDataTable(true);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}

function DirModalShow(url) {
    $.get(url).done(function (data) {
        placeholderElement.html(data);
        placeholderElement.find('.modal').modal('show');
    });

}

function ModalAction() {
    placeholderElement.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        let form = $(this).parents('.modal').find('form');
        let actionUrl = form.attr('action');
        let dataToSend = form.serialize();

        $.post(actionUrl, dataToSend).done(function (data) {
            let newBody = $('.modal-body', data);
            placeholderElement.find('.modal-body').replaceWith(newBody);
            let isValid = newBody.find('[name="IsValid"]').val() === 'True';

            if (isValid) {
                placeholderElement.find('.modal').modal('hide');
                location.reload();
            }

        });
    });
}

function NewDirModalShow() {
    $.get(`${path}CreateDirectorate`).done(function (data) {
        newdirplaseholder.html(data);
        newdirplaseholder .find('.modal').modal('show');
    });

}

function NewDirModalAction() {
    newdirplaseholder.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        let form = $(this).parents('.modal').find('form');
        let actionUrl = form.attr('action');
        let dataToSend = form.serialize();

        $.post(actionUrl, dataToSend).done(function (data) {
            let newBody = $('.modal-body', data);
            newdirplaseholder.find('.modal-body').replaceWith(newBody);
            let isValid = newBody.find('[name="IsValid"]').val() === 'True';

            if (isValid) {
                newdirplaseholder.find('.modal').modal('hide');
                location.reload();
            }

        });
    });
}
