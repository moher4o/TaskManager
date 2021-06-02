$(() => {
    const delayInMilliseconds = 200;
    const lochref = window.location.href;
    const locOrigin = window.location.origin;
    let path;
    if (lochref.toLowerCase().indexOf("taskmanager") >= 0) {
        path = locOrigin + "\\TaskManager";
    }
    else {
        path = locOrigin;
    }
    var currentUserId = document.getElementById("currentUserId").value;
    var userId = document.getElementById("employeeId") == null ? currentUserId : document.getElementById("employeeId").value;
    var slideSource = document.getElementById('AREA_PARTIAL_VIEW');

    AddDatePicker();
    GetDominions();
    //attachEvents();

    function attachEvents() {
    }

    function CheckSelectedDominion() {
        var url = window.location.href;
        let newDate = $('#dateSelector2').datepicker("getDate");
        if (url.indexOf('?') > -1) {
            url = url.substring(0, url.indexOf('?')) + '?userId=' + $('#bosses :selected').val() + '&workDate=' + newDate.toUTCString();
        } else {
            url += '?userId=' + $('#bosses :selected').val() + '&workDate=' + newDate.toUTCString();
        }
        //console.log($('#bosses :selected').val());
        // window.location.href = url;
    }

    function GetDominions() {
        var url = path + "\\Users\\GetDominionUsers";
        $.getJSON(url, { get_param: 'value' }, function (response) {
            if (response.data.length > 0) {
                $('#dominions')
                    .append(
                        $(document.createElement('label')).prop({
                            for: 'bigbosses'
                        }).html(`Сесия от името на:&nbsp&nbsp`)
                    )
                    .append(
                        $(document.createElement('select')).prop({
                            id: 'bosses',
                            name: 'bosses'
                        })
                    )
                $('#bosses').change(function () {
                    slideSource.classList.toggle('fade');
                    setTimeout(function () {
                        GetUserTaskForDate();
                    }, delayInMilliseconds);
                });
            }
            $.each(response.data, function (i, item) {
                if (userId == item.id) {
                    $("#bosses").append(new Option(item.textValue, item.id, true, true));
                }
                else {
                    $("#bosses").append(new Option(item.textValue, item.id));
                }
            });
        });
    }

    function AddDatePicker() {
        //var selectedText = document.getElementById("workDate").value
        var selectedText = document.getElementById("workDate") == null ? new Date().toDateString() : document.getElementById("workDate").value;
        //console.log(selectedText);
        $('#dateSelector2').datepicker({ dateFormat: 'dd-M-yy', changeYear: true, showOtherMonths: true, firstDay: 1, maxDate: "+0d", inline: true });
        $('#dateSelector2').datepicker('setDate', new Date(selectedText));
        //$('#dateSelector2').datepicker("refresh");
        //let date2 = $('#dateSelector2').datepicker("getDate");
        //console.log(date2);
        $('#dateSelector2').datepicker({
            onSelect: function (d, i) {
                if (d !== i.lastVal) {
                    $(this).change();
                }
            }
        });
        $('#dateSelector2').change(function () {
            slideSource.classList.toggle('fade');
            setTimeout(function () {
                GetUserTaskForDate();
            }, delayInMilliseconds);

        });
    }

    function GetUserTaskForDate() {
        var currentUrl = window.location.href;
        let newDate = $('#dateSelector2').datepicker("getDate");
        let bossUserId = $('#bosses :selected') == null ? currentUserId : $('#bosses :selected').val();

        $.ajax({
            type: 'GET',
            url: '..\\Home\\GetDateTasks',
            data: {
                userId: bossUserId,
                workDate: newDate.toUTCString()
            },
            success: function (result) {
                $('#AREA_PARTIAL_VIEW').html("");
                $('#AREA_PARTIAL_VIEW').html(result);
                slideSource.classList.toggle('fade');
            }
        });
    }

    $(function () { //jQuery shortcut for .ready (ensures DOM ready)
        GetUserTaskForDate();
        setTimeout(function () {
            $('.PrimeBox3').click(function () {
                var serviceID = this.id;
                $('.PrimeBox3').css('background-color', '#fff');
                $(this).css('background-color', '#cadefd');
            });
            $('#showInactive').click(function () {
                document.getElementById('showclosedTasks').classList.toggle('displayno');
                //$('#showclosedTasks').toggleClass('displayno');
            });
        }, 1000);
        //GetUserTaskForDate();
    });

});