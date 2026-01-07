<script setup>
  import Header from './components/Header.vue'
  import Footer from './components/Footer.vue'
</script>

<template>
  <Header />
  <main class="content">
    <router-view />
  </main>
  <Footer />
</template>

<style scoped>
  .content {
    flex: 1;
    padding: 2rem 1rem; /* Plus respirant sur mobile */
    max-width: 1280px;
    width: 100%;
    margin: 0 auto;
  }

  /* Responsive fin */
  @media (min-width: 768px) {
    .content {
      padding: 3rem 2rem;
    }
  }

  @media (min-width: 1024px) {
    .content {
      padding: 4rem;
    }
  }
</style>
