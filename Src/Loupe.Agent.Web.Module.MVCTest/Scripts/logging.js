function Logging() {

    return {
        verbose: logVerbose,
        information: logInformation,
        warning: logWarning,
        error: logError,
        critical: logCritical,
        write: logWrite
    }

    function logVerbose() {
        var data = getData();
        loupe.verbose(data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
    }

    function logInformation() {
        var data = getData();
        loupe.information(data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
    }

    function logWarning() {
        var data = getData();
        loupe.warning(data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
    }

    function logError() {
        var data = getData();
        loupe.error(data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
    }

    function logCritical() {
        var data = getData();
        loupe.critical(data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
    }

    function logWrite() {
        var data = getData();
        var severity = $('#severity').val();
        loupe.write(severity, data.category, data.caption, data.description, data.parameters, null, data.details, data.methodSourceInfo);
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

        return {
            category: getInputVal("#categoryInput"),
            caption: getInputVal("#captionInput"),
            description: getInputVal("#descriptionInput"),
            parameters: parameters,
            details: getInputVal("#detailsInput"),
            methodSourceInfo: {
                file:getInputVal("#fileInput"),
                line: getInputVal("#lineInput"),
                column: getInputVal("#columnInput")
            }
        };
    }


}