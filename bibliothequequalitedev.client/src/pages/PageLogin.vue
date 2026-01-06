<template>
  <div class="login-page">
    <h1>Login / Register</h1>

    <div v-if="!user">
      <input v-model="user_mail" placeholder="Adresse email" />
      <input v-model="user_pswd" type="password" placeholder="Mot de passe" />

      <button @click="login">Se connecter</button>
      <button @click="register">Créer un compte</button>
    </div>

    <div v-else>
      <p>Connecté en tant que : {{ user.user_mail }}</p>
      <button @click="logout">Se déconnecter</button>
    </div>

    <div v-if="error" style="color:red">{{ error }}</div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'

  const user_mail = ref('')
  const user_pswd = ref('')
  const user = ref(null)
  const error = ref('')

  const api = '/auth' // proxy vers ASP.NET Core

  // Vérifier si une session existe déjà
  async function fetchMe() {
    try {
      const res = await fetch(`${api}/me`, { credentials: 'include' });
      if (res.ok) {
        user.value = await res.json();
      } else if (res.status === 401) {
        user.value = null; // pas connecté
      }
    } catch {
      user.value = null;
    }
  }

  // Se connecter
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
    } catch (err) {
      error.value = err.message
    }
  }

  // Créer un compte
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
    } catch (err) {
      error.value = err.message
    }
  }

  // Se déconnecter
  async function logout() {
    await fetch(`${api}/logout`, { method: 'POST', credentials: 'include' })
    user.value = null
  }

 // onMounted(fetchMe)
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
