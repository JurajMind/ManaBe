<template>
  <div class="review-div" id="reviewChartDiv" style="position:relative">
    <canvas class="review-chart" id="reviewChart"></canvas>
  </div>
</template>

<script>
  export default {
    name: 'review_chart',
    props: {
      reviewData: Object
    },
    data: () => {
      return {
        radarChart: '',
        chartOptions: {
          datalabels: {
            display: false
          },
          legend: {
            display: false
          },
          scale: {
            ticks: {
              beginAtZero: true,
              min: 0,
              max: 10,
              stepSize: 1
            },
            pointLabels: {
              fontSize: 16
            }
          }
        },
        datasetLabels: [
          "Session Quality",
          "Tobacco Taste",
          "Tobacco Quality",
          "Overall Rating"
        ]
      }
    },
    methods: {
      drawChart: function () {
        var chartData = {
          labels: [
            "Smoke Quality",
            "Tobacco Taste",
            "Tobacco Quality",
            "Overall Rating"
          ],
          datasets: [
            {
              label: "Rating",
              backgroundColor: "transparent",
              borderColor: "#3f51b5",
              fill: false,
              radius: 5,
              pointRadius: 5,
              pointBorderWidth: 3,
              pointBackgroundColor: "#3f51b5",
              pointBorderColor: "#3f51b5",
              pointHoverRadius: 6,
              data: [
                this.review.Smoke,
                this.review.Taste,
                this.review.Quality,
                this.review.Overall
              ]
            }
          ]
        };
        $("canvas#reviewChart").remove();
        $("div#reviewChartDiv").append('<canvas id="reviewChart"></canvas>');
        this.radarChart = new Chart($("#reviewChart"),
          {
            type: 'radar',
            data: chartData,
            options: this.chartOptions
          });
      }
    }
  }
</script>
