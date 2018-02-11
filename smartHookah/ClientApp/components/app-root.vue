<template>
    <v-app id="inspire" dark>
        <v-navigation-drawer clipped
                             fixed
                             v-model="drawer"
                             app>
            <nav-menu params="route: router"></nav-menu>
        </v-navigation-drawer>
        <v-toolbar app fixed clipped-left>
            <v-toolbar-side-icon @click.stop="drawer = !drawer"></v-toolbar-side-icon>
            <v-toolbar-title>Application</v-toolbar-title>
        </v-toolbar>
        <v-content>
            <v-container fluid fill-height>
                <v-layout justify-center align-center>
                    <router-view></router-view>
                </v-layout>
            </v-container>
        </v-content>
        <nav-bottom></nav-bottom>
        <v-footer app fixed>

            <span>&copy; 2017</span>
        </v-footer>
    </v-app>
</template>

<script>
    import Vue from 'vue'
    import CounterExample from './counter-example'
    import FetchData from './fetch-data'
    import HomePage from './home-page'
    import NavMenu from './nav-menu'
    import NavBottom from './nav-bottom'

    Vue.component('counter-example', CounterExample);
    Vue.component('fetch-data', FetchData);
    Vue.component('home-page', HomePage);
    Vue.component('nav-menu', NavMenu);
    Vue.component('nav-bottom', NavBottom);
    export default {
        data: {
            drawer:null
        },
        props: {
            source: String
        }
    }
</script>
<style>

</style>