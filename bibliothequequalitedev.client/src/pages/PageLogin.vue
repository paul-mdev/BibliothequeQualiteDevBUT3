<template>
  <div class="login-page">
    <h1>Connexion / Inscription</h1>

    <!-- ===== FORMULAIRE DE CONNEXION/INSCRIPTION ===== -->
    <!-- Affiché uniquement si l'utilisateur n'est pas connecté -->
    <div v-if="!user">
      <!-- Champs de saisie -->
      <input v-model="user_mail" placeholder="Adresse email" type="email" required />
      <input v-model="user_pswd" type="password" placeholder="Mot de passe" required />
      <!-- Boutons d'action -->
      <button @click="login">Se connecter</button>
      <button @click="register">Créer un compte</button>
    </div>

    <!-- ===== ÉTAT CONNECTÉ ===== -->
    <!-- Affiché si l'utilisateur est déjà connecté -->
    <div v-else>
      <p>Connecté en tant que : <strong>{{ user.user_mail }}</strong></p>
      <button @click="logout">Se déconnecter</button>
    </div>

    <!-- ===== MESSAGE D'ERREUR ===== -->
    <!-- Affiché en cas d'erreur lors de la connexion/inscription -->
    <div v-if="error" class="error-message">{{ error }}</div>
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
  const api = '/auth'

  /**
   * ===== VÉRIFICATION DE SESSION =====
   * Vérifie si l'utilisateur est déjà connecté
   * Appelé au montage du composant
   * Utilise le endpoint /auth/me pour récupérer les infos de session
   */
  async function fetchMe() {
    try {
      const res = await fetch(`${api}/me`, { credentials: 'include' })       // Inclut les cookies de session
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
        body: JSON.stringify({ user_mail: user_mail.value, user_pswd: user_pswd.value })
      })

      // Gestion de l'erreur
      if (!res.ok) throw new Error(await res.text())

      // Succès : stockage des données utilisateur
      user.value = await res.json()

      // Redirection vers l'accueil
      router.push('/')
    } catch (err) {

      // Affichage de l'erreur à l'utilisateur
      error.value = err.message || 'Erreur de connexion'
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
        body: JSON.stringify({ user_mail: user_mail.value, user_pswd: user_pswd.value })
      })

      // Gestion de l'erreur
      if (!res.ok) throw new Error(await res.text())

      // Succès : stockage des données utilisateur
      user.value = await res.json()

      // Redirection vers l'accueil
      router.push('/')
    } catch (err) {
      // Affichage de l'erreur à l'utilisateur
      error.value = err.message || 'Erreur lors de l’inscription'
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
    router.push('/login')
  }
</script>

<style scoped>
  .login-page {
    max-width: 400px;
    margin: 6rem auto;
    padding: 2rem;
    background: var(--color-background-soft, #fff);
    border-radius: 12px;
    box-shadow: 0 10px 30px rgba(0,0,0,0.1);
    text-align: center;
  }

    .login-page input {
      display: block;
      width: 100%;
      padding: 12px;
      margin: 12px 0;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: 1rem;
    }

    .login-page button {
      padding: 12px 24px;
      margin: 10px;
      background: var(--vt-c-indigo, #3b82f6);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 1rem;
    }

      .login-page button:hover {
        background: #2563eb;
      }

  .error-message {
    color: #e74c3c;
    margin-top: 1rem;
    font-weight: bold;
  }

  @media (prefers-color-scheme: dark) {
    .login-page {
      background: #1e293b;
    }
  }
</style>
