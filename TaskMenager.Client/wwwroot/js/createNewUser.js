$(() => {
    attachEvents();

    function attachEvents() {
        //$('#notactive').on('change', function () { $("#notactive").css('background-color', '#ffffff'); });
        //$('#notactive').on('click', DeactivateUser);
        $('#emailId').on('change', function () { $("#emailId").css('background-color', '#ffffff'); });
        $('#emailId').on('click', function () { $("#emailId").css('background-color', '#ffffff'); });
        $('#userNameId').on('change', function () { $("#userNameId").css('background-color', '#ffffff'); });
        $('#userNameId').on('click', function () { $("#userNameId").css('background-color', '#ffffff'); });
        $('#telNumberId').on('change', function () { $("#telNumberId").css('background-color', '#ffffff'); });
        $('#telNumberId').on('click', function () { $("#telNumberId").css('background-color', '#ffffff'); });
        $('#jobTitleId').on('change', function () { $("#jobTitleId").css('background-color', '#ffffff'); });
        $('#jobTitleId').on('click', function () { $("#jobTitleId").css('background-color', '#ffffff'); });
        $('#daeuaccauntId').on('change', function () { $("#daeuaccauntId").css('background-color', '#ffffff'); });
        $('#daeuaccauntId').on('click', function () { $("#daeuaccauntId").css('background-color', '#ffffff'); });

        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckFieldsChoose() {
        let result = true;

        var num = document.getElementById("telNumberId").value;
        if (num != null) {
            if (!(/^\d+$/.test(num))) {
                $("#telNumberId").css('background-color', 'rgb(250, 204, 204)');
                result = false;
            }
        }
        else {
            $("#telNumberId").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }
        
        let ac = document.getElementById("daeuaccauntId").value;
        console.log(ac)
        if (ac == null || ac == "") {
            $("#daeuaccauntId").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }


        let un = document.getElementById("userNameId").value;
        if (un == null || un == "") {
            $("#userNameId").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        let em = document.getElementById("emailId").value;
        if (em == null || em == "") {
            $("#emailId").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if ($('#jobTitleId :selected').text() === 'Моля изберете...') {
            $("#jobTitleId").css('background-color', 'rgb(250, 204, 204)');
            result = false;
            $(':focus').blur();
        }

        if (result) {
            if ($('#directorateId :selected').text() === 'Моля изберете...') {
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
            text: "Не сте избрали дирекция. Само ограничен кръг потребители не са назначени в дирекция. Сигурни ли сте?",
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