var dataTable;
var permisionType = 'Guest';
var empdirectorateId;
var empdepartmentId;
var empsectorId;
var userFullName;
var userId;
let isaprovalActive = false;
const loc = window.location.href;
const path = loc.substr(0, loc.lastIndexOf('/') + 1);
let placeholderElement = $('#modal-placeholder');
let newdirplaseholder = $('#newdir-placeholder');
let datepickplaseholder = $('#datepick-placeholder');

$(document).ready(function () {
    $('#showDeleted').on('change', DeletedSectorsShowOrHile);
    GetUserPermision();
    GetAprovalStatus();
    loadDataTable(false);
    ModalAction();
    NewDepModalAction();
    DatepickAction();
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
            { "data": "directorateName", "width": "40%" },
            { "data": "departmentName", "width": "20%" },
            { "data": "name", "width": "28%" },
            {
                "data": null,
                "render": function (data, type, row) {
                    if (deleted) {
                        if (permisionType == 'SuperAdmin') {
                        return `<div>
                        <a style='cursor:pointer; padding-left:10px;' onclick=Restore('${path}RestoreSector?secId=${row.id}') title='Възстановяване'>
                            <img class="chatnotifications" src="../png/restore-icon.png" />
                        </a>
                        </div>`;
                        }
                        else {
                            return `<div></div>`;
                        }
                    }
                    else {
                        if (permisionType == 'SuperAdmin') {
                            return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' onclick=EditSecModalShow('${path}EditSector?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteSector?secId=${row.id}') title='Изтриване'>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        </a>
                            <a style='cursor:pointer; padding-left:10px;' title='Приключване на отчети' data-toggle='ajax-modal' data-target='#add-contact' ${isaprovalActive ? "" : "hidden"} onclick=DatepickShow('${path}AproveSecReport?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/Green-Check-Mark.png" />
                        </a>
                        </div>`;
                        }
                        else if (permisionType == 'DirectorateAdmin') {
                            if (row.directorateId == empdirectorateId) {
                                return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' onclick=EditSecModalShow('${path}EditSector?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteSector?secId=${row.id}') title='Изтриване'>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        </a>
                            <a style='cursor:pointer; padding-left:10px;' title='Приключване на отчети' data-toggle='ajax-modal' data-target='#add-contact' ${isaprovalActive ? "" : "hidden"} onclick=DatepickShow('${path}AproveSecReport?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/Green-Check-Mark.png" />
                        </a>
                        </div>`;
                            }
                            else {
                                return `<div></div>`;
                            }
                        }
                        else if (permisionType == 'DepartmentAdmin') {
                            if (row.departmentId == empdepartmentId) {
                                return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' onclick=EditSecModalShow('${path}EditSector?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        <a style='cursor:pointer; padding-left:5px;'
                            onclick=Delete('${path}DeleteSector?secId=${row.id}') title='Изтриване'>
                            <img class="chatnotifications" src="../png/delete2.png" />
                        </a>
                        </a>
                            <a style='cursor:pointer; padding-left:10px;' title='Приключване на отчети' data-toggle='ajax-modal' data-target='#add-contact' ${isaprovalActive ? "" : "hidden"} onclick=DatepickShow('${path}AproveSecReport?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/Green-Check-Mark.png" />
                        </a>
                        </div>`;
                            }
                            else {
                                return `<div></div>`;
                            }
                        }
                        else if (permisionType == 'SectorAdmin') {
                            if (row.id == empsectorId) {
                                return `<div>
                            <a style='cursor:pointer; padding-left:10px;' title='Редакция' data-toggle='ajax-modal' data-target='#add-contact' onclick=EditSecModalShow('${path}EditSector?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/edit-icon.png" />
                        </a>
                        </a>
                            <a style='cursor:pointer; padding-left:10px;' title='Приключване на отчети' data-toggle='ajax-modal' data-target='#add-contact' ${isaprovalActive ? "" : "hidden"} onclick=DatepickShow('${path}AproveSecReport?secId=${row.id}') >
                            <img class="chatnotifications" src="../png/Green-Check-Mark.png" />
                        </a>
                        </div>`;

                            }
                            else {
                                return `<div></div>`;
                            }
                        }
                        else {
                            return `<div></div>`;
                        }
                    }

                }, "width": "17%"
            }
        ],
        "iDisplayLength": 25,
        "language": {
            "emptyTable": "Няма сектори отговарящи на условието"
        },
        "width": "100%"
    });

}

function GetAprovalStatus() {
    $.ajax({
        type: "Get",
        url: path + '\GetAprovalStatus',
        success: function (data) {
            isaprovalActive = data;
        }
    });
}

function GetUserPermision() {
    $.ajax({
        type: "Get",
        url: path + '\GetUserRole',
        success: function (data) {
            //console.log(data);
            permisionType = data.roleName;
            empdirectorateId = data.directorateId;
            empdepartmentId = data.departmentId;
            empsectorId = data.sectorId;
            userFullName = data.fullNam;
            userId = data.id;
        }
    });
}

function DatepickShow(url) {
    $.get(url).done(function (data) {
        datepickplaseholder.html(data);
        datepickplaseholder.find('.modal').modal('show');
    });
}

function DatepickAction() {
    datepickplaseholder.on('click', '[data-save="modal"]', function (event) {
        event.preventDefault();
        let form = $(this).parents('.modal').find('form');
        let actionUrl = form.attr('action');
        let dataToSend = form.serialize();

        $.post(actionUrl, dataToSend).done(function (data) {
            let newBody = $('.modal-body', data);
            datepickplaseholder.find('.modal-body').replaceWith(newBody);
            let isValid = newBody.find('[name="IsValid"]').val() === 'True';

            if (isValid) {
                datepickplaseholder.find('.modal').modal('hide');
                location.reload();
            }

        });
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

function EditSecModalShow(url) {
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
    $.get(`${path}CreateSector`).done(function (data) {
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
