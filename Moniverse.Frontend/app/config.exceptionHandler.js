// Include in index.html so that app level exceptions are handled.
// Exclude from testRunner.html which should run exactly what it wants to run
function NoModuleFoundError(message) {
    this.name = "NoModuleFoundError";
    this.message = (message || "");
}
NoModuleFoundError.prototype = Error.prototype;

function ParameterError(message) {
    this.name = "ParameterError";
    this.message = (message || "");
}
ParameterError.prototype = Error.prototype;

function NoSuchChartError(message) {
    this.name = "NoModuleFoundError";
    this.message = (message || "");
}
NoSuchChartError.prototype = Error.prototype;

function NoModuleFoundError(message) {
    this.name = "NoModuleFoundError";
    this.message = (message || "");
}
NoModuleFoundError.prototype = Error.prototype;

(function () {
    'use strict';
    
    var app = angular.module('app');

    // Configure by setting an optional string value for appErrorPrefix.
    // Accessible via config.appErrorPrefix (via config value).

    app.config(['$provide', function ($provide) {
        $provide.decorator('$exceptionHandler',
            ['$delegate', 'config', 'logger', extendExceptionHandler]);
    }]);
    
    // Extend the $exceptionHandler service to also display a toast.
    function extendExceptionHandler($delegate, config, logger) {
        var appErrorPrefix = config.appErrorPrefix;
        var logError = logger.getLogFn('app', 'error');
        return function (exception, cause) {
            $delegate(exception, cause);
            if (appErrorPrefix && exception.message && exception.message.indexOf(appErrorPrefix) === 0) { return; }

            var errorData = { exception: exception, cause: cause };
            var msg = appErrorPrefix + exception.message;
            logError(msg, errorData, true);
        };
    }
})();