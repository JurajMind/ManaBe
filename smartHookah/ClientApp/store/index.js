import Vue from 'vue'
import Vuex from 'vuex'

Vue.use(Vuex)

// TYPES
const MAIN_SET_COUNTER = 'MAIN_SET_COUNTER'

// STATE
const state = {
    counter: 0,
    isLoggedIn: !!localStorage.getItem("token")
}


const LOGIN = "LOGIN";
const LOGIN_SUCCESS = "LOGIN_SUCCESS";
const LOGOUT = "LOGOUT";

// MUTATIONS
const mutations = {
    [MAIN_SET_COUNTER](state, obj) {
        state.counter = obj.counter
    },
    [LOGIN](state) {
        state.pending = true;
    },
    [LOGIN_SUCCESS](state) {
        state.isLoggedIn = true;
        state.pending = false;
    },
    [LOGOUT](state) {
        state.isLoggedIn = false;
    }
}

// ACTIONS
const actions = ({
    setCounter({ commit }, obj) {
        commit(MAIN_SET_COUNTER, obj)
    }
})

export default new Vuex.Store({
    state,
    mutations,
    actions
});
