<template>
  <q-page class="flex flex-center">
  <color-circle>
  <div class="column float left">
  <label>Code</label>
    <div class="input-widget">
   <input type="number" max="9999" class="input" value="01234" >
   <div class="digit-background">
     <div class="digit"></div>
     <div class="digit"></div>
     <div class="digit"></div>
     <div class="digit"></div>
      <div class="digit"></div>
   </div>
  </div>
  </div>
  </color-circle>
  </q-page>
</template>

<script>
import ColorCircle from '../../components/colorCircle.vue'

export default {
  name: 'SmokeSessionEnter',
  components: {
    'colorCircle': ColorCircle
  }
}
</script>

<style>

.input-widget {
    vertical-align: top;
    margin-left: 1rem;
    font-size: 3rem;
    display: inline-block;
    position: relative;
}

.input-widget .input {
    width: 8rem;
    font-size: inherit;
    font-family: inherit;
    letter-spacing: 5px;
    background-color: transparent;
    border: none;
    -moz-appearance: textfield;
    color:white;
}

.input-widget .input::-webkit-inner-spin-button,
.input-widget .input::-webkit-outer-spin-button {
    -webkit-appearance: none;
    margin: 0;
}

.input-widget .digit-background {
    position: absolute;
    top: 1px;
    left: 0;
    z-index: -1;
}

.input-widget .digit-background .digit {
    display: inline-block;
    float: left;
}

.input-widget .digit-background .digit::before {
    content: '0';
    color: transparent;
    background-color: transparent;
    display: inline-block;
    padding: 1px;
    margin: -1px 4px 0 -1px;
        border-bottom: 5px solid white;
}

</style>
