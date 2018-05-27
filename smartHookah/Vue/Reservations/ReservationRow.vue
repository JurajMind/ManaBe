<template>
  <tr>
    <td>{{table.Name}}</td>
    <td class='tableCapacity'>{{table.Capacity}}</td>
    <reservation_colunm v-for="(time,index) in timesOrder"
                        :key="index"
                        v-bind:table="table"
                        v-bind:timeSlot="time"
                        v-bind:index="index"
                        v-bind:reservation="reservations[time.Id]"
                        v-bind:selected="selected" v-bind:selection="selection">
    </reservation_colunm>

  </tr>
</template>

<script>
  import ReservationColunm from './ReservationColunm.vue'

  export default {  
    name: 'reservation_row',
    components: {
      'reservation_colunm': ReservationColunm,
    },
  props: ["table", "times", "reservations", "selected", "selection"],
    computed: {
    timesOrder: function() {

      function compare(a, b) {

        var aValue = a.Value > 400 ? a.Value : a.Value + 3000;
        var bValue = b.Value > 400 ? b.Value : b.Value + 3000;
        if (aValue < bValue)
          return -1;
        if (aValue > bValue)
          return 1;
        return 0;
      }

      return this.times.sort(compare);
    }
    }
  }
</script>
