//$(() => {
//{
const delayInMilliseconds = 200;
const lochref = window.location.href;
const locOrigin = window.location.origin;
let path;
if (lochref.toLowerCase().indexOf("taskmanager") > 0) {
    path = locOrigin + "\\TaskManager";
}
else {
    path = locOrigin;
}
var currentUserId = document.getElementById("currentUserId").value;
var userId = document.getElementById("employeeId") == null ? currentUserId : document.getElementById("employeeId").value;
var slideSource = document.getElementById('AREA_PARTIAL_VIEW');
var btnZapis = document.getElementById("btnZapis");

document.getElementById("apiNotWorking").hidden = true;
GetDominions();
//setTimeout(function () {
//}, 100);
//AddDatePicker();
//GetHolidays();

$(function () { //jQuery shortcut for .ready (ensures DOM ready)
    //    setTimeout(function () {
    //GetUserTaskForDate();
    //}, 2000);


    //onclick евент за бутон Запис
    $('#btnZapis').on('click', UpdateHours);
    $('#holiday').on('change', ShowHolidayWaterMark);
    $('#illness').on('change', ShowIllWaterMark);
});

function ShowHolidayWaterMark() {
    if ($("#holiday").prop('checked') == false) {
        $("#illness").prop('checked', false);
        $('#watermarkholiday').hide('fast');
        $('.input-number').prop('readonly', false);
        $(".btn-number").removeAttr("disabled");
        if (!btnZapis.classList.contains("special")) {
            btnZapis.classList.add("special");
        }
    }
    else {
        $("#illness").prop('checked', false);
        $('#watermarkill').hide('fast');
        toastr.error('Отчетените часове за тази дата ще бъдат нулирани при запис!');
        $('#watermarkholiday').show('fast');
        //var element = document.getElementById("btnZapis");
        if (!btnZapis.classList.contains("special")) {
            btnZapis.classList.add("special");
        }
        $('.input-number').prop('readonly', true);
        $(".btn-number").attr("disabled", true);

    }
}

function ShowIllWaterMark() {
    if ($("#illness").prop('checked') == false) {
        $("#holiday").prop('checked', false);
        $('#watermarkill').hide('fast');
        $('.input-number').prop('readonly', false);
        $(".btn-number").removeAttr("disabled");
        if (!btnZapis.classList.contains("special")) {
            btnZapis.classList.add("special");
        }
    }
    else {
        $("#holiday").prop('checked', false);
        $('#watermarkholiday').hide('fast');
        toastr.error('Отчетените часове за тази дата ще бъдат нулирани при запис!');
        $('#watermarkill').show('fast');
        //var element = document.getElementById("btnZapis");
        if (!btnZapis.classList.contains("special")) {
            btnZapis.classList.add("special");
        }
        $('.input-number').prop('readonly', true);
        $(".btn-number").attr("disabled", true);

    }
}

function CheckMaxHours() {
    var totalMaxHours = document.getElementById("maxHours") == null ? 16 : parseInt(document.getElementById("maxHours").value);
    //var currentTotal = document.getElementById("totalLabel") == null ? 16 : parseInt(document.getElementById("totalLabel").textContent);
    var sum = parseInt(0);
    $('.PrimeBox3').each(function () {
        let currenttaskId = $(this).attr('id');
        //if (currenttaskId != 'closedTask') {
        let todayhours = parseInt($(this).find('input').first().val());
        sum = sum + todayhours;
        //}
    });
    if (sum > totalMaxHours) {
        return false;
    }
    else {
        if (sum > 8) {
            toastr.error('Въведени са повече от 8 часа за деня. Максималния брой е: ' + totalMaxHours);
        }
        document.getElementById('totalLabel').innerHTML = sum;
        return true;
    }
}

function CheckSelectedDominion() {
    var url = window.location.href;
    let newDate = $('#dateSelector2').datepicker("getDate");
    if (url.indexOf('?') > -1) {
        url = url.substring(0, url.indexOf('?')) + '?userId=' + $('#bosses :selected').val() + '&workDate=' + newDate.toUTCString();
    } else {
        url += '?userId=' + $('#bosses :selected').val() + '&workDate=' + newDate.toUTCString();
    }
    //console.log($('#bosses :selected').val());
    // window.location.href = url;
}

function GetDominions() {
    var url = path + "\\Users\\GetDominionUsers";
    $.getJSON(url, { get_param: 'value' }, function (response) {
        if (response.data.length > 0) {
            $('#dominions')
                .append(
                    $(document.createElement('label')).prop({
                        for: 'bigbosses'
                    }).html(`Сесия от името на:&nbsp&nbsp`)
                )
                .append(
                    $(document.createElement('select')).prop({
                        id: 'bosses',
                        name: 'bosses'
                    })
                )
            $('#bosses').change(function () {
                slideSource.classList.toggle('fade');
                setTimeout(function () {
                    GetHolidays();                    //При смяна на акаунта се презареждат задачите
                }, delayInMilliseconds);
            });
        }
        $.each(response.data, function (i, item) {
            if (userId == item.id) {
                $("#bosses").append(new Option(item.textValue, item.id, true, true));
            }
            else {
                $("#bosses").append(new Option(item.textValue, item.id));
            }
        });
    }).done(function () { //use this
        GetHolidays();
    });
}

function GetHolidays() {
    let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
    //console.log(bossUserId);
    currentUrl = path + '\/Tasks\/GetHolidayDates';
    $.ajax({
        type: 'GET',
        url: currentUrl,   // '..\\Tasks\\GetHolidayDates'
        data: {
            userId: bossUserId
        }
    }).done(function (data) { //use this
        currentUrl = path + '\/Tasks\/GetIlldayDates';
        $.ajax({
            type: 'GET',
            url: currentUrl,   // '..\\Tasks\\GetIlldayDates'
            data: {
                userId: bossUserId
            }
        }).done(function (illdates) { //use this
            AddDatePicker(data.data, illdates.data);
        });
    });
}

function AddDatePicker(holidays, illdays) {
    //holidays = ["2021/07/01", "2021/07/05", "2021/06/22", "2021/06/30", "2021/06/27", "2021/07/15"];
    //console.log(holidays);
    //console.log(illdays);
    jQuery('#dateSelector2').datepicker('destroy');
    var selectedText = document.getElementById("workDate") == null ? new Date().toDateString() : document.getElementById("workDate").value;
    //console.log(selectedText);

    $('#dateSelector2').datepicker({ dateFormat: 'dd-M-yy', changeYear: true, showOtherMonths: true, firstDay: 1, maxDate: "+1m", inline: true, beforeShowDay: highLight });

    //$("#dateSelector2").datepicker({ beforeShowDay: highLight });

    $('#dateSelector2').datepicker('setDate', new Date(selectedText));
    //$('#dateSelector2').datepicker("refresh");
    //let date2 = $('#dateSelector2').datepicker("getDate");
    //console.log(date2);

    function highLight(date) {
        //for (var i = 0; i < holidays.length; i++) {
        //    if (new Date(holidays[i]).toString() == date.toString()) {
        //        return [true, 'ui-state-holiday'];
        //    }
        let delitel = '/';;
        if (holidays.length > 0) {
            if (holidays[0].includes('.')) {
                delitel = '.';
            }
            else if (holidays[0].includes('-')) {
                delitel = '-';
            }
        }
        var search = date.getFullYear("yyyy") + delitel + (("0" + (date.getMonth() + 1)).slice(-2)) + delitel + ("0" + date.getDate()).slice(-2);
        if (holidays.includes(search)) {
            //console.log('inhy');
            return [true, 'ui-state-holiday'];
        }
        if (illdays.includes(search)) {
            //console.log('inill');
            return [true, 'ui-state-ill'];
        }
        //}
        return [true];
    }


    $('#dateSelector2').datepicker({
        onSelect: function (d, i) {
            if (d !== i.lastVal) {
                $(this).change();
            }
        }
    });
    $('#dateSelector2').change(function () {
        slideSource.classList.toggle('fade');
        setTimeout(function () {
            GetUserTaskForDate();
        }, delayInMilliseconds);

    });
    GetUserTaskForDate();
}

function GetUserTaskForDate() {
    var currentUrl = path + '\\Home\\GetDateTasks';
    let newDate = $('#dateSelector2').datepicker("getDate");
    let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
    // stop animation of button Zapis
    //var element = document.getElementById("btnZapis");
    if (btnZapis.classList.contains("special")) {
        btnZapis.classList.remove("special");
    }
    $.ajax({
        type: 'GET',
        url: currentUrl,   //'..\\Home\\GetDateTasks'
        data: {
            userId: bossUserId,
            workDate: newDate.toUTCString()
        },
        success: function (result) {
            // "избухване" на данните за новата дата
            $('#AREA_PARTIAL_VIEW').html("");
            $('#AREA_PARTIAL_VIEW').html(result);
            slideSource.classList.toggle('fade');
            //добавяне на eventListener
            document.querySelectorAll('.PrimeBox3').forEach(item => {
                let backcolor = item.style["background-color"];
                item.addEventListener('mouseover', event => {
                    item.style["background-color"] = "#e7f6fb";
                })
                item.addEventListener('mouseout', event => {
                    item.style["background-color"] = backcolor;
                })
            })
            //onclick евент за показване на приключените задачи 
            $('#showInactive').click(function () {
                document.getElementById('showclosedTasks').classList.toggle('displayno');
            });
            //функцията добавя елемента за отчитане на часовете
            AddHoursCounter();
            //функцията добавя балончето за бележки
            attachEvents();
            //управление на отпуска и болничен
            setTimeout(function () {
                var systemtask = document.getElementById("systemtask");    //отпуска, болничен или не
                if (systemtask.value == 'holiday') {
                    $('.input-number').prop('readonly', true);
                    $(".btn-number").attr("disabled", true);
                    $("#holiday").prop('checked', true);
                    $("#illness").prop('checked', false);
                    $('#watermarkholiday').show('fast');
                }
                else if (systemtask.value == 'ill') {
                    $('.input-number').prop('readonly', true);
                    $(".btn-number").attr("disabled", true);
                    $("#illness").prop('checked', true);
                    $("#holiday").prop('checked', false);
                    $('#watermarkill').show('fast');
                }
                else {
                    $('.input-number').prop('readonly', false);
                    $(".btn-number").removeAttr("disabled");
                    $("#holiday").prop('checked', false);
                    $("#illness").prop('checked', false);
                }
                var reportapproved = document.getElementById("reportapproved");    //одобрен или не

                if (reportapproved.value == 'approved') {
                    $('.input-number').prop('readonly', true);
                    $(".btn-number").attr("disabled", true);
                    $("#illness").attr("disabled", true);
                    $("#holiday").attr("disabled", true);
                    $('#watermarkapproved').show('fast');
                    $('a[data-toggle="ajax-modal"]').prop("onclick", null).off("click");
                    //$("#btnZapis").prop("onclick", null).off("click");
                    //$('#btnZapis').on('click', UpdateBlur);
                }
            //    else {
            //        $('#btnZapis').on('click', UpdateHours);
            //    }
            }, delayInMilliseconds);
        }
    });
}

function UpdateHours() {
    let reportapproved = document.getElementById("reportapproved");
    if (reportapproved.value == 'none') {
        let currentUrl = path + '\\Tasks\\FrontEndDateCheck';
        let newDate = $('#dateSelector2').datepicker("getDate");
        let url = currentUrl + '?workDate=' + newDate.toUTCString();
        $.get(url).done(function (data) {
            if (!data.success) {
                swal({
                    title: "Информация",
                    text: "Ще редактирате данните за дата, която е от минал отчетен период. Промените ще бъдат отразени, но ще бъде отбелязано, че промените са правени след отчетния период.",
                    icon: "warning",
                    closeOnEsc: false,
                    buttons: ["Отказ", "Потвърждение"],
                    dangerMode: true
                }).then((willEdit) => {
                    if (willEdit) {
                        SetHours(newDate);
                    }
                    else {
                        if (btnZapis.classList.contains("special")) {
                            btnZapis.classList.remove("special");
                        }
                        $('#btnZapis').blur();
                        slideSource.classList.toggle('fade');
                        setTimeout(function () {
                            //GetUserTaskForDate();
                            GetHolidays();
                        }, 300);
                    }
                });
            }
            else {
                SetHours(newDate);
            }
        });
    }
    else {
        $('#btnZapis').blur();
    }
 
}

function SetHours(newDate) {
    let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
    var isholiday = $("#holiday").prop('checked');
    var isill = $("#illness").prop('checked');
    currentUrl = path + '\/Tasks\/SetDateTasksHours';
    let totallSuccess = true;
    var messageInfo = 'Часовете са записани успешно';
    if (!isholiday && !isill) {                                    //ако не е чекнато отпуска или болничен
        var removeSystemTAsktUrl = path + '\/Tasks\/RemoveSystemTasks';
        $.ajax({
            type: 'GET',
            url: removeSystemTAsktUrl,   // '..\\Tasks\\RemoveSystemTasks'
            data: {
                userId: bossUserId,
                workDate: newDate.toUTCString()
            },
            success: function (data) {
                totallSuccess = true;
                messageInfo = data.message;
            },
            error: function (data) {
                totallSuccess = false;
                messageInfo = data.message;
            }
        });
        if (totallSuccess) {                                    //ако успешно са изтрити болнични или отпуски --> почва отбелязването на работните часове
            $('.PrimeBox3').each(function () {
                let currenttaskId = $(this).attr('id');
                if (currenttaskId != 'closedTask') {
                    let todayhours = $(this).find('input').first().val();
                    // заявка за запис на часовете
                    $.ajax({
                        type: 'GET',
                        url: currentUrl,   // '..\\Tasks\\SetDateTasksHours'
                        data: {
                            userId: bossUserId,
                            workDate: newDate.toUTCString(),
                            taskId: currenttaskId,
                            hours: todayhours
                        },
                        success: function (data) {
                            if (data.success) {
                                if (todayhours != '0') {
                                    messageInfo = 'Часовете са записани успешно';
                                }
                            }
                            else {
                                totallSuccess = false;
                                messageInfo = data.message;
                            }
                        },
                        error: function (data) {
                            totallSuccess = false;
                            messageInfo = data.message;
                        }
                    });
                }
            });
        }
    }
    else {    //ако е чекнато отпуска или болничен
        currentUrl = path + '\/Tasks\/SetDateSystemTasks';

        $.ajax({
            type: 'GET',
            url: currentUrl,   // '..\\Tasks\\SetDateSystemTasks'
            data: {
                userId: bossUserId,
                workDate: newDate.toUTCString(),
                isholiday: isholiday,
                isill: isill
            },
            success: function (data) {
                totallSuccess == true;
                messageInfo = data.message;
            },
            error: function (data) {
                totallSuccess == false;
                messageInfo = data.message;
            }
        });
    }
    slideSource.classList.toggle('fade');
    setTimeout(function () {
        //GetUserTaskForDate();
        GetHolidays();
    }, 300);

    setTimeout(function () {
        if (totallSuccess == false) {
            toastr.error(messageInfo);
            $('#btnZapis').blur();
        }
        else {
            //var element = document.getElementById("btnZapis");
            if (btnZapis.classList.contains("special")) {
                btnZapis.classList.remove("special");
            }
            $('#btnZapis').blur();
            toastr.success(messageInfo);
        }
    }, 800);
}

function attachEvents() {

    let placeholderElement = $('#modal-placeholder');
    $('a[data-toggle="ajax-modal"]').click(function (event) {
        var currentUrl = path + '\\Tasks\\AddDateNote';
        let newDate = $('#dateSelector2').datepicker("getDate");
        let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
        let url = currentUrl + '?taskId=' + $(this).data('taskid') + '&userId=' + bossUserId + '&taskName=' + $(this).data('taskname') + '&workDate=' + newDate.toUTCString();
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
                //setTimeout(function () {
                //GetUserTaskForDate();
                //}, 1500);
                //location.reload();

            }

        });
    });
}

function AddHoursCounter() {
    /////////////////////
    //setTimeout(function () {
    $('.btn-number').click(function (e) {
        e.preventDefault();

        fieldName = $(this).attr('data-field');
        type = $(this).attr('data-type');
        var input = $("input[name='" + fieldName + "']");
        var currentVal = parseInt(input.val());    //запис на старата стойност, за да се върне, ако се окаже, че новия сбор е по голям от максимума
        input.data('oldValue', currentVal);
        if (!isNaN(currentVal)) {
            if (type == 'minus') {

                if (currentVal > input.attr('min')) {
                    input.val(currentVal - 1).change();
                }
                if (parseInt(input.val()) == input.attr('min')) {
                    input.css('color', 'dodgerblue');
                    $(this).attr('disabled', true);
                }

            } else if (type == 'plus') {

                if (currentVal < input.attr('max')) {
                    if (currentVal == 0) {
                        input.css('color', 'red');
                    }
                    input.val(currentVal + 1).change();
                }
                if (parseInt(input.val()) == input.attr('max')) {
                    $(this).attr('disabled', true);
                }

            }
        } else {
            input.val(0);
        }
    });
    $('.input-number').focusin(function () {
        $(this).data('oldValue', $(this).val());
    });

    $('.input-number').change(function (lastval) {
        // start animation of button Zapis
        //var element = document.getElementById("btnZapis");
        if (!btnZapis.classList.contains("special")) {
            btnZapis.classList.add("special");
        }
        minValue = parseInt($(this).attr('min'));
        maxValue = parseInt($(this).attr('max'));
        valueCurrent = parseInt($(this).val());

        name = $(this).attr('name');
        if (valueCurrent >= minValue) {
            $(".btn-number[data-type='minus'][data-field='" + name + "']").removeAttr('disabled')
            if (valueCurrent > minValue) {
                $(this).css('color', 'red');
            }
        } else {
            toastr.error('Въведеното число е по-малко от допустимото');
            $(this).val($(this).data('oldValue'));
        }
        if (valueCurrent <= maxValue) {
            $(".btn-number[data-type='plus'][data-field='" + name + "']").removeAttr('disabled')
        } else {
            toastr.error('Въведеното число е по-голямо от допустимото');
            $(this).val($(this).data('oldValue'));
            if ($(this).data('oldValue') == 0) {
                $(this).css('color', 'dodgerblue');
            }
        }
        //проверка за общия сбор и ако трябва се връща старата стойност на полето инпут
        if (valueCurrent <= maxValue && valueCurrent >= minValue) {
            if (!CheckMaxHours()) {
                $(this).val($(this).data('oldValue'));
                toastr.error('Общия брой часове надвишава максимално допустимия за деня');
            }
        }

    });
    $(".input-number").keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 190]) !== -1 ||
            // Allow: Ctrl+A
            (e.keyCode == 65 && e.ctrlKey === true) ||
            // Allow: home, end, left, right
            (e.keyCode >= 35 && e.keyCode <= 39)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
    //}, 1000);
    ///////////////////
}

    //});
//}