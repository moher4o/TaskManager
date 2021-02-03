$(() => {
    attachEvents();

    function attachEvents() {
        $('#hours').on('change', function () { $("#hours").css('background-color', '#ffffff'); });
        $('#hours').on('click', function () { $("#hours").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
        $('#workdate').on('change', checkDate);
        $('#workdate').on('click', function () { $("#workdate").css('background-color', '#ffffff'); });

    }

    function checkDate() {
        var selectedText = document.getElementById("workdate").value;
        console.log(selectedText);
        var selectedDate = new Date(selectedText);
        console.log(selectedDate);
        var now = new Date();
        if (selectedDate > now) {
            notify.showError('Въведете днешна или минала дата');
            $("#workdate").css('background-color', 'rgb(250, 204, 204)');
            return false;
        }
        return true;
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
            notify.showError('Моля въведете коректни стойности.');
        }
    }

});