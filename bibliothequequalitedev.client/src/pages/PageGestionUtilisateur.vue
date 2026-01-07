<template>
  <div class="gestion-page">
    <h1>Gestion des utilisateurs</h1>

    <button @click="addUser">Ajouter un utilisateur</button>

    <table>
      <thead>
        <tr>
          <th>ID</th>
          <th>Nom</th>
          <th>Email</th>
          <th>Rôle</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in users" :key="u.user_id">
          <td>{{ u.user_id }}</td>
          <td>{{ u.user_name }}</td>
          <td>{{ u.user_mail }}</td>
          <td>{{ roleName(u.role_id) }}</td>
          <td>
            <button @click="editUser(u.user_id)">Modifier</button>
            <button @click="deleteUser(u.user_id)">Supprimer</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Formulaire modale simple pour Ajouter / Modifier -->
    <div v-if="showForm" class="modal">
      <div class="modal-content">
        <h2>{{ formLabel }}</h2>
        <input v-model="form.user_name" placeholder="Nom" />
        <input v-model="form.user_mail" placeholder="Email" />
        <select v-model="form.role_id">
          <option v-for="r in roles" :key="r.role_id" :value="r.role_id">
            {{ r.role_name }}
          </option>
        </select>
        <input v-if="isNewUser" type="password" v-model="form.user_pswd" placeholder="Mot de passe" />

        <div class="modal-buttons">
          <button @click="submitForm">{{ formLabel }}</button>
          <button @click="closeForm">Annuler</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()
  const users = ref([])
  const roles = ref([]) // récupère la table ROLE du backend
  const showForm = ref(false)
  const formLabel = ref('Ajouter un utilisateur')
  const isNewUser = ref(true)
  const form = ref({
    user_id: null,
    user_name: '',
    user_mail: '',
    user_pswd: '',
    role_id: 1
  })

  // ----- Fonctions backend -----
  const fetchUsers = async () => {
    const res = await fetch('/users', { credentials: 'include' })
    if (res.ok) users.value = await res.json()
  }

  const fetchRoles = async () => {
    const res = await fetch('/roles', { credentials: 'include' })
    if (res.ok) roles.value = await res.json()
  }

  onMounted(async () => {
    await fetchUsers()
    await fetchRoles()
  })

  // ----- Helpers -----
  const roleName = (roleId) => {
    const r = roles.value.find(r => r.role_id === roleId)
    return r ? r.role_name : 'Inconnu'
  }

  // ----- Actions -----
  const addUser = () => {
    isNewUser.value = true
    formLabel.value = 'Ajouter un utilisateur'
    form.value = { user_name: '', user_mail: '', user_pswd: '', role_id: 1 }
    showForm.value = true
  }

  const editUser = async (id) => {
    const res = await fetch(`/users/${id}`, { credentials: 'include' })
    if (!res.ok) return alert('Erreur récupération utilisateur')
    const u = await res.json()
    form.value = { ...u, user_pswd: '' } // ne jamais pré-remplir le mot de passe
    isNewUser.value = false
    formLabel.value = 'Modifier utilisateur'
    showForm.value = true
  }

  const deleteUser = async (id) => {
    if (!confirm('Supprimer cet utilisateur ?')) return
    const res = await fetch(`/users/${id}`, { method: 'DELETE', credentials: 'include' })
    if (res.ok) users.value = users.value.filter(u => u.user_id !== id)
    else alert('Erreur suppression')
  }

  // ----- Formulaire -----
  const submitForm = async () => {
    const payload = { ...form.value }
    if (isNewUser.value && !payload.user_pswd) return alert('Mot de passe requis')
    const method = isNewUser.value ? 'POST' : 'PUT'
    const url = isNewUser.value ? '/users' : `/users/${payload.user_id}`

    const res = await fetch(url, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
      credentials: 'include'
    })

    if (res.ok) {
      showForm.value = false
      await fetchUsers()
    } else {
      const text = await res.text()
      alert('Erreur : ' + text)
    }
  }

  const closeForm = () => {
    showForm.value = false
  }
</script>

<style scoped>
  .gestion-page {
    max-width: 900px;
    margin: 2rem auto;
    text-align: center;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
  }

  th, td {
    border: 1px solid #ccc;
    padding: 0.5rem;
  }

  button {
    margin: 0 0.25rem;
    padding: 0.25rem 0.5rem;
  }

  .modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0,0,0,0.5);
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .modal-content {
    background: white;
    padding: 1rem;
    border-radius: 6px;
    min-width: 300px;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .modal-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
  }
</style>
