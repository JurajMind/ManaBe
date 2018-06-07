<template>
  <q-layout view="lHh Lpr lFf">
    <q-layout-header v-if="!$q.platform.is.mobile">
      <q-toolbar
        color="primary"
        :glossy="$q.theme === 'mat'"
        :inverted="$q.theme === 'ios'"
      >
        <q-btn
          flat
          dense
          round
          @click="leftDrawerOpen = !leftDrawerOpen"
          aria-label="Menu"
          v-if="!$q.platform.is.mobile"
        >
          <q-icon name="menu" />
        </q-btn>

        <q-toolbar-title>

        </q-toolbar-title>
      </q-toolbar>
    </q-layout-header>

    <q-layout-drawer
      v-model="leftDrawerOpen"
      :content-class="$q.theme === 'mat' ? 'bg-grey-2' : null"
    >
      <q-list
        no-border
        link
        inset-delimiter
      >
        <q-list-header>Essential Links</q-list-header>
      </q-list>
    </q-layout-drawer>
 <q-page-container class="mainBody">
  <transition
  name="fade"
>
      <router-view></router-view>
            </transition>
 </q-page-container>
<q-layout-footer v-if="$q.platform.is.mobile">
<bottom-navigation/>
</q-layout-footer>
  </q-layout>
</template>

<script>
import { openURL } from 'quasar'
import BottomNavigation from './bottom-navigation.vue'
export default {
  name: 'LayoutDefault',
  data () {
    return {
      leftDrawerOpen: this.$q.platform.is.desktop
    }
  },
  methods: {
    openURL
  },
  components: {
    'bottom-navigation': BottomNavigation
  }
}
</script>

<style lang="stylus">
@import '~variables'

div.q-tab:hover{
color:red;
}

.mainBody{
 background-color: $background;
}

.fade-enter-active, .fade-leave-active {
  transition-property: opacity;
  transition-duration: .15s;
}

.fade-enter-active {
  transition-delay: .15s;
}

.fade-enter, .fade-leave-active {
  opacity: 0
}

</style>
