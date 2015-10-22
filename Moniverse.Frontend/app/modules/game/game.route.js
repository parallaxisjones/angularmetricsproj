(function () {
    'use strict';

    angular
        .module('app.game')
        .run(appRun);

    appRun.$inject = ['routerHelper'];
    /* @ngInject */
    function appRun(routerHelper) {
        routerHelper.configureStates(getStates());
    }

    function getStates() {
        return [
            {
                url: '/GameSessions',
                config: {
                    title: 'Game Sessions',
                    templateUrl: 'app/modules/game/game.html',
                    settings: {
                        nav: 2,
                        content: '<i class="fa fa-gamepad"></i> Game Sessions'
                    }
                }
            }
            //{
            //    state: 'admin',
            //    config: {
            //        url: '/admin',
            //        templateUrl: 'app/admin/admin.html',
            //        controller: 'AdminController',
            //        controllerAs: 'vm',
            //        title: 'Admin',
            //        settings: {
            //            nav: 2,
            //            content: '<i class="fa fa-lock"></i> Admin'
            //        }
            //    }
            //}
        ];
    }
})();