<template>
<div class="flex flex-center startButton">
<slot></slot>
</div>
</template>

<script>
export default {
  // name: 'ComponentName',
  data () {
    return {}
  }
}
</script>

<style>

div.startButton{
  position: fixed;
  color:white;
  border-radius: 100%;
  width: 30em;
  height: 30em;
  background:
  linear-gradient(
cyan,transparent
),
  linear-gradient(
-45deg,magenta,transparent
),
linear-gradient(
45deg,yellow,transparent);
  background-blend-mode: multiply;

}

div.startButton a {
  color:white;
  text-decoration: none;
  font-weight: bold;
  font-size: 2em;

}

</style>
