var cpuUsageChart;
var memUsageChart;
var networkUsageChart;

$(document).ready(function () {
    $('#bills-section').collapse('hide');
    $('#oneTime-section').collapse('hide');
    $('#donations-section').collapse('hide');
    $('#takedowns-section').collapse('hide');

    /* ----------------------------------------
                CPU Usage                      
    -----------------------------------------*/
    cpuUsageChart = new Highcharts.chart({
        chart: {
            useUTC: false,
            renderTo: 'cpu-usage-chart',
            type: 'line',
            marginRight: 10
        },
        title: {
            text: 'CPU Usage'
        },
        xAxis: {
            type: 'datetime',
            tickPixelInterval: 150
        },
        yAxis: {
            title: {
                text: 'Percentage'
            },
            max: 100,
            min: 0,
            labels: {
                format: '{value}%'
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }]
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            pointFormat: '<span style="color:{point.color}">\u25CF</span> {series.name}: <b>{point.y:.2f}%</b><br/>'
        },
        plotOptions: {
            line: {
                marker: {
                    enabled: false,
                    symbol: 'circle',
                    radius: 2,
                    states: {
                        hover: {
                            enabled: true
                        }
                    }
                }
            }
        },
        credits: {
            enabled: false
        },
        series: [
            {
                name: 'Total',
                data: []
            },
            {
                name: 'Website',
                data: []
            },
            {
                name: 'Database',
                data: []
            }
        ]
    });

    /* ----------------------------------------
                Memory usage                      
    -----------------------------------------*/
    memUsageChart = new Highcharts.chart({
        chart: {
            useUTC: false,
            renderTo: 'mem-usage-chart',
            type: 'line',
            marginRight: 10
        },
        title: {
            text: 'Memory Usage'
        },
        xAxis: {
            type: 'datetime',
            tickPixelInterval: 150
        },
        yAxis: {
            title: {
                text: 'Memory'
            },
            min: 0,
            max: totalMemory,
            tickInterval: tickInterval,
            labels: {
                formatter: function () {
                    return filesize(this.value);
                }
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }]
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            pointFormatter: function () {
                var yVal = filesize(this.y);

                return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + yVal + '</b><br/>';
            }
        },
        plotOptions: {
            line: {
                marker: {
                    enabled: false,
                    symbol: 'circle',
                    radius: 2,
                    states: {
                        hover: {
                            enabled: true
                        }
                    }
                }
            }
        },
        credits: {
            enabled: false
        },
        series: [
            {
                name: 'Total',
                data: []
            },
            {
                name: 'Website',
                data: []
            },
            {
                name: 'Database',
                data: []
            }
        ]
    });

    /* ----------------------------------------
                Network Usage                      
    -----------------------------------------*/
    networkUsageChart = new Highcharts.chart({
        chart: {
            useUTC: false,
            renderTo: 'network-usage-chart',
            marginRight: 10
        },
        title: {
            text: 'Network Usage'
        },
        xAxis: {
            type: 'datetime',
            tickPixelInterval: 150
        },
        yAxis: {
            title: {
                text: 'Speed'
            },
            min: 0,
            labels: {
                formatter: function () {
                    return getReadableBandwidthString(this.value);
                }
            },
            plotLines: [{
                value: 0,
                width: 1,
                color: '#808080'
            }]
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            pointFormatter: function () {
                var yVal = getReadableBandwidthString(this.y);

                return '<span style="color:' + this.color + '">\u25CF</span> ' + this.series.name + ': <b>' + yVal + '</b><br/>';
            }
        },
        plotOptions: {
            area: {
                marker: {
                    enabled: false,
                    symbol: 'circle',
                    radius: 2,
                    states: {
                        hover: {
                            enabled: true
                        }
                    }
                }
            },
            line: {
                marker: {
                    enabled: false,
                    symbol: 'circle',
                    radius: 2,
                    states: {
                        hover: {
                            enabled: true
                        }
                    }
                }
            }
        },
        credits: {
            enabled: false
        },
        series: [
            {
                type: 'line',
                name: 'Sent',
                dashStyle: 'Dash',
                color: '#7cb5ec',
                data: []
            },
            {
                type: 'area',
                name: 'Received',
                color: '#7cb5ec',
                fillOpacity: 0.3,
                data: []
            }
        ]
    });

    /* ----------------------------------------
                Visitor History                      
    -----------------------------------------*/
    visitChart = new Highcharts.chart({
        chart: {
            renderTo: 'visitor-chart',
            type: 'line',
            marginRight: 10
        },
        title: {
            text: 'Daily Visitors'
        },
        xAxis: {
            type: 'datetime',
            dateTimeLabelFormats: { // don't display the dummy year
                month: '%e. %b',
                year: '%b'
            },
            title: {
                text: 'Date'
            }
        },
        yAxis: {
            title: {
                text: 'Visitors'
            },
            min: 0
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            headerFormat: '<span style="font-size: 10px">{point.key:%B %e, %Y}</span><br/>',
            pointFormat: '<span style="color:{point.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>'
        },
        credits: {
            enabled: false
        },
        series: [
            {
                name: 'All Visitors',
                data: []
            },
            {
                name: 'Unique Visitors',
                data: []
            }
        ]
    });


    /* ----------------------------------------
       Websocket for updating realtime stats                      
    -----------------------------------------*/
    var ticker = $.connection.serverUsage;

    ticker.client.updateServerUsage = function (serverUsage) {
        // Tick Time
        var x = (new Date()).getTime();

        // CPU Usage
        cpuUsageChart.series[0].addPoint([x, serverUsage.cpu.total], false, cpuUsageChart.series[0].data.length > 20);
        if (showWebCPU) {
            cpuUsageChart.series[1].addPoint([x, serverUsage.cpu.website], false, cpuUsageChart.series[1].data.length > 20);
        }
        if (showDatabaseCPU) {
            cpuUsageChart.series[2].addPoint([x, serverUsage.cpu.database], false, cpuUsageChart.series[2].data.length > 20);
        }

        // Database Usage
        memUsageChart.series[0].addPoint([x, serverUsage.memory.used], false, memUsageChart.series[0].data.length > 20);
        if (showWebCPU) {
            memUsageChart.series[1].addPoint([x, serverUsage.memory.websiteUsed], false, memUsageChart.series[1].data.length > 20);
        }
        if (showDatabaseCPU) {
            memUsageChart.series[2].addPoint([x, serverUsage.memory.databaseUsed], false, memUsageChart.series[2].data.length > 20);
        }

        // Network Usage
        networkUsageChart.series[0].addPoint([x, serverUsage.network.sent], false, networkUsageChart.series[0].data.length > 20);
        networkUsageChart.series[1].addPoint([x, serverUsage.network.received], false, networkUsageChart.series[1].data.length > 20);

        // Redraw the charts
        cpuUsageChart.redraw();
        memUsageChart.redraw();
        networkUsageChart.redraw();
    }

    $.connection.hub.start();

    /* ----------------------------------------
       Load the data for Visitor History                      
    -----------------------------------------*/
    if (showVisitorStats) {
        $.ajax({
            type: "GET",
            url: getVisitorDataURL,
            success: function (response) {
                if (response.result) {
                    visitChart.series[0].setData(response.result.totalVisitors, false);
                    visitChart.series[1].setData(response.result.uniqueVisitors, false);
                    visitChart.redraw();
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(response) + '</div>');
                }
            }
        });
    }

    // Resize the chart when viewing the tab (initial width is wrong due to chart being hidden)
    $('a[href="#site-stats"]').on('shown.bs.tab',
        function (e) {
            if (showVisitorStats) {
                visitChart.setSize($('#visitor-chart').width(), $('#visitor-chart').height());
            }
        });

    // Resize the chart when viewing the tab (initial width is wrong due to chart being hidden)
    $('a[href="#realtime-stats"]').on('shown.bs.tab',
        function(e) {
            cpuUsageChart.setSize($('#cpu-usage-chart').width(), $('#cpu-usage-chart').height());
            memUsageChart.setSize($('#mem-usage-chart').width(), $('#mem-usage-chart').height());
            networkUsageChart.setSize($('#network-usage-chart').width(), $('#network-usage-chart').height());
        });
});
