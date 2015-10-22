(function () {
    'use strict';

    var app = angular.module('anon');



    app.directive('ccScrollToTop', ['$window',
        // Usage:
        // <span data-cc-scroll-to-top></span>
        // Creates:
        // <span data-cc-scroll-to-top="" class="totop">
        //      <a href="#"><i class="fa fa-chevron-up"></i></a>
        // </span>
        function ($window) {
            var directive = {
                link: link,
                template: '<a href="#"><i class="fa fa-chevron-up"></i></a>',
                restrict: 'A'
            };
            return directive;

            function link(scope, element, attrs) {
                var $win = $($window);
                element.addClass('totop');
                $win.scroll(toggleIcon);

                element.find('a').click(function (e) {
                    e.preventDefault();
                    // Learning Point: $anchorScroll works, but no animation
                    //$anchorScroll();
                    $('body').animate({ scrollTop: 0 }, 500);
                });

                function toggleIcon() {
                    $win.scrollTop() > 300 ? element.slideDown() : element.slideUp();
                }
            }
        }
    ]);

    app.directive('ccSpinner', ['$window', function ($window) {
        // Description:
        //  Creates a new Spinner and sets its options
        // Usage:
        //  <div data-cc-spinner="vm.spinnerOptions"></div>
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            scope.spinner = null;
            scope.$watch(attrs.ccSpinner, function (options) {
                if (scope.spinner) {
                    scope.spinner.stop();
                }
                scope.spinner = new $window.Spinner(options);
                scope.spinner.spin(element[0]);
            }, true);
        }
    }]);
    app.directive('pvNavLogin', ['Auth', function (auth) {

        var uiToDisplay = (auth.isLoggedIn()) ? {
            restrict: 'A',
            template: "<a href='#' class='user-profile-link'>" + auth.isLoggedIn() + "</a>",
            link: function (scope, iElement, iAttrs) {

                function onLoginSubmit(e) {
                    e.preventDefault();

                    $.ajax({
                        type: "POST",
                        url: "/Account/LogOff",
                        success: function (response) {
                            if (response.status === "success") {
                                window.localStorage.clear();
                                location.href = "/";
                            }
                        },
                        error: function () {
                            var errorBar = document.createElement('div');
                            errorBar.className = "error-bar";
                            errorBar.style.background = "#000";
                            errorBar.style.color = "#fff";

                            $('.navbar-collapse').append(errorBar);
                        }
                    });
                }
                var form = $(iElement[0]).find('a')[0];
                form.addEventListener('click', onLoginSubmit);
            }
        } :
            {
            restrict: 'A',
            templateUrl: '/app_anon/layout/navlogin.html',
            link: function (scope, iElement, iAttrs) {
                scope.vm.busy = false;
                var background = window.getComputedStyle($("#cover-image")[0]).getPropertyValue("background");
                

                function onLoginSubmit(e) {
                    var spinner = new Spinner(scope.vm.spinnerOptions);
                    scope.vm.isBusy = true;
                    e.preventDefault();
                    var fields = $(this);
                    $(fields[0]).find('.form-control').each(function (idx, el) {
                        if (isEmptyOrSpaces($(el).val())) {
                            displayError("you wot mate?  must input both username and password.");
                            scope.vm.isBusy = false;
                            return;
                        } else {
                            
                            $("#cover-image").css("background", "white");
                            spinner.spin(document.getElementById('cover-image'));
                        }
                    });
                    

                    $.ajax({
                        type: "POST",
                        url: "/Account/Login",
                        data: fields.serialize(),
                        success: function (response) {
                            if (response.isAuthenticated) {
                                auth.setUser(response);
                                location.href = "/";
                            }
                        },
                        error: function (error) {
                            scope.vm.isBusy = false;
                            displayError("there was a problem with your username/password.  Please try again.");
                            spinner.stop();
                            $("#cover-image").css("background", background);
                        }
                    });
                }
                var form = $(iElement[0]).find('form')[0];
                form.addEventListener('submit', onLoginSubmit);
            }
        }

        return uiToDisplay;
    }])
    function isEmptyOrSpaces(str) {
        return str === null || str.match(/^ *$/) !== null;
    }

    function displayError(message, timeout) {
        
        var errorClass = "error-bar";
        if ($("." + errorClass).length > 0) {
            return;
        }

        var time = (timeout) ? timeout : 3000;
        var errorBar = document.createElement('div');
        var clearfix = document.createElement('div');
        clearfix.className = "clearfix";

        var pError = document.createElement('p');
        pError.style.display = "block";
        pError.style.textAlign = "right";
        pError.style.position = "relative";
        pError.style.right = "0";
        pError.style.margin = "0 auto !important";

        $(pError).text(message);
        $(errorBar).html(pError);

        errorBar.className = errorClass;
        errorBar.style.background = "#000";
        errorBar.style.color = "#fff";
        errorBar.style.height = "25px";
        errorBar.style.width = "100%;";
        errorBar.style.position = "relative";
        errorBar.style.top = "50px";

        $('.navbar-collapse').append(errorBar);

        setTimeout(function () {
            $(errorBar).remove();
        }, time)
    }
})(); 