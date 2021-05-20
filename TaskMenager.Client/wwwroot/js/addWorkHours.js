$(() => {
    attachEvents();
    GetDateInfo();

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
        var userId = document.getElementById("employeeId").value;
        console.log(userId);
        var selectedText = document.getElementById("workdate").value;
        var selectedDate = new Date(selectedText);
        var now = new Date();
        if (selectedDate > now) {
            toastr.error('Въведете днешна или минала дата');
            $("#workdate").css('background-color', 'rgb(250, 204, 204)');
        }
        else {
            $("tr:has(th)").remove();
            $("tr:has(td)").remove();
            $.getJSON('..\\Tasks\\GetDateWorkedHours\?searchedDate=' + selectedDate.toUTCString() + "&userId=" + userId, { get_param: 'value' }, function (response) {
                if (response.data.length > 0) {
                    $head = $('<thead>').append(
                        $('<tr>').append(
                        $('<th>').text('Задача'),
                        $('<th>').text('Часове'),
                        $('<th>').text('Бележка')
                        ));
                    $head.appendTo('#dateinfo');
                }
                $.each(response.data, function (i, item) {
                    var $tr = $('<tr>').append(
                        $('<td>').text(item.taskName),
                        $('<td>').text(item.workedHours),
                        $('<td>').text(item.note)
                    );  
                    $tr.appendTo('#dateinfo');
                });

            });
        }
    }


    function CheckFieldsChoose() {
        let result = true;
        var num = document.getElementById("hours").value;
        if (!(/^-?\d+$/.test(num))) {
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
            toastr.error('Моля въведете коректни стойности.');
        }
    }

});