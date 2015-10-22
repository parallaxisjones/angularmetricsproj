(function () {
    'use strict';

    var app = angular.module('app');

    // Collect the routes
    app.constant('routes', getRoutes());
    
    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);

    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            $routeProvider.when(r.url, r.config);
        });

        $routeProvider.otherwise({ redirectTo: '/GameSessions' });
    }

    // Define the routes 
    function getRoutes() {
        return [
             //{
             //    url: '/',
             //    config: {
             //        templateUrl: 'app/dashboard/dashboard.html',
             //        title: 'dashboard',
             //        settings: {
             //            nav: 1,
             //            content: '<i class="fa fa-dashboard"></i> Dashboard'
             //        }
             //    }
             //}, 
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
            },
            {
                url: '/UserSessions',
                config: {
                    title: 'User Sessions',
                    templateUrl: 'app/modules/user/user.html',
                    settings: {
                        nav: 3,
                        content: '<i class="fa fa-users"></i> User Sessions'
                    }
                }
            },
            {
                url: '/HostingInstances',
                config: {
                    title: 'Hosting Instances',
                    templateUrl: 'app/modules/hosting/hosting.html',
                    settings: {
                        nav: 4,
                        content: '<i class="fa  fa-floppy-o"></i> Hosting Instances'
                    }
                }
            },
            {
                url: '/Economy',
                config: {
                    title: 'Economy',
                    templateUrl: 'app/modules/economy/economy.html',
                    settings: {
                        nav: 5,
                        content: '<i class="fa fa-money"></i> Economy'
                    }
                }
            },
            {
                url: '/inbox',
                config: {
                    title: 'Economy',
                    templateUrl: 'app/modules/admin/inbox.html',
                    settings: {
                    }
                }
            },
            {
                url: '/ice',
                config: {
                    title: 'Economy',
                    templateUrl: 'app/modules/admin/admin.html',
                }
            }
        ];
    }
})();