(function () {
    'use strict';
    var $q;
    var serviceId = 'Notifications';
    angular.module('app').factory(serviceId, ['common', 'config', notifications]);

    function Notification(responseObj) {
        responseObj.CreatedAt = moment.utc(parseCSharpDateTime(responseObj.CreatedAt)).format('MM/DD/YYYY HH:mm:ss');
        $.extend(this, responseObj);
    }

    function notifications(common, config) {
        // console.log("app config %o", config);
        var serviceEndpointUrl = config.appDataUrl;
        $q = common.$q;
        var getLogFn = common.logger.getLogFn;
        var errorLog = getLogFn(serviceId, 'error');

        var service = {
            get: GetMessages
        };

        return service;

        function GetMessages(Request) {

            var Module = 'Notification';
            var Method = 'GetNotifications';
            var GetNotes = new common.Request(Module, Method);

            GetNotes.process(Request);

        }



        }

})();