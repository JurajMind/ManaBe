<template>
 <q-tabs slot="footer"
   position="bottom" >
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
         to="/"
        exact
        slot="title"
      >
      <div class="circle">
           <img
        src="~assets/manaMBlack.svg"
        style="width:8vw;max-width:150px;"
      >
          </div>

      </q-route-tab>
            <q-route-tab
           replace  to="/enter"
        exact
        slot="title"
        icon="alarm"
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

.circle{
    border-radius: 100%;
    background-color: white;
    padding: 1rem;
}
</style>
