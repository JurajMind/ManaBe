
// Or import them all -- but notice that your
// bundle will probably include unused ones,
// so more KB over the wire that never get used.
import 'quasar-extras/animate'

<template>

  <div id="q-app">
    <router-view />
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
</style>
