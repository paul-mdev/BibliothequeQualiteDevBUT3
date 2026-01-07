< <script setup>
  import Header from './components/Header.vue'
  import Footer from './components/Footer.vue'
 
</script>

<template>
  <div class="layout">
    <Header />

    <main class="content">
      <router-view />
    </main>

    <Footer />
  </div>
</template>

<style>
  html, body, #app {
    height: 100%;
    margin: 0;
  }

  .layout {
    min-height: 100%;
    display: flex;
    flex-direction: column;
  }

  .content {
    flex: 1;
    padding: 2rem;
  }
</style>
