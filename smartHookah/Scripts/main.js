function DrawnMixChart(chartId) {

    var $char = $('#' + chartId + ':visible');

    if ($char.length === 0)
        return true;

    if ($char.siblings('iframe').length !== 0)
        return true;

    var data = {
        labels: [
            "Red",
            "Blue",
            "Yellow"
        ],
        datasets: [
            {
                data: [300, 50, 100],
                backgroundColor: [
                    "#FF6384",
                    "#36A2EB",
                    "#FFCE56",
                    "#7CFC00",
                    "#660066",

                ],
                hoverBackgroundColor: [
                    "#FF6384",
                    "#36A2EB",
                    "#FFCE56",
                    "#7CFC00",
                    "#660066",
                ]
            }]
    };

    var options = {
        responsive: true,
        maintainAspectRatio: false
    }
    

    var ctx = document.getElementById(chartId).getContext("2d");
    ctx.canvas.width = 250;
    ctx.canvas.height = 250;

    data.labels = [];
    data.datasets[0].data = [];
    var $mixPart = $('.mixPart');

    var totalWeight = 0;
    $mixPart.each(function (index) {

        var mixData = $(this).data('parts');

        if (mixData !== null && mixData !== undefined ) {
            console.log(mixData);

            mixData.forEach(function (element) {
                data.labels.push(element.brand + ' ' + element.name);
                data.datasets[0].data.push(element.fraction);
            });
            return true;
        }
        var name = $(this).data('name');
        var weight = $(this).data('weight');

        data.labels.push(name);
        data.datasets[0].data.push(weight);
        totalWeight = totalWeight + parseInt(weight);
    });

    if (data.labels.length === 0) {
        data.labels.push('No tobacco');
        data.datasets[0].data.push(100);
    }


    var mixChart = new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: options
    });
}

function clearDrawerClasses($container) {
    var classes = ["bmd-drawer-f-l", "bmd-drawer-f-r", "bmd-drawer-f-t", "bmd-drawer-f-b"];

    $.each(classes, function (index, value) {
        $container.removeClass(value)
    })
}

function setDrawerPosition(position) {
    var $container = $('.bmd-layout-container')

    clearDrawerClasses($container)
    $container.addClass(position)
}

$(document).ready(function () {
    var buttons = ["drawer-f-l", "drawer-f-r", "drawer-f-t", "drawer-f-b"]

    $.each(buttons, function (index, position) {
        $('#' + position).click(function () {
            setDrawerPosition('bmd-' + position)
        })
    })

    // add a toggle for drawer visibility that shows anytime
    $('#drawer-visibility').click(function () {
        var $container = $('.bmd-layout-container')

        // once clicked, just do away with responsive marker
        //$container.removeClass('bmd-drawer-in-md')

        var $btn = $(this)
        var $icon = $btn.find('.material-icons')
        if ($icon.text() === 'visibility') {
            $container.addClass('bmd-drawer-out') // demo only, regardless of the responsive class, we want to force it close
            $icon.text('visibility_off')
            $btn.attr('title', 'Drawer allow responsive opening')
        }
        else {
            $container.removeClass('bmd-drawer-out') // demo only, regardless of the responsive class, we want to force it open
            $icon.text('visibility')
            $btn.attr('title', 'Drawer force closed')
        }
    })
})

function findWithAttr(array, attr, value) {
    for (var i = 0; i < array.length; i += 1) {
        if (array[i][attr] === value) {
            return i;
        }
    }
    return -1;
}
