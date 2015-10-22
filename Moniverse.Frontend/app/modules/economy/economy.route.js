(function () {
    'use strict';

    angular
        .module('economy')
        .run(appRun);

    appRun.$inject = ['routerHelper'];
    /* @ngInject */
    function appRun(routerHelper) {
        routerHelper.configureStates(getStates());
    }

    function getStates() {
        return [

            {
                state: 'Economy',
                config: {
                    url: '/Economy',
                    title: 'Economy',
                    templateUrl: 'app/modules/economy/economy.html',
                    settings: {
                        nav: 5,
                        content: '<i class="fa fa-money"></i> Economy'
                    }
                }
            }

            //{
            //    url: '/Economy',
            //    config: {
            //        title: 'Economy',
            //        templateUrl: 'app/modules/economy/economy.html',
            //        settings: {
            //            nav: 5,
            //            content: '<i class="fa fa-money"></i> Economy'
            //        }
            //    }
            //}

            //{
            //    state: 'dashboard',
            //    config: {
            //        url: '/',
            //        templateUrl: 'app/dashboard/dashboard.html',
            //        controller: 'DashboardController',
            //        controllerAs: 'vm',
            //        title: 'dashboard',
            //        settings: {
            //            nav: 1,
            //            content: '<i class="fa fa-dashboard"></i> Dashboard'
            //        }
            //    }
            //}
        ];
    }
})();