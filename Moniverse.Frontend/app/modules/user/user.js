(function () {
    'use strict';
    var controllerId = 'user';
    angular.module('app').controller(controllerId, ['common','datacontext', 'widgetinterface', usersessions]);
        

    function usersessions(common, datacontext, widgets) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
                
        var intervals = common.OptionsEnums.TimeInterval;
        var regions = common.OptionsEnums.AWSRegions;
        var chartTypes = common.OptionsEnums.ChartTypes;
        
        var vm = this;

        vm.title = 'User Sessions';
        vm.PlaytricsModule = 'UserSessions';

        function TimeSeriesConfiguration(moduleName, metricName, chartTitle, extraParams, opts) {
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

            if (arguments.length > 3) {
                this.config.extraParams = extraParams;
                this.config.extraOptions = (opts) ? opts : {};
            }
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
            userInfo: [
                {
                    name: "Currently Online",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "currentOnline", "Current Online", {
                        end: moment.utc().toJSON(),
                        start: moment.utc().subtract(14, 'days').startOf('day').toJSON()
                    })
                },
                {
                    name: "Users Online By Region",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "usersOnlineByRegion", "Users Online By Region", {
                        end: moment.utc().toJSON(),
                        start: moment.utc().subtract(1, 'days').startOf('day').toJSON()
                    })
                },
                {
                    name: "Daily Active Users",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "DailyActiveUserByGame", "Daily Active")
                },
                {
                    name: "Dollar Cost per DAU",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "DollarCostPerDAU", "Dollar Cost per DAU")
                },
                {
                    name: "DAU Breakdown by Logins and Installs",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "dailyinstallslogins", "DAU Breakdown by Logins and Installs")
                }
            ],
            NCR: {
                chart: {
                name: "NURR CURR RURR",
                config: new TimeSeriesConfiguration(vm.PlaytricsModule, "ReturnerTimeSeries", "NURR CURR RURR", {
                    start: moment.utc().subtract(14, 'days').startOf('day').toJSON(),
                    end: moment.utc().startOf('day').toJSON()
                })
                },
                table : {
                    name: "NURR CURR RURR",
                    config: new DataTablesConfiguration(vm.PlaytricsModule, "ReturnersDataTable", "NURR CURR RURR", "NCRTable")
                }
            },
            Retention: {
                chart:{
                    name: "Average Weekly 14 Day Retention",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "RetentionAverageTimeseries", "Average Weekly 14 Day Retention")
                },
                table : {
                    name: "Retention",
                    config: new DataTablesConfiguration(vm.PlaytricsModule, "RetentionReport", "Retention Data", "retentionTable")
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

    //moduleInfo.chart[0].name    
})();