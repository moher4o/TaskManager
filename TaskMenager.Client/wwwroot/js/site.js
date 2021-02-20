$(() => {
    attachEvents();

    function attachEvents() {
        $('#showInactive').on('change', InactiveTasksShowOrHide);
    }

    function InactiveTasksShowOrHide() {
        if ($("#showInactive").prop('checked') == true) {
            $(".displayno").show();
        }
        else {
            $(".displayno").hide();
        }
    }
});