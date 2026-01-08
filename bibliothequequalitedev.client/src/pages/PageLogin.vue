<template>
  <div class="login-page">
    <h1>Connexion / Inscription</h1>

    <div v-if="!user">
      <input v-model="user_mail" placeholder="Adresse email" type="email" required />
      <input v-model="user_pswd" type="password" placeholder="Mot de passe" required />
      <button @click="login">Se connecter</button>
      <button @click="register">Créer un compte</button>
    </div>

    <div v-else>
      <p>Connecté en tant que : <strong>{{ user.user_mail }}</strong></p>
      <button @click="logout">Se déconnecter</button>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()
  const user_mail = ref('')
  const user_pswd = ref('')
  const user = ref(null)
  const error = ref('')
  const api = '/auth'

  async function fetchMe() {
    try {
      const res = await fetch(`${api}/me`, { credentials: 'include' })
      if (res.ok) {
        user.value = await res.json()
      } else {
        user.value = null
      }
    } catch {
      user.value = null
    }
  }

  onMounted(fetchMe)

  // LOGIN
  async function login() {
    error.value = ''
    try {
      const res = await fetch(`${api}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ user_mail: user_mail.value, user_pswd: user_pswd.value })
      })
      if (!res.ok) throw new Error(await res.text())
      user.value = await res.json()
      router.push('/')
    } catch (err) {
      error.value = err.message || 'Erreur de connexion'
    }
  }

  // REGISTER
  async function register() {
    error.value = ''
    try {
      const res = await fetch(`${api}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({ user_mail: user_mail.value, user_pswd: user_pswd.value })
      })
      if (!res.ok) throw new Error(await res.text())
      user.value = await res.json()
      router.push('/')
    } catch (err) {
      error.value = err.message || 'Erreur lors de l’inscription'
    }
  }

  // LOGOUT
  async function logout() {
    await fetch(`${api}/logout`, { method: 'POST', credentials: 'include' })
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
