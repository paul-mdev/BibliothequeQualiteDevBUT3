<script setup>
  // ===== IMPORTS DES COMPOSANTS GLOBAUX =====
  // Ces composants sont affichés sur toutes les pages
  import Header from './components/Header.vue'
  import Footer from './components/Footer.vue'
</script>

<template>
  <!-- ===== STRUCTURE DE LA PAGE ===== -->
  <!-- Header : navigation et authentification -->
  <Header />

  <!-- Main : contenu dynamique des pages -->
  <!-- router-view injecte le composant correspondant à la route active -->
  <main class="content">
    <router-view />
  </main>

  <!-- Footer : copyright -->
  <Footer />
</template>

<style scoped>
  /**
   * ===== CONTENEUR PRINCIPAL =====
   * flex: 1 permet au contenu de prendre tout l'espace disponible
   * Cela pousse le footer en bas de page (utilisé avec display: flex sur #app)
   */
  .content {
    flex: 1;
    padding: 2rem 1rem;
    max-width: 1280px;
    width: 100%;
    margin: 0 auto;
  }

  /* ===== RESPONSIVE DESIGN ===== */
  /* Tablettes */
  @media (min-width: 768px) {
    .content {
      padding: 3rem 2rem;
    }
  }

  /* Desktop */
  @media (min-width: 1024px) {
    .content {
      padding: 4rem;
    }
  }
</style>
