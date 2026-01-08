<template>
  <footer class="site-footer">
    <p>© 2026 Bibliothèque Qualité Dev</p>
  </footer>
</template>

<style scoped>
  .site-footer {
    background: var(--color-background-soft);
    color: var(--color-text);
    text-align: center;
    padding: 1.5rem 1rem;
    font-size: 0.9rem;
    border-top: 1px solid var(--color-border);
    /* ===== IMPORTANT POUR LE LAYOUT ===== */
    /* margin-top: auto pousse le footer en bas de page
       quand utilisé avec display: flex sur le parent (App.vue) */
    margin-top: auto;
  }
</style>
