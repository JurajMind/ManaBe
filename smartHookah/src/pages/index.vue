<template>
  <q-page class="flex flex-center">
    <div v-if="$q.platform.is.desktop">
  I'm only rendered on desktop!
</div>

<div v-if="$q.platform.is.mobile">
  I'm only rendered on mobile!
</div>

<div v-if="$q.platform.is.electron">
  I'm only rendered on Electron!
</div>
  </q-page>
</template>

<style>
</style>

<script>
export default {
  name: 'PageIndex'
}
</script>
