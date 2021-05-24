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
    GetDominions();
    attachEvents();

    function attachEvents() {
    }

    function CheckSelectedDominion() {
        var url = window.location.href;
        if (url.indexOf('?') > -1) {
            url = url.substring(0, url.indexOf('?')) + '?userId=' + $('#bosses :selected').val();
        } else {
            url += '?userId=' + $('#bosses :selected').val();
        }
        
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
                //var img = $('<img class="chatnotifications" style="padding-left:3px; cursor: pointer;" id="domInfo">'); //Equivalent: $(document.createElement('img'))
                //img.attr('src', '../png/info2.png');
                //img.appendTo('#dominions');
                $('#bosses').on('change', CheckSelectedDominion);
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

});