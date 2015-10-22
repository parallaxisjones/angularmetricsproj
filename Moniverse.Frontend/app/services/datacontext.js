
(function () {
    'use strict';
    var $q;
    var serviceId = 'datacontext';
    angular.module('app').factory(serviceId, ['common', 'config', datacontext]);
    
    function datacontext(common, config) {
        // console.log("app config %o", config);
        var serviceEndpointUrl = config.appDataUrl;
        $q = common.$q;
        var getLogFn = common.logger.getLogFn;
        var errorLog = getLogFn(serviceId, 'error');
        var service = {
            GameEconomy: GameEconomy,
            GameSessions: GameSessions,
            HostingInstances: HostingInstances,
            UserSessions: UserSessions
        };

        return service;
        

        function GameEconomy(scope, isDataTable) {

            var moduleName = "Economy";

            var coinFlow = function () {
                var widgetName = "GetCoinFlowMacro";
                var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Response) {
                    var CAT_PROCESS = false;

                    scope.ts.ChartConfig.options.exporting = {
                        enabled: true
                    };

                    scope.ts.ChartConfig.options.chart = {
                        type: 'column',
                        events: {
                            load: function(){
                                console.log("loaded");
                            }
                        },
                        zoomType: 'x'
                    };

                    scope.ts.ChartConfig.options.xAxis = {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    };

                    scope.ts.ChartConfig.options.yAxis = {
                        title: {
                            text: 'Credits'
                        },
                        stackLabels: {
                            enabled: false,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        },
                        alternateGridColor: '#FDFFD5'
                    };

                    scope.ts.ChartConfig.options.tooltip = {
                        formatter: function () {
                            var nFormat = function (num) {
                                if (num >= 1000000000 || num <= -1000000000) {
                                    return (num / 1000000000).toFixed(2).replace(/\.00$/, '') + 'G';
                                }
                                if (num >= 1000000 || num <= -1000000) {
                                    return (num / 1000000).toFixed(2).replace(/\.00$/, '') + 'M';
                                }
                                if (num >= 1000 || num <= -1000) {
                                    return (num / 1000).toFixed(2).replace(/\.00$/, '') + 'K';
                                }
                                return num;
                            }
                            var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                sum = 0;
                            $.each(this.points, function (i, point) {
                                s += '<br/>' + point.series.name + ': <b>' +
                                nFormat(point.y) + ' credits</b>';
                                sum += point.y;
                            });

                            s += '<br/><b>Net: ' + nFormat(sum) + ' credits</b>'

                            return s;
                        },
                        shared: true
                    };

                    scope.ts.ChartConfig.options.plotOptions = {
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
                                enabled: true,
                                color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                style: {
                                    textShadow: '0 0 3px black'
                                },
                                formatter: function () {
                                    var num = this.y;

                                    if (num >= 1000000000 || num <= -1000000000) {
                                        return (num / 1000000000).toFixed(2).replace(/\.00$/, '') + 'G';
                                    }
                                    if (num >= 1000000 || num <= -1000000) {
                                        return (num / 1000000).toFixed(2).replace(/\.00$/, '') + 'M';
                                    }
                                    if (num >= 1000 || num <= -1000) {
                                        return (num / 1000).toFixed(2).replace(/\.00$/, '') + 'K';
                                    }
                                    return num;
                                }
                            } 
                        }                       
                    };


                    var SeriesFactory = function (TimeSeries) {

                        if (TimeSeries.Type == "Purchase") {
                            var series = {
                                pointStart: TimeSeries.StartDate,
                                pointInterval: TimeSeries.interval, // one day
                                stack: 'aggregate',
                                name: "Outflow",
                                data: $.map(TimeSeries.coinData, function (x) {
                                    return -x;
                                })
                            };
                        } else if (TimeSeries.Type == "AddCredits") {
                            var series = {
                                pointStart: TimeSeries.StartDate,
                                pointInterval: TimeSeries.interval, // one day
                                stack: 'aggregate',
                                name: "Inflow",
                                data: TimeSeries.coinData
                            };
                        }

                        return series;
                    }

                    var processAggregates = function (Response) {
                        var s = [];

                        if ((Response.data.hasOwnProperty("Inflows") && Response.data.Inflows.length > 0) ||
                            (Response.data.hasOwnProperty("Outflows") && Response.data.Outflows.length > 0)) {
                            $.each(Response.data.Inflows, function (idx, flowData) {
                                // console.log(SeriesFactory(flowData));
                                scope.ts.ChartConfig.series.push(SeriesFactory(flowData));
                                //console.log(flowData);
                            });
                            $.each(Response.data.Outflows, function (idx, flowData) {
                                scope.ts.ChartConfig.series.push(SeriesFactory(flowData));
                                //console.log(flowData);
                            });
                        } else {
                            //$rootScope.$broadcast('NoData');

                        }
                    }
                    if (!CAT_PROCESS) {
                        processAggregates(Response);
                    }
                    //console.log(results);
                }
                // console.log(ProcessEconomy);
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var coinFlowCat = function () {
                var widgetName = "GetCoinFlowMacroByCategory";
                var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Response) {
                    var CAT_PROCESS = false;

                    scope.ts.ChartConfig.options.exporting = {
                        enabled: true
                    };

                    scope.ts.ChartConfig.options.chart = {
                        type: 'column',
                        events: {
                            load: function () {
                                console.log("loaded");
                            }
                        },
                        zoomType: 'x'
                    };

                    scope.ts.ChartConfig.options.xAxis = {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    };

                    scope.ts.ChartConfig.options.yAxis = {
                        title: {
                            text: 'Credits'
                        },
                        stackLabels: {
                            enabled: false,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        },
                        alternateGridColor: '#FDFFD5'
                    };

                    scope.ts.ChartConfig.options.tooltip = {
                        formatter: function () {
                            var nFormat = function (num) {
                                if (num >= 1000000000 || num <= -1000000000) {
                                    return (num / 1000000000).toFixed(2).replace(/\.00$/, '') + 'G';
                                }
                                if (num >= 1000000 || num <= -1000000) {
                                    return (num / 1000000).toFixed(2).replace(/\.00$/, '') + 'M';
                                }
                                if (num >= 1000 || num <= -1000) {
                                    return (num / 1000).toFixed(2).replace(/\.00$/, '') + 'K';
                                }
                                return num;
                            }
                            var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>',
                                sum = 0;
                            $.each(this.points, function (i, point) {
                                s += '<br/>' + point.series.name + ': <b>' +
                                nFormat(point.y) + ' credits</b>';
                                sum += point.y;
                            });

                            s += '<br/><b>Net: ' + nFormat(sum) + ' credits</b>'

                            return s;
                        },
                        shared: true
                    };

                    scope.ts.ChartConfig.options.plotOptions = {
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
                                enabled: true,
                                color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white',
                                style: {
                                    textShadow: '0 0 3px black'
                                },
                                formatter: function () {
                                    var num = this.y;

                                    if (num >= 1000000000 || num <= -1000000000) {
                                        return (num / 1000000000).toFixed(2).replace(/\.00$/, '') + 'G';
                                    }
                                    if (num >= 1000000 || num <= -1000000) {
                                        return (num / 1000000).toFixed(2).replace(/\.00$/, '') + 'M';
                                    }
                                    if (num >= 1000 || num <= -1000) {
                                        return (num / 1000).toFixed(2).replace(/\.00$/, '') + 'K';
                                    }
                                    return num;
                                }
                            }
                        }
                    };

                    var SeriesFactory = function (TimeSeries) {
                        
                        if (TimeSeries.Type == "Purchase") {
                            var series = {
                                pointStart: TimeSeries.StartDate,
                                pointInterval: TimeSeries.interval, // one day
                                stack: 'aggregate',
                                name: TimeSeries.Category,
                                data: $.map(TimeSeries.coinData, function (x) {
                                    return -x;
                                })
                            };
                        } else if (TimeSeries.Type == "AddCredits") {
                            var series = {
                                pointStart: TimeSeries.StartDate,
                                pointInterval: TimeSeries.interval, // one day
                                stack: 'aggregate',
                                name: TimeSeries.Category,
                                data: TimeSeries.coinData
                            };
                        }

                        return series;
                    }

                    var processAggregates = function (Response) {
                        var s = [];

                        if ((Response.data.hasOwnProperty("Inflows") && Response.data.Inflows.length > 0) ||
                            (Response.data.hasOwnProperty("Outflows") && Response.data.Outflows.length > 0)) {
                            $.each(Response.data.Inflows, function (idx, flowData) {
                                // console.log(SeriesFactory(flowData));
                                scope.ts.ChartConfig.series.push(SeriesFactory(flowData));
                                //console.log(flowData);
                            });
                            $.each(Response.data.Outflows, function (idx, flowData) {
                                scope.ts.ChartConfig.series.push(SeriesFactory(flowData));
                                //console.log(flowData);
                            });
                        } else {
                            //$rootScope.$broadcast('NoData');

                        }
                    }
                    if (!CAT_PROCESS) {
                        processAggregates(Response);
                    }
                    //console.log(results);
                }
                // console.log(ProcessEconomy);
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function (data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })
                    },
                    fail: function (data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }
            var whaleBuyReport = function() {
                var widgetName = "GetBuyWhales";
                var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Response) {

                    scope.ts.ChartConfig.options = $.extend({
                        title: { text: Response.data.title },
                        subtitle: { text: Response.data.subtitle },
                        chart: { type: 'pie' },
                        tooltip: {
                            headerFormat: '',
                            pointFormat: '<b>{point.name}:</b> {point.percentage:.1f}%<br/>Total bought: ${point.y}<br/>Count: {point.metadata.count}'
                        }
                    }, scope.ts.ChartConfig.options);

                    scope.ts.ChartConfig.options.plotOptions = {
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


                    var data = Response.data.categoryData.map(function (x) {
                        return {
                            name: x.name,
                            y: x.value,
                            drilldown: x.drilldown,
                            metadata: x.metadata
                        };
                    });

                    scope.ts.ChartConfig.series = [{ name: "Category", colorByPoint: true, data: data }];

                    // pie chart stuff
                    //console.log(scope);
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function (data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        });
                    },
                    fail: function (data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        });
                    }
                }
           }
            var whaleSpendReport = function () {
               var widgetName = "GetSpendWhales";
               var processor = new Processor(moduleName, widgetName);
               function onTimeSeriesSuccess(Response) {
	               scope.ts.ChartConfig.options = $.extend({
                       title: { text: Response.data.title },
                       subtitle: { text: Response.data.subtitle },
                       chart: { type: 'pie' },
                       tooltip: {
                           headerFormat : '',
                           pointFormat: '<b>{point.name}:</b> {point.percentage:.1f}%<br/>Total spent: {point.y} credits<br/>Count: {point.metadata.count}'
                       }
                   }, scope.ts.ChartConfig.options);

                   scope.ts.ChartConfig.options.plotOptions = {
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
                   };

                   var data = Response.data.categoryData.map(function (x) {
                       return {
                           name: x.name,
                           y: x.value,
                           drilldown: x.drilldown,
                           metadata: x.metadata
                       };
                   });

                   scope.ts.ChartConfig.series = [{ name: "Category", colorByPoint: true, data: data }];

                   // pie chart stuff
                   //console.log(scope);
               }

               return {
                   process: processor,
                   successTimeSeries: onTimeSeriesSuccess,
                   successDataTable: function (data) {
                       errorLog("No Data Table data Implemented", {
                           module: moduleName,
                           metric: widgetName
                       });
                   },
                   fail: function (data) {
                       errorLog("call failed", {
                           module: moduleName,
                           metric: widgetName
                       });
                   }
               }
           }
            var AccessibleModule = {};
            
            function HighChartMethods(){
                return {
                    coinFlow: coinFlow,
                    coinFlowByCategory: coinFlowCat,
                    whaleBuyReport: whaleBuyReport,
                    whaleSpendReport: whaleSpendReport
                };
            }
            function DTMethods(){
                return {
                    noTableMethods:"no table methods Implemented"
                };
            } 

            if(!isDataTable){
                AccessibleModule = HighChartMethods()
            } else{
                AccessibleModule = DTMethods()
            }
            return AccessibleModule;   
        };


        function UserSessions(scope, isDataTable) {
            var moduleName = "User";
            var privacyCompare = function () {
                var processor = new Processor(moduleName, widgetName);
                var widgetName = "PrivacyChartData"
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){

                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        });
                    },
                    fail: function(data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        });
                    }
                }
            }
            var installsDAU = function () {

                moduleName = "Retention";
                var widgetName = "installslogins";
                var processor = new Processor(moduleName, widgetName);
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Response){
                    if(Object.prototype.toString.call( Response.data ) === '[object Array]'){
                        scope.ts.ChartConfig.options.yAxis = [];                        
                        for (var i = Response.data.length - 1; i >= 0; i--) {

                            for(var prop in Response.data[i]){
                                if(prop === 'yAxis'){
                                    scope.ts.ChartConfig.options.yAxis.push({
                                        title: {
                                            text: Response.data[i].name
                                        },
                                        stackLabels: {
                                            enabled: false,
                                            style: {
                                                fontWeight: 'bold',
                                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                            }
                                        },
                                        alternateGridColor: '#FDFFD5'
                                    })
                                }
                            }
                            scope.ts.ChartConfig.series.push(Response.data[i]); 
                        };
                    }else{
                        scope.ts.ChartConfig.series = Response.data;
                    }
                    scope.ts.ChartConfig.options.tooltip = {
                        formatter: function () {
                            var s = '<b>' + moment.utc(this.x).format('ddd YYYY/MM/DD') + '</b>',
                                sum = 0;

                            $.each(this.points, function (i, point) {
                                s += '<br/><span><b>' + point.series.name + '</b><span style="color:' + point.series.color + ' !important">\u25CF</span>' + '</span>: ' +
                                    point.y.commaSeparate();
                                sum += point.y;
                            });

                            s += '<br/><b>Sum</b>: ' + sum.commaSeparate()

                            return s;
                        },
                        shared: true
                    }
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        });
                    },
                    fail: function(data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        });
                    }
                }
            }            
            var usersByRegion = function () {
                var widgetName = "GetUsersByRegion";
                var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){

                    scope.ts.ChartConfig.options.exporting = {
                        enabled: true
                    };

                    scope.ts.ChartConfig.options.chart = {
                        type: 'area',
                        events: {
                            load: function(){
                                console.log("loaded");
                            }
                        },
                        zoomType: 'x'
                    };

                    scope.ts.ChartConfig.options.xAxis = {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    };

                    scope.ts.ChartConfig.options.yAxis = {
                        min: 0,
                        title: {
                            text: 'total Users Online'
                        },
                        stackLabels: {
                            enabled: false,
                            style: {
                                fontWeight: 'bold',
                                color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                            }
                        }
                    };

                    scope.ts.ChartConfig.options.tooltip = {
                        formatter: function () {
                            var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                sum = 0;
                            $.each(this.points, function (i, point) {
                                s += '<br/>' + point.series.name + ': <b>' +
                                    point.y + ' users</b>';
                                sum += point.y;
                            });

                            s += '<br/><b>Sum: ' + sum + ' users</b>'

                            return s;
                        },
                        shared: true
                    };

                    scope.ts.ChartConfig.options.plotOptions = {
                        area: {
                            stacking: 'normal',
                            lineColor: '#666666',
                            lineWidth: 1,
                            marker: {
                                lineWidth: 1,
                                lineColor: '#666666'
                            }
                        }
                    };
                    scope.ts.ChartConfig.series = data.data;
                }   

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }   
            var currentOnline = function (game, region, interval, start, end) {

                var widgetName = "getCurrentOn";
                var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){
                    scope.ts.ChartConfig.options.xAxis = {
                        type: 'datetime',
                        dateTimeLabelFormats: {
                            //day: '%e of %b'
                        }
                    };
                scope.ts.ChartConfig = {
                        options: {
                            exporting: {
                                enabled: true
                            },
                            chart: {
                                type: 'area',
                                events: {
                                    load: function(){
                                        console.log("loaded");
                                    }
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Total Users Online'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                        sum = 0;
                                    $.each(this.points, function (i, point) {
                                        s += '<br/>' + point.series.name + ': <b>' +
                                            point.y + ' users</b>';
                                        sum += point.y;
                                    });

                                    s += '<br/><b>Sum: ' + sum + ' users</b>'

                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: data.data,
                        title: {
                            text: 'Currently Online'
                        },
                        loading: false
                   }                    
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var RetentionReport = function(){
                moduleName = "Retention";
                var widgetName = "Report";
                var deferred = $q.defer();                
                var dtRetentionArray = [];

                var processor = new Processor(moduleName, widgetName);
                
                function onDataTableSuccess(R){


                    
                    scope.dt.isRefresh = true;
                    scope.dt.datarefreshRate = 1000 * 60 * 60 * 12;
                    scope.dt.toggleRefresh = function (val) {
                        if (!val) {
                            clearInterval(vm.dataRefreshInterval);
                            $(".timerstatus").html("<span style='color: red;'>Data Refresh off</span>");
                            setTimeout(function () {
                                $(".timerstatus").html("");
                            }, 5 * 1000)
                            return;
                        }
                        scope.dt.dataRefreshInterval = setInterval(scope.dt.reloadData, scope.dt.datarefreshRate);
                        $(".timerstatus").html("<span style='color: green;'>Table will refresh every {0} hours</span>".format(scope.dt.datarefreshRate / (1000* 60 * 60)));
                        setTimeout(function () {
                            $(".timerstatus").html("");
                        }, 5 * 1000)
                    }

                    scope.dt.reloadData = function () {

                        var resetPaging = true;
                        dtRetentionArray = [];
                        scope.dt.dtInstance.reloadData(callback, resetPaging);
                    };

                    var percents = {};
                    var Rows = getDays(R.data);
                    $.each(Rows, function (idx, el) {
                        var dtRow = {
                            RecordDate: el.date,
                            installs: el.newUsers,
                            logins: el.logins,
                            day1: "-",
                            day2: "-",
                            day3: "-",
                            day4: "-",
                            day5: "-",
                            day6: "-",
                            day7: "-",
                            day8: "-",
                            day9: "-",
                            day10: "-",
                            day11: "-",
                            day12: "-",
                            day13: "-",
                            day14: "-",
                        };

                        $.each(el.twoWeeks, function (idx, perc) {
                            dtRow["day" + (idx + 1).toString()] = perc;
                            if (perc != "N/A" || "-") {
                                perc.replace("%", "");
                                //console.log(perc);
                                if (!percents["day" + (idx + 1).toString()]) {
                                    percents["day" + (idx + 1).toString()] = 0
                                }
                                perc = parseInt(perc);
                                percents["day" + (idx + 1).toString()] = parseInt(perc);
                                percents["day" + (idx + 1).toString()] = ((percents["day" + (idx + 1).toString()] + perc) / Rows.length);
                            }

                        });
                        //console.log(percents);
                        dtRetentionArray.push(dtRow);
                    });

                    var columns = [];
                        var dtRow = {
                            RecordDate: "",
                            installs: "",
                            day1: "-",
                            day2: "-",
                            day3: "-",
                            day4: "-",
                            day5: "-",
                            day6: "-",
                            day7: "-",
                            day8: "-",
                            day9: "-",
                            day10: "-",
                            day11: "-",
                            day12: "-",
                            day13: "-",
                            day14: "-",
                        };
                        for(var property in dtRow){
                            var colObj = {};
                            colObj.title = property.toUpperCase();
                            colObj.data = property;
                            colObj.class = "center";
                            columns.push(colObj);
                        }           
                        var tbl = $('#' + scope.dt.tableInfo.id).dataTable( {
                            "data": dtRetentionArray,
                            "columns": columns,
                            "lengthMenu": [14, 28, 42, 56],
                            "ordering": true,
                            "aaSorting": []
                        });

                      
                    scope.dt.dataRefreshInterval = setInterval(scope.dt.reloadData, scope.dt.datarefreshRate);
                }

                function onTimeSeriesSuccess(data){

                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: onDataTableSuccess,
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }
            
            var RetentionAverageTimeseries = function(){

                moduleName = "Retention";
                var widgetName = "ReportAverage";
                var processor = new Processor(moduleName, widgetName);
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){

                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                type: 'line',
                                events: {
                                    load: function () { }
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Weekly Average'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var AverageSessionLengthPoints = [];
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                        sum = 0;
                                    $.each(this.points, function (i, point) {
                                        s += '<br/>' + point.series.name + ': <b>' +
                                            parseFloat(point.y).toFixed(2) + ' %</b>';
                                        AverageSessionLengthPoints.push(point);
                                        //sum += point.y;
                                    });

                                    AverageSessionLengthPoints.sort(function (a, b) {
                                        return b.y - a.y;
                                    })
                                    s += '<br/><b>Max: ' + AverageSessionLengthPoints[0].series.name + ': ' + parseFloat(AverageSessionLengthPoints[0].y).toFixed(2) + '% </b>'

                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: data.data,
                        title: {
                            text: 'Weekly Average'
                        },
                    }
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }                
            }

            var ReturnerTimeSeries = function(){
                moduleName = "Retention";
                var widgetName = "ReturnersSeries";
                var processor = new Processor(moduleName, widgetName);
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){
                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                type: 'line',
                                events: {
                                    load: function () { }
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: '%'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var AverageSessionLengthPoints = [];
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>';
                                    $.each(this.points, function (idx, el) {
                                        s += "<br\><b>" + this.series.name +": " + this.point.y + " %</b>"
                                    })
                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: data.data,
                        title: {
                            text: 'NURR CURR RURR'
                        },
                    }
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }
            var ReturnersDataTable = function(){
                moduleName = "Retention";
                var widgetName = "ReturnersDataTable";
                var processor = new Processor(moduleName, widgetName);
                // var processor = new Processor(moduleName, widgetName);
                function onDataTableSuccess(Response){
                    console.log("RETURNER DATA %o", Response);
                    scope.dt.columns = [];
                    Response.data[0].columns.forEach(function(x){
                        var colObj = {};
                        colObj.title = x.toUpperCase();
                        colObj.data = x;
                        colObj.class = "center";
                        scope.dt.columns.push(colObj);
                    })   
                    Response.data.shift();
                    Response.data.map(function (x) {
                        x.Date = moment.utc(x.Date).format("MM/DD/YYYY");
                    });
                    scope.dt.data = Response.data;
                    $('#' + scope.dt.tableInfo.id).dataTable( {
                        "data": scope.dt.data,
                        "columns": scope.dt.columns,
                        "lengthMenu": [14, 28, 42, 56],
                        "ordering": true,
                        "aaSorting": []
                    });
                }

                function onTimeSeriesSuccess(data){

                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: onDataTableSuccess,
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }            
            var DailyActiveUserByGame = function (Request) {
                var widgetName = "DailyActiveUserByGame"
                var processor = new Processor(moduleName, widgetName);

                function onTimeSeriesSuccess(Request) {

                    console.log(Request);

                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                type: 'column',
                                events: {
                                    load: function () { }
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Daily Active Users'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var AverageSessionLengthPoints = [];
                                    var s = this.y + '<br/>' + 
                                        '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b>';

                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: [],
                        title: {
                            text: 'Daily Active Users'
                        },
                    }
                    Request.data.forEach(function (x) {
                        scope.ts.ChartConfig.series.push(x);
                    })
                }
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function (data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })
                    },
                    fail: function (data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var DollarCostPerDAU = function(Request) {
                moduleName = "Cost";
                var widgetName = "DollarCostAveragePerDAU";
                var processor = new Processor(moduleName, widgetName);

                function onTimeSeriesSuccess(Response) {
                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                events: {
                                    load: function () { }
                                },
                                zoomType: 'xy'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY") + '</b><br>';
                                    
                                    // dau
                                    s += '<br>' + this.points[0].series.name + ': <b>' + this.points[0].y.commaSeparate() + ' users</b>';
                                    
                                    // dollar
                                    s += '<br>' + this.points[1].series.name + ': <b>$' + this.points[1].y.toFixed(2) + '</b>';

                                    return s;
                                },
                                shared: true
                            }
                        },
                        title: {
                            text: 'Dollar Cost per DAU'
                        }
                    }
                    scope.ts.ChartConfig.options.yAxis = [];
                    scope.ts.ChartConfig.series = [];

                    for (var i = 0; i < Response.data.length; i++) {
                        for (var prop in Response.data[i]) {
                            if (prop === 'yAxis') {
                                scope.ts.ChartConfig.options.yAxis.push({
                                    title: {
                                        text: Response.data[i].name
                                    },
                                    stackLabels: {
                                        enabled: false,
                                        style: {
                                            fontWeight: 'bold',
                                            color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                        }
                                    },
                                    alternateGridColor: '#FDFFD5',
                                    opposite: (i % 2 === 1) ? true : false
                                });
                            }
                        }
                        scope.ts.ChartConfig.series.push(Response.data[i]);
                    }
                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function (data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })
                    },
                    fail: function (data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var AccessibleModule = {};
            
            function HighChartMethods(){
                return {
                    privacyCompare: privacyCompare,
                    dailyinstallslogins: installsDAU,
                    currentOnline: currentOnline,
                    usersOnlineByRegion: usersByRegion,
                    RetentionAverageTimeseries: RetentionAverageTimeseries,
                    ReturnerTimeSeries: ReturnerTimeSeries,
                    DailyActiveUserByGame: DailyActiveUserByGame,
                    DollarCostPerDAU: DollarCostPerDAU
                };
            }
            function DTMethods(){
                return {
                    RetentionReport:RetentionReport,
                    ReturnersDataTable : ReturnersDataTable
                };
            }            
            if(!isDataTable){
                AccessibleModule = HighChartMethods()
            } else{
                AccessibleModule = DTMethods()
            }
            return AccessibleModule;        
        };
        function HostingInstances(scope, isDataTable) {
            var moduleName = "HostingInstances";

            var privacyCompare = function () {
                var processor = new Processor(moduleName, widgetName);
                var widgetName = "PrivacyChartData"
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(data){

                }

                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var AccessibleModule = {};
            function HighChartMethods(){
                return {
                    noChartMethods:"no chart methods Implemented"
                };
            }
            function DTMethods(){
                return {
                    noTableMethods:"no table methods Implemented"
                };
            }            
            if(!isDataTable){
                AccessibleModule = HighChartMethods()
            } else{
                AccessibleModule = DTMethods()
            }
            return AccessibleModule;   
        
        };
        function GameSessions(scope, isDataTable) {
            var moduleName = "Game";

            var privacyCompare = function () {
                var widgetName = "PrivateVsPublic"
                var processor = new Processor(moduleName, widgetName);                
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Request){
                    console.log(Request);
                    scope.ts.ChartConfig = {
                        options: {
                            chart: {
                                type: 'area',
                                events: {
                                    load: function(){}
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Game Sessions'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                        sum = 0;
                                    $.each(this.points, function (i, point) {
                                        s += '<br/>' + point.series.name + ': <b>' +
                                            point.y + ' sessions</b>';
                                        sum += point.y;
                                    });

                                    s += '<br/><b>Sum: ' + sum + ' sessions</b>'

                                    return s;
                                },
                                shared: true
                            },
                            plotOptions: {
                                column: {
                                    stacking: 'normal'
                                }
                            }
                        },

                        series: [],
                        title: {
                            text: 'Private vs Public Sessions'
                        },
                        loading: false
                    }
                    Request.data.forEach(function(x){
                        scope.ts.ChartConfig.series.push(x);
                    })                    
                }
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }

            var sessionLength = function(Request){
                var widgetName = "SessionLengthGraphData"
                var processor = new Processor(moduleName, widgetName);                

                function onTimeSeriesSuccess(Request){

                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                type: 'spline',
                                events: {
                                    load: function(){}
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Average Length (in minutes)'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var AverageSessionLengthPoints = [];
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                        sum = 0;
                                    $.each(this.points, function (i, point) {
                                        s += '<br/>' + point.series.name + ': <b>' +
                                            parseFloat(point.y).toFixed(2) + ' minutes</b>';
                                        AverageSessionLengthPoints.push(point);
                                        //sum += point.y;
                                    });
                                    
                                    AverageSessionLengthPoints.sort(function (a, b) {
                                        return b.y - a.y;
                                    })
                                    s += '<br/><b>Max: ' + AverageSessionLengthPoints[0].series.name  + ' ' + parseFloat(AverageSessionLengthPoints[0].y).toFixed(2) + ' minutes</b>'

                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: [],
                        title: {
                            text: 'Average Game Session Length'
                        },
                    }
                    Request.data.forEach(function (x) {
                        x.data = $.map(x.data, function (x) {
                           return x / 60000;
                        });
                        scope.ts.ChartConfig.series.push(x);
                    })                    
                }
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function(data){
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })                        
                    },
                    fail: function(data){
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }

            }

            var OnlineBySessionType = function (Request) {
                var widgetName = "UsersOnlineBySessionType"
                var processor = new Processor(moduleName, widgetName);
                // var processor = new Processor(moduleName, widgetName);
                function onTimeSeriesSuccess(Request) {

                    console.log(Request);

                    scope.ts.ChartConfig = {
                        options: {
                            loading: {
                                style: {
                                    opacity: 1
                                }
                            },
                            chart: {
                                type: 'spline',
                                events: {
                                    load: function () { }
                                },
                                zoomType: 'x'
                            },
                            xAxis: {
                                type: 'datetime',
                                dateTimeLabelFormats: {
                                    //day: '%e of %b'
                                }
                            },
                            yAxis: {
                                min: 0,
                                title: {
                                    text: 'Users Online'
                                },
                                stackLabels: {
                                    enabled: false,
                                    style: {
                                        fontWeight: 'bold',
                                        color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                                    }
                                }
                            },
                            tooltip: {
                                formatter: function () {
                                    var s = '<b>' + moment.utc(this.x).format("MM/DD/YYYY HH:mm:ss") + '</b>',
                                        sum = 0;
                                    $.each(this.points, function (i, point) {
                                        s += '<br/>' + point.series.name + ': <b>' +
                                            point.y + ' users</b>';
                                        sum += point.y;
                                    });

                                    s += '<br/><b>Sum: ' + sum + ' users</b>'

                                    return s;
                                },
                                shared: true
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
                                }
                            }
                        },

                        series: [],
                        title: {
                            text: 'Users Online By Session Type'
                        },
                    }
                    Request.data.forEach(function (x) {
                        scope.ts.ChartConfig.series.push(x);
                    })
                }
                return {
                    process: processor,
                    successTimeSeries: onTimeSeriesSuccess,
                    successDataTable: function (data) {
                        errorLog("No Data Table data Implemented", {
                            module: moduleName,
                            metric: widgetName
                        })
                    },
                    fail: function (data) {
                        errorLog("call failed", {
                            module: moduleName,
                            metric: widgetName
                        })
                    }
                }
            }



            var AccessibleModule = {};

            function HighChartMethods(){
                return {
                    privacyCompare : privacyCompare,
                    sessionLength: sessionLength,
                    OnlineBySessionType: OnlineBySessionType
                };
            }

            function DTMethods() {
                return {
                    noTableMethods:"no table methods Implemented"
                };
            }            
            if(!isDataTable){
                AccessibleModule = HighChartMethods()
            } else{
                AccessibleModule = DTMethods()
            }
            return AccessibleModule;   
        
        };

    }

    function Processor(moduleName, metricName){
        var p = this;
        p.widgetName = metricName;
        p.moduleName = moduleName;
        p.serviceUrl = window.location.origin;
    }
   
    Processor.prototype.makeServiceCall = function(attrs){
        if(attrs && typeof attrs.dataUrl != "string"){
            throw new Error("No url in service call");
            errorLog("No url in service call", attrs);                
        }
        console.log("attrs %o", attrs);

        var DataCallAttrs = {
            moduleName: this.moduleName ? this.moduleName : null,
            widgetName: this.widgetName ? this.widgetName : null,
            controllerName: (attrs && attrs.controllerId) ? attrs.controllerId : null,
            url: (attrs && attrs.dataUrl) ? attrs.dataUrl : null,
            ajaxCallback: (attrs && attrs.success) ? attrs.success : function(){

            }
        }

        $.extend(DataCallAttrs, attrs);

        return this.getDataPromise(DataCallAttrs);
    }

    Processor.prototype.getDataPromise = function(DataCallAttrs) {
    
        // so this will grab a success function out of the attributes and call that function which can do stuff in another context,
        // or it will return a promise for data.  
        //these ideas are very similar and this and the above function should be combined and pick one method or the other 

        var deferral = $q.defer();

        var eventArgs = {
            moduleId: DataCallAttrs.moduleName,
            controllerId: DataCallAttrs.controllerName,
            widgetId: DataCallAttrs.widgetName
        };

        var successFn = function (data, status, xhr) {
            //common.$broadcast(common.config.ajaxSuccess, eventArgs);
            var coolResponse = new Response(data, eventArgs, {
                status: status,
                response: xhr.status
            });
            DataCallAttrs.ajaxCallback.call(this, coolResponse);

            deferral.resolve(coolResponse);
        }
        var failFn = function(){}
        console.log("send %o", DataCallAttrs);
        $.ajax({
            url: DataCallAttrs.url,
            crossDomain: true,
            success: successFn,
            beforeSend: DataCallAttrs.before
        })

        return $q.when(deferral.promise);
    }

    Processor.prototype.parseArgs = function(attrs){

        var requestString = "?";
        for(var property in attrs){

            if(attrs[property] !== null && typeof attrs[property] === 'function'){
                continue;
            }
            if(attrs[property] !== null && typeof attrs[property] === 'object'){
                requestString = requestString + property + "=" + attrs[property].id + "&";                                            
            }else if(property !== null){
                requestString = requestString + property + "=" + attrs[property] + "&";    
            }
        
        }
         // console.log("requestString %s", requestString);
         return requestString.substring(0, requestString.length - 1);
    }

    Processor.prototype.process = function(request){

        //if Request.method = 'GET'
        //when I need to implement more HTTP methods can decide about that here now if there is a property for that on the request obj
        var args = (request) ? request : {}; 
        var urlAttrs = (request) ? Processor.prototype.parseArgs(request) : ""; 
        this.urlAttrs = urlAttrs;
        this.dataUrl = args.dataUrl = this.serviceUrl + "/" + this.moduleName +"/" + this.widgetName + urlAttrs; 
    
        console.log("process request : %o", args);
        return this.makeServiceCall(args);

    }
})();

