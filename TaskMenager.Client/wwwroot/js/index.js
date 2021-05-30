$(() => {
    const lochref = window.location.href;
    const locOrigin = window.location.origin;
    let path;
    if (lochref.toLowerCase().indexOf("taskmanager") >= 0) {
        path = locOrigin + "\\TaskManager";
    }
    else {
        path = locOrigin;
    }
    var userId = document.getElementById("employeeId").value;
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
        console.log(newDate);
        window.location.href = url;

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
                //$('#bosses').on('change', CheckSelectedDominion());
                $('#bosses').change(function () {
                    CheckSelectedDominion();
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
        var selectedText = document.getElementById("workDate").value;
        //var selectedDate = new Date(selectedText);

        $('#dateSelector2').datepicker({ dateFormat: 'dd-M-yy', changeYear: true, showOtherMonths: true, firstDay: 1, maxDate: "+0d" });
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
                CheckSelectedDominion();
         });
    }
});