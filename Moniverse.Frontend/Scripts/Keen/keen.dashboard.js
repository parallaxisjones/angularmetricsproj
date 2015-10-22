var client = new Keen({
  projectId: "54d4f1d7d2eaaa109ff065c7",
  readKey: "5a293e884fd9b132ebadf60554f793e357039b8deb150811b90df45053bbebf5d0df0b535a81abbd8b58c5548e2d15e7d40535201edfd06056e8521867d111a1f403fe14f900116ca3f1c9fd2ae6feef04c1ce71cc17b71171917cf237c392d56e46cfe510b8e1aa6a562a206dcb038f"
});


Keen.ready(function(){

  // ----------------------------------------
  // Pageviews Area Chart
  // ----------------------------------------
  //var latency = new Keen.Query("sum", {
  //  eventCollection: "latency",
  //  timeframe: "this_24_hours",
  //  targetProperty: "value",
  //  interval: "hourly"
  //});
  //client.draw(latency, document.getElementById("chart-01"), {
  //  chartType: "areachart",
  //  title: false,
  //  height: 109,
  //  width: "auto",
  //  chartOptions: {
  //    chartArea: {
  //      height: "85%",
  //      left: "5%",
  //      top: "5%",
  //      width: "80%"
  //    },
  //    isStacked: true
  //  }
  //});

  //var uptime = new Keen.Query("sum", {
  //  eventCollection: "uptime",
  //  timeframe: "this_24_hours",
  //  targetProperty: "value",
  //  interval: "hourly"
  //});

  //client.draw(uptime, document.getElementById("chart-02"), {
  //  chartType: "areachart",
  //  title: false,
  //  height: 109,
  //  width: "auto",
  //  chartOptions: {
  //    chartArea: {
  //      height: "85%",
  //      left: "5%",
  //      top: "5%",
  //      width: "80%"
  //    },
  //    isStacked: true
  //  }
  //});

  //var packets = new Keen.Query("sum", {
  //  eventCollection: "packets",
  //  timeframe: "this_24_hours",
  //  targetProperty: "value",
  //  interval: "hourly"
  //});
  //client.draw(packets, document.getElementById("chart-03"), {
  //  chartType: "areachart",
  //  title: false,
  //  height: 109,
  //  width: "auto",
  //  chartOptions: {
  //    chartArea: {
  //      height: "85%",
  //      left: "5%",
  //      top: "5%",
  //      width: "80%"
  //    },
  //    isStacked: true
  //  }
  //});

  // client.draw(pageviews_timeline, document.getElementById("chart-04"), {
  //   chartType: "areachart",
  //   title: false,
  //   height: 109,
  //   width: "auto",
  //   chartOptions: {
  //     chartArea: {
  //       height: "85%",
  //       left: "5%",
  //       top: "5%",
  //       width: "80%"
  //     },
  //     isStacked: true
  //   }
  // });



});