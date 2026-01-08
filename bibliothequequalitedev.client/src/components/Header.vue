<script setup>
  import { computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser } from '@/stores/user'

  const router = useRouter()

  // Computed pour que le template réagisse aux changements du store
  const isAdmin = computed(() => userState.isAdmin)
  const isLoggedIn = computed(() => userState.isLoggedIn)

  const logout = async () => {
    await fetch('/auth/logout', { method: 'POST', credentials: 'include' })
    userState.user = null
    userState.isLoggedIn = false
    userState.isAdmin = false
    router.push('/login')
  }

  onMounted(() => {
    fetchUser()
  })
</script>

<template>
  <header class="site-header">
    <nav class="nav">
      <ul>
        <li><router-link to="/">Accueil</router-link></li>
        <li><router-link to="/statistiques">Statistiques</router-link></li>

        <li v-if="isAdmin">
          <router-link to="/gestion/livres">Gestion des livres</router-link>
        </li>
        <li v-if="isAdmin">
          <router-link to="/gestion/utilisateurs">Gestion des utilisateurs</router-link>
        </li>
        <li v-if="isAdmin">
          <router-link to="/gestion/emprunts">Gestion des emprunts</router-link>
        </li>

        <li v-if="!isLoggedIn">
          <router-link to="/login">Connexion</router-link>
        </li>
        <li v-else>
          <button @click="logout" class="logout-btn">Déconnexion</button>
        </li>

      </ul>
    </nav>
  </header>
</template>

<style scoped>
  .site-header {
    background: var(--color-background-soft);
    border-bottom: 1px solid var(--color-border);
    padding: 1rem;
  }

  .nav ul {
    list-style: none;
    display: flex;
    flex-wrap: wrap;
    gap: 2rem;
    justify-content: center;
    margin: 0;
    padding: 0;
  }

  .nav a {
    color: var(--color-text);
    text-decoration: none;
    font-weight: 500;
    font-size: 1rem;
    padding: 0.5rem 0;
    white-space: nowrap;
    transition: opacity 0.2s;
  }

    .nav a:hover {
      opacity: 0.8;
    }

    .nav a.router-link-active {
      text-decoration: underline;
      font-weight: 600;
    }

  /* Mobile */
  @media (max-width: 640px) {
    .nav ul {
      flex-direction: column;
      align-items: center;
      gap: 1rem;
    }
  }
</style>
