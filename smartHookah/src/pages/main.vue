<template>
  <q-page class="flex flex-center">
    <div class="fixed-top Mlogo">
<img
        src="~assets/manaM.svg"
        style="width:8vw;max-width:150px;"
      >
    </div>

<color-circle>
  <a class="startText" href="/enter">Start&nbsp;<i class="fa fa-play"></i></a>
</color-circle>
      <div>
<q-carousel class="text-white vartical-bottom q-mt-lg" style="margin-top:120%">
  <q-carousel-slide class="bg-primary">
    Place 1
  </q-carousel-slide>
  <q-carousel-slide class="bg-secondary">
    Place 2
  </q-carousel-slide>
  <q-carousel-slide class="bg-tertiary">
    Place 3
  </q-carousel-slide>
</q-carousel>
      </div>
  </q-page>
</template>

<script>

import Slick from 'vue-slick'
import ColorCircle from '../components/colorCircle.vue'

export default {
  name: 'PageMain',
  data () {
    return {
      slickOptions: {
        slidesToShow: 3
        // Any other options that can be got from plugin documentation
      }
    }
  },
  components: {
    'slick': Slick,
    'colorCircle': ColorCircle
  }
}
</script>

<style>
div.Mlogo {
  padding: 1rem;
}

</style>
