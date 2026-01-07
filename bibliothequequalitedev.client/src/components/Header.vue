<template>
  <header class="site-header">
    <nav class="nav">
      <ul>
        <li><router-link to="/">Accueil</router-link></li>
        <li><router-link to="/statistiques">Statistiques</router-link></li>
        <li><router-link to="/parametres">Paramètres</router-link></li>
        <li><router-link to="/gestion">Gestion</router-link></li>
        <li><router-link to="/login">Connexion</router-link></li>



      </ul>
    </nav>
  </header>
</template>

<style scoped>
  .site-header {
    background-color: #1f2937;
    padding: 1rem 2rem;
  }

  .nav ul {
    list-style: none;
    display: flex;
    gap: 2rem;
    margin: 0;
    padding: 0;
  }

  .nav a {
    color: white;
    text-decoration: none;
    font-weight: 500;
  }

    .nav a.router-link-active {
      text-decoration: underline;
    }
</style>
