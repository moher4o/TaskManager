var dataTable;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);


$(document).ready(function () {
    $('#showDeleted').on('change', DeletedUsersShowOrHile);
    loadDataTable(false);
    

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
                        <a style='cursor:pointer; padding-left:10px;' onclick=Restore('${path}RestoreDirectorate?dirId=${row.id}') title='Възстановяване'>
                            <img class="chatnotifications" src="../png/restore-icon.png" />
                        </a>
                        </div>`;
                    }
                   // <a href="${path}RenameDirectorate?dirId=${row.id}&dirName=${row.name}" style='cursor:pointer; padding-left:10px;' title='Редакция'>
                   // <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle="ajax-modal" data-target="#add-contact" data-url="${path}RenameDirectorate" data-dirid="${row.id}" data-dirname="${row.name}">
                    else {
                        return `<div>
                            <div id="modal-placeholder"></div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' onclick=PopModal(event) data-url="${path}RenameDirectorate?dirId=${row.id}&dirName=${row.name}" >
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
    attachEvents();
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

function PopModal(event) {
    let url = $(this).data('url');
    $.get(url).done(function (data) {
        placeholderElement.html(data);
        placeholderElement.find('.modal').modal('show');
    });

}

function attachEvents() {
    console.log(1);
    let placeholderElement = $('#modal-placeholder');
    $('a[data-toggle="ajax-modal"]').click(function (event) {
        console.log(2);
        let url = $(this).data('url') + '?dirId=' + $(this).data('dirid') + '&dirName=' + $(this).data('dirname');
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });

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

