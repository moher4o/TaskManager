$(() => {
    attachEvents();

    function attachEvents() {
        $('#assignerId').on('change', function () { $("#select2-assignerId-container").css('background-color', '#ffffff'); });
        $('#taskTypesId').on('change', CheckSelectedParent);
        $('#parentsId').on('change', CheckSelectedTaskType);
        $('#hourslimit').on('change', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#hourslimit').on('click', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#taskName').on('change', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#taskName').on('click', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckSelectedTaskType() {
        if ($('#parentsId :selected').text() != 'Моля изберете...' && document.getElementById('taskTypesId').selectedIndex === 6) {
            document.getElementById('taskTypesId').selectedIndex = 4;    //Индекса на "Специфични задачи"  !!!!! (индексите почват от 0)    --> $('#taskTypesId :selected').text() === 'Глобална'

        }
        else if ($('#parentsId :selected').text() === 'Моля изберете...' && document.getElementById('taskTypesId').selectedIndex != 6) {
            document.getElementById('taskTypesId').selectedIndex = 6;    //Индекса на "Глобална"  !!!!! (индексите почват от 0)
        }
        if ($('#parentsId :selected').text() != 'Моля изберете...') {
            $('#Subjects_dropdown').multiselect('enable');
        }
        else {
            $('#Subjects_dropdown').multiselect('disable');
            DeselectEmployees();
        }
    }

    function DeselectEmployees() {
        $('#Subjects_dropdown').multiselect('deselectAll', false);
        $('#Subjects_dropdown').multiselect('updateButtonText');

    }

    function CheckSelectedParent() {
        if (document.getElementById('taskTypesId').selectedIndex != 6 && $('#parentsId :selected').text() === 'Моля изберете...') {    // $('#taskTypesId :selected').text() != 'Глобална'
            swal("Информация", "Изберете глобална задача(родител), преди да смените типа.", "info");
            document.getElementById('taskTypesId').selectedIndex = 6;    //Индекса на "Глобална"  !!!!! (индексите почват от 0)
        }
        else if (document.getElementById('taskTypesId').selectedIndex === 6 && $('#parentsId :selected').text() != 'Моля изберете...') {
                swal("Информация", "Задачите от тип \"Глобална\", не трябва да имат задача родител. Изборът на родител ще бъде нулиран и назначените експерти(ако има такива) ще бъдат премахнати", "info");
            $("#parentsId").val('0');    //Индекса на "Моля изберете..."  !!!!! (индексите почват от 0)
            $("#parentsId").select2().trigger('change');
        }
        if (document.getElementById('taskTypesId').selectedIndex != 6) {
            $('#Subjects_dropdown').multiselect('enable');
        }
        else {
            $('#Subjects_dropdown').multiselect('disable');
            DeselectEmployees();
        }

    }

    function CheckFieldsChoose() {
        let result = true;
        let errormessage = "";
        
        if ($('#select2-assignerId-container').text() === 'Моля изберете...') {
            $("#select2-assignerId-container").css('background-color', 'rgb(250, 204, 204)');
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
            if ($('#parentsId :selected').text() === 'Моля изберете...') {
               
                ParentCheck();
                $(':focus').blur();
            }
            else {
                
                $("#realsend").click();
            }
            
        }
        else {
            toastr.error('Моля попълнете всички задължителни полета', { timeOut: 10000 });
        }
    }

    function ParentCheck() {
        swal({
            title: "Потвърждение",
            text: "Не сте избрали задача родител. Новата задача ще бъде от тип \"Глобална задача\". Сигурни ли сте?",
            icon: "warning",
            closeOnEsc: false,
            buttons: ["Отказ", "Запис!"],
            dangerMode: true
        }).then((willWrite) => {
            if (willWrite) {
                $("#realsend").click();
            }
        });
    }

});