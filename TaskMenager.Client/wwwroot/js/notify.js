﻿let notify = (() => {

    $(document).on({
        ajaxStart: () => $("#loadingBox").show(),
        ajaxStop: () => $('#loadingBox').fadeOut()
    });

    function showInfo(message) {
        let infoBox = $('#loadingBox');
        infoBox.find('span').text(message);
        infoBox.fadeIn();
        setTimeout(() => infoBox.fadeOut(), 3000);
    }

    function showError(message) {
        let errorBox = $('#errorBox');
        errorBox.find('span').text(message);
        errorBox.fadeIn();
        setTimeout(() => errorBox.fadeOut(), 8000);
    }
    function showSuccess(message) {
        let successBox = $('#successBox');
        successBox.find('span').text(message);
        successBox.fadeIn();
        setTimeout(() => successBox.fadeOut(), 8000);
    }

    return {
        showInfo,
        showError,
        showSuccess
    };
})();