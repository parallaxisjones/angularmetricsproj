(function () {
    'use strict';
    var controllerId = 'dashboard';
    angular
        .module('app')
        .controller("ModalCtrl", [
            '$scope', 'CSVconverter', function($scope, converter) {

                $scope.refresh = false;
                $scope.pointInterval = 24 * 3600 * 1000;
                $scope.formats = ['yyyy-MM-ddTHH:mm:ssZ', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];

                var today = new Date(Date.parse($scope.formats[1]));
                $scope.format = $scope.formats[0];

                var from = new Date();
                from.setHours(0, 0, 0, 0);

                $scope.$parent.start = from;
                $scope.$parent.end = new Date();

                $scope.today = function(datepickerClass) {
                    if (datepickerClass == null || datepickerClass == 'undefined') return today;

                    if (datepickerClass == "start-date") {
                        $scope.$parent.start = today;
                    }
                    if (datepickerClass == "end-date") {
                        $scope.$parent.end = today;
                    }
                };


                $scope.maxDate = $scope.today();
                $scope.clear = function() {
                    $scope.$parent.start = clear;
                    $scope.$parent.end = clear;
                };

                //Disable dates after today
                $scope.disabled = function(date, mode) {
                    return (mode === 'day' && (date > new Date()));
                };

                $scope.toggleMin = function() {
                    $scope.minDate = null; //$scope.minDate ? null : new Date();
                };
                $scope.toggleMin();

                $scope.open = function($event) {
                    $event.preventDefault();
                    $event.stopPropagation();

                    $.each($event.currentTarget.classList, function(idx, el) {
                        if (this == "start-date") {
                            $scope.startOpened = true;
                        }
                        if (this == "end-date") {
                            $scope.endOpened = true;
                        }
                    });

                };

                $scope.dateOptions = {
                    formatYear: 'yy',
                    startingDay: 1
                };

                $scope.$parent.openModal = function() {
                    $scope.showModal = true;
                };
                $scope.$parent.okModal = function() {

                    var eventArgs = {
                        start: $scope.start.toJSON(),
                        end: $scope.end.toJSON(),
                        interval: $scope.selectedInterval.interval,
                        region: $scope.selectedRegion.id,
                        chart: $scope.selectedChartType.type,
                        refresh: $scope.refresh,
                        updateInterval: ($scope.refresh) ? ($scope.refresh) : null
                    }
                    $scope.$emit("PlaytricsChartUpdate", eventArgs);

                };

                $scope.$parent.cancelModal = function() {
                    $scope.showModal = false;
                };

                var csv = converter.ConvertToCSV;

                $scope.isDLdisabled = true;
                $scope.downloadCSV = function() {
                    //console.log($scope.$parent);
                    var results = csv(JSON.stringify($scope.$parent.ChartConfig.series));

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

                $scope.$on('ChartSuccess', function(event, args) {
                    $scope.isDLdisabled = false;
                });


            }
        ]).controller(controllerId, ['common', 'datacontext', 'widgetinterface', dashboard]);

    function dashboard(common, datacontext, widgetinterface) {
        var vm = this;
        vm.title = 'dashboard';

        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);


        vm.dashboardOptionsTest = {
            widgetButtons: false,
            widgetDefinitions: widgetinterface.getWidgetDefinitions(),
            defaultWidgets: widgetinterface.defaultWidgets,
            storage: common.$window.localStorage,
            storageId: 'playtricsWidgets'
        };
        vm.randomValue = Math.random();
        common.$interval(function () {
            vm.randomValue = Math.random();
        }, 500);

        activate();

        function activate() {
            common.activateController([getMessageCount()], controllerId)
                .then(function () { log('Activated dashboard View'); });
        }

        function getMessageCount() {
            // console.log("getMessageCount");

            return vm.messageCount = 666;
            // return datacontext.getMessageCount().then(function (data) {
            //     return vm.messageCount = data;
            // });
        }

    }
})();