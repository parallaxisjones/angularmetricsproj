(function () {
    'use strict';
    var controllerId = 'timeseries';
    angular.module('app').controller(controllerId, ['common', 'datacontext', pvtimeseries]);

    function pvtimeseries(common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var ts = this;
        ts.widgetMeta = getWidgetMeta();
        ts.series = [];

        activate();
        // ts.ChartConfig = {
        //     options: {
        //         loading: {
        //             style: {
        //                 opacity: 1
        //             }
        //         },
        //         chart: {
        //             type: "column",
        //             events: {
        //                 // load: ts.onLoadCallback
        //             },
        //             zoomType: 'x'
        //         },
        //         xAxis: {
        //             type: 'datetime',
        //             dateTimeLabelFormats: {
        //                 //day: '%e of %b'
        //             }
        //         },
        //         yAxis: {
        //             title: {
        //                 // text: 'Credits'
        //             },
        //             stackLabels: {
        //                 enabled: false,
        //                 style: {
        //                     fontWeight: 'bold',
        //                     color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
        //                 }
        //             },
        //             alternateGridColor: '#FDFFD5'
        //         },
        //         tooltip: {
        //             // formatter: function () {
        //             //     var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>',
        //             //         sum = 0;
        //             //     $.each(this.points, function (i, point) {
        //             //         s += '<br/>' + point.series.name + ': <b>' +
        //             //             Math.abs(point.y) + ' credits</b>';
        //             //         sum += point.y;
        //             //     });

        //             //     //s += '<br/><b>Sum: ' + sum + ' users</b>'

        //             //     return s;
        //             // },
        //             // shared: true
        //         },
        //         plotOptions: {
        //             area: {
        //                 stacking: 'normal',
        //                 lineColor: '#666666',
        //                 lineWidth: 1,
        //                 marker: {
        //                     lineWidth: 1,
        //                     lineColor: '#666666'
        //                 }
        //             },
        //             column: {
        //                 stacking: 'normal',
        //                 dataLabels: {
        //                     enabled: false,
        //                     color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
        //                     style: {
        //                         textShadow: '0 0 3px black'
        //                     }
        //                 }
        //             }
        //         }
        //     },

        //     series: [],
        //     title: {
        //         // text: 'Premium Credit Inflow / Outflow'
        //     },
        // }
        var onFailure = function (error) {
            console.log(error);
        }

        var onLoadCallback = function () {
            var refreshRate = 1000 * 60 * 60 * 24;

            $.each(ts.SessionTypeConfig.series, function (idx, el) {
                //$timeout(function () {
                //    series.data = $scope.dau.DD2;
                //}, refreshRate);

            });

        }

        var onSuccess = function (results) {
            for (var key in results.data.SeriesData) {
                //console.log(results.data.SeriesData[key]);
                var series = new Charts.Series(key, results.data.SeriesData[key], results.data.StartDate, ts.pointInterval);
                ts.series.push(series);
            };
            $rootScope.$broadcast('ChartSuccess', {
                data: ts.series
            });
        }
        function getWidgetMeta() {
            return {
                id: 123,
                seriesData:[],
                game: 'TimeSeries widget',
                module: 'This is a module string',
                series: 'TimeSeries name',
                description: 'Hot Towel Angular is a SPA template for Angular developers.'
            };
        }

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                    // log(data);
                    ts.datacontext = datacontext;
                });
        }

    }
})();

(function () {
    'use strict';
    var controllerId = 'TimeSeriesModalCtrl';
    angular.module('app').controller(controllerId, ['$scope','$modalInstance', 'common', 'datacontext','widget', pvTimeSeriesModalCtrl]);

    function pvTimeSeriesModalCtrl($scope,$modalInstance, common, datacontext, widget) {
        var widgetOptions = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
        var wc = new common.widgetControl();
        var intervalMultiplier = (60 * 1000);
        var pointInterval = 24 * 3600 * 1000;
        var intervals = common.OptionsEnums.TimeInterval;
        var regions = common.OptionsEnums.AWSRegions;
        var chartTypes = common.OptionsEnums.ChartTypes;
        var modules = wc.getModules(datacontext);
        var metrics = modules[0].methods.charts;


        widgetOptions.widget = widget;
        widgetOptions.module = modules;
        widgetOptions.metricArray = metrics;
        widgetOptions.widget.dataModelOptions.metricArray

        console.log("startTime: %o", widgetOptions.widget.dataModelOptions.ajaxParams.start);
        console.log("find the ChartConfig! : %o", widget.dataModelOptions);
        activate();
        function activate() {
            var promises = [];
            // checkEdits();
            common.activateController(promises, controllerId)
                .then(function (data) {
                    var store = wc.getWidgetMeta();
                    // console.log(widgetOptions.intervals);
                    // console.log("html modal options activated: %o", store);

                });
        }

        // widgetOptions.GetModules = GetModules;


        widgetOptions.setMetricState = function(module){
            console.log(module);
            var mod = $.grep(modules, function(m){
                console.log("m: %o, module: %o", m, module);
                return (m.name === module.name);
            });
            // console.log(mod);
            widgetOptions.widget.dataModelOptions.metricArray = mod[0].methods.charts;
        }

        widgetOptions.cancel = function () {
            $modalInstance.close();
        }

        widgetOptions.ok = ok;
        
        function ok () {
            // console.log('calling ok from widget-specific settings controller!');
            console.log($scope);
            // widgetOptions.ChartConfig.series = widgetOptions.series;
            // widgetOptions.showModal = false;

            var eventArgs = {
                widgetType: 'TimeSeries',
                widgetState: widgetOptions.widgetConfig
            }

            //console.log(eventArgs);
            $scope.$emit("PlaytricsWidgetUpdate", eventArgs);
            $modalInstance.close(widgetOptions);
        }

        widgetOptions.today = today;

        function today(datepickerClass) {
            if (datepickerClass == null || datepickerClass == 'undefined') return Date.UTC(Date.parse(common.dateFormats[1]));

            if (datepickerClass == "start-date") {
                widgetOptions.widgetConfig.playchartParams.start = today;
            }
            if (datepickerClass == "end-date") {
                widgetOptions.widgetConfig.playchartParams.end = today;
            }
        };

        widgetOptions.clear = function () {
            widgetOptions.widgetConfig.playchartParams.start = clear;
            widgetOptions.widgetConfig.playchartParams.end = clear;
        };

         //Disable dates after today
        widgetOptions.disabled = function (date, mode) {
            return (mode === 'day' && (date > new Date()));
            //return false;
        };

        // widgetOptions.toggleMin = function () {
        //     $scope.minDate = null; //$scope.minDate ? null : new Date();
        // };
        // widgetOptions.toggleMin();

        widgetOptions.open = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();

            $.each($event.currentTarget.classList, function (idx, el) {
                if (this == "start-date") {
                    //console.log("start clicked");
                    widgetOptions.widgetConfig.datepickerOptions.startOpened = true;
                }
                if (this == "end-date") {
                    //console.log("end clicked");
                    widgetOptions.widgetConfig.datepickerOptions.endOpened = true;
                }
            });

        };



        $scope.$parent.openModal = function () {
            $scope.showModal = true;
        };

        // widgetOptions.isDLdisabled = true;
        widgetOptions.downloadCSV = function () {
            //console.log($scope.$parent);
            var csv = common.CSVconverter.ConvertToCSV;
            var results = csv(JSON.stringify('poop'));

            //TODO: pick up the series and put them into the csv function
            // this doesn't work right now

            var csvContent = "data:text/csv;charset=utf-8," + escape(results);
            var link = document.createElement("a");
            var d = new Date(),
            curr_date = d.getDate(),
            curr_month = d.getMonth() + 1,
            curr_year = d.getFullYear(),
            datestring = curr_date + "-" + curr_month + "-" + curr_year;

            link.setAttribute("href", csvContent);
            link.setAttribute("download", datestring + "-data.csv");

            link.click();
        }

        // $scope.$on('ChartSuccess', function (event, args) {
        //     // widgetOptions.isDLdisabled = false;
        // });
        


    }
})();