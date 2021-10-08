$(() => {
    attachEvents();

    function attachEvents() {

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

});