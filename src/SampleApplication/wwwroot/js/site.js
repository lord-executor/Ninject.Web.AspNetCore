// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    const logContainer = $("#logContainer");
    window.ServiceTests = function (testCases) {
        testCases.forEach(testCase => {
            const caseResult = $("<div></div>").text(`${testCase.name}: Running...`).addClass("test-case-pending");
            logContainer.append(caseResult);
            Promise.all(testCase.run())
                .then(results => {
                    const successCount = results.filter(r => r.success).length;
                    const failedCount = results.filter(r => !r.success).length;

                    caseResult
                        .removeClass("test-case-pending")
                        .text(`${testCase.name}: Succeeded ${successCount} | Failed ${failedCount}`);

                    if (failedCount > 0) {
                        caseResult.addClass("test-case-failure");
                    } else {
                        caseResult.addClass("test-case-success");
                    }

                    console.log(`${testCase.name} results:`, results);
                }).catch(e => {
                    caseResult
                        .removeClass("test-case-pending")
                        .addClass("test-case-failure")
                        .text(`${testCase.name}: Error ${e.statusText}`);
                    console.error(e);
                });
        });
    };

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
