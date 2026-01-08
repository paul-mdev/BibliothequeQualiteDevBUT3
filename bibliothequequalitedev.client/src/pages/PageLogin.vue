<template>
  <div class="login-page">
    <h1>Connexion / Inscription</h1>

    <!-- ===== FORMULAIRE DE CONNEXION/INSCRIPTION ===== -->
    <!-- Affiché uniquement si l'utilisateur n'est pas connecté -->
    <div v-if="!user">
      <!-- Champs de saisie -->
      <input v-model="user_mail" placeholder="Adresse email" />
      <input v-model="user_pswd" type="password" placeholder="Mot de passe" />

      <!-- Boutons d'action -->
      <button @click="login">Se connecter</button>
      <button @click="register">Créer un compte</button>
    </div>

    <!-- ===== ÉTAT CONNECTÉ ===== -->
    <!-- Affiché si l'utilisateur est déjà connecté -->
    <div v-else>
      <p>Connecté en tant que : {{ user.user_mail }}</p>
      <button @click="logout">Se déconnecter</button>
    </div>

    <!-- ===== MESSAGE D'ERREUR ===== -->
    <!-- Affiché en cas d'erreur lors de la connexion/inscription -->
    <div v-if="error" style="color:red">{{ error }}</div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()

  // ===== ÉTAT RÉACTIF =====
  const user_mail = ref('')        // Email saisi par l'utilisateur
  const user_pswd = ref('')        // Mot de passe saisi
  const user = ref(null)           // Données de l'utilisateur connecté
  const error = ref('')            // Message d'erreur éventuel

  // URL de base de l'API d'authentification
  const api = '/auth'

  /**
   * ===== VÉRIFICATION DE SESSION =====
   * Vérifie si l'utilisateur est déjà connecté
   * Appelé au montage du composant
   * Utilise le endpoint /auth/me pour récupérer les infos de session
   */
  async function fetchMe() {
    try {
      const res = await fetch(`${api}/me`, {
        credentials: 'include'  // Inclut les cookies de session
      })

      if (res.ok) {
        // Utilisateur déjà connecté
        user.value = await res.json()
      } else {
        // Pas de session active
        user.value = null
      }
    } catch {
      user.value = null
    }
  }

  // Vérifie la session au chargement de la page
  onMounted(fetchMe)

  /**
   * ===== FONCTION DE CONNEXION =====
   * Authentifie l'utilisateur avec email et mot de passe
   * Redirige vers la page d'accueil en cas de succès
   */
  async function login() {
    error.value = ''  // Réinitialise les erreurs précédentes

    try {
      const res = await fetch(`${api}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          user_mail: user_mail.value,
          user_pswd: user_pswd.value
        })
      })

      // Gestion de l'erreur
      if (!res.ok) throw new Error(await res.text())

      // Succès : stockage des données utilisateur
      user.value = await res.json()

      // Redirection vers l'accueil
      router.push('/')
    } catch (err) {
      // Affichage de l'erreur à l'utilisateur
      error.value = err.message
    }
  }

  /**
   * ===== FONCTION D'INSCRIPTION =====
   * Crée un nouveau compte utilisateur
   * Connecte automatiquement l'utilisateur après inscription
   * Redirige vers la page d'accueil
   */
  async function register() {
    error.value = ''  // Réinitialise les erreurs précédentes

    try {
      const res = await fetch(`${api}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
          user_mail: user_mail.value,
          user_pswd: user_pswd.value
        })
      })

      // Gestion de l'erreur
      if (!res.ok) throw new Error(await res.text())

      // Succès : stockage des données utilisateur
      user.value = await res.json()

      // Redirection vers l'accueil
      router.push('/')
    } catch (err) {
      // Affichage de l'erreur à l'utilisateur
      error.value = err.message
    }
  }

  /**
   * ===== FONCTION DE DÉCONNEXION =====
   * Termine la session utilisateur
   * Supprime les cookies côté serveur
   * Réinitialise l'état local
   */
  async function logout() {
    await fetch(`${api}/logout`, {
      method: 'POST',
      credentials: 'include'
    })

    // Réinitialise l'état utilisateur
    user.value = null
  }
</script>

<style scoped>
  .login-page {
    max-width: 400px;
    margin: 3rem auto;
    text-align: center;
  }

    .login-page input {
      display: block;
      margin: 0.5rem auto;
      width: 80%;
      padding: 0.5rem;
    }

    .login-page button {
      margin: 0.5rem;
      padding: 0.5rem 1rem;
    }
</style>
