$(() => {
    attachEvents();

    function attachEvents() {
        $('#mobNumberCode').on('change', function () { $("#mobNumberCode").css('background-color', '#ffffff'); });
        $('#mobNumberCode').on('click', function () { $("#mobNumberCode").css('background-color', '#ffffff'); });

        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckFieldsChoose() {
        let result = true;
        var num = document.getElementById("mobNumberCode").value;
        if (num == null || num == "") {
            $("#mobNumberCode").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if (result) {
                $("#realsend").click();
       }
        else {
            toastr.error('Моля въведете получения код!', { timeOut: 10000 });
        }
    }
    
});