(function () {
    'use strict';
    var controllerId = 'datatable';
    angular.module('app').controller(controllerId, ['common', 'datacontext', pvdatatable]);

    function pvdatatable(common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var dt = this;
        dt.widgetMeta = getWidgetMeta();

        activate();

        var onFailure = function (error) {
            console.log(error);
        }

        var onLoadCallback = function () {


        }

        var onSuccess = function (results) {

        }
        function getWidgetMeta(storageName) {
//            var localStorage = common.$window.localStorage;

            //if (storageName) {

            //}

            // console.log("storage: %o", JSON.parse(localStorage.getItem("demo_simple")));
            //var store = JSON.parse(localStorage.demo_simple).widgets;
            // console.log(store);
            //for (var key in store) {
            //    console.log(store[key]);
            //}
            var store = {}
            return store;
            //return {
            //    id: 123,
            //    seriesData: [],
            //    game: 'TimeSeries widget',
            //    module: 'This is a module string',
            //    series: 'TimeSeries name',
            //    description: 'Hot Towel Angular is a SPA template for Angular developers.'
            //};
        }

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                });
        }
    }
})();

(function () {
    'use strict';
    var controllerId = 'DatatableModalCtrl';
    angular.module('app').controller(controllerId, ['$scope','$modalInstance', 'common', 'datacontext','widget', pvDataTableModalCtrl]);

    function pvDataTableModalCtrl($scope,$modalInstance, common, datacontext, widget) {
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
        var metrics = modules[0].methods;

        widgetOptions.widget = widget;

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
            widgetOptions.widget.dataModelOptions.metricArray = mod[0].methods;
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