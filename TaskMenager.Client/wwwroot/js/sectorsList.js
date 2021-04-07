var dataTable;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);
let placeholderElement = $('#modal-placeholder');
let newdirplaseholder = $('#newdir-placeholder');
let newsecplaseholder = $('#newsec-placeholder');

$(document).ready(function () {
    $('#showDeleted').on('change', DeletedSectorsShowOrHile);
    loadDataTable(false);
    ModalAction();
    NewDepModalAction();
    NewSecModalAction()
});

function DeletedSectorsShowOrHile() {
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
    var url = path + "GetAllSectors?deleted=" + deleted;
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "3%" },
            { "data": "directorateName", "width": "30%" },
            { "data": "departmentName", "width": "30%" },
            { "data": "name", "width": "35%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        return `<div>
                        <a style='cursor:pointer; padding-left:10px;' onclick=Restore('${path}RestoreSector?secId=${row.id}') title='Възстановяване'>
                            <img class="chatnotifications" src="../png/restore-icon.png" />
                        </a>
                        </div>`;
                    }
                    else {
                        return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' onclick=DirModalShow('${path}EditSector?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteSector?secId=${row.id}') title='Изтриване'>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>

                        </div>`;
                    }

                }, "width": "10%"
            }
        ],
        "language": {
            "emptyTable": "Няма сектори отговарящи на условието"
        },
        "width": "100%"
    });

}

function Delete(url) {
    swal({
        title: "Потвърждение",
        text: "Сигурни ли сте, че искате да изтриете сектора?",
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
        text: "Сигурни ли сте, че искате да възстановите сектора?",
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

function AddSecModalShow(url) {
    $.get(url).done(function (data) {
        newsecplaseholder.html(data);
        newsecplaseholder.find('.modal').modal('show');
    });
}

function NewSecModalAction() {
    newsecplaseholder.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        let form = $(this).parents('.modal').find('form');
        let actionUrl = form.attr('action');
        let dataToSend = form.serialize();

        $.post(actionUrl, dataToSend).done(function (data) {
            let newBody = $('.modal-body', data);
            newsecplaseholder.find('.modal-body').replaceWith(newBody);
            let isValid = newBody.find('[name="IsValid"]').val() === 'True';

            if (isValid) {
                newsecplaseholder.find('.modal').modal('hide');
                location.reload();
            }

        });
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

function NewDepModalShow() {
    $.get(`${path}CreateDepartment`).done(function (data) {
        newdirplaseholder.html(data);
        newdirplaseholder.find('.modal').modal('show');
    });
}

function NewDepModalAction() {
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
