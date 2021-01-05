$(() => {
    attachEvents();

    function attachEvents() {
        $('#btnHelp').on('click', ShowHelp);
        $('#send').on('click', CheckFieldsChoose);
    }

    function ShowHelp() {
        alert("Click");
        $('#helpImage').show();
    }

    function CheckFieldsChoose() {
        let result = true;

        if ($('#fileName').text() === 'Не е избран файл с елементи') {
            notify.showError('Моля изберете файл');
            result = false;
        }

        if (result) {
            $("#realsend").click();
        }
    }

});