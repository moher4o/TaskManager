$(() => {
    const delayInMilliseconds = 200;
    const lochref = window.location.href;
    const locOrigin = window.location.origin;
    let path;
    if (lochref.toLowerCase().indexOf("taskmanager") >= 0) {
        path = locOrigin + "\\TaskManager";
    }
    else {
        path = locOrigin;
    }
    var currentUserId = document.getElementById("currentUserId").value;
    var userId = document.getElementById("employeeId") == null ? currentUserId : document.getElementById("employeeId").value;
    var slideSource = document.getElementById('AREA_PARTIAL_VIEW');

    document.getElementById("apiNotWorking").hidden = true;
    AddDatePicker();
    GetDominions();

    $(function () { //jQuery shortcut for .ready (ensures DOM ready)
        GetUserTaskForDate();
        //$('#divbtnZapis').show();
        //onclick евент за бутон Запис
        $('#btnZapis').on('click', UpdateHours);
        $('#holiday').on('change', ShowHolidayWaterMark);
        //$('#illness').on('change', ShowIllWaterMark());
    });

    function ShowHolidayWaterMark() {
        if ($("#holiday").prop('checked') == false) {
            $("#illness").prop('checked', false);
            $('#watermark').hide();

        }
        else {
            $("#illness").prop('checked', false);
            $('#watermark').show();
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
                        GetUserTaskForDate();
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
        });
    }

    function AddDatePicker() {
        //var selectedText = document.getElementById("workDate").value
        var selectedText = document.getElementById("workDate") == null ? new Date().toDateString() : document.getElementById("workDate").value;
        //console.log(selectedText);
        $('#dateSelector2').datepicker({ dateFormat: 'dd-M-yy', changeYear: true, showOtherMonths: true, firstDay: 1, maxDate: "+1d", inline: true });
        $('#dateSelector2').datepicker('setDate', new Date(selectedText));
        //$('#dateSelector2').datepicker("refresh");
        //let date2 = $('#dateSelector2').datepicker("getDate");
        //console.log(date2);
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
    }

    function GetUserTaskForDate() {
        var currentUrl = path + '\\Home\\GetDateTasks';
        let newDate = $('#dateSelector2').datepicker("getDate");
        let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
        // stop animation of button Zapis
        var element = document.getElementById("btnZapis");
        if (element.classList.contains("special")) {
            element.classList.remove("special");
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
                    item.addEventListener('mouseover', event => {
                        item.style["background-color"] = "#e7f6fb";
                    })
                    item.addEventListener('mouseout', event => {
                        item.style["background-color"] = "#fff";
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
            }
        });
    }

    function UpdateHours() {
        var currentUrl = path + '\/Tasks\/SetDateTasksHours';
        let newDate = $('#dateSelector2').datepicker("getDate");
        let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();
        let totallSuccess = true;
        var messageInfo = 'Часовете са записани успешно';
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
        setTimeout(function () {
            if (totallSuccess == false) {
                toastr.error(messageInfo);
                $('#btnZapis').blur();
            }
            else {
                var element = document.getElementById("btnZapis");
                if (element.classList.contains("special")) {
                    element.classList.remove("special");
                }
                $('#btnZapis').blur();
                toastr.success(messageInfo);
            }
        }, 1200);
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
            var element = document.getElementById("btnZapis");
            if (!element.classList.contains("special")) {
                element.classList.add("special");
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

});