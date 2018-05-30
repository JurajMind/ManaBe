<template>
  <td v-on:dragstart="dragStart($event)" v-on:dragend="dragEnd()" :draggable="check" v-bind:colspan="colSpan" v-if="display" class="timeSlot taken" v-on:click="onTakenClick()" v-bind:class="[stateText,check ? 'selected' : '']">
    <div v-if="colSpan !== 0">
      {{reservation.Name}}&nbsp;<i class="fa fa-user"></i>{{reservation.Persons}} ({{table.Capacity - reservation.Persons}} )
      <i v-if="reservation.Message" class="fa fa-comment" data-toggle="tooltip" data-placement="bottom" :title="reservation.Message"> </i>
      <div class="controlls" v-if="check && unconfirmed">
        <i style="cursor:pointer" class="fa fa-check" v-on:click="confirmReservation()"></i>
      </div>
      <div class="controlls" v-if="check && unvisited">
        <i style="cursor:pointer" class="fa fa-heart" v-on:click="confirmVisit()"></i>
      </div>
    </div>
  </td>
  <td v-on:drop="drop($event)" v-on:dragover="allowDrop($event)" v-on:dragenter="dragEnter()" v-on:dragleave="dragLeave()" v-else-if="!timeSlot.Reserved" v-bind:class="{selected : check,over:over}" class="timeSlot free" v-on:click="onFreeClick()"></td>
</template>

<script>
  export default {    
    name: 'ReservationColunm',    
    props: ["table", "timeSlot", "index", "reservation", "selected", "selection"],    
    data: function () {
      return {
        over: false
      }
    },
    computed: {
      colSpan: function () {
        if (this.reservation == undefined)
          return 0;

        return this.reservation.TimeSlots;
      },
      unconfirmed: function () {
        if (this.reservation == undefined)
          return false;
        if (this.reservation.State === 6 || this.reservation.State === 0)
          return true;
        return false;
      },
      unvisited: function () {
        if (this.reservation == undefined)
          return false;
        if (this.reservation.State === 3)
          return true;
        return false;
      },
      display: function () {
        if (this.timeSlot.Reserved && this.reservation != undefined) {
          return true;
        }
        return false;
      },
      check: function () {
        if (this.selected.Table !== this.table.Id)
          return false;

        return this.selection.indexOf(this.index) > -1;
      },
      stateText: function () {
        if (this.reservation == null)
          return "";
        return this.reservation.StateText;
      }

    },
    mounted() {
      this.$root.$on('drop', data => {
        this.over = false;
      });
    },
    methods: {
      onFreeClick: function (taken = false) {

        this.selected.Table = this.table.Id;
        this.selected.Time = this.timeSlot.Value;
        this.selected.TimeIndex = this.index;

        if (this.$root.AddTableMode) {
          this.$root.addTable();
          return;
        }
        if (!taken)
          this.selected.Reservation = null;

      },
      dragStart: function (ev) {
        ev.dataTransfer.setData("reservation", this.reservation.Id);
        ev.dataTransfer.setData("table", this.table.Id);
        // console.log(ev);
      },
      dragEnd: function () {
        this.over = false;
      },
      drop: function (ev) {
        // ev.dataTransfer.setData("text", ev.target.id);
        var reservation = ev.dataTransfer.getData("reservation");
        var changedTable = ev.dataTransfer.getData("table");
        console.log(reservation);
        this.selected.Reservation = reservation;
        this.selected.Table = this.table.Id;
        this.selected.Time = this.timeSlot.Value;
        this.selected.TimeIndex = this.index;
        this.selected.LastReservedTable = changedTable;
        var _this = this;
        var newSelectedTime = this.index;
        var table = this.table.Id;
        this.$root.changeReservation(function () {
          _this.selected.Table = table;
          _this.selected.Time = newSelectedTime;
        });
        this.$root.$emit('drop', null);


      },
      allowDrop: function (ev) {
        ev.preventDefault();
      },
      dragEnter: function () {
        this.over = true;

      },
      dragLeave: function () {
        this.over = false;

      },
      onTakenClick: function () {

        this.selected.Reservation = this.reservation.Id;
        this.selected.Persons = this.reservation.Persons;
        this.selected.Name = this.reservation.Name;
        this.selected.Duration = this.reservation.TimeSlots;
        this.onFreeClick(true);
        this.selected.LastReservedTable = this.table.Id;
      },
      confirmReservation: function () {
        this.$root.changeReservationStatus(3);
      },
      confirmVisit: function () {
        this.$root.changeReservationStatus(4);
      },
      selectReservation: function (selected) {
        $emit('selectReservation', { select: this.reservation, tableId: this.table.Id })
      }
    }
  }
</script>
