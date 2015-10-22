(function () {
    'use strict';
    var controllerId = 'profile';
    angular.module('app').controller(controllerId, ['common','datacontext', hosting]);

    function hosting(common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.title = 'User Profile';
        vm.news = {
            title: 'Playtrics Fancypants UI For ' + vm.title,
            description: 'Coming Soon'
        };


        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                });
        }

    }
})();