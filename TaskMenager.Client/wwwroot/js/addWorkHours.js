$(() => {
    attachEvents();

    function attachEvents() {
        $('#hours').on('change', function () { $("#hours").css('background-color', '#ffffff'); });
        $('#hours').on('click', function () { $("#hours").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
        $('#workdate').on('change', GetDateInfo);
        $('#workdate').on('click', function () { $("#workdate").css('background-color', '#ffffff'); });

    }

    function checkDate() {
        var selectedText = document.getElementById("workdate").value;
        var selectedDate = new Date(selectedText);
        var now = new Date();
        if (selectedDate > now) {
            toastr.error('Въведете днешна или минала дата');
            $("#workdate").css('background-color', 'rgb(250, 204, 204)');
            return false;
        }
        return true;
    }

    function GetDateInfo() {
        var selectedText = document.getElementById("workdate").value;
        var selectedDate = new Date(selectedText);
        var now = new Date();
        if (selectedDate > now) {
            toastr.error('Въведете днешна или минала дата');
            $("#workdate").css('background-color', 'rgb(250, 204, 204)');
        }
        else {
            var content = document.getElementById('dateinfo');
            $.getJSON('..\\Tasks\\GetDateWorkedHours\?searchedDate=' + selectedDate.toUTCString(), { get_param: 'value' }, function (data) {
                
                $.each(data.data, function (i, item) {
                    var $tr = $('<tr>').append(
                        $('<td>').text(item.taskName),
                        $('<td>').text(item.workedHours),
                        $('<td>').text(item.text)
                    ).appendTo('#dateinfo');
                //content.innerHTML = '<div style = \'text-align:left;\'><div class="row"><div class="col-md-3">N:</div><div class="col-md-4"><label><strong>' + data.data.id + '</strong></label></div></div><div class="row"><div class="col-md-3">Име:</div><div class="col-md-9"><label><strong>' + data.data.fullName + '</strong></label></div></div><div class="row"><div class="col-md-3">Длъжност:</div><div class="col-md-9"><label><strong>' + data.data.jobTitleName + '</strong></label></div></div><div class="row"><div class="col-md-3">Email:</div><div class="col-md-9"><label><strong>' + (data.data.email ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Роля:</div><div class="col-md-9"><label><strong>' + data.data.roleName + '</strong></label></div></div><div class="row"><div class="col-md-3">Телефон:</div><div class="col-md-9"><label><strong>' + (data.data.telephoneNumber ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Мобилен:</div><div class="col-md-9"><label><strong>' + (data.data.mobileNumber ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Дирекция:</div><div class="col-md-9"><label><strong>' + (data.data.directorateName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Отдел:</div><div class="col-md-9"><label><strong>' + (data.data.departmentName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Сектор:</div><div class="col-md-9"><label><strong>' + (data.data.sectorName ?? '<span style=\'color:red;\'>Няма информация</span>') + '</strong></label></div></div><div class="row"><div class="col-md-3">Статус:</div><div class="col-md-9"><label><strong>' + (data.data.status == "Деактивиран акаунт" ? '<span style=\'color:red;\'>Деактивиран акаунт</span>' : (data.data.status == "Активен акаунт" ? '<span style=\'color:green;\'>Активен акаунт</span>' : '<span style=\'color:lightgreen;\'>Чакащ одобрение</span>')) + '</strong></label></div></div></div>';

            });
        }
    }


    function CheckFieldsChoose() {
        let result = true;
        var num = document.getElementById("hours").value;
        if (!(/^\d+$/.test(num))) {
            $("#hours").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if (!checkDate()) {
            result = false;
            $("#workdate").css('background-color', 'rgb(250, 204, 204)');
        } 

        if (result) {
            $("#realsend").click();
        }
        else {
            //notify.showError('Моля въведете коректни стойности.');
            toastr.error('Моля въведете коректни стойности.');
        }
    }

});