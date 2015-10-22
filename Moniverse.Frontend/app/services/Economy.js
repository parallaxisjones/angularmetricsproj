(function () {
    'use strict';
    var $q;
    var serviceId = 'Economy';
    angular.module('app').factory(serviceId, ['common', 'config', service]);

    function service(common, config) {
        return function GameEconomy(scope, isDataTable) {
            var Processor = common.Processor;

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
            var whaleBuyReport = function () {
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
                            headerFormat: '',
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

            function HighChartMethods() {
                return {
                    coinFlow: coinFlow,
                    coinFlowByCategory: coinFlowCat,
                    whaleBuyReport: whaleBuyReport,
                    whaleSpendReport: whaleSpendReport
                };
            }
            function DTMethods() {
                return {
                    noTableMethods: "no table methods Implemented"
                };
            }

            if (!isDataTable) {
                AccessibleModule = HighChartMethods()
            } else {
                AccessibleModule = DTMethods()
            }
            return AccessibleModule;
        };
    }



})();