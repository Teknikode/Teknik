var cpuUsageChart;
var memUsageChart;
var networkUsageChart;

$(document).ready(function () {
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
                text: 'Percentage %'
            },
            max: 100,
            min: 0,
            format: '{value}',
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

    networkUsageChart = new Highcharts.chart({
        chart: {
            useUTC: false,
            renderTo: 'network-usage-chart',
            type: 'line',
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
        series: [
            {
                name: 'Sent',
                data: []
            },
            {
                name: 'Received',
                data: []
            }
        ]
    });

    // Fire Off the request data
    requestData();
});

function requestData() {
    $.ajax({
        type: "GET",
        url: getUsageURL,
        success: function (response) {
            if (response.result) {
                // Tick Time
                var x = (new Date()).getTime();

                // CPU Usage
                cpuUsageChart.series[0].addPoint([x, response.result.cpu.total], false, cpuUsageChart.series[0].data.length > 20);
                if (showWebCPU) {
                    cpuUsageChart.series[1].addPoint([x, response.result.cpu.web], false, cpuUsageChart.series[1].data.length > 20);
                }
                if (showDatabaseCPU) {
                    cpuUsageChart.series[2].addPoint([x, response.result.cpu.db], false, cpuUsageChart.series[2].data.length > 20);
                }

                // Database Usage
                memUsageChart.series[0].addPoint([x, response.result.memory.totalUsed], false, memUsageChart.series[0].data.length > 20);
                if (showWebCPU) {
                    memUsageChart.series[1].addPoint([x, response.result.memory.webUsed], false, memUsageChart.series[1].data.length > 20);
                }
                if (showDatabaseCPU) {
                    memUsageChart.series[2].addPoint([x, response.result.memory.dbUsed], false, memUsageChart.series[2].data.length > 20);
                }

                // Network Usage
                networkUsageChart.series[0].addPoint([x, response.result.network.sent], false, networkUsageChart.series[0].data.length > 20);
                networkUsageChart.series[1].addPoint([x, response.result.network.received], false, networkUsageChart.series[1].data.length > 20);

                // Redraw the charts
                cpuUsageChart.redraw();
                memUsageChart.redraw();
                networkUsageChart.redraw();

                // call it again right away
                setTimeout(requestData, 100);
            }
            else {
                var err = response;
                if (response.error) {
                    err = response.error.message;
                }
                $("#top_msg").css('display', 'inline', 'important');
                $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + err + '</div>');
            }
        },
        cache: false
    });
}