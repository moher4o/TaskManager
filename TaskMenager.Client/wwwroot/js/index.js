$(() => {
    $("#notificationsCountValue").show();
    //$(function () {
    //    $("#chatnotifications").click(function () {
    //        alert($(this).attr("data-ispoen"));
    //        var isopen = $(this).attr("data-ispoen");
    //        if (isopen == "false") {
    //            $(this).attr("data-ispoen", "true");
    //            $("#notificationsCountValue").show();
    //            $("#notificationsContent").show();
    //        } else {
    //            $(this).attr("data-ispoen", "false");

    //            $("#notificationsCountValue").hide();
    //            $("#notificationsContent").hide();
    //        }
    //    })

    //    // Declare a proxy to reference the hub.
    //    var chat = $.connection.chatHub;
    //    // Create a function that the hub can call to broadcast messages.
    //    chat.client.broadcastMessage = function (name, message) {
    //        // Html encode display name and message.
    //        var encodedName = $('<div />').text(name).html();
    //        var encodedMsg = $('<div />').text(message).html();
    //        // Add the message to the page.
    //        $('#discussion').append('<li><strong>' + encodedName
    //            + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');


    //        //append to notification

    //        var count = parseInt($("#notificationsCountValue").text());
    //        count++;
    //        $("#notificationsCountValue").text(count);
    //        $("#notificationsCountValue").show();

    //        //you can append notification content dynamically based on your requirement
    //    };

    //    // Get the user name and store it to prepend to messages.
    //    $('#displayname').val(prompt('Enter your name:', ''));
    //    // Set initial focus to message input box.
    //    $('#message').focus();
    //    // Start the connection.
    //    $.connection.hub.start().done(function () {
    //        $('#sendmessage').click(function () {
    //            // Call the Send method on the hub.
    //            chat.server.send($('#displayname').val(), $('#message').val());
    //            // Clear text box and reset focus for next comment.
    //            $('#message').val('').focus();

    //        });
    //    });
    //});
});