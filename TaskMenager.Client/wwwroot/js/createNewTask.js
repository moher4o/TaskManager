$(() => {
    attachEvents();
    const loc = window.location.href;
    const path = loc.substr(0, loc.lastIndexOf('/') + 1);
    var childrensCount = document.getElementById("childrensCount").value;
    var taskNomer = document.getElementById("taskNomer").value;
    var taskTypeIdOriginal = document.getElementById('taskTypesId').selectedIndex;
    if (document.getElementById('taskTypesId').selectedIndex === 7) {     //ако е екипна задача от много дирекции
        GetAllEmployees(true, taskNomer);
    }
    else {
        GetAllEmployees(false, taskNomer);
    }
    
    function attachEvents() {
        $('#assignerId').on('change', function () { $("#select2-assignerId-container").css('background-color', '#ffffff'); });
        $('#taskTypesId').on('change', CheckSelectedParent);
        $('#taskTypesId').on('mousedown', GetIndex);
        $('#parentsId').on('change', CheckSelectedTaskType);
        $('#validFrom').on('change', function () { $("#validFrom").css('background-color', '#ffffff'); });
        $('#validTo').on('change', function () { $("#validTo").css('background-color', '#ffffff'); });
        $('#hourslimit').on('change', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#hourslimit').on('click', function () { $("#hourslimit").css('background-color', '#ffffff'); });
        $('#taskName').on('change', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#taskName').on('click', function () { $("#taskName").css('background-color', '#ffffff'); });
        $('#send').on('click', CheckFieldsChoose);
    }

    function GetIndex() {
        taskTypeIdOriginal = document.getElementById('taskTypesId').selectedIndex;
    }

    function CheckSelectedTaskType() {
        if (childrensCount === '0') {   //ако не е родител

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
        else {     //ако е родител
            swal("Информация", "Задачата е родител на <" + childrensCount + "> други задачи. Пренасочете ги към други Глобални задачи, преди да смените типа на тази!", "info");
            document.getElementById('taskTypesId').selectedIndex = 6;    //Индекса на "Глобална"  !!!!! (индексите почват от 0)
            $("#parentsId").val('0');    //Индекса на "Моля изберете..."  !!!!! (индексите почват от 0)
        }
    }

    function DeselectEmployees() {
        $('#Subjects_dropdown').multiselect('deselectAll', false);
        $('#Subjects_dropdown').multiselect('updateButtonText');

    }

    function CheckSelectedParent() {
       var taskTypeId = document.getElementById('taskTypesId').selectedIndex;
        if (childrensCount === '0') {  //ако не е родител

            if ((taskTypeId != 6) && $('#parentsId :selected').text() === 'Моля изберете...') {    // $('#taskTypesId :selected').text() != 'Глобална'
                swal("Информация", "Изберете глобална задача (полето \"Подзадача на:\"), преди да смените типа.", "info");
                document.getElementById('taskTypesId').selectedIndex = 6;    //Индекса на "Глобална"  !!!!! (индексите почват от 0)
            }
            else if (taskTypeId === 6 && $('#parentsId :selected').text() != 'Моля изберете...') {

                swal({
                    title: "Потвърждение",
                    text: "Задачите от тип \"Глобална\", не трябва да имат задача родител (полето \"Подзадача на:\"). Изборът на родител ще бъде нулиран и назначените експерти(ако има такива) ще бъдат премахнати. Ще бъдат изтрити отработените часове!",
                    icon: "warning",
                    closeOnEsc: false,
                    buttons: ["Отказ", "Потвърди!"],
                    dangerMode: true
                }).then((willDelete) => {
                    if (willDelete) {
                        $("#parentsId").val('0');    //Индекса на "Моля изберете..."  !!!!! (индексите почват от 0)
                        $("#parentsId").select2().trigger('change');
                    }
                    else {
                        console.log('testt');
                        document.getElementById('taskTypesId').selectedIndex = taskTypeIdOriginal;
                    }
                });

                //swal("Информация", "Задачите от тип \"Глобална\", не трябва да имат задача родител (полето \"Подзадача на:\"). Изборът на родител ще бъде нулиран и назначените експерти(ако има такива) ще бъдат премахнати", "info");
            }
            if (taskTypeId != 6) {

                if (taskTypeId === 7) {     //ако е екипна задача от много дирекции
                    GetAllEmployees(true, taskNomer);
                }
                else {
                    GetAllEmployees(false, taskNomer);
                }
                $('#Subjects_dropdown').multiselect('enable');
            }
            //else {
                //console.log('test')
                //$('#Subjects_dropdown').multiselect('disable');
                //DeselectEmployees();
           // }
        }
        else {  //ако е родител
            swal("Информация", "Задачата е родител на <" + childrensCount + "> други задачи. Пренасочете ги към други Глобални задачи, преди да смените типа на тази!", "info");
            document.getElementById('taskTypesId').selectedIndex = 6;    //Индекса на "Глобална"  !!!!! (индексите почват от 0)
            $("#parentsId").val('0');    //Индекса на "Моля изберете..."  !!!!! (индексите почват от 0)
            $("#parentsId").select2().trigger('change');

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

        if ($('#validFrom').val().length === 0) {
            $("#validFrom").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if ($('#validTo').val().length === 0) {
            $("#validTo").css('background-color', 'rgb(250, 204, 204)');
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
            text: "Не сте избрали задача родител (полето \"Подзадача на:\"). Новата задача ще бъде от тип \"Глобална задача\". Сигурни ли сте?",
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

    function GetAllEmployees(isAll, taskId) {
        //swal("Информация", "Избрания тип задача е специален. Дава възможност за сформиране на екип от експерти от различни дирекции.", "info");
        //console.log(taskId);
        if (taskId == undefined || taskId === '0') {
            taskId = null;
        }
        var url = path + "getEmployees?isAll=" + isAll + "&taskId=" + taskId;
        $.ajax({
            type: "Get",
            url: url,
            success: function (data) {
                //console.log(data.taskEmployees);
                let selectedExp = [];
                $.each($('#Subjects_dropdown option:selected'), function (i, expert) {
                    selectedExp.push(expert.value)
                });
                let selectedAssigner = [];
                $.each($('#assignerId option:selected'), function (i, expert) {
                    selectedAssigner.push(expert.value)
                });
                $('#Subjects_dropdown').empty();
                $('#assignerId').empty();
                ///////////
                var dataF = {
                    id: 0,
                    text: 'Моля изберете...'
                };
                if (selectedAssigner.includes(0)) {
                    var newOption = new Option(dataF.text, dataF.id, true, true);                           //добавям Моля изберете... 
                }
                else {
                    var newOption = new Option(dataF.text, dataF.id, false, false);
                }
                $('#assignerId').append(newOption).trigger('change');
                ///////////
                let group = null;
                $.each(data.taskEmployees, function (i, expert) {
                    
                    if (group != (expert.group == null ? null : expert.group.name)) {
                        //console.log(expert.group.name);
                        $('#Subjects_dropdown').append('<optgroup label=\'' + expert.group.name + '\'>');
                        $('#assignerId').append('<optgroup label=\'' + expert.group.name + '\'>');
                        group = expert.group.name;
                    }
                    ///////////   добавян на експертa към списъка с assigners
                    var data = {
                        id: expert.value,
                        text: expert.text
                    };
                    if (selectedAssigner.includes(expert.value)) {
                        var newOption = new Option(data.text, data.id, true, true);
                    }
                    else {
                        var newOption = new Option(data.text, data.id, false, false);
                    }
                    $('#assignerId optgroup[label =\'' + expert.group.name + '\']').append(newOption).trigger('change');

                    ///////////  добавян на експертa към списъка с изпълнители
                    if (expert.selected || selectedExp.includes(expert.value)) {
                        $('#Subjects_dropdown').append('<option value=\'' + expert.value + '\' selected="selected">' + expert.text + '</option>');
                    }
                    else {
                        $('#Subjects_dropdown').append('<option value=\'' + expert.value + '\'>' + expert.text + '</option>');
                    }

                });
                $('#Subjects_dropdown').multiselect('rebuild');
            }
        }).done(function () { //use this
            if (document.getElementById('taskTypesId').selectedIndex === 6) {
                $('#Subjects_dropdown').multiselect('disable');
                DeselectEmployees();
            }
            //for (let i = 0; i < 5; i++) {
            //    $('.blinking').fadeOut(500);
            //    $('.blinking').fadeIn(500);
            //} 

        });

    }


});