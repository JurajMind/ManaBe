<template>
 <q-tabs>
      <q-route-tab
        icon="mail"
        to="/mails"
        exact
        slot="title"
        label="Mixology"
        two-lines="true"
        glossy="true"

      />

      <q-route-tab
        icon="alarm"
        to="/alarms"
        exact
        slot="title"
        label="Places"
        two-lines="true"
        glossy="true"
      />
            <q-route-tab
        icon="alarm"
        to="/alarms"
        exact
        slot="title"
      />
            <q-route-tab
        icon="alarm"
        to="/alarms"
        exact
        slot="title"
                label="Gear"
        two-lines="true"
        glossy="true"
      />
            <q-route-tab
        icon="alarm"
        to="/alarms"
        exact
        slot="title"
                label="Mixology"
        two-lines="true"
        glossy="Profile"
      />
    </q-tabs>
</template>

<script>
export default {
  name: 'BottomNavigation',
  data () {

  },
  methods: {
  }
}
</script>

<style>
div.q-tab:hover{
color:red;
}

.q-tab-label{
    font-size: 8px
}
</style>
