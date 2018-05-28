<template>
  <div>
    <div class="card">
      <div class="card-body">
        <div class="row">
          <div class="form-group col-md-2">
            <label><i class="fa fa-calendar"></i>&nbsp;@LocalizationHelper.Translate("date", "Reservation")</label>
            <input id="datepicker" class="form-control" />
            <div>
              <button class="btn btn-sm btn-raised" onclick="changeDate(-1)"><i class="fa fa-chevron-left"></i></button>
              <button class="btn btn-sm btn-raised" onclick="$('#datepicker').datepicker().value(moment().format('DD.MM.YYYY'))">{{window.resReservation.dbRes('today')}}</button>
              <button class="btn btn-sm btn-raised" onclick="changeDate(1)"><i class="fa fa-chevron-right"></i></button>
            </div>
          </div>

          <div class="form-group col-md-2">
            <label><i class="fa fa-clock-o"></i>@LocalizationHelper.Translate("place", "Reservation")</label>
            <select v-model="Selected.Table" class="form-control oldSelect" v-cloak>
              <option v-for="time in Reservations.Tables" :value="time.Id">{{time.Name}}</option>
            </select>
          </div>
          <div class="form-group col-md-1">
            <label><i class="fa fa-clock-o"></i>@LocalizationHelper.Translate("time", "Reservation")</label>
            <select v-model="Selected.Time" class="form-control oldSelect" v-cloak>
              <option v-for="time in Reservations.TimeSlots" :value="time.Value">{{time.Text}}</option>
            </select>
          </div>
          <div class="form-group col-md-1">
            <label><i class="fa fa-users">@LocalizationHelper.Translate("persons", "Reservation")</i></label>
            <select v-model="Selected.Persons" name="persons" class="form-control oldSelect" v-cloak>
              <option v-for="n in 8" :value="n">{{n}}</option>
              <option value="100">100</option>
            </select>
          </div>

          <div class="form-group col-md-1">
            <label><i class="fa fa-clock-o"></i>@LocalizationHelper.Translate("visitDuration", "Reservation")</label>
            <select v-model="Selected.Duration" class="form-control oldSelect" v-cloak>
              <option v-for="time in Reservations.Times" :value="time.Key">{{time.Value}}</option>
            </select>
          </div>

          <div class="form-group col-md-1">
            <label for="exampleTextarea" class="bmd-label-static">@LocalizationHelper.Translate("name", "Reservation")</label>
            <input type="text" class="form-control" v-model="Selected.Name" id="exampleTextarea" rows="3" />
          </div>

          <div class="form-group col-md-1">
            <label for="exampleTextarea" class="bmd-label-static">@LocalizationHelper.Translate("message", "Reservation")</label>
            <textarea class="form-control" v-model="Selected.Text" id="exampleTextarea" rows="3"></textarea>
          </div>
          <div class="col-md-3">
            <button v-if="canCreateReservation" class="btn btn-success btn-raised" v-on:click="postReservation()"><i class="fa fa-plus-circle"></i>&nbsp;@LocalizationHelper.Translate("create", "Reservation")</button>
            <button v-if="canCreateReservation" class="btn btn-success btn-raised" v-on:click="clearReservation()"><i class="fa fa-eraser"></i></button>
            <div v-if="!canCreateReservation">
              <div v-if="selectedReservationStatus == 3">
                <a v-on:click="changeReservationStatus(4)" class="btn btn-success btn-raised"><i class="fa fa-heart"></i>&nbsp;@LocalizationHelper.Translate("visit", "Reservation")</a>
                <a v-on:click="changeReservationStatus(7)" v-if="selectedReservationStatus == 3" class="btn btn-danger btn-raised"><i class="fa fa-bomb"></i>&nbsp;@LocalizationHelper.Translate("noVisit", "Reservation")</a>
                <br />
              </div>
              <a v-if="selectedReservationStatus == 6 || selectedReservationStatus == 0  " v-on:click="changeReservationStatus(3)" class="btn btn-success btn-raised"><i class="fa fa-check"></i>&nbsp;@LocalizationHelper.Translate("confirm", "Reservation")</a>
              <a v-on:click="changeReservation() " class="btn btn-warning btn-raised btn-sm"><i class="fa fa-edit"></i>&nbsp;@LocalizationHelper.Translate("change", "Reservation")</a>

              <a v-on:click="addTableStart()" class="btn btn-info btn-raised btn-sm" v-bind:class="{ 'btn-success': AddTableMode }"> <i class="fa fa-plus-circle"></i>&nbsp;@LocalizationHelper.Translate("addTable", "Reservation")</a>
              <a v-on:click="cancelReservation()" class="btn btn-danger btn-raised btn-sm"><i class="fa fa-trash"></i>&nbsp;@LocalizationHelper.Translate("cancel", "Reservation")</a>
            </div>
          </div>
        </div>




        <div class="table-responsive">
          <table class="table table-bordered reservationTable">
            <thead>
              <tr>
                <th>@LocalizationHelper.Translate("table", "Reservation")</th>
                <th><i class="fa fa-users"></i></th>
                <th v-for="time in Reservations.TimeSlots">{{time.Text}}</th>
              </tr>
            </thead>

            <tr is="reservation-row" v-for="item in Reservations.Tables" :key="item.Id"
                v-bind:table="item" v-bind:times="Reservations.Reservations[item.Id]"
                v-bind:reservations="Reservations.TodayReservation" v-bind:selected="Selected" v-bind:selection="selection"></tr>


          </table>
        </div>

        <hr />


      </div>
      <div class="card-footer">

      </div>
    </div>
    <div>
      <div class="card-deck">
        <div class="card">
          <h4 class="card-header">@LocalizationHelper.Translate("canceledReservation", "Reservation")</h4>
          <div class="card-body">
            <ul class="canceled">
              <li v-for="canceled in Reservations.Canceled">
                <i class="fa" v-bind:class="[canceled.State == 7 ? 'fa-bomb':'', canceled.State == 1 ? 'fa-trash':'' ]"></i>
                {{canceled.TimeText}} :
                <span v-if="Reservations.Times[canceled.TimeSlots]">{{Reservations.Times[canceled.TimeSlots].Value}}</span>
                {{canceled.Name}}
                <i class="fa fa-undo" v-on:click="changeReservationStatus(3,canceled.Id)"></i>

              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>


<script>
  import ReservationRow from './ReservationRow.vue'
  import moment from 'moment'

  export default {
    props: ["placeId"],
    name: 'reservation_place',
    components: {
      'reservation-row': ReservationRow
    },
    created: function () {
      this.loadReservations();
      $.connection.hub.start()
        .done(function () {
          hub.server.joinPlace(this.placeId);
        });


      $.connection.hub.disconnected(function () {
        console.log("disconected");
        setTimeout(function () {
          $.connection.hub.start();
          console.log("reconect");
        },
          5000); // Re-start connection after 5 seconds
      });
    },
    data: function () {
      return {
        DeniedReason: null,
        PlaceId: this.placeId,
        Selected: {
          Table: null,
          format: 'dd.mm.yyyy',
          Date: moment().format('DD.MM.YYYY'),
          Time: '',
          TimeIndex: 0,
          Duration: 5,
          Persons: 2,
          Reservation: null,
          Name: '',
          Text: '',
          TableItems: [],
          LastReservedTable: 0,
        },
        Reservations: {
          TimeSlots: [],
          Tables: [],
          Reservations: [],
          Canceled: [],
          Times: [],
          MinimumReservationTime: 0,
        },
        AddTableMode: false,
      }
    },
    computed:
      {
        canCreateReservation: function () {
          return this.Selected.Reservation == null;
        },
        selection: function () {
          var result = [];
          for (var i = this.Selected.TimeIndex; i < this.Selected.TimeIndex + this.Selected.Duration; i++) {
            result.push(i);
          }
          return result;
        },
        selectedReservationStatus: function () {
          if (this.Selected.Reservation) {
            var res = this.Reservations.TodayReservation[this.Selected.Reservation];
            return res ? res.State : -1;
          }
          return -1;
        }
      },
    methods: {
      dateChanged() {
        this.Selected.Date = $("#datepicker").val();

        var date = moment($('#datepicker').datepicker().value(), 'DD.MM.YYYY').locale('@CultureInfo.CurrentUICulture.TwoLetterISOLanguageName');
        var titleDate = date.format('D.M.YYYY');
        $('#titleDate').text(titleDate);
        try {
          var titleDay = date.format('dddd').capitalize();
          $('#titleDay').text(titleDay);
        } catch (e) {
          var titleDay = date.format('dddd');
          $('#titleDay').text(titleDay);
        }

        this.loadReservations();
      },
      clearReservation() {

        this.Selected.Name = '';
        this.Selected.Text = '';
        this.Selected.Duration = this.Reservations.Times[0].Key;

      },
      cancelReservation() {
        var r = confirm("Confirm");
        if (r == true) {
          this.changeReservationStatus(1);
        }
      },
      changeReservationStatus: function (status, reservationId = null) {
        if (reservationId == null) {
          reservationId = this.Selected.Reservation;
        }
        $.post(
          '/Reservation/UpdateStatus/',
          {
            id: reservationId,
            state: status
          },
          function (data) {

          });
      },
      addTable: function () {
        this.AddTableMode = false;
        $.post(
          '/Reservation/AddTable/',
          {
            id: this.Selected.Reservation,
            tableId: this.Selected.Table,

          },
          function (data) {
            // v.loadReservations();
          });
      },
      addTableStart: function () {
        this.AddTableMode = true;
      },
      changeReservation: function (callback) {

        $.post(
          '/Reservation/ChangeReservation',
          {
            id: this.Selected.Reservation,
            model: this.Selected,
            tableId: this.Selected.LastReservedTable
          },
          function (data) {
            if (callback)
              callback();


          }
        );
      },
      loadReservations: function () {
        var _this = this;
        $.getJSON('/Reservation/PlaceReservation/',
          {
            id: this.placeId,
            date: this.Selected.Date,
          },
          function (json) {
            _this.Reservations = json;
            if (json.TimeSlots.length > 0)
              _this.Selected.Time = json.TimeSlots[0].Value;

            if (_this.Reservations.Times.length > 0)
              _this.Selected.Duration =
                _this.Reservations.Times[_this.Reservations.MinimumReservationTime].Key;

          });
      },
      postReservation: function () {
        var req = $.post(
          '/Reservation/PostReservation?id=' + '@Model.place.Id',
          this.Selected,
          function (data) {
            //v.loadReservations();
          });


      }
    }
  }
</script>
