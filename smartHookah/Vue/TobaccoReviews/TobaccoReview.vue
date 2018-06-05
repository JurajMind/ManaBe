<template>
  <div id="tobaccoReview" class="form-horizontal" style="margin-left: auto; margin-right: auto; max-width: 40em">

    <review-chart />

    <div v-show="!edit" class="review-div" id="reviewSliders">
      <fieldset class="form-group">
        <div class="bmd-label-static">@LocalizationHelper.Translate("_TobaccoRevSmokeQ", "SmokeSession")&nbsp;-&nbsp;<h6 style="display: inline; font-weight: bold">{{review.Smoke + '/10'}}</h6></div>
        <div class="form-control">
          <input class="col-md-12 review-slider" type="range" min="0" max="10" step="1" id="Smoke" v-bind:disabled="edit" v-model="review.Smoke" />
        </div>
      </fieldset>

      <div class="form-group">
        <div class="bmd-label-static">@LocalizationHelper.Translate("_TobaccoRevTobTaste", "SmokeSession")&nbsp;-&nbsp;<h6 style="display: inline; font-weight: bold">{{review.Taste + '/10'}}</h6></div>
        <div class="form-control">
          <input class="col-md-12 review-slider" type="range" min="0" max="10" step="1" id="Taste" v-bind:disabled="edit" v-model="review.Taste" />
        </div>
      </div>

      <div class="form-group">
        <div class="bmd-label-static">@LocalizationHelper.Translate("_TobaccoRevTobaccoQ", "SmokeSession")&nbsp;-&nbsp;<h6 style="display: inline; font-weight: bold">{{review.Quality + '/10'}}</h6></div>
        <div class="form-control">
          <input class="col-md-12 review-slider" type="range" min="0" max="10" step="1" id="Quality" v-bind:disabled="edit" v-model="review.Quality" />
        </div>
      </div>

      <div class="form-group">
        <div class="bmd-label-static">@LocalizationHelper.Translate("_TobaccoRevOverallRating", "SmokeSession")&nbsp;-&nbsp;<h6 style="display: inline; font-weight: bold">{{review.Overall + '/10'}}</h6></div>
        <div class="form-control">
          <input class="col-md-12 review-slider" type="range" min="0" max="10" step="1" id="Rating" v-bind:disabled="edit" v-model="review.Overall" />
        </div>
      </div>
    </div>


    <div class="form-group">
      <label class="bmd-label-static">@LocalizationHelper.Translate("_TobaccoRevTextReview", "SmokeSession")</label>
      <div>
        <textarea class="col-md-12 review-text form-control" id="Text" v-bind:disabled="edit" v-model="review.Text"></textarea>
      </div>
    </div>

    <div class="form-group">
      <button v-show="edit" class="btn btn-primary btn-raised" v-bind:disabled="!edit" v-on:click="edit = !edit">@LocalizationHelper.Translate("Edit", "Common")</button>

      <button v-show="!edit" class="btn btn-danger btn-raised" v-bind:disabled="edit" v-on:click="cancelReview()">@LocalizationHelper.Translate("Cancel", "Common")</button>

      <button class="btn btn-success btn-raised" style="float: right" v-bind:disabled="edit" v-on:click="saveReview()">@LocalizationHelper.Translate("SaveChanges", "Common")</button>
    </div>
  </div>
</template>


<script>
    import review_chart from './ReviewChart.vue';

    export default {
    name: 'tobacco_review',
    props: '[sessionId]',
    components: {
      'review-chart' : review_chart
    },
    data: () => {
      return {
        review: {
          Id: '',
          Quality: 0,
          Taste: 0,
          Smoke: 0,
          Overall: 0,
          Text: '',
          SmokeSessionId: ''
        },
        edit: false,
        tobaccoFilled: true
      }
    },

    created: function() {

      $.getJSON('/TobaccoReviews/GetReviewVue/',
        {
          sessionId = this.sessionId
        },
        function(data) {
          if (data != null) {
            vm.$data.review = data;
            vm.$data.edit = true;
            vm.$data.tobaccoFilled = true;
            vm.drawChart();
            if (getTextLines() < 11) {
              $("#Text").animate({ rows: getTextLines() }, 'fast');
            } else {
              $("#Text").animate({ rows: 10 }, 'fast');
            }
          }
        });
    },

    methods: {
      cancelReview: function() {
        $.getJSON('/TobaccoReviews/GetReviewVue/',
          {
            sessionId: this.sessionId
          },
          function(data) {
            if (data != null) {
              vm.$data.review = data;
              vm.$data.edit = true;
              if (getTextLines() < 11) {
                $("#Text").animate({ rows: getTextLines() }, 'fast');
              } else {
                $("#Text").animate({ rows: 10 }, 'fast');
              }
            }
          });
      },

      saveReview: function() {
        this.review.SmokeSessionId = this.sessionId;

        var jqxhr = $.ajax({
          type: "POST",
          url: "/TobaccoReviews/SaveVueReview/",
          data: JSON.stringify({
            Id: this.review.Id,
            Quality: this.review.Quality,
            Taste: this.review.Taste,
            Smoke: this.review.Smoke,
            Overall: this.review.Overall,
            Text: this.review.Text,
            SmokeSessionId: this.review.SmokeSessionId
          }),
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          success: function(data) {
            vm.$data.review = data;
            vm.$data.edit = true;
            vm.drawChart();
          }
        }).fail(function(jqXHR, exception) {
          var msg = '';
          if (jqXHR.status === 0) {
            msg = 'Not connect.\n Verify Network.';
          } else if (jqXHR.status == 404) {
            msg = 'Requested page not found. [404]';
          } else if (jqXHR.status == 500) {
            msg = 'Internal Server Error [500].';
          } else if (exception === 'parsererror') {
            msg = 'Requested JSON parse failed.';
          } else if (exception === 'timeout') {
            msg = 'Time out error.';
          } else if (exception === 'abort') {
            msg = 'Ajax request aborted.';
          } else {
            msg = 'Uncaught Error.\n' + jqXHR.responseText;
          }
          console.log(msg);
        });
      },

      textAreaFocusBlur: function (el, event) {
        switch (event){
          case 'focus':
            console.log('review focus');
            break;
          case 'blur':
            console.log('review blur');
            break;
        };
      }
  }
</script>
