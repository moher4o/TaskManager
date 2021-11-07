$(() => {
    //$.getScript("../js/moment-with-locales.min.js");
    const lochref = window.location.href;
    const locOrigin = window.location.origin;
    let path;
    if (lochref.toLowerCase().indexOf("taskmanager") > 0) {
        path = locOrigin + "\\TaskManager";
    }
    else {
        path = locOrigin;
    }
    let userId = document.getElementById("employeeId").value;

    attachEvents();

    function attachEvents() {
        $('.canapprove').click(function (event) {
            let currow = $(this).closest('tr').find('.setcolor');
            let acceptDateStr = $(this).data('dateaccept');
            let year = acceptDateStr.slice(0, 4);
            let month = acceptDateStr.slice(5, 7);
            let day = acceptDateStr.slice(8);
            var datetoaccept = new Date(year, month - 1, day);

            if ($(this).prop('checked') == true) {
                 var currentUrl = path + '\\Report\\AcceptDateReport';
                let url = currentUrl + '?userId=' + userId + '&workDate=' + datetoaccept.toUTCString();
                $.get(url).done(function (data) {
                    if (data.success) {
                        currow.css('background-color', '#f3fff3');
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                });
            }
            else {
                var currentUrl = path + '\\Report\\RejectDateReport';
                let url = currentUrl + '?userId=' + userId + '&workDate=' + datetoaccept.toUTCString();
                $.get(url).done(function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        currow.css('background-color', 'ghostwhite');
                    }
                    else {
                        toastr.error(data.message);
                    }
                });

            }
        });
        $('.cannotapprove').click(function (event) {
            event.preventDefault();
        })

    }
});