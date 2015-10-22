(function () {
    'use strict';
    var controllerId = 'economy';
    angular.module('app').controller(controllerId, ['common','datacontext', 'widgetinterface', economy]);
        

    function economy(common, datacontext, widgets) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
                
        var intervals = common.OptionsEnums.TimeInterval;
        var regions = common.OptionsEnums.AWSRegions;
        var chartTypes = common.OptionsEnums.ChartTypes;
        
        var vm = this;

        vm.title = 'Economy';
        vm.PlaytricsModule = 'GameEconomy';

        function TimeSeriesConfiguration(moduleName, metricName, chartTitle, extraParams){
            var widgetDefs = widgets.getWidgetDefinitions();            
            var tsdef = widgetDefs[0];
            
            this.config = {};
            $.extend(this.config, tsdef);
            this.config.dataModelOptions = Object.create(tsdef.dataModelOptions);
            this.module = getModule(moduleName);
            this.metric = getMetric(this.module, metricName);
            this.config.title = chartTitle;
            this.config.dataModelOptions.module = this.module;
            this.config.dataModelOptions.metricName = this.metric;
            this.config.definition = tsdef;                        
            //console.log(this.metric + " > %o ", this.config);
            if (arguments.length > 3) {
                this.config.extraParams = extraParams;
            }
            return this.config;

        }

        function PieChartConfiguration(moduleName, metricName, chartTitle, extraParams) {
            var widgetDefs = widgets.getWidgetDefinitions();
            var piedef = widgetDefs[1];

            this.config = {};
            $.extend(this.config, piedef);
            this.config.dataModelOptions = Object.create(piedef.dataModelOptions);
            this.module = getModule(moduleName);
            this.metric = getMetric(this.module, metricName);
            this.config.title = chartTitle;
            this.config.dataModelOptions.module = this.module;
            this.config.dataModelOptions.metricName = this.metric;
            this.config.definition = Object.create(piedef);

            if (arguments.length > 3) {
                this.config.extraParams = extraParams;
            }

            //console.log('config---- %o', this);
            return this.config;

        }

        function DataTablesConfiguration(moduleName, metricName, chartTitle, tableId){
            var widgetDefs = widgets.getWidgetDefinitions();            
            
            //need to put a pointer to the data promise on data model for data tables
            // as well as some representation of the column definitions.

            //we need to build our columns here probably as an array of objects, each with a solumn name and a display name.

            var dtdef = widgetDefs[2];   
            this.config = {};
            $.extend(this.config, dtdef); //try Object.create(tsde)f
            this.config.dataModelOptions = Object.create(dtdef.dataModelOptions);        
            this.module = getModule(moduleName);
            this.metric = getMetric(this.module, metricName);
            this.config.title = chartTitle;
            this.config.dataModelOptions.module = this.module;
            this.config.dataModelOptions.metricName = this.metric;            
            this.config.tableId = tableId;
            this.config.definition = dtdef;
            return this.config;

        }

        function getModule(moduleName){
            var generator = new common.widgetControl();            
            var modules = generator.getModules(datacontext);
            var module = null;
            for(var i = 0; i < modules.length; ++i) {
                if(modules[i].name == moduleName) {
                    module = modules[i];
                    break;
                }
            }
            return module;            
        }
        function getMetric(moduleName, metricName){
            var module = null;
            var metric = null;

            if(typeof moduleName === "object"){
                module = moduleName;
                //console.log(module);                
            } else if(typeof moduleName == "string"){
                var module = getModule(moduleName);  
                //console.log(module);
            }
            searchObj(module);
            function searchObj( obj ){

                for( var key in obj ) {

                    if( typeof obj[key] === 'object' ){
                        searchObj( obj[key] );
                    }

                    if( key === metricName ){
                        //console.log( 'property=' + key + ' value=' + obj[key]);
                        metric = key;
                        break;                        
                    }

                }

            }            
            return metric;           
        }        
       
        vm.moduleInfo = {
            economyInfo: [
                {
                    name: "Inflow Outflow Category Breakdown",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "coinFlowByCategory", "Inflow Outflow Category Breakdown")
                }
            ],
            whale: {
                buys: {
                    name: "Buy Whales",
                    pies: [
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 1%", { cohort: 1, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 5%", { cohort: 5, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 10%", { cohort: 10, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 1%", { cohort: 1, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 5%", { cohort: 5, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleBuyReport", "Top 10%", { cohort: 10, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        }
                    ],
                    tables: []
                },
                spends: {
                    name: "Spend Whales",
                    pies: [
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 1%", { cohort: 1, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 5%", { cohort: 5, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 10%", { cohort: 10, start: moment.utc('2015-04-28 15:00').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 1%", { cohort: 1, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 5%", { cohort: 5, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        },
                        {
                            config: new PieChartConfiguration(vm.PlaytricsModule, "whaleSpendReport", "Top 10%", { cohort: 10, start: moment.utc().subtract(4, 'weeks').toJSON() })
                        }
                    ],
                    tables: []
                },
                purchase: {
                    pies: [],
                    tables: []
                }
            }
        }
        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                    log("activate fired");
                });
        }        
    }
 
})();