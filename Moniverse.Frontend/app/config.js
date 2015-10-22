(function () {
    'use strict';

    var app = angular.module('app');
    var appDataUrl = window.location.origin;

    // Configure Toastr
    toastr.options.timeOut = 4000;
    toastr.options.positionClass = 'toast-bottom-right';

    // For use with the HotTowel-Angular-Breeze add-on that uses Breeze
    var remoteServiceName = 'breeze/Breeze';

    var events = {
        controllerActivateSuccess:  'controller.activateSuccess',
        WidgetSuccess:              'widget.success',
        TimeSeriesWidgetSuccess:    'widget.success.TimeSeries',
        DataTableWidgetSuccess:     'widget.success.DataTable',
        ajaxSuccess:                'ajax.success',
        spinnerToggle:              'spinner.toggle'
    };

    var config = {
        appDataUrl: appDataUrl,
        appErrorPrefix: '[Playtrics Error] ', //Configure the exceptionHandler decorator
        docTitle: 'Playtrics: ',
        events: events,
        remoteServiceName: remoteServiceName,
        version: '2.1.0'
    };

    app.value('config', config);
    
    app.config(['$logProvider', function ($logProvider) {
        // turn debugging off/on (no info or warn)
        if ($logProvider.debugEnabled) {
            $logProvider.debugEnabled(true);
        }
    }]);
    
    //#region Configure the common services via commonConfig
    app.config(['commonConfigProvider', function (cfg) {
        cfg.config.controllerActivateSuccessEvent = config.events.controllerActivateSuccess;
        cfg.config.spinnerToggleEvent = config.events.spinnerToggle;
        cfg.config.WidgetSuccess = config.events.WidgetSuccess,
        cfg.config.TimeSeriesWidgetSuccess = config.events.TimeSeriesWidgetSuccess,
        cfg.config.DataTableWidgetSuccess = config.events.DataTableWidgetSuccess,
        cfg.config.ajaxSuccess = config.events.ajaxSuccess
    }]);
    //#endregion

    //#region Adding logging to the Broadcast event to see what is going on
    // app.config(function($provide) {
    //     $provide.decorator("$rootScope", function($delegate) {
    //         var Scope = $delegate.constructor;
    //         var broadcast = Scope.prototype.$broadcast;
    //         var emit = Scope.prototype.$emit;

    //         Scope.prototype.$broadcast = function(name, args) {
    //             var returnVal = broadcast.apply(this, arguments);
    //             var event = {
    //                 "event name": name,
    //                 "event arguments": args,
    //                 "result": returnVal
    //             };

    //             //use this to send off the events as a string to somewhere
    //             //var weirdJsonString = JSON.stringify(JSON.parse(angular.toJson(event)), null, 2);

    //             console.log("[broadcast] : %o", event);
    //             return returnVal;
    //         };
    //         Scope.prototype.$emit = function (name, args) {
    //             var returnVal = emit.apply(this, arguments);
    //             var event = {
    //                 "event name": name,
    //                 "event arguments": args,
    //                 "result": returnVal
    //             };

    //             //use this to send off the events as a string to somewhere
    //             //var weirdJsonString = JSON.stringify(JSON.parse(angular.toJson(event)), null, 2);

    //             console.log("[emit] : %o", event);
    //             return returnVal;
    //         };
    //         return $delegate;
    //     });
    // })
    //#endregion
})();