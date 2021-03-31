$(() => {
    const loc = window.location.href;
    const path = loc.substr(0, loc.lastIndexOf('/') + 1);
    
    attachEvents();

    function attachEvents() {
        $('#comment').on('change', function () { $("#comment").css('background-color', '#ffffff'); });
        $('#comment').on('click', function () { $("#comment").css('background-color', '#ffffff'); });

        $('#send').on('click', CheckFieldsChoose);
    }

    function CheckFieldsChoose() {
        let result = true;

        let noteText = document.getElementById("comment").value;
        if (noteText == null || noteText == "") {
            $("#comment").css('background-color', 'rgb(250, 204, 204)');
            result = false;
        }

        if (result) {
            let taskId = document.getElementById('taskIdNote').textContent;;
            SendNote(noteText, taskId);
        }
        else {
            toastr.error('Моля въведете коментар', { timeOut: 10000 });
        }
    }

    function SendNote(noteText, taskId) {
        console.log(taskId);
        console.log(noteText);
        var url = path + "addNote?text=" + noteText + "&taskId=" + taskId;
        $.ajax({
            type: "Get",
            url: url,
            success: function (data) {
                if (data.success) {
                    $(document).ajaxStop(function () { location.reload(true); });
                    toastr.success(data.message);
                }
                else {
                    //window.location.reload();
                    toastr.error(data.message);
                }
            }
        });
    }

});