$(() => {
    GetDominions();

    function GetDominions() {
        var userId = document.getElementById("employeeId").value;


        //var values = ["dog", "cat", "parrot", "rabbit"];
        //for (const val of values) {
        //    $('#bosses').append($(document.createElement('option')).prop({
        //        value: val,
        //        text: val.charAt(0).toUpperCase() + val.slice(1)
        //    }))
        //}
        $.getJSON('..\\Users\\GetDominionUsers', { get_param: 'value' }, function (response) {
            console.log(response);
            if (response.data.length > 0) {
                $('#dominions')
                    .append(
                        $(document.createElement('label')).prop({
                            for: 'bigbosses'
                        }).html(`Сесия от името на: `)
                    )
                    .append(
                        $(document.createElement('select')).prop({
                            id: 'bosses',
                            name: 'bosses'
                        })
                    )
            }
            $.each(response.data, function (i, item) {
                $("#dominions").append(new Option(item.textValue, item.id));
            });

        });
        
    }

});