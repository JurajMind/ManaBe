import Vue from 'vue'
import moment  from 'moment'

import reservation_place from './Reservations/ReservationPlace.vue'
import tobacco_review from './TobaccoReviews/TobaccoReview.vue'
import review_chart from './TobaccoReviews/ReviewChart.vue'

Vue.component('reservation_place', reservation_place);
Vue.component('tobacco_review', tobacco_review);
Vue.component('review_chart', review_chart);


var v = Vue;

export default v;
