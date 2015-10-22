(function () {
    'use strict';

    // Define the common module 
    // Contains services:
    //  - common
    //  - logger
    //  - spinner
    var commonModule = angular.module('common', []);

    // Must configure the common service and set its 
    // events via the commonConfigProvider
    commonModule.provider('commonConfig', function () {
        this.config = {
            // These are the properties we need to set
            //controllerActivateSuccessEvent: '',
            //spinnerToggleEvent: ''
        };

        this.$get = function () {
            return {
                config: this.config
            };
        };
    });

    commonModule.constant('OptionsEnums', {
    AWSRegions: [
        { id: 0, name: 'All', friendly: 'All Regions' },
        { id: 1, name: 'USEast_NorthVirg', friendly: 'US East (Northern Virginia)' },
        { id: 2, name: 'USWest_NorthCali', friendly: 'US West (Northern California)' },
        { id: 3, name: 'USWest_Oregon', friendly: 'US West (Oregon)' },
        { id: 4, name: 'EU_Ireland', friendly: 'EU (Ireland)' },
        { id: 5, name: 'AsiaPac_Singapore', friendly: 'Asia Pacific (Singapore)' },
        { id: 6, name: 'AsiaPac_Sydney', friendly: 'Asia Pacific (Sydney)' },
        { id: 7, name: 'AsiaPac_Tokyo', friendly: 'Asia Pacific (Tokyo)' },
        { id: 8, name: 'SouthAmer_SaoPaulo', friendly: 'South America (Sao Paulo)' }
    ],
    TimeInterval: [
        { id: 1, interval: 5, friendly: '5 Minute' },
        { id: 3, interval: 15, friendly: '15 Minute' },
        { id: 4, interval: 30, friendly: '30 Minute' },
        { id: 5, interval: 60, friendly: '1 hour' },
        { id: 7, interval: 360, friendly: '6 hour' },
        { id: 8, interval: 720, friendly: '12 Hours' },
        { id: 9, interval: 1440, friendly: 'Day' }
    ],
    ChartTypes: [
        { id: 0, type: 'line', friendly: 'Line' },
        { id: 1, type: 'spline', friendly: 'Spline' },
        { id: 2, type: 'area', friendly: 'Area' },
        { id: 3, type: 'areaspline', friendly: 'Areaspline' },
        { id: 4, type: 'column', friendly: 'Column' },
        { id: 5, type: 'bar', friendly: 'Bar' },
        { id: 6, type: 'pie', friendly: 'Pie' }
    ]

    });

    commonModule.factory('RequestProc', ['$q', function ($q) {
        function Processor(moduleName, metricName) {
            var p = this;
            p.widgetName = metricName;
            p.moduleName = moduleName;
            p.serviceUrl = window.location.origin;
        }

        Processor.prototype.makeServiceCall = function (attrs) {
            if (attrs && typeof attrs.dataUrl != "string") {
                throw new Error("No url in service call");
                errorLog("No url in service call", attrs);
            }
            console.log("attrs %o", attrs);

            var DataCallAttrs = {
                moduleName: this.moduleName ? this.moduleName : null,
                widgetName: this.widgetName ? this.widgetName : null,
                controllerName: (attrs && attrs.controllerId) ? attrs.controllerId : null,
                url: (attrs && attrs.dataUrl) ? attrs.dataUrl : null,
                ajaxCallback: (attrs && attrs.success) ? attrs.success : function () {

                }
            }

            $.extend(DataCallAttrs, attrs);

            return this.getDataPromise(DataCallAttrs);
        }

        Processor.prototype.getDataPromise = function (DataCallAttrs) {

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
            var failFn = function () { }

            $.ajax({
                url: DataCallAttrs.url,
                crossDomain: true,
                success: successFn,
                beforeSend: DataCallAttrs.before
            })

            return $q.when(deferral.promise);
        }

        Processor.prototype.parseArgs = function (attrs) {

            var requestString = "?";

            for (var property in attrs) {

                if (attrs[property] == null || typeof attrs[property] === 'function') {
                    continue;
                }
                if (attrs[property] !== null) {
                    
                    switch (typeof attrs[property]) {
                        case 'object':
                            requestString = requestString + property + "=" + attrs[property].id + "&";
                            break;
                        default:
                            requestString = requestString + property + "=" + attrs[property] + "&";
                            break;
                    }
                } 

            }
            return requestString.substring(0, requestString.length - 1);
        }

        Processor.prototype.process = function (request) {

            //if Request.method = 'GET'
            //when I need to implement more HTTP methods can decide about that here now if there is a property for that on the request obj
            var args = (request) ? request : {};


            var urlAttrs = (request) ? Processor.prototype.parseArgs(request) : "";

            this.dataUrl = args.dataUrl = this.serviceUrl + "/" + this.moduleName + "/" + this.widgetName + urlAttrs;

            console.log("process request : %o", args);
            return this.makeServiceCall(args);

        }

        return Processor;
    }]);

    commonModule.factory('Auth', function () {
        var user;
        var host = window.location.host;
        return {
            setUser: function (aUser) {
                window.localStorage.userInfo = {};
                user = aUser;
                window.localStorage[user.localStorageID] = JSON.stringify(user);
            },
            isLoggedIn: function () {
                var storageId = "playtrics";
                for (var key in localStorage) {
                    if (key.indexOf(storageId)) {
                        var user = localStorage[key];
                        return (user) ? JSON.parse(user) : false;
                    }
                }
            }
        }
    });

    commonModule.factory('moduleConfig', function () {
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
        return {
            Pie: PieChartConfiguration,
            TimeSeries: TimeSeriesConfiguration,
            DataTable: DataTablesConfiguration
            }
    });

    commonModule.factory('CSVconverter', [function () {
        var factory = {};

        var checkEntryForTimestamp = function (timestamp) {
            var valid = (new Date(timestamp)).getTime() > 0;

            return valid;
        }
        Number.prototype.padLeft = function (base, chr) {
            var len = (String(base || 10).length - String(this).length) + 1;
            return len > 0 ? new Array(len).join(chr || '0') + this : this;
        }
        function JSDateToExcelDate(inDate) {

            var d = inDate;
            dformat = [(d.getMonth() + 1).padLeft(),
                d.getDate().padLeft(),
                d.getFullYear()].join('-') +
                ' ' +
              [d.getHours().padLeft(),
                d.getMinutes().padLeft(),
                d.getSeconds().padLeft()].join(':');

            return dformat;
        }

        var parse = function (item) {
            //var dateFomat = 'yyyy-MM-ddTHH:mm:ssZ';
            var getLogLength = function(n){
                if (n == 0) return 1; // because Math.log(0) is undefined
                if (n < 0) n = -n; // because Math.log() doesn't work for negative numbers

                return Math.floor((Math.log10(n))); // log base 10 will return the natural log value common to the number at least for base 10 values, 12 for datetimestamps.  hell yeah
            }

            if (getLogLength(parseInt(item)) < 12 || getLogLength(parseInt(item)) == 'NaN') return item;

            if (checkEntryForTimestamp(item) == true) {
                var date;
                date = new Date(item);
                item = JSDateToExcelDate(date);
                //console.log(item);
            }

            return item;
        }

        factory.ConvertToCSV = function (objArray) {

            //only implement isArray if no native implementation is available
            if (typeof Array.isArray === 'undefined') {
                Array.isArray = function(obj) {
                    return Object.prototype.toString.call(obj) === '[object Array]';
                }
            };

            var array = (objArray != 'undefined' && typeof objArray != 'object') ? JSON.parse(objArray) : objArray;

            var seperator = ",";
            var delimiter = "";
            var str = '';
            var firstLine = '"Series"' + seperator + '"Datetime"'+ seperator + '"Value"' + delimiter + "\n";
            str += firstLine;

            //looking for stupid unix timestamps and turning them back into human readable date time format
            //maybe this is gay and stupid
            for (var i = 0; i < array.length; i++) {
                var seriesName = array[i]["name"];

                for (var x = 0; x < array[i]["data"].length; x++) {
                    var points = array[i]["data"][x];

                    var pointDate = parse(points[0]);
                    var pointValue = points[1];

                    var line = '"' + seriesName + '"' + seperator + '"' + pointDate + '"' + seperator + '"' + pointValue + '"' + "\n";

                    str += line;
                }
            }

            return str;
        }

        return factory;

    }]);

    commonModule.factory('common',
        [
            '$q',
            '$rootScope',
            '$timeout',
            '$window',
            '$interval',
            'commonConfig',
            'logger',
            'OptionsEnums',
            'CSVconverter',
            'Auth',
            'moduleConfig',
            'RequestProc',
            common
        ]);

    function common($q, $rootScope, $timeout, $window, $interval, commonConfig, logger, OptionsEnums, CSVconverter, auth, moduleConfig, proc) {
        var throttles = {};

        var service = {
            // common angular dependencies
            $broadcast: $broadcast,
            $on: $on,
            $q: $q,
            $timeout: $timeout,
            $interval: $interval,
            $window: $window,
            OptionsEnums: OptionsEnums,
            CSVconverter: CSVconverter,
            // generic
            activateController: activateController,
            createSearchThrottle: createSearchThrottle,
            debouncedThrottle: debouncedThrottle,
            isNumber: isNumber,
            logger: logger, // for accessibility
            textContains: textContains,
            widgetControl: WidgetControl,
            config: commonConfig.config,
            Auth: auth,
            Request: proc,
            dateFormats: ['yyyy-MM-ddTHH:mm:ssZ', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate']
        };

        return service;

        function activateController(promises, controllerId) {

            return $q.all(promises).then(function (eventArgs) {
                var data = { controllerId: controllerId };
                $broadcast(commonConfig.config.controllerActivateSuccessEvent, data);
            });
        }

        function $broadcast() {
            return $rootScope.$broadcast.apply($rootScope, arguments);
        }

        function $on(eventName, callback) {

            return $rootScope.$on.call($rootScope, eventName, callback);
        }

        function WidgetControl() {       
            function GetModules(datacontext){
                 var modules = [];
                 (_.memoize(function(datacontext){
                    //console.log(datacontext);
                    var moduleNames = Object.getOwnPropertyNames(datacontext);
                    //console.log("datacontext getOwnPropertyNames: %o", moduleNames);
                    for (var i = moduleNames.length - 1; i >= 0; i--) {
                        var moduleName = moduleNames[i];
                        var newModule = {
                            name: moduleNames[i],
                            methods: {
                                charts: {},
                                tables: {}
                            }
                        };
                        if(typeof datacontext[moduleNames[i]]){
                            var mod = datacontext[moduleNames[i]]();
                            newModule.methods.charts = datacontext[moduleNames[i]]({}, false);
                            newModule.methods.tables = datacontext[moduleNames[i]]({}, true);    
                            // console.log(Object.getOwnPropertyNames(mod));
                        }
                        modules.push(newModule);
                    }
                    //console.log("modules from datacontext: %o", modules);            

                    
                }))(datacontext);
                return modules;                
            } 
                       
            function TimeSeriesConfiguration(moduleName, metricName, chartTitle){
                var widgetDefs = widgets.getWidgetDefinitions();            
                var tsdef = widgetDefs[0];
                var dtdef = widgetDefs[1].dataModelOptions;   
                
                this.config = {};
                $.extend(this.config, tsdef);
                this.config.dataModelOptions = Object.create(tsdef.dataModelOptions);
                this.module = getModule(moduleName);
                this.metric = getMetric(this.module, metricName);
                this.config.title = chartTitle
                this.config.dataModelOptions.module = this.module;
                this.config.dataModelOptions.metricName = this.metric;            
                console.log(this.metric + " > %o ", this.config);
                return this.config;

            }

            function getModule(moduleName, datacontext){           
                var modules = getModules(datacontext);
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
                } else if(typeof moduleName == "string"){
                    var module = getModule(moduleName);   
                }
                for(var i = 0; i < module.methods.length; ++i) {
                    if(module.methods[i] == metricName) {
                        metric = module.methods[i];
                        break;
                    }
                }
                return metric;            
            }     
            function generateWidgetId() {
                function s4() {
                    return Math.floor((1 + Math.random()) * 0x10000)
                      .toString(16)
                      .substring(1);
                }
                return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                  s4() + '-' + s4() + s4() + s4();
            }
            function getWidgetMeta(storageName) {
                var localStorage = $window.localStorage;

                if (storageName) {

                }

                // console.log("storage: %o", JSON.parse(localStorage.getItem("demo_simple")));
                var store = JSON.parse(localStorage.demo_simple).widgets;
                // console.log(store);
                for (var key in store) {
                    // console.log(store[key]);
                }

                return store;
            }
            return {
                getWidgetMeta: getWidgetMeta,
                generateId: generateWidgetId,
                getModules: GetModules,
                getModule: getModule,
                getMetric: getMetric,
                TimeSeriesConfig: TimeSeriesConfiguration
            }
        }

        function createSearchThrottle(viewmodel, list, filteredList, filter, delay) {
            // After a delay, search a viewmodel's list using 
            // a filter function, and return a filteredList.

            // custom delay or use default
            delay = +delay || 300;
            // if only vm and list parameters were passed, set others by naming convention 
            if (!filteredList) {
                // assuming list is named sessions, filteredList is filteredSessions
                filteredList = 'filtered' + list[0].toUpperCase() + list.substr(1).toLowerCase(); // string
                // filter function is named sessionFilter
                filter = list + 'Filter'; // function in string form
            }

            // create the filtering function we will call from here
            var filterFn = function () {
                // translates to ...
                // vm.filteredSessions 
                //      = vm.sessions.filter(function(item( { returns vm.sessionFilter (item) } );
                viewmodel[filteredList] = viewmodel[list].filter(function(item) {
                    return viewmodel[filter](item);
                });
            };

            return (function () {
                // Wrapped in outer IFFE so we can use closure 
                // over filterInputTimeout which references the timeout
                var filterInputTimeout;

                // return what becomes the 'applyFilter' function in the controller
                return function(searchNow) {
                    if (filterInputTimeout) {
                        $timeout.cancel(filterInputTimeout);
                        filterInputTimeout = null;
                    }
                    if (searchNow || !delay) {
                        filterFn();
                    } else {
                        filterInputTimeout = $timeout(filterFn, delay);
                    }
                };
            })();
        }

        function debouncedThrottle(key, callback, delay, immediate) {
            // Perform some action (callback) after a delay. 
            // Track the callback by key, so if the same callback 
            // is issued again, restart the delay.

            var defaultDelay = 1000;
            delay = delay || defaultDelay;
            if (throttles[key]) {
                $timeout.cancel(throttles[key]);
                throttles[key] = undefined;
            }
            if (immediate) {
                callback();
            } else {
                throttles[key] = $timeout(callback, delay);
            }
        }

        function isNumber(val) {
            // negative or positive
            return /^[-]?\d+$/.test(val);
        }

        function textContains(text, searchText) {
            return text && -1 !== text.toLowerCase().indexOf(searchText.toLowerCase());
        }

        //function GetDatatables() {
        //    return dt;
        //}
    }
})();
