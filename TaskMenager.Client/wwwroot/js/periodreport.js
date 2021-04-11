$(() => {
    attachEvents();

    function attachEvents() {
        $('#send').on('click', CheckFieldsChoose);
        $('#enddate').on('change', checkendDate);
        $('#enddate').on('click', function () { $("#enddate").css('background-color', '#ffffff'); });
        $('#startdate').on('change', checkstartDate);
        $('#startdate').on('click', function () { $("#startdate").css('background-color', '#ffffff'); });


    }

    function checkstartDate() {
        var selectedText = document.getElementById("startdate").value;
        var selectedDate = new Date(selectedText);
        var now = new Date();
        if (selectedDate > now) {
            toastr.error('Въведете минала дата');
            $("#startdate").css('background-color', 'rgb(250, 204, 204)');
            return false;
        }
        return true;
    }

    function checkendDate() {
        var selectedText = document.getElementById("enddate").value;
        var selectedDate = new Date(selectedText);
        var now = new Date();
        if (selectedDate > now) {
            toastr.error('Въведете днешна или минала дата');
            $("#enddate").css('background-color', 'rgb(250, 204, 204)');
            return false;
        }
        return true;
    }

    function checkDates() {
        var selectedText = document.getElementById("startdate").value;
        var startDate = new Date(selectedText);
        selectedText = document.getElementById("enddate").value;
        var endDate = new Date(selectedText);
        if (startDate > endDate) {
            toastr.error('Невалиден период!');
            $("#startdate").css('background-color', 'rgb(250, 204, 204)');
            $("#enddate").css('background-color', 'rgb(250, 204, 204)');
            return false;
        }
        return true;
    }


    function CheckFieldsChoose() {
        let result = true;

        if (!checkstartDate()) {
            result = false;
            $("#startdate").css('background-color', 'rgb(250, 204, 204)');
        }

        if (!checkendDate()) {
            result = false;
            $("#enddate").css('background-color', 'rgb(250, 204, 204)');
        }
        if (!checkDates()) {
            result = false;
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