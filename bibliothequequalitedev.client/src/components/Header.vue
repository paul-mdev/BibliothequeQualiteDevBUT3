<template>
  <header class="site-header">
    <nav class="nav">
      <ul>
        <!-- ===== LIENS DE NAVIGATION PRINCIPAUX ===== -->
        <li><router-link to="/">Accueil</router-link></li>
        <li><router-link to="/statistiques">Statistiques</router-link></li>
        <li><router-link to="/parametres">Paramètres</router-link></li>
        <li><router-link to="/compte">Compte</router-link></li>

        <!-- ===== LIENS CONDITIONNELS BASÉS SUR LES DROITS ===== -->
        <!-- v-if réactive : affichage dynamique selon les droits de l'utilisateur -->
        <!-- Lien visible uniquement si l'utilisateur peut gérer les livres -->
        <li v-if="hasGererLivres">
          <router-link to="/gestion/livres">Gestion des livres</router-link>
        </li>

        <!-- Lien visible uniquement si l'utilisateur peut gérer les utilisateurs -->
        <li v-if="hasGererUtilisateurs">
          <router-link to="/gestion/utilisateurs">Gestion des utilisateurs</router-link>
        </li>

        <!-- Lien de gestion des emprunts (basé sur gerer_livres) -->
        <li v-if="hasGererLivres">
          <router-link to="/gestion/emprunts">Gestion des emprunts</router-link>
        </li>

        <!-- ===== AUTHENTIFICATION ===== -->
        <!-- Lien connexion si non connecté -->
        <li v-if="!isLoggedIn">
          <router-link to="/login">Connexion</router-link>
        </li>

        <!-- Nom utilisateur et bouton déconnexion si connecté -->
        <li v-else>
          <span class="user-info">{{ userState.user?.user_name }}</span>
          <button @click="logout" class="logout-btn">Déconnexion</button>
        </li>
      </ul>
    </nav>
  </header>
</template>

<script setup>
  import { computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser, hasRight } from '@/stores/user'

  const router = useRouter()

  // ===== COMPUTED PROPERTIES RÉACTIVES =====
  // Ces valeurs se mettent à jour automatiquement quand userState change

  /**
   * Vérifie si l'utilisateur est connecté
   * Basé sur userState.isLoggedIn du store
   */
  const isLoggedIn = computed(() => userState.isLoggedIn)

  /**
   * Vérifie si l'utilisateur a le droit de gérer les livres
   */
  const hasGererLivres = computed(() => hasRight('gerer_livres'))

  /**
   * Vérifie si l'utilisateur a le droit de gérer les utilisateurs
   */
  const hasGererUtilisateurs = computed(() => hasRight('gerer_utilisateurs'))

  /**
   * ===== FONCTION DE DÉCONNEXION =====
   * - Appelle l'API de déconnexion
   * - Réinitialise le store utilisateur
   * - Redirige vers la page de connexion
   */
  const logout = async () => {
    try {
      await fetch('/auth/logout', { method: 'POST', credentials: 'include' })

      // Réinitialisation du store
      userState.user = null
      userState.isLoggedIn = false
      userState.rights = []

      // Redirection
      router.push('/login')
    } catch (err) {
      console.error('Erreur déconnexion:', err)
    }
  }

  // ===== CYCLE DE VIE =====
  /**
   * Au montage, récupère les informations utilisateur
   */
  onMounted(() => {
    fetchUser()
  })

  /**
   * ===== HOOK APRÈS NAVIGATION =====
   * Rafraîchit les données utilisateur après chaque changement de route
   * Garantit que les droits sont toujours à jour
   */
  router.afterEach(() => {
    fetchUser()
  })
</script>

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

    /* Soulignement pour la route active */
    .nav a.router-link-active {
      text-decoration: underline;
      font-weight: 600;
    }

  /* Responsive mobile */
  @media (max-width: 640px) {
    .nav ul {
      flex-direction: column;
      align-items: center;
      gap: 1rem;
    }
  }
</style>
