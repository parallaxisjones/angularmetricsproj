(function () {
    'use strict';
    var controllerId = 'game';
    angular.module('app').controller(controllerId, ['common','datacontext', 'widgetinterface', gamesessions]);
        

    function gamesessions(common, datacontext, widgets) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);
                
        var intervals = common.OptionsEnums.TimeInterval;
        var regions = common.OptionsEnums.AWSRegions;
        var chartTypes = common.OptionsEnums.ChartTypes;
        
        var vm = this;

        vm.title = 'Game Sessions';
        vm.PlaytricsModule = 'GameSessions';
        function TimeSeriesConfiguration(moduleName, metricName, chartTitle, extraParams) {
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

        function PieChartConfiguration(moduleName, metricName, chartTitle) {
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

        function DataTablesConfiguration(moduleName, metricName, chartTitle, tableId) {
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
            gameInfo: [
                {
                    name: "Average Game Session Length",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "sessionLength", "Average Game Session Length")
                },
                {
                    name: "Online Session Types",
                    config: new TimeSeriesConfiguration(vm.PlaytricsModule, "OnlineBySessionType", "Online Session Types", {
                        end: moment.utc().toJSON()
                    })
                }
            ],          
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
