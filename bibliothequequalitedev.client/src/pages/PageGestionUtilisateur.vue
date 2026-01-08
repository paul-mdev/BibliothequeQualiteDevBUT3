
<template>
  <div v-if="loading">
    <p>Chargement...</p>
  </div>

  <div v-else-if="canManageUsers" class="gestion-page">
    <h1>Gestion des utilisateurs</h1>
    <p>Bienvenue {{ userState.user?.user_name }}, vous pouvez gérer les utilisateurs.</p>

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
          <td>{{ u.role_name || 'N/A' }}</td>
          <td>
            <button @click="editUser(u.user_id)">Modifier</button>
            <button @click="deleteUser(u.user_id)">Supprimer</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Formulaire modale -->
    <div v-if="showForm" class="modal">
      <div class="modal-content">
        <h2>{{ formLabel }}</h2>
        <input v-model="form.user_name" placeholder="Nom" />
        <input v-model="form.user_mail" placeholder="Email" />
        <select v-model="form.role_id">
          <option value="" disabled>-- Sélectionnez un rôle --</option>
          <option v-for="r in roles" :key="r.role_id" :value="r.role_id">
            {{ r.role_name }}
          </option>
        </select>
        <small style="color: #666;">{{ roles.length }} rôles disponibles</small>
        <input v-if="isNewUser" type="password" v-model="form.user_pswd" placeholder="Mot de passe" />

        <div class="modal-buttons">
          <button @click="submitForm">{{ formLabel }}</button>
          <button @click="closeForm">Annuler</button>
        </div>
      </div>
    </div>
  </div>

  <div v-else>
    <p>Vous n'avez pas les droits pour accéder à cette page.</p>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser, hasRight } from '@/stores/user'

  const router = useRouter()
  const users = ref([])
  const roles = ref([])
  const loading = ref(true)
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

  // ⭐ Computed pour vérifier le droit
  const canManageUsers = computed(() => hasRight('gerer_utilisateurs'))

  // ⭐ Récupération des infos utilisateur connecté
  const fetchCurrentUser = async () => {
    try {
      await fetchUser()
      console.log('👤 Utilisateur connecté:', userState.user)
      console.log('🔑 Droits:', userState.rights)

      // Vérification du droit
      if (!hasRight('gerer_utilisateurs')) {
        console.log('❌ Droit "gerer_utilisateurs" manquant')
        alert('Accès refusé : vous n\'avez pas le droit de gérer les utilisateurs')
        router.push('/')
        return false
      }

      console.log('✅ Droit "gerer_utilisateurs" présent')
      return true
    } catch (err) {
      console.error('⚠️ Erreur récupération utilisateur:', err)
      router.push('/login')
      return false
    } finally {
      loading.value = false
    }
  }

  const fetchUsers = async () => {
    try {
      const res = await fetch('/users', { credentials: 'include' })
      if (res.ok) {
        users.value = await res.json()
        console.log('📋 Utilisateurs chargés:', users.value.length)
      }
    } catch (err) {
      console.error('Erreur fetchUsers:', err)
    }
  }

  const fetchRoles = async () => {
    try {
      const res = await fetch('/roles', { credentials: 'include' })
      if (res.ok) {
        roles.value = await res.json()
        console.log('👥 Rôles chargés:', roles.value)
      }
    } catch (err) {
      console.error('Erreur fetchRoles:', err)
    }
  }

  onMounted(async () => {
    const hasAccess = await fetchCurrentUser()
    if (hasAccess) {
      await fetchUsers()
      await fetchRoles()
    }
  })

  const addUser = () => {
    isNewUser.value = true
    formLabel.value = 'Ajouter un utilisateur'
    form.value = { user_name: '', user_mail: '', user_pswd: '', role_id: 1 }
    showForm.value = true
  }

  const editUser = async (id) => {
    try {
      const res = await fetch(`/users/${id}`, { credentials: 'include' })
      if (!res.ok) return alert('Erreur récupération utilisateur')
      const u = await res.json()
      form.value = { ...u, user_pswd: '' }
      isNewUser.value = false
      formLabel.value = 'Modifier utilisateur'
      showForm.value = true
    } catch (err) {
      console.error('Erreur editUser:', err)
      alert('Erreur lors de la récupération de l\'utilisateur')
    }
  }

  const deleteUser = async (id) => {
    if (!confirm('Supprimer cet utilisateur ?')) return
    try {
      const res = await fetch(`/users/${id}`, {
        method: 'DELETE',
        credentials: 'include'
      })
      if (res.ok) {
        await fetchUsers()
        console.log('✅ Utilisateur supprimé')
      } else {
        alert('Erreur suppression')
      }
    } catch (err) {
      console.error('Erreur deleteUser:', err)
      alert('Erreur lors de la suppression')
    }
  }

  const submitForm = async () => {
    const payload = { ...form.value }
    if (isNewUser.value && !payload.user_pswd) {
      return alert('Mot de passe requis')
    }

    const method = isNewUser.value ? 'POST' : 'PUT'
    const url = isNewUser.value ? '/users' : `/users/${payload.user_id}`

    try {
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        credentials: 'include'
      })

      if (res.ok) {
        showForm.value = false
        await fetchUsers()
        console.log('✅ Utilisateur sauvegardé')
      } else {
        const text = await res.text()
        alert('Erreur : ' + text)
      }
    } catch (err) {
      console.error('Erreur submitForm:', err)
      alert('Erreur lors de la sauvegarde')
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
    cursor: pointer;
  }

    button:hover {
      opacity: 0.8;
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
    z-index: 1000;
  }

  .modal-content {
    background: white;
    padding: 1.5rem;
    border-radius: 6px;
    min-width: 300px;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    box-shadow: 0 4px 6px rgba(0,0,0,0.1);
  }

    .modal-content h2 {
      margin: 0 0 0.5rem 0;
    }

    .modal-content input,
    .modal-content select {
      padding: 0.5rem;
      border: 1px solid #ccc;
      border-radius: 4px;
      font-size: 1rem;
    }

  .modal-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
    margin-top: 0.5rem;
  }

    .modal-buttons button {
      padding: 0.5rem 1rem;
    }
</style>
