<template>
    <div>
        <v-list dense v-for="route in routes">
            <!-- TODO: highlight active link -->
            <router-link :to="route.path">
                <span :class="route.style"></span> {{ route.display }}
            </router-link>
        </v-list>
    </div>
</template>

<script>
import { routes } from '../routes'

    export default {   
    data() {
        return {
            routes
        }
    }
}
</script>

<style>

</style>