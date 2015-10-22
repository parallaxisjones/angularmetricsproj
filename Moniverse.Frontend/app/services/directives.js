var Day = function (date, newUsers, logins, percentArray) {
    var formatDate = function (datestring) {
        var splitDate = datestring.split(" ");
        return splitDate[0];
    }
    var formatPercents = function (percentArray) {
        // console.log(percentArray);
        var l = percentArray.length;
        for (var i = 0; i <= l; i++) {
            percentArray[i] = Math.floor(percentArray[i]);
            if (percentArray[i] == -1) {
                percentArray[i] = '-';
            } else if (percentArray[i] == 0) {
                percentArray[i] = "N/A"
            }
            else {
                percentArray[i] = percentArray[i] + '%';
            }
        }
        //this is a big WTF fix this later
        percentArray.pop();
        return percentArray;
    }
    var ChartProcessedEvent = new CustomEvent(
        "RetentionProcessed",
        {
            detail: {
                message: "Retention Chart Finished",
                time: new Date(),
            },
            bubbles: true,
            cancelable: true
        }
    );

    this.date = formatDate(date); //(typeof date === "undefined") ? new Date() : formatDate(date);
    this.newUsers = (typeof newUsers === "undefined") ? 0 : newUsers;
    this.logins = (typeof logins === "undefined") ? 0 : logins;
    this.twoWeeks = (typeof percentArray === "undefined") ? [] : formatPercents(percentArray);
};

var getDays = function (res) {
    var DayArray = [];
    $.each(res, function (idx) {
        if (!this.date || this.date == "undefined") {

        } else {

        }
        var d = new Day(this.date, this.installsOnThisDay, this.loginsOnThisDay, this.days);

        DayArray.push(d);
    });

    return DayArray;
};

(function () {
    'use strict';

    var app = angular.module('app');

    app.directive('ccImgPerson', ['config', function (config) {
        //Usage:
        //<img data-cc-img-person="{{s.speaker.imageSource}}"/>
        var basePath = config.imageSettings.imageBasePath;
        var unknownImage = config.imageSettings.unknownPersonImageSource;
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$observe('ccImgPerson', function (value) {
                value = basePath + (value || unknownImage);
                attrs.$set('src', value);
            });
        }
    }]);


    app.directive('ccSidebar', function () {
        // Opens and clsoes the sidebar menu.
        // Usage:
        //  <div data-cc-sidebar>
        // Creates:
        //  <div data-cc-sidebar class="sidebar">
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            var $sidebarInner = element.find('.sidebar-inner');
            var $dropdownElement = element.find('.sidebar-dropdown a');
            element.addClass('sidebar');
            $dropdownElement.click(dropdown);

            function dropdown(e) {
                var dropClass = 'dropy';
                e.preventDefault();
                if (!$dropdownElement.hasClass(dropClass)) {
                    hideAllSidebars();
                    $sidebarInner.slideDown(350);
                    $dropdownElement.addClass(dropClass);
                } else if ($dropdownElement.hasClass(dropClass)) {
                    $dropdownElement.removeClass(dropClass);
                    $sidebarInner.slideUp(350);
                }

                function hideAllSidebars() {
                    $sidebarInner.slideUp(350);
                    $('.sidebar-dropdown a').removeClass(dropClass);
                }
            }
        }
    });


    app.directive('ccWidgetClose', function () {
        // Usage:
        // <a data-cc-widget-close></a>
        // Creates:
        // <a data-cc-widget-close="" href="#" class="wclose">
        //     <i class="fa fa-remove"></i>
        // </a>
        var directive = {
            link: link,
            template: '<i class="fa fa-remove"></i>',
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set('href', '#');
            attrs.$set('wclose');
            element.click(close);

            function close(e) {
                e.preventDefault();
                element.parent().parent().parent().hide(100);
            }
        }
    });

    app.directive('ccWidgetMinimize', function () {
        // Usage:
        // <a data-cc-widget-minimize></a>
        // Creates:
        // <a data-cc-widget-minimize="" href="#"><i class="fa fa-chevron-up"></i></a>
        var directive = {
            link: link,
            template: '<i class="fa fa-chevron-up"></i>',
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            //$('body').on('click', '.widget .wminimize', minimize);
            attrs.$set('href', '#');
            attrs.$set('wminimize');
            element.click(minimize);

            function minimize(e) {
                e.preventDefault();
                var $wcontent = element.parent().parent().next('.widget-content');
                var iElement = element.children('i');
                if ($wcontent.is(':visible')) {
                    iElement.removeClass('fa fa-chevron-up');
                    iElement.addClass('fa fa-chevron-down');
                } else {
                    iElement.removeClass('fa fa-chevron-down');
                    iElement.addClass('fa fa-chevron-up');
                }
                $wcontent.toggle(500);
            }
        }
    });

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

    app.directive('ccWidgetHeader', function () {
        //Usage:
        //<div data-cc-widget-header title="vm.map.title"></div>
        var directive = {
            link: link,
            scope: {
                'title': '@',
                'subtitle': '@',
                'rightText': '@',
                'allowCollapse': '@'
            },
            templateUrl: 'app/layout/widgetheader.html',
            restrict: 'A',
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set('class', 'widget-head');
        }
    });

    app.directive('ccAddDash', function () {
        //Usage:
        //<div data-cc-widget-header title="vm.map.title"></div>
        var directive = {
            link: link,
            scope: false,
            template: '<i class="fa fa-plus"></i>',
            restrict: 'A',
        };
        return directive;

        function link(scope, element, attrs) {
            console.log(element[0]);
            var plus = element[0];
            plus.addEventListener('click', function (e) {
                var options = (scope.ts) ? scope.ts.modelOptions : scope.dt.modelOptions;
                console.log(scope);
                var widget = {
                    dataModelOptions: {},
                    name: {},
                    size: {},
                    style: {},
                    title: "widget"
                }
            })
            attrs.$set('class', 'hideme');
            attrs.$set('class', 'widget-head');
            attrs.$set('class', 'add-dash');
        }
    });
    app.directive('modaloptions', function () {
        return {
            restrict: 'E',
            templateUrl: 'app/common/widget/timeseries/ChartOptionsModal.html',
            replace: true,
            scope: true,
            controller: 'ModalCtrl',
            link: function (scope, element, attrs) {

            }
        };
    });

    app.directive('pvNavLogin', [
        '$cookies', 'common', function($cookies, common) {
            var user = common.Auth.isLoggedIn();
            var uiToDisplay = (true) ? {
                restrict: 'A',
                templateUrl: '/app/layout/navUserInfo.html',
                link: function(scope, iElement, iAttrs) {
                    scope.vm.user = user;
                    console.log(user);

                    function onLoginSubmit(e) {
                        e.preventDefault();

                        $.ajax({
                            type: "POST",
                            url: "/Account/LogOff",
                            success: function(response) {
                                console.log(response);
                                if (response.status === "success") {
                                    window.localStorage.clear();
                                    location.href = "/";
                                }
                            }
                        });
                    }

                    var logout = $(iElement[0]).find('.logout')[0];
                    logout.addEventListener('click', onLoginSubmit);
                }
            } :
            {
                restrict: 'A',
                templateUrl: '/app_anon/layout/navlogin.html',
                link: function(scope, iElement, iAttrs) {

                    function onLoginSubmit(e) {
                        e.preventDefault();
                        var fields = $(this);
                        console.log(fields);
                        $.ajax({
                            type: "POST",
                            url: "/Account/Login",
                            data: fields.serialize(),
                            success: function(response) {
                                if (response.isAuthenticated) {
                                    auth.setUser(response);
                                    location.href = "/";
                                }
                            }
                        });
                    }

                    var form = $(iElement[0]).find('form')[0];
                    form.addEventListener('submit', onLoginSubmit);
                }
            }

            return uiToDisplay;
        }
    ]);

    app.directive('playchart', function (datacontext) {

        var playScope = {
            source: "@",
            config: "&config"
        };

        var directive = {
            restrict: 'E',
            controller: 'timeseries as ts',
            templateUrl: "app/common/widget/timeseries/playchart.html",
            link: link,
            scope: playScope
        };

        return directive;

        function tsSuccess(results) {

        }
        function tsFail(results) {

        }

        function link(scope, element, attrs) {
            // console.log("link function");

            var config = scope.config();
            element[0].addEventListener('mouseenter', function (e) {
                $(element.parent()[0]).find('.add-dash').removeClass('hideme');
            })

            element[0].addEventListener('mouseleave', function (e) {
                $(element.parent()[0]).find('.add-dash').addClass('hideme');
            })

            scope.ts.ChartConfig = {
                options: {
                    loading: {
                        style: {
                            opacity: 1
                        }
                    },
                    chart: {
                        type: "column",
                        events: {
                            // load: ts.onLoadCallback
                        },
                        zoomType: 'x'
                    },
                    xAxis: {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    },
                    yAxis: [{
                        title: {
                            // text: 'Credits'
                        },
                        stackLabels: {
                            enabled: false,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        },
                        alternateGridColor: '#FDFFD5'
                    }],
                    tooltip: {
                        // formatter: function () {
                        //     var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>',
                        //         sum = 0;
                        //     $.each(this.points, function (i, point) {
                        //         s += '<br/>' + point.series.name + ': <b>' +
                        //             Math.abs(point.y) + ' credits</b>';
                        //         sum += point.y;
                        //     });

                        //     //s += '<br/><b>Sum: ' + sum + ' users</b>'

                        //     return s;
                        // },
                        // shared: true
                    },
                    plotOptions: {
                        area: {
                            stacking: 'normal',
                            lineColor: '#666666',
                            lineWidth: 1,
                            marker: {
                                lineWidth: 1,
                                lineColor: '#666666'
                            }
                        },
                        column: {
                            stacking: 'normal',
                            dataLabels: {
                                enabled: false,
                                color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                style: {
                                    textShadow: '0 0 3px black'
                                }
                            }
                        }
                    }
                },

                series: [],
                title: {
                    text: config.title
                },
            }


            var dm = new config.dataModelType();

            dm.setup(config, scope);
            console.log(element[0]);
            dm.init(element[0]);

        }


    });

    app.directive('playtable', function (datacontext, $q) {
        var playScope = {
            source: "@",
            config: "&config"
        };

        var directive = {
            restrict: 'E',
            controller: 'datatable as dt',
            templateUrl: "app/common/widget/datatable/playtable.html",
            link: link,
            scope: playScope
        };

        return directive;

        function link(scope, element, attrs) {
            var config = scope.dt.config = scope.config();
            var dm = new config.dataModelType();

            scope.dt.tableInfo = {};
            scope.dt.tableInfo.id = config.tableId;

            dm.setup(config, scope);
            dm.init();

        }

    });

    app.directive('playpiechart', function (datacontext) {

        var playScope = {
            source: "@",
            config: "&config"
        };

        var directive = {
            restrict: 'E',
            controller: 'piechart as ts',
            templateUrl: "app/common/widget/piechart/playpiechart.html",
            link: link,
            scope: playScope
        };

        return directive;

        function tsSuccess(results) {

        }
        function tsFail(results) {

        }

        function link(scope, element, attrs) {
            // console.log("link function");

            var config = scope.config();
            element[0].addEventListener('mouseenter', function (e) {
                $(element.parent()[0]).find('.add-dash').removeClass('hideme');
            })

            element[0].addEventListener('mouseleave', function (e) {
                $(element.parent()[0]).find('.add-dash').addClass('hideme');
            })
            console.log("piechart scope: %o", scope);


            scope.ts.ChartConfig = {

                options: {
                    //loading: {
                    //    style: {
                    //        opacity: 1
                    //    }
                    //},
                    //chart: {
                    //    plotBackgroundColor: null,
                    //    plotBorderWidth: null,
                    //    plotShadow: false,
                    //    type: 'line'
                    //},
                    //tooltip: {
                    //    // formatter: function () {
                    //    //     var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>',
                    //    //         sum = 0;
                    //    //     $.each(this.points, function (i, point) {
                    //    //         s += '<br/>' + point.series.name + ': <b>' +
                    //    //             Math.abs(point.y) + ' credits</b>';
                    //    //         sum += point.y;
                    //    //     });

                    //    //     //s += '<br/><b>Sum: ' + sum + ' users</b>'

                    //    //     return s;
                    //    // },
                    //    // shared: true
                    //},
                    plotOptions: {
                        pie: {
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: true,
                                format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                                style: {
                                    color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                                }
                            }
                        }
                    }
                },

                series: [],
                title: {
                    text: config.title
                }
            }


            var dm = new config.dataModelType();

            dm.setup(config, scope);
            console.log(element[0]);
            dm.init(element[0]);

        }


    });

    app.directive('pvNotifyDash', function () {
        var directive = {
            restrict: 'A',
            templateUrl: "app/dashboard/notification-dash.html",
            link: link,
            scope: false
        };

        return directive;

        function Notification(responseObj) {
            responseObj.CreatedAt = moment.utc(parseCSharpDateTime(responseObj.CreatedAt)).format('MM/DD/YYYY HH:mm:ss');;
            $.extend(this, responseObj);
            console.log(this);
        }

        function link(scope, element, attrs) {
            scope.Notifications = [];
            function UpdateNotifications() {
                $.ajax({
                    url: '/Notification/GetNotifications?game=DD2start=2015-10-01T00:00:00.000Z&end=2015-10-09T16:16:14.950Z',
                    success: NotificationResult
                });
            }

            function NotificationResult(result) {
                $(result).each(function () {
                    scope.Notifications.push(new Notification(this));
                })

            }

            $.ajax({
                url: '/Notification/GetNotifications?game=DD2start=2015-10-01T00:00:00.000Z&end=2015-10-09T16:16:14.950Z',
                success: NotificationResult
            });

        }
    });
    app.directive('notification', function () {
        
        var template = "<div class='pull-left'><i class='fa fa-exclamation-triangle'></i></div><div class='datas-text pull-right'>{{datetime}}<span class='bold'>{{message}}</span>{{subtext}}</div><div class='clearfix'></div>";

        var directive = {
            restrict: 'A',
            scope : {
                notification: '&notification',
            },
            template: template,
            link: link,
        };

        function link(scope, element, attrs) {
            var cache = scope.$parent.$parent.Notifications;

            var note = scope.notification();

            scope.message = note.Message;
            scope.subtext = note.Subject;
            scope.datetime = note.CreatedAt;
            console.log("notification: %o", note);

            element[0].addEventListener('click', function (e) {
                var thisOne = scope.notification();

                for (var i = 0; i < cache.length; i++) {
                    if (cache[i].Id == thisOne.Id) {
                        cache.splice(i, 1);
                        break;
                    }
                }

                
            })

        }

        return directive;

    });


    //need to update these, which were directly pasted over from playtrics
    app.directive('pvChartConfig', function () {
        return {
            controller: function ($scope) { }
        }
    });
})(); 