(function () { 
    'use strict';
    
    var controllerId = 'useranonshell';
    angular.module('anon').controller(controllerId,
        ['$rootScope', 'Auth', useranonshell]);

    function useranonshell($rootScope, auth) {
        var vm = this;
        vm.busyMessage = 'Please wait ...';
        vm.isBusy = false
        vm.spinnerOptions = {
            radius: 40,
            lines: 7,
            length: 0,
            width: 30,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            top: 0,
            left: 0,
            color: '#F58A00'
        };

        activate();

        function activate() {
            vm.isBusy = false;
        }

   
        $rootScope.$on('$routeChangeStart',
            function (event, next, current) {

                toggleSpinner(true);
            }
        );
        
        vm.href = window.location.href;
    };
})();