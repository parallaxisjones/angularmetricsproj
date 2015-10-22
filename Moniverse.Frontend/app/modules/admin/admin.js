(function () {
    'use strict';
    var controllerId = 'admin';
    angular.module('app').controller(controllerId, ['common','datacontext', admin]);

    function admin(common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.news = {
            title: 'Playtrics Fancypants UI',
            description: 'New Fancypants UI for the masses.'
        };
        vm.messageCount = 0;
        vm.people = [];
        vm.title = 'User Data';

        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                    log("activate fired");
                });
        }

        function getMessageCount() {
            // console.log("getMessageCount");
            return datacontext.getMessageCount().then(function (data) {
                return vm.messageCount = data;
            });
        }

        function getPeople() {
            return datacontext.getPeople().then(function (data) {
                return vm.people = data;
            });
        }
    }
})();