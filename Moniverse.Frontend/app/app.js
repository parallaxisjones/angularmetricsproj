//cb function is a function that executes after the ajax call has completed
(function () {
    'use strict';
    
    var app = angular.module('app', [
        // Angular modules 
        'ngAnimate',        // animations
        'ngRoute',          // routing
        'ngSanitize',       // sanitizes html bindings (ex: sidebar.js)
        'ui.sortable',
        'highcharts-ng',
        'ngCookies',
        // Custom modules 
        'common',           // common functions, logger, spinner
        'common.bootstrap', // bootstrap dialog wrapper functions
        // 3rd Party Modules
        'ui.bootstrap',      // ui-bootstrap (ex: carousel, pagination, dialog),
        'ui.bootstrap.modal',
        'ui.dashboard',
        'pvWidgets'
    ]);
    
    // Handle routing errors and success events
    app.run(['$route',  function ($route) {
            // Include $route to kick start the router.
        }]);        
})();
Highcharts.setOptions({
    lang: {
        loading: "No data to display"
    }
});

if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined' ? args[number] : match;
        });
    };
}

if (!Number.prototype.commaSeparate) {
    Number.prototype.commaSeparate = function (val) {
        var val = this;
        while (/(\d+)(\d{3})/.test(val.toString())) {
            val = val.toString().replace(/(\d+)(\d{3})/, '$1' + ',' + '$2');
        }
        return val;
    }
}
if (!Date.prototype.addDays) {
    Date.prototype.addDays = function (days) {
        var dat = new Date(this.valueOf());
        dat.setDate(dat.getDate() + days);
        return dat;
    }
}

function Response(data, meta, status){
    this.data = data;
    this.meta = meta;
    this.status = status;
}
Response.prototype = {
    data: [],
    meta: {},
    status: {}
}
function PlaytricsRequest(requestParams) {

    if (!requestParams) return;
    
    if(requestParams.success && typeof requestParams.success === 'function'){
        this.success = requestParams.success;                
    }
    if (requestParams.before && typeof requestParams.before === 'function') {
        this.before = requestParams.before;
    }
    if (requestParams.complete && typeof requestParams.complete === 'function') {
        this.complete = requestParams.complete;
    }
    if(requestParams.game && typeof requestParams.game === 'string'){
        this.game = requestParams.game;                
    }
    if(requestParams.region && (typeof requestParams.region === 'string' || typeof requestParams.region === 'Number')){
        this.region = requestParams.region.id;
    }
    if(requestParams.interval && (typeof requestParams.interval === 'string' || typeof requestParams.interval === 'Number')){
        this.interval = requestParams.interval.id;                
    }
    if(requestParams.start && typeof requestParams.start === 'string'){
        this.start = requestParams.start;                
    }
    else if(requestParams.start && requestParams.start instanceof Date || requestParams.start instanceof moment().constructor){
        this.start = moment(requestParams.start).toJSON();                
    }
    if(requestParams.end && typeof requestParams.end === 'string'){
        this.end = requestParams.end;                
    }
    else if(requestParams.end && requestParams.end instanceof Date || requestParams.end instanceof moment().constructor){
        this.end = moment(requestParams.end).toJSON();                
    }

    if(requestParams.extraParams && typeof requestParams.extraParams === 'object' )
    {
        $.extend(this, requestParams.extraParams);
    }

} 
PlaytricsRequest.prototype = {
    game: "all",
    region: 0,
    interval: 15,
    start: moment.utc().subtract(7, 'days').startOf('day').toJSON(),
    end: moment.utc().subtract(1, 'days').endOf('day').toJSON(),
    success: null,
    before: null
}

function parseCSharpDateTime(jsonDate) {
    //this is for lazy cases where for whatever reason you have the c# DateTime representation of a JSON date eg "\/Date(1444361464000)\/"
    var offset = new Date().getTimezoneOffset() * 60000;
    var parts = /\/Date\((-?\d+)([+-]\d{2})?(\d{2})?.*/.exec(jsonDate);

    if (parts[2] == undefined)
        parts[2] = 0;

    if (parts[3] == undefined)
        parts[3] = 0;

    return new Date(+parts[1] + offset + parts[2] * 3600000 + parts[3] * 60000);
};