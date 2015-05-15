function Logging() {

    var requestHeader = loupe.clientSessionHeader();
    var agentSessionId = requestHeader.headerValue;

    $('#AgentSessionId').text(agentSessionId);
    updateSequeceNumber();

    return {
        verbose: logVerbose,
        information: logInformation,
        warning: logWarning,
        error: logError,
        critical: logCritical,
        write: logWrite,
        unhandledException: throwUnhandledException,
        ajaxCall: makeAjaxCall
    }

    function logVerbose() {
        var data = getData();
        loupe.verbose(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logInformation() {
        var data = getData();
        loupe.information(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logWarning() {
        var data = getData();
        loupe.warning(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logError() {
        var data = getData();
        loupe.error(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logCritical() {
        var data = getData();
        loupe.critical(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logWrite() {
        var data = getData();
        var severity = $('#severity').val();
        loupe.write(severity, data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function throwUnhandledException() {
        var foo = {};
        setTimeout(updateSequeceNumber, 15);
        foo.bar();
    }

    function makeAjaxCall() {
        $('#ajaxCallResult').text("");

        var header = requestHeader.headerName;
        var headerValue = requestHeader.headerValue;

        $.ajax({
            type: "GET",
            url: '/home/data',
            headers: { header: headerValue }
        }).done(function (data) {
            $('#ajaxCallResult').text("succeeded");
        }).error(function(jqXHR, textStatus) {
            $('#ajaxCallResult').text("failed:" + jqXHR.status + " " + jqXHR.statusText);
        });
    }

    function getInputVal(inputName) {
        return $(inputName).val();
    }

    function getData() {

        var parameters = null;
        var parametersValue = getInputVal('#parametersInput');
        if (parametersValue) {
            parameters = parametersValue.split(',');
        }

        var exception = null;
        var exceptionMessage = getInputVal('#exceptionMessageInput');
        if (exceptionMessage) {
            exception = new Error(exceptionMessage);
        }

        return {
            category: getInputVal("#categoryInput"),
            caption: getInputVal("#captionInput"),
            description: getInputVal("#descriptionInput"),
            parameters: parameters,
            details: getInputVal("#detailsInput"),
            exception: exception,
            methodSourceInfo: {
                file:getInputVal("#fileInput"),
                line: getInputVal("#lineInput"),
                column: getInputVal("#columnInput")
            }
        };
    }

    function updateSequeceNumber() {
        var sequenceNumber = "";
        try {
            sequenceNumber = sessionStorage.getItem("LoupeSequenceNumber") || "0";
            
        } catch (e) {
            // seems we can't get it from sessionStorage
        }

        $('#sequenceNumber').text(sequenceNumber);
    }

}
