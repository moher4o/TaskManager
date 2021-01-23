$(() => {
    attachEvents();

    function attachEvents() {
        $('#assigners').on('click', function () { $("#assigners").css('background-color', '#ffffff'); });
        $('#directorateId').on('click', function () { $("#directorateId").css('background-color', '#ffffff'); });
        $('#priority').on('click', function () { $("#priority").css('background-color', '#ffffff'); });
        $('#hourslimit').on('change', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#hourslimit').on('click', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckFieldsChoose() {
        let result = true;
        let errormessage = "";
        if ($('#assigners :selected').text() === 'Моля изберете...') {
            $("#assigners").css('background-color', 'rgb(250, 204, 204)');
            $(':focus').blur()
            result = false;
        }
        if ($('#directorateId :selected').text() === 'Моля изберете...') {
            $("#directorateId").css('background-color', 'rgb(250, 204, 204)');
            $(':focus').blur()
            result = false;
        }
        if ($('#priority :selected').text() === 'Моля изберете...') {
            $("#priority").css('background-color', 'rgb(250, 204, 204)');
            $(':focus').blur()
            result = false;
        }

        var num = document.getElementById("hourslimit").value;
        console.log(num);
        if (!(/^\d+$/.test(num))) {
            $("#hourslimit").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if (result) {
            $("#realsend").click();
        }
        else {
            notify.showError('Моля попълнете всички задължителни полета');
        }
    }

});