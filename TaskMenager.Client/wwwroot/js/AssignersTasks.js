$(() => {
    const lochref = window.location.href;
    const locOrigin = window.location.origin;
    let path;
    if (lochref.toLowerCase().indexOf("taskmanager") > 0) {
        path = locOrigin + "\\TaskManager";
    }
    else {
        path = locOrigin;
    }

    attachEvents();

    function attachEvents() {
        $(".downarrow").each(function () {
            var downarrow = this;
            downarrow.addEventListener("click", ShowChildren);
        });
        $(".uparrow").each(function () {
            var uparrow = this;
            uparrow.addEventListener("click", HideChildren);
        });

        //$(".downarrow").on('click', ShowChildren);
        //$('.uparrow').on('click', HideChildren);

        let placeholderElement = $('#modal-placeholder');
        $('a[data-toggle="ajax-modal"]').click(function (event) {
            let url = $(this).data('url') + '?taskId=' + $(this).data('taskid') + '&taskName=' + $(this).data('taskname');
            $.get(url).done(function (data) {
                placeholderElement.html(data);
                placeholderElement.find('.modal').modal('show');
            });
        });

        placeholderElement.on('click', '[data-save="modal"]', function (event) {
            event.preventDefault();

            let form = $(this).parents('.modal').find('form');
            let actionUrl = form.attr('action');
            let dataToSend = form.serialize();
            
            $.post(actionUrl, dataToSend).done(function (data) {
                let newBody = $('.modal-body', data);
                placeholderElement.find('.modal-body').replaceWith(newBody);
                let isValid = newBody.find('[name="IsValid"]').val() === 'True';
                
                if (isValid) {
                    placeholderElement.find('.modal').modal('hide');
                    location.reload();
                }
                
            });
        });
    }

    function ShowChildren() {
        $(this).hide();
         let uparrow = $(this).closest(".PrimeBox3").find(".uparrow").first();
        uparrow.show();
        let nextDivClass = $(this).closest(".PrimeBox3").find(".taskSubList").first();
        nextDivClass.empty();
        $p3 = $('<p3 style=\"font-weight:700; font-size:larger;\">').text('Подзадачи:')
        $p3.appendTo(nextDivClass);
        $head = $('<thead>').append(
            $('<tr>').append(
                $('<th>').text('N:'),
                $('<th style=\"max-width:650px; padding-left:7px;\">').text('Задача'),
                $('<th style=\"min-width:80px; padding-left:7px;\">').text('Изтрита?'),
                $('<th style=\"min-width:100px; padding-left:7px;\">').text('Статус'),
                $('<th style=\"min-width:150px; padding-left:7px;\">').text('Тип')
            ));
        $table = $('<table class=\"table-bordered\">');
        $head.appendTo($table).appendTo(nextDivClass);

        let url = $(this).data('url') + '?taskId=' + $(this).data('taskid');
        $.get(url).done(function (data) {
            //console.log(data.data);
            $.each(data.data, function (i, item) {
                let name = (item.taskName.length > 110 ? item.taskName.substring(0, 107) + "..." : item.taskName);
                $taskLink = $(`<a href=\"${path + "\\Tasks\\TaskDetails?taskId=" + item.id}\" style=\"${item.isDeleted ? "color:red" : "color:#5f9fec;"}\" target="_blank">`).text(name);
                let $tr = $(`<tr>`).append(
                    $('<td>').text(item.id),
                    //$(`<td style=\"min-width:750px; max-width:750px; padding-left:7px; cursor:pointer;\">`).text(name),
                    $(`<td style=\"min-width:750px; max-width:750px; padding-left:7px; cursor:pointer;\">`).append($taskLink),
                    $(`<td style=\"min-width:80px; padding-left:7px; ${item.isDeleted ? "color:red" : "color:black;"}\">`).text(!item.isDeleted ? "Не" : "Да"),
                    $('<td style=\"min-width:100px; padding-left:7px;\">').text(item.taskStatusName),
                    $('<td style=\"min-width:150px; padding-left:7px;\">').text(item.taskTypeName)
                );
                $tr.appendTo(nextDivClass);
            });
        });

        nextDivClass.toggle();
    }

    function HideChildren() {
        $(this).hide();
        var downarrow = $(this).closest(".PrimeBox3").find(".downarrow").first();
        downarrow.show();
        var nextDivClass = $(this).closest(".PrimeBox3").find(".taskSubList").first();
        nextDivClass.empty();
        nextDivClass.toggle();
    }
});