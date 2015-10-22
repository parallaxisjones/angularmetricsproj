//cb function is a function that executes after the ajax call has completed
(function () {
    'use strict';
    
    var app = angular.module('anon', [
        // Angular modules 
        'ngAnimate',        // animations
        'ngRoute',          // routing
        'ngSanitize',       // sanitizes html bindings (ex: sidebar.js)
        'ngCookies',
        // 3rd Party Modules
        'ui.bootstrap',      // ui-bootstrap (ex: carousel, pagination, dialog),
        'ui.bootstrap.modal'
    ]);
    // Collect the routes
    app.constant('routes', getRoutes());

    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    
    app.factory('Auth', function () {
        var user;

        return {
            setUser: function (aUser) {
                user = aUser;
                window.localStorage = {};
                window.localStorage[user.localStorageID] = JSON.stringify(user);
            },
            isLoggedIn: function () {
                return (user && window.localStorage[user.localStorageID]) ? JSON.parse(window.localStorage[user.localStorageID]) : false;
            }
        }
    });



    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            $routeProvider.when(r.url, r.config);
        });

        $routeProvider.otherwise({ redirectTo: '/' });
    }

    // Define the routes 
    function getRoutes() {
        return [
             {
                 url: '/',
                 config: {
                     template: '<span>hello world!</span>',
                     title: 'dish'
                 }
             }]
    }
})();
 