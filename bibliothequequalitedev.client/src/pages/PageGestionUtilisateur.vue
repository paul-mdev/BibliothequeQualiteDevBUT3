<template>
  <!-- Affichage d'un message de chargement pendant que les données sont récupérées -->
  <div v-if="loading">
    <p>Chargement...</p>
  </div>
  <!-- Section principale si l'utilisateur a les droits pour gérer les utilisateurs -->
  <div v-else-if="canManageUsers" class="gestion-page">
    <h1>Gestion des utilisateurs</h1>
    <!-- Message de bienvenue personnalisé avec le nom de l'utilisateur connecté -->
    <p>Bienvenue {{ userState.user?.user_name }}, vous pouvez gérer les utilisateurs.</p>
    <!-- Bouton pour ouvrir le formulaire d'ajout d'un nouvel utilisateur -->
    <button @click="addUser">Ajouter un utilisateur</button>
    <!-- Tableau listant tous les utilisateurs -->
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
        <!-- Boucle sur la liste des utilisateurs pour afficher chaque ligne -->
        <tr v-for="u in users" :key="u.user_id">
          <td>{{ u.user_id }}</td>
          <td>{{ u.user_name }}</td>
          <td>{{ u.user_mail }}</td>
          <!-- Affichage du nom du rôle ou 'N/A' si non défini -->
          <td>{{ u.role_name || 'N/A' }}</td>
          <td>
            <!-- Boutons pour modifier ou supprimer l'utilisateur -->
            <button @click="editUser(u.user_id)">Modifier</button>
            <button @click="deleteUser(u.user_id)">Supprimer</button>
          </td>
        </tr>
      </tbody>
    </table>
    <!-- Modale pour le formulaire d'ajout ou de modification -->
    <div v-if="showForm" class="modal">
      <div class="modal-content">
        <!-- Titre dynamique de la modale (Ajouter ou Modifier) -->
        <h2>{{ formLabel }}</h2>
        <!-- Champs du formulaire liés au modèle 'form' -->
        <input v-model="form.user_name" placeholder="Nom" />
        <input v-model="form.user_mail" placeholder="Email" />
        <!-- Sélecteur de rôle avec options dynamiques -->
        <select v-model="form.role_id">
          <option value="" disabled>-- Sélectionnez un rôle --</option>
          <option v-for="r in roles" :key="r.role_id" :value="r.role_id">
            {{ r.role_name }}
          </option>
        </select>
        <!-- Indication du nombre de rôles disponibles -->
        <small style="color: #666;">{{ roles.length }} rôles disponibles</small>
        <!-- Champ mot de passe visible seulement pour un nouvel utilisateur -->
        <input v-if="isNewUser" type="password" v-model="form.user_pswd" placeholder="Mot de passe" />
        <!-- Boutons de la modale -->
        <div class="modal-buttons">
          <button @click="submitForm">{{ formLabel }}</button>
          <button @click="closeForm">Annuler</button>
        </div>
      </div>
    </div>
  </div>
  <!-- Message d'accès refusé si l'utilisateur n'a pas les droits -->
  <div v-else>
    <p>Vous n'avez pas les droits pour accéder à cette page.</p>
  </div>
</template>
<script setup>
  // Importation des hooks et stores nécessaires de Vue et Vue Router
  import { ref, computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser, hasRight } from '@/stores/user'

  const router = useRouter()

  // Références réactives pour les données
  const users = ref([]) // Liste des utilisateurs
  const roles = ref([]) // Liste des rôles disponibles
  const loading = ref(true) // État de chargement
  const showForm = ref(false) // Visibilité de la modale formulaire
  const formLabel = ref('Ajouter un utilisateur') // Label dynamique pour le formulaire
  const isNewUser = ref(true) // Indique si c'est un nouvel utilisateur ou une édition
  const form = ref({ // Modèle du formulaire
    user_id: null,
    user_name: '',
    user_mail: '',
    user_pswd: '',
    role_id: 1 // Valeur par défaut pour le rôle
  })

  // Computed pour vérifier si l'utilisateur a le droit 'gerer_utilisateurs'
  const canManageUsers = computed(() => hasRight('gerer_utilisateurs'))

  // Fonction asynchrone pour récupérer les infos de l'utilisateur connecté et vérifier les droits
  const fetchCurrentUser = async () => {
    try {
      await fetchUser() // Récupère les données de l'utilisateur connecté
      // Vérification du droit spécifique
      if (!hasRight('gerer_utilisateurs')) {
        alert('Accès refusé : vous n\'avez pas le droit de gérer les utilisateurs')
        router.push('/') // Redirection vers la page d'accueil
        return false
      }
      return true
    } catch (err) {
      console.error('Erreur récupération utilisateur:', err)
      router.push('/login') // Redirection vers la page de login en cas d'erreur
      return false
    } finally {
      loading.value = false // Fin du chargement, quel que soit le résultat
    }
  }

  // Fonction asynchrone pour récupérer la liste des utilisateurs via API
  const fetchUsers = async () => {
    try {
      const res = await fetch('/users', { credentials: 'include' }) // Appel API avec credentials pour l'authentification
      if (res.ok) {
        users.value = await res.json() // Mise à jour de la liste des utilisateurs
      }
    } catch (err) {
      console.error('Erreur fetchUsers:', err)
    }
  }

  // Fonction asynchrone pour récupérer la liste des rôles via API
  const fetchRoles = async () => {
    try {
      const res = await fetch('/roles', { credentials: 'include' })
      if (res.ok) {
        roles.value = await res.json() // Mise à jour de la liste des rôles
      }
    } catch (err) {
      console.error('Erreur fetchRoles:', err)
    }
  }

  // Hook onMounted pour exécuter les fetches au montage du composant
  onMounted(async () => {
    const hasAccess = await fetchCurrentUser() // Vérifie l'accès et récupère l'utilisateur
    if (hasAccess) {
      await fetchUsers() // Récupère les utilisateurs si accès autorisé
      await fetchRoles() // Récupère les rôles
    }
  })

  // Fonction pour préparer l'ajout d'un nouvel utilisateur (ouvre la modale)
  const addUser = () => {
    isNewUser.value = true
    formLabel.value = 'Ajouter un utilisateur'
    form.value = { user_name: '', user_mail: '', user_pswd: '', role_id: 1 } // Réinitialise le formulaire
    showForm.value = true // Affiche la modale
  }

  // Fonction asynchrone pour éditer un utilisateur existant
  const editUser = async (id) => {
    try {
      const res = await fetch(`/users/${id}`, { credentials: 'include' })
      if (!res.ok) return alert('Erreur récupération utilisateur')
      const u = await res.json()
      form.value = { ...u, user_pswd: '' } // Charge les données de l'utilisateur, mot de passe vide pour ne pas le modifier par défaut
      isNewUser.value = false
      formLabel.value = 'Modifier utilisateur'
      showForm.value = true
    } catch (err) {
      console.error('Erreur editUser:', err)
      alert('Erreur lors de la récupération de l\'utilisateur')
    }
  }

  // Fonction asynchrone pour supprimer un utilisateur après confirmation
  const deleteUser = async (id) => {
    if (!confirm('Supprimer cet utilisateur ?')) return // Demande de confirmation
    try {
      const res = await fetch(`/users/${id}`, {
        method: 'DELETE',
        credentials: 'include'
      })
      if (res.ok) {
        await fetchUsers() // Rafraîchit la liste après suppression
      } else {
        alert('Erreur suppression')
      }
    } catch (err) {
      console.error('Erreur deleteUser:', err)
      alert('Erreur lors de la suppression')
    }
  }

  // Fonction asynchrone pour soumettre le formulaire (ajout ou modification)
  const submitForm = async () => {
    const payload = { ...form.value } // Copie du formulaire pour l'envoi
    if (isNewUser.value && !payload.user_pswd) {
      return alert('Mot de passe requis') // Vérification mot de passe pour nouvel utilisateur
    }
    const method = isNewUser.value ? 'POST' : 'PUT' // Méthode HTTP en fonction du mode
    const url = isNewUser.value ? '/users' : `/users/${payload.user_id}` // URL dynamique
    try {
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
        credentials: 'include'
      })
      if (res.ok) {
        showForm.value = false // Ferme la modale
        await fetchUsers() // Rafraîchit la liste
      } else {
        const text = await res.text()
        alert('Erreur : ' + text) // Affiche le message d'erreur du serveur
      }
    } catch (err) {
      console.error('Erreur submitForm:', err)
      alert('Erreur lors de la sauvegarde')
    }
  }

  // Fonction pour fermer la modale sans sauvegarde
  const closeForm = () => {
    showForm.value = false
  }
</script>
<style scoped>
  /* Styles pour la page de gestion */
  .gestion-page {
    max-width: 900px;
    margin: 2rem auto;
    text-align: center;
  }
  /* Styles pour le tableau */
  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
  }

  th, td {
    border: 1px solid #ccc;
    padding: 0.5rem;
  }
  /* Styles pour les boutons */
  button {
    margin: 0 0.25rem;
    padding: 0.25rem 0.5rem;
    cursor: pointer;
  }

    button:hover {
      opacity: 0.8; /* Effet hover pour les boutons */
    }
  /* Styles pour la modale overlay */
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
  /* Styles pour le contenu de la modale */
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
      margin: 0 0 0.5rem 0; /* Marge pour le titre de la modale */
    }

    .modal-content input,
    .modal-content select {
      padding: 0.5rem;
      border: 1px solid #ccc;
      border-radius: 4px;
      font-size: 1rem; /* Styles pour les inputs et select */
    }
  /* Styles pour les boutons de la modale */
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
