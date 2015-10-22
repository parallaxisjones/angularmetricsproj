(function () {
    'use strict';
    var controllerId = 'hosting';
    angular.module('app').controller(controllerId, ['common','datacontext', hosting]);

    function hosting(common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.news = {
            title: 'Playtrics Fancypants UI For Hosting Instance Metrics',
            description: 'Coming Soon'
        };
        vm.title = 'Hosting Instances';

        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                });
        }

    }
})();