$(document).ready(function () {
    $('#bills-section').collapse('hide');
    $('#oneTime-section').collapse('hide');
    $('#donations-section').collapse('hide');
    $('#takedowns-section').collapse('hide');

    var visitChartOptions = {
        chart: {
            renderTo: 'visitor-chart'
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
            }
        },
        tooltip: {
            shared: true,
            crosshairs: true,
            headerFormat: '<span style="font-size: 10px">{point.key:%B %e, %Y}</span><br/>',
            pointFormat: '<span style="color:{point.color}">\u25CF</span> {series.name}: <b>{point.y}</b><br/>'
        },
        series: [
            {
                name: 'All Visitors'
            },
            {
                name: 'Unique Visitors'
            }
        ]
    };

    if (statsEnabled) {
        $.ajax({
            type: "GET",
            url: getVisitorDataURL,
            success: function (response) {
                if (response.result) {
                    visitChartOptions.series[0].data = response.result.totalVisitors;
                    visitChartOptions.series[1].data = response.result.uniqueVisitors;
                    var visitChart = new Highcharts.chart(visitChartOptions);
                }
                else {
                    var err = response;
                    if (response.error) {
                        err = response.error.message;
                    }
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + err + '</div>');
                }
            }
        });
    }
});