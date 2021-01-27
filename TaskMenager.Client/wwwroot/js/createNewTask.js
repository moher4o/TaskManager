$(() => {
    attachEvents();

    function attachEvents() {
        $('#assignerId').on('click', function () { $("#assignerId").css('background-color', '#ffffff'); });
        //$('#directorateId').on('click', function () { $("#directorateId").css('background-color', '#ffffff'); });
        $('#taskPriorityId').on('click', function () { $("#taskPriorityId").css('background-color', '#ffffff'); });
        $('#hourslimit').on('change', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#hourslimit').on('click', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#taskName').on('change', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#taskName').on('click', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckFieldsChoose() {
        let result = true;
        let errormessage = "";
        if ($('#assignerId :selected').text() === 'Моля изберете...') {
            $("#assignerId").css('background-color', 'rgb(250, 204, 204)');
            $(':focus').blur()
            result = false;
        }
        //if ($('#directorateId :selected').text() === 'Моля изберете...') {
        //    $("#directorateId").css('background-color', 'rgb(250, 204, 204)');
        //    $(':focus').blur()
        //    result = false;
        //}
        if ($('#taskPriorityId :selected').text() === 'Моля изберете...') {
            $("#taskPriorityId").css('background-color', 'rgb(250, 204, 204)');
            $(':focus').blur()
            result = false;
        }

        var num = document.getElementById("hourslimit").value;
        if (!(/^\d+$/.test(num))) {
            $("#hourslimit").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        let a = document.getElementById("taskName").value;
        if (a == null || a == "") {
            $("#taskName").css('background-color', 'rgb(250, 204, 204)');
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