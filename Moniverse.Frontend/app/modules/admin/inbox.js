(function () {
    'use strict';
    var controllerId = 'inboxCtrl';
    var messageCtrlId = 'messageCtrl';
    angular.module('app').controller(controllerId, ['$scope', '$filter', 'common', 'Notifications', inbox]);
    angular.module('app').controller(messageCtrlId, ['$scope', message]);

    function inbox($scope, $filter, common, notifications) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        $scope.date = new Date;
        $scope.sortingOrder = 'id';
        $scope.pageSizes = [10, 20, 50, 100];
        $scope.reverse = false;
        $scope.filteredItems = [];
        $scope.groupedItems = [];
        $scope.itemsPerPage = 10;
        $scope.pagedItems = [];
        $scope.currentPage = 0;
        /* inbox functions -------------------------------------- */
        $scope.table;
        // get data and init the filtered items
        $scope.init = function () {
            var request = new PlaytricsRequest({
                game: "DD2",
                success: GetNotificationsTable,
                end: moment.utc().toJSON()
            });

            notifications.get(request);

            $('#notifications tbody').on('click', 'tr', function () {
                $scope.readMessage($scope.table.row(this).index());
            });
        }


        function GetNotificationsTable(result) {
            $scope.items = result.data;
            $scope.columns = [];

            for (var prop in $scope.items[0]) {
                var colObj = {};
                colObj.title = prop;
                colObj.data = prop;
                colObj.class = "center";
                $scope.columns.push(colObj);
            }
            $scope.items.forEach(function (x) {
                x.CreatedAt = moment.utc(parseCSharpDateTime(x.CreatedAt)).format("MM-DD-YYYY HH:mm:SS");
            })
            //$scope.search();
            $scope.table = $('#notifications').DataTable({
                "data": $scope.items,
                "columns": $scope.columns,
                "lengthMenu": [14, 28, 42, 56],
                "ordering": true,
                "aaSorting": []
            });

            $('#notifications tbody').on('click', 'tr', function () {
                $scope.readMessage($scope.table.row(this).index())
            });
        }

        function toTitleCase(str) {
            return str.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
        }

        var searchMatch = function (haystack, needle) {
            if (!needle) {
                return true;
            }
            return haystack.toLowerCase().indexOf(needle.toLowerCase()) !== -1;
        };

        // filter the items
        $scope.search = function () {
            $scope.filteredItems = $filter('filter')($scope.items, function (item) {
                for (var attr in item) {
                    if (searchMatch(item[attr], $scope.query))
                        return true;
                }
                return false;
            });
            $scope.currentPage = 0;
            // now group by pages
            $scope.groupToPages();
        };

        // calculate page in place
        $scope.groupToPages = function () {
            $scope.selected = null;
            $scope.pagedItems = [];

            for (var i = 0; i < $scope.filteredItems.length; i++) {
                if (i % $scope.itemsPerPage === 0) {
                    $scope.pagedItems[Math.floor(i / $scope.itemsPerPage)] = [$scope.filteredItems[i]];
                } else {
                    $scope.pagedItems[Math.floor(i / $scope.itemsPerPage)].push($scope.filteredItems[i]);
                }
            }
        };

        $scope.range = function (start, end) {
            var ret = [];
            if (!end) {
                end = start;
                start = 0;
            }
            for (var i = start; i < end; i++) {
                ret.push(i);
            }
            return ret;
        };

        $scope.prevPage = function () {
            if ($scope.currentPage > 0) {
                $scope.currentPage--;
            }
            return false;
        };

        $scope.nextPage = function () {
            if ($scope.currentPage < $scope.pagedItems.length - 1) {
                $scope.currentPage++;
            }
            return false;
        };

        $scope.setPage = function () {
            $scope.currentPage = this.n;
        };

        $scope.deleteItem = function (idx) {
            var itemToDelete = $scope.pagedItems[$scope.currentPage][idx];
            var idxInItems = $scope.items.indexOf(itemToDelete);
            $scope.items.splice(idxInItems, 1);
            $scope.search();

            return false;
        };

        $scope.isMessageSelected = function () {
            if (typeof $scope.selected !== "undefined" && $scope.selected !== null) {
                return true;
            }
            else {
                return false;
            }
        };

        $scope.readMessage = function (idx) {
            $scope.items[idx].read = true;
            $scope.selected = $scope.items[idx];
        };

        $scope.readAll = function () {
            for (var i in $scope.items) {
                $scope.items[i].read = true;
            }
        };

        $scope.closeMessage = function () {
            $scope.selected = null;
        };

        $scope.renderMessageBody = function (html) {
            return html;
        };

        /* end inbox functions ---------------------------------- */


        // initialize
        $scope.init();
        activate();

        function activate() {
            var promises = [];
            common.activateController(promises, controllerId)
                .then(function (data) {
                    log("activate fired");
                });
        }

    }// end inboxCtrl
    function message($scope) {

        $scope.message = function (idx) {
            return messages(idx);
        };

    };// end messageCtrl
})();

