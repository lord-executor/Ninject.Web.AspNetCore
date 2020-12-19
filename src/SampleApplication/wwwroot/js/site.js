// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    window.apiCall = function (url, data) {
        return new Promise((resolve, reject) => {
            $.ajax(url, {
                data: JSON.stringify(data),
                method: "POST",
                contentType: "application/json",
                dataType: "json",
                success: (response) => resolve(response),
                error: (jqXHR) => reject(jqXHR),
            });
        });
    }
})();
