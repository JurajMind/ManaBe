import Vue from 'vue'
import Vuex from 'vuex'

import example from './module-example'
import layout from './layout'

Vue.use(Vuex)

const store = new Vuex.Store({
  modules: {
    example,
    layout
  }
})

export default store
