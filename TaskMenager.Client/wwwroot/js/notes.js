$(() => {
    const loc = window.location.href;
    const path = loc.substr(0, loc.lastIndexOf('/') + 1);
    let taskId = document.getElementById('taskIdNote').textContent;
    
   
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
            SendNote(noteText);
        }
        else {
            toastr.error('Моля въведете коментар');
        }
    }

    function SendNote(noteText) {
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