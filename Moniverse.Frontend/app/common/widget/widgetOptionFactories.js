(function () {
    'use strict';

    var pvwidgetModule = angular.module('pvWidgets', []);
    pvwidgetModule.factory('TimeSeriesDataModel', ['$interval', 'datacontext', 'WidgetDataModel', 'common' , function ($interval, datacontext, WidgetDataModel, common) {

        function TimeSeriesDataModel() {

        }

        TimeSeriesDataModel.prototype = Object.create(WidgetDataModel.prototype);
        TimeSeriesDataModel.prototype.constructor = WidgetDataModel;
        var dc = datacontext;
        TimeSeriesDataModel.prototype.module;
        TimeSeriesDataModel.prototype.metric;
        TimeSeriesDataModel.prototype.widgetId;        
        TimeSeriesDataModel.prototype.element;

        angular.extend(TimeSeriesDataModel.prototype, {
            setup: function(widget, scope){
                WidgetDataModel.prototype.setup.apply(this, arguments);
                if(!scope.ts){
                    scope.ts = {}
                }
                scope.ts.modelOptions = scope.modelOptions = widget.dataModelOptions;

                this.module = dc[scope.modelOptions.module.name](scope, false);
                this.metric = this.module[scope.modelOptions.metricName]();
                this.extraParams = (widget.extraParams) ? widget.extraParams : {};
                this.extraOptions = (widget.extraOptions) ? widget.extraOptions : {
                    bRefresh: false,
                    RefreshInterval: 1000 * 1 * 2
                }
            },
            init: function (element) {
                var WC = new common.widgetControl();
                var options = (this.dataModelOptions) ? this.dataModelOptions : {};
                this.widgetId = (options && options.widgetId != null) ? options.widgetId : WC.generateId();
                options.widgetId = this.widgetId;
                this.element = element;
                var bRefresh = this.extraOptions.bRefresh;
                var refreshInterval = this.extraOptions.RefreshInterval;

                $(element).find('.playchart-conf').toggle();
                
                options.before = function () {
                    var opts = {
                        lines: 13, // The number of lines to draw
                        length: 7, // The length of each line
                        width: 4, // The line thickness
                        radius: 10, // The radius of the inner circle
                        corners: 1, // Corner roundness (0..1)
                        rotate: 0, // The rotation offset
                        color: '#000', // #rgb or #rrggbb
                        speed: 1, // Rounds per second
                        trail: 60, // Afterglow percentage
                        shadow: false, // Whether to render a shadow
                        hwaccel: false, // Whether to use hardware acceleration
                        className: 'spinner', // The CSS class to assign to the spinner
                        zIndex: 2e9, // The z-index (defaults to 2000000000)
                        top: '200', // Top position relative to parent in px
                        left: 'auto' // Left position relative to parent in px
                    };
                    var stage = $(element).find('.chart-stage');
                    var spinner = new Spinner(opts).spin(stage[0]);
                }
                if (bRefresh) {
                    this.getData(options);
                    this.startInterval(refreshInterval, options);
                } else {
                    this.getData(options);
                    this.updateScope(options);
                }
            },
            getData: function(options, success, fail){
                var params;                
                if(!options.ajaxParams && options.dataModelOptions){
                    options = options.dataModelOptions;
                }
                var sucFun = (success) ? success : this.metric.successTimeSeries;
                var el = this.element;
                var successFn = function (data) {
                    $(el).find('.playchart-conf').toggle();
                    $(el).find('.spinner').remove()

                    sucFun.call(null, data);
                }
                var failFn = (fail) ? fail : this.metric.fail;
                
                var params = {
                    game: options.ajaxParams.game,
                    region: options.ajaxParams.region.id,
                    interval: options.ajaxParams.interval.id,
                    start: options.ajaxParams.start,
                    end: options.ajaxParams.end,
                    chartType: options.ajaxParams.chartType.id,
                    success: successFn,
                    before: (options.before) ? options.before : function(){}
                }
                if (!$.isEmptyObject(this.extraParams)) {
                    params.extraParams = this.extraParams;
                }
                var processor = this.metric.process;
                return processor.process(new PlaytricsRequest(params));                
            },
            startInterval: function (timeoutMs, options) {
                var el = this.element;
                $interval.cancel(this.intervalPromise);


                this.intervalPromise = $interval(function () {
                    $(el).find('.playchart-conf').toggle();
                    this.getData(options);
                    this.updateScope(options);
                }.bind(this), timeoutMs);
            },

            destroy: function () {
                WidgetDataModel.prototype.destroy.call(this);
                $interval.cancel(this.intervalPromise);
            }
        });



        return TimeSeriesDataModel;
    }]);

    pvwidgetModule.factory('PieChartDataModel', ['$interval', 'datacontext', 'WidgetDataModel', 'common', function ($interval, datacontext, WidgetDataModel, common) {

        function PieChartDataModel() {

        }

        PieChartDataModel.prototype = Object.create(WidgetDataModel.prototype);
        PieChartDataModel.prototype.constructor = WidgetDataModel;
        var dc = datacontext;
        PieChartDataModel.prototype.module;
        PieChartDataModel.prototype.metric;
        PieChartDataModel.prototype.widgetId;
        PieChartDataModel.prototype.element;

        angular.extend(PieChartDataModel.prototype, {
            setup: function (widget, scope) {
                WidgetDataModel.prototype.setup.apply(this, arguments);
                if (!scope.ts) {
                    scope.ts = {}
                }
                scope.ts.modelOptions = scope.modelOptions = widget.dataModelOptions;

                this.module = dc[scope.modelOptions.module.name](scope, false);
                this.metric = this.module[scope.modelOptions.metricName]();
                this.extraParams = (widget.extraParams) ? widget.extraParams : {};
                this.extraOptions = (widget.extraOptions) ? widget.extraOptions : {
                    bRefresh: true
                }
            },
            init: function (element) {
                var WC = new common.widgetControl();
                var options = (this.dataModelOptions) ? this.dataModelOptions : {};
                this.widgetId = (options && options.widgetId != null) ? options.widgetId : WC.generateId();
                options.widgetId = this.widgetId;
                this.element = element;
                $(element).find('.playchart-conf').toggle();

                options.before = function () {
                    var opts = {
                        lines: 13, // The number of lines to draw
                        length: 7, // The length of each line
                        width: 4, // The line thickness
                        radius: 10, // The radius of the inner circle
                        corners: 1, // Corner roundness (0..1)
                        rotate: 0, // The rotation offset
                        color: '#000', // #rgb or #rrggbb
                        speed: 1, // Rounds per second
                        trail: 60, // Afterglow percentage
                        shadow: false, // Whether to render a shadow
                        hwaccel: false, // Whether to use hardware acceleration
                        className: 'spinner', // The CSS class to assign to the spinner
                        zIndex: 2e9, // The z-index (defaults to 2000000000)
                        top: '200', // Top position relative to parent in px
                        left: 'auto' // Left position relative to parent in px
                    };
                    var stage = $(element).find('.chart-stage');
                    var spinner = new Spinner(opts).spin(stage[0]);
                }
                this.getData(options);
                this.updateScope(options);

            },
            getData: function (options, success, fail) {
                var params;
                if (!options.ajaxParams && options.dataModelOptions) {
                    options = options.dataModelOptions;
                }
                var sucFun = (success) ? success : this.metric.successTimeSeries;
                var el = this.element;
                var successFn = function (data) {
                    $(el).find('.playchart-conf').toggle();
                    $(el).find('.spinner').remove()

                    sucFun.call(null, data);
                }
                var failFn = (fail) ? fail : this.metric.fail;
                console.log("PIE CHART WIDGET --> %o", options);
                var params = {
                    game: options.ajaxParams.game,
                    region: options.ajaxParams.region.id,
                    interval: options.ajaxParams.interval.id,
                    start: options.ajaxParams.start,
                    end: options.ajaxParams.end,
                    chartType: 6,
                    success: successFn,
                    before: (options.before) ? options.before : function () { },
                }
                if (!$.isEmptyObject(this.extraParams)) {
                    params.extraParams = this.extraParams;
                }
                var processor = this.metric.process;
                return processor.process(new PlaytricsRequest(params));
            },
            startInterval: function (timeoutMs) {
                $interval.cancel(this.intervalPromise);

                this.intervalPromise = $interval(function () {
                    this.getData(options, metric);
                    this.updateScope(value);
                }.bind(this), timeoutMs);
            },

            destroy: function () {
                WidgetDataModel.prototype.destroy.call(this);
                $interval.cancel(this.intervalPromise);
            }
        });



        return PieChartDataModel;
    }]);
    pvwidgetModule.factory('DataTableDataModel', ['$interval', 'datacontext', 'WidgetDataModel', 'common' , function ($interval, datacontext, WidgetDataModel, common) {

        function DataTableDataModel() {
        }

        DataTableDataModel.prototype = Object.create(WidgetDataModel.prototype);
        DataTableDataModel.prototype.constructor = WidgetDataModel;
        var dc = datacontext;
        DataTableDataModel.prototype.module;
        DataTableDataModel.prototype.metric;
        DataTableDataModel.prototype.widgetId;

        angular.extend(DataTableDataModel.prototype, {
            setup: function(widget, scope){
                WidgetDataModel.prototype.setup.apply(this, arguments);
                if(!scope.dt){
                    scope.dt = {}
                }
                scope.dt.modelOptions = scope.modelOptions = widget.dataModelOptions;
                this.module = dc[scope.modelOptions.module.name](scope, true);
                this.metric = this.module[scope.modelOptions.metricName]();

            },            
            init: function () {
                var WC = new common.widgetControl();
                var options = (this.dataModelOptions) ? this.dataModelOptions : {};
                this.widgetId = (options && options.widgetId != null) ? options.widgetId : WC.generateId();
                options.widgetId = this.widgetId;
                this.getData(options);
                this.updateScope(options);
            },
            getData: function(options, success, fail){
                var params;                
                if(!options.ajaxParams && options.dataModelOptions){
                    options = options.dataModelOptions;
                }
                console.log("in data tables getData : %o", options);
                var successFn = (success) ? success : this.metric.successDataTable;
                var failFn = (fail) ? fail : this.metric.fail;
                
                if(options.ajaxParams){
                    params = {
                        game: options.ajaxParams.game,
                        region: options.ajaxParams.region.id,
                        interval: options.ajaxParams.interval.id,
                        start: options.ajaxParams.start,
                        end: options.ajaxParams.end,
                        chartType: options.ajaxParams.chartType.id,
                        success : successFn
                    }                    
                } else{
                    params = {}
                }
                var processor = this.metric.process;
                return processor.process(new PlaytricsRequest(params));               
            },
            startInterval: function () {
                $interval.cancel(this.intervalPromise);

                this.intervalPromise = $interval(function () {
                    var value = Math.floor(Math.random() * this.limit);
                    this.updateScope(value);
                }.bind(this), 500);
            },

            updateLimit: function (limit) {
                this.dataModelOptions = this.dataModelOptions ? this.dataModelOptions : {};
                this.dataModelOptions.limit = limit;
                this.limit = limit;
            },

            destroy: function () {
                WidgetDataModel.prototype.destroy.call(this);
                $interval.cancel(this.intervalPromise);
            }
        });

        return DataTableDataModel;
    }]);
    pvwidgetModule.factory('EditHtmlDataModel', function ($interval, datacontext, common, WidgetDataModel) {

        function EditHtmlDataModel() {
        }

        EditHtmlDataModel.prototype = Object.create(WidgetDataModel.prototype);
        EditHtmlDataModel.prototype.constructor = WidgetDataModel;
        var WC = new common.widgetControl();
        var w;
        angular.extend(EditHtmlDataModel.prototype, {
            setup: function(widget, scope){
                w = widget;
                WidgetDataModel.prototype.setup.apply(this, arguments);
                var options = widget.dataModelOptions;
                this.widgetId = (options && options.widgetId != null) ? options.widgetId : WC.generateId();
                options.widgetId = this.widgetId;
                scope.vm.widgetId = this.widgetId;
                this.updateScope(options.widgetId);

                // console.log("")

                common.$on('GetDataModelData', function(e, data){
                    common.$broadcast('EditHtml', {
                        w:w,
                        o:options
                    })
                })
            },
            init: function () {
                // console.log("IN EDIT HTML INTIT!!!!!1 %o", this);
                var dataModelOptions = this.dataModelOptions;
                // this.limit = (dataModelOptions && dataModelOptions.limit) ? dataModelOptions.limit : 100;

                // this.updateScope(widgetId);
                // this.startInterval();
            },

            startInterval: function () {
                $interval.cancel(this.intervalPromise);

                this.intervalPromise = $interval(function () {
                    var value = Math.floor(Math.random() * this.limit);
                    this.updateScope(value);
                }.bind(this), 500);
            },

            updateLimit: function (limit) {
                this.dataModelOptions = this.dataModelOptions ? this.dataModelOptions : {};
                this.dataModelOptions.limit = limit;
                this.limit = limit;
            },

            destroy: function () {
                var options = w.dataModelOptions;
                $interval.cancel(this.intervalPromise);
                localStorage.removeItem(options.widgetId);
                WidgetDataModel.prototype.destroy.call(this);
            }
        });

        return EditHtmlDataModel;
    });
    pvwidgetModule.factory('DataModelController', ['TimeSeriesDataModel', 'PieChartDataModel','DataTableDataModel', 'EditHtmlDataModel', function (ts, pie, dt, eh) {
        return {
            TimeSeriesModel: ts,
            PiechartModel: pie,
            DataTableModel: dt,
            EditHtmlModel: eh
        }
    }])
    pvwidgetModule.factory('pvwidgetOptionsModalConfig', function () {
         return {
            TimeSeriesOptions: {
                    templateUrl: 'app/common/widget/timeseries/ChartOptionsModal.html',
                    controller: 'TimeSeriesModalCtrl as widgetOptions', // defined elsewhere,
                    animation: true,
                    keyboard: true
            },
            PieChartOptions: {
                templateUrl: 'app/common/widget/piechart/ChartOptionsModal.html',
                controller: 'PieChartModalCtrl as widgetOptions', // defined elsewhere,
                animation: true,
                keyboard: true
            },
            DataTableOptions: {
                    templateUrl: 'app/common/widget/datatable/ChartOptionsModal.html',
                    controller: 'DatatableModalCtrl', // defined elsewhere,
                    animation: true,
                    keyboard: true
                },
            EditHtmlOptions: {
                    templateUrl: 'app/common/widget/editHtml/ChartOptionsModal.html',
                    controller: 'EditHtmlModalCtrl as widgetOptions', // defined elsewhere,
                    animation: true,
                    keyboard: true
                }
         }
    });

    pvwidgetModule.factory('pvwidgetDefinitions', ['DataModelController','pvwidgetOptionsModalConfig','common','datacontext', function (dmc, opt, common, datacontext) {

        var generator = new common.widgetControl();
        var modules = generator.getModules(datacontext);
        var metrics = modules[0].methods.charts;
        var intervals = common.OptionsEnums.TimeInterval;
        var regions = common.OptionsEnums.AWSRegions;
        var chartTypes = common.OptionsEnums.ChartTypes;
        var ChartTitle = 'Premium Credit Inflow / Outflow';
        var Units = 'Credits';
        var onLoadCallback = function(){}
        var toolTipFormatter = function () {
                        var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>',
                            sum = 0;
                        $.each(this.points, function (i, point) {
                            s += '<br/>' + point.series.name + ': <b>' +
                                Math.abs(point.y) + ' credits</b>';
                            sum += point.y;
                        });

                        //s += '<br/><b>Sum: ' + sum + ' users</b>'

                        return s;
                    };
        return [
          {
              name: 'TimeSeries',
              directive: 'playchart',
              title: 'Premium Credit Inflow/Outflow',
              dataModelType: dmc.TimeSeriesModel,
              dataModelOptions: {
                refresh: false,
                refreshRate: intervals[0],
                moduleArray: modules,
                metricArray: metrics,
                chartTypeArray:chartTypes,
                regionArray: regions,
                intervalArray: intervals,
                module: modules[0],
                metricName: modules[0].methods[0],
                chartOptions: {
                    chart: {
                        type: chartTypes[4].type,
                        events: {
                            load: onLoadCallback
                        }
                    },
                    xAxis: {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    },                    
                    yAxis: {
                        title: {
                            text: Units
                            }
                    },
                    tooltip: {
                        formatter: toolTipFormatter,
                    },
                    title: {
                        text: ChartTitle
                    },
                },
                datepickerOptions: {
                    maxDate: moment.utc(),
                    minDate: false,
                    startOpened : false,
                    endOpened: false,
                    format: common.dateFormats[0],
                    dateOptions: {
                        formatYear: 'yy',
                        startingDay: 1,
                        showWeeks: false
                    }
                },
                timepickerOptions: {
                    isMeridian: false
                },
                ajaxParams: {
                    game: 'DD2',
                    region: regions[0],
                    interval: intervals[1],
                    start: moment.utc().subtract(30, 'days').startOf('day').toJSON(),
                    end: moment.utc().subtract(1, 'days').endOf('day').toJSON(),
                    chartType: chartTypes[4]
                }
              },
              settingsModalOptions: opt.TimeSeriesOptions,
              onSettingsClose: function(result, widget, dashboardScope) {
                // do something to update widgetModel, like the default implementation:
                // console.log("result: %o", dashboardScope);
                // widget.dataModelOptions = result.widget;
                jQuery.extend(true, widget, result);
                dashboardScope.saveDashboard();
                console.log("widget: %o", widget);
                //scope has been updated *should have been*
               //make ajax call or dont and only set refresh rate

              },
              onSettingsDismiss: function(reasonForDismissal, dashboardScope) {
                // probably do nothing here, since the user pressed cancel
              }
          },
          {
              name: 'PieChart',
              directive: 'playpiechart',
              title: 'Pie Chart',
              dataModelType: dmc.PiechartModel,
              dataModelOptions: {
                  refresh: false,
                  refreshRate: intervals[0],
                  moduleArray: modules,
                  metricArray: metrics,
                  chartTypeArray: chartTypes,
                  regionArray: regions,
                  intervalArray: intervals,
                  module: modules[0],
                  metricName: modules[0].methods[0],
                  chartOptions: {
                      chart: {
                          type: 'pie',
                          events: {
                              load: onLoadCallback
                          }
                      },
                      tooltip: {
                          headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
                          pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y:.2f}%</b> of total<br/>'
                      },
                      title: {
                          text: ChartTitle
                      },
                      series: [],
                      drilldown: []
                  },
                  datepickerOptions: {
                      maxDate: moment.utc(),
                      minDate: false,
                      startOpened: false,
                      endOpened: false,
                      format: common.dateFormats[0],
                      dateOptions: {
                          formatYear: 'yy',
                          startingDay: 1,
                          showWeeks: false
                      }
                  },
                  timepickerOptions: {
                      isMeridian: false
                  },
                  ajaxParams: {
                      game: 'DD2',
                      region: regions[0],
                      interval: intervals[1],
                      start: moment.utc().subtract(30, 'days').toJSON(),
                      end: moment.utc().toJSON(),
                      chartType: chartTypes[6]
                  }
              },
              settingsModalOptions: opt.PieChartOptions,
              onSettingsClose: function (result, widget, dashboardScope) {
                  // do something to update widgetModel, like the default implementation:
                  // console.log("result: %o", dashboardScope);
                  // widget.dataModelOptions = result.widget;
                  jQuery.extend(true, widget, result);
                  dashboardScope.saveDashboard();
                  console.log("widget: %o", widget);
                  //scope has been updated *should have been*
                  //make ajax call or dont and only set refresh rate

              },
              onSettingsDismiss: function (reasonForDismissal, dashboardScope) {
                  // probably do nothing here, since the user pressed cancel
              }
          },
          {
              name: 'DataTable',
              directive: 'playtable',
              dataModelType: dmc.DataTableModel,
              dataModelOptions: {
                widgetId: null,
                controllerId: null,
                seriesId: null,
                refresh: false,
                refreshRate: intervals[0],
                moduleArray: modules,
                metricArray: metrics,
                chartTypeArray:chartTypes,
                regionArray: regions,
                intervalArray: intervals,
                module: modules[0],
                metricName: modules[0].methods[0],
                datepickerOptions: {
                    maxDate: moment.utc(),
                    minDate: false,
                    startOpened : false,
                    endOpened: false,
                    format: common.dateFormats[0],
                    dateOptions: {
                        formatYear: 'yy',
                        startingDay: 1,
                        showWeeks: false
                    }
                },
                timepickerOptions: {
                    isMeridian: false
                },                                
                ajaxParams: {
                    game: 'DD2',
                    region: regions[0],
                    interval: intervals[1],
                    start: moment.utc().subtract(14, 'days').toJSON(),
                    end: moment.utc().toJSON(),
                    chartType: chartTypes[4]
                } 
            },
              settingsModalOptions: opt.DataTableOptions,
              onSettingsClose: function(result, widget, dashboardScope) {
                // do something to update widgetModel, like the default implementation:
                // widget.title = result.dataModel.widgetTitle;
                // console.log
                jQuery.extend(true, widget, result);
                dashboardScope.saveDashboard();
                console.log("widget: %o", widget);
              },
              onSettingsDismiss: function(reason, scope) {
                // probably do nothing here, since the user pressed cancel
              }
          },
          {
              name: 'Html',
              directive: 'html-edit',
              dataModelType: dmc.EditHtmlModel,
              dataModelOptions: {
                widgetId: null,
                controllerId: null,
                seriesId: null,
                ajaxParams: {
                    game: 'DD2',
                    region: regions[0],
                    interval: intervals[1],
                    start: moment.utc().subtract(14, 'days').toJSON(),
                    end: moment.utc().toJSON(),
                    chartType: chartTypes[4]
                }                
              },
              settingsModalOptions: opt.EditHtmlOptions,
              onSettingsClose: function(result, widget, dashboardScope) {
                // do something to update widgetModel, like the default implementation:
                jQuery.extend(true, widget, result);
              },
              onSettingsDismiss: function(reasonForDismissal, dashboardScope) {
                // probably do nothing here, since the user pressed cancel
                // console.log("working");
              }
          }
        ];
    }]);
    pvwidgetModule.value('defaultWidgets', [
    // { name: 'DataTable' },
    // { name: 'TimeSeries' },
    // { name: 'Html' }
    ]);

    pvwidgetModule.factory('widgetinterface', ['pvwidgetDefinitions', 'pvwidgetOptionsModalConfig', 'defaultWidgets',function (definitions, ModalConfig, defaultWidgets) {

        var getWidgetDefinitionsFN = function (definition) {
            if (definition) {
                return [ /*return the definition*/];
            }
            return definitions;
        }

        var getConfigFN = function (definition) {
            if (definition) {
                return [ /*return the config*/];
            }
            return ModalConfig;
        }        

        return {
            getWidgetDefinitions: getWidgetDefinitionsFN,
            defaultWidgets: defaultWidgets,
            getConfig: getConfigFN
        }
    }]);

    
})();



