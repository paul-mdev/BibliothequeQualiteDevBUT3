<template>
  <!-- État de chargement tant que l'utilisateur n'est pas vérifié -->
  <div v-if="loading">
    <p>Chargement...</p>
  </div>

  <!-- Page accessible uniquement si l'utilisateur a le droit "gerer_livres" -->
  <div v-else-if="hasRight('gerer_livres')" class="gestion-page">
    <h1>Gestion des livres</h1>
    <p>
      Bienvenue {{ userState.user?.user_name }},
      vous pouvez gérer les livres.
    </p>

    <!-- Bouton de création d'un nouveau livre -->
    <button @click="addBook">Ajouter un livre</button>

    <!-- Tableau listant tous les livres -->
    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Nom</th>
          <th>Auteur</th>
          <th>Éditeur</th>
          <th>Date</th>
          <th>Actions</th>
        </tr>
      </thead>

      <tbody>
        <!-- Boucle sur la liste des livres -->
        <tr v-for="b in books" :key="b.book_id">
          <td>{{ b.book_id }}</td>
          <td>{{ b.book_name }}</td>
          <td>{{ b.book_author }}</td>
          <td>{{ b.book_editor }}</td>
          <td>{{ new Date(b.book_date).toLocaleDateString() }}</td>
          <td>
            <!-- Actions possibles sur un livre -->
            <button @click="editBook(b.book_id)">Modifier</button>
            <button @click="deleteBook(b.book_id)">Supprimer</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <!-- Message affiché si l'utilisateur n'a pas les droits -->
  <div v-else>
    <p>Vous n'avez pas les droits pour accéder à cette page.</p>
  </div>
</template>

<script setup>
  // Imports Vue
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  // Store utilisateur (authentification + droits)
  import { userState, fetchUser, hasRight } from '@/stores/user'

  // Router pour la navigation
  const router = useRouter()

  // Liste des livres
  const books = ref([])

  // Indique si la page est en cours de chargement
  const loading = ref(true)

  // ============================
  // Récupération de l'utilisateur connecté
  // ============================
  const fetchCurrentUser = async () => {
    try {
      // Appel API pour récupérer l'utilisateur
      await fetchUser()

      // Vérification des droits
      if (!hasRight('gerer_livres')) {
        alert('Accès refusé : vous n\'avez pas le droit de gérer les livres')
        router.push('/')
        return false
      }

      return true
    } catch (err) {
      console.error('Erreur récupération utilisateur:', err)
      router.push('/login')
      return false
    } finally {
      // Fin du chargement dans tous les cas
      loading.value = false
    }
  }

  // ============================
  // Récupération de la liste des livres
  // ============================
  const fetchBooks = async () => {
    try {
      const res = await fetch('/book', {
        credentials: 'include' // Envoi des cookies de session
      })

      if (res.ok) {
        books.value = await res.json()
      }
    } catch (err) {
      console.error('Erreur récupération livres:', err)
    }
  }

  // ============================
  // Actions utilisateur
  // ============================

  // Redirection vers la page d'ajout
  const addBook = () => {
    router.push('/book/new')
  }

  // Redirection vers la page d'édition
  const editBook = (id) => {
    router.push(`/livre/edit/${id}`)
  }

  // Suppression d'un livre
  const deleteBook = async (id) => {
    if (!confirm('Supprimer ce livre ?')) return

    try {
      const res = await fetch(`/book/${id}`, {
        method: 'DELETE',
        credentials: 'include'
      })

      if (res.ok) {
        // Rafraîchit la liste après suppression
        await fetchBooks()
        console.log('Livre supprimé')
      } else {
        alert('Erreur suppression')
      }
    } catch (err) {
      console.error('Erreur deleteBook:', err)
      alert('Erreur lors de la suppression')
    }
  }

  // ============================
  // Cycle de vie du composant
  // ============================
  onMounted(async () => {
    // Vérifie l'utilisateur avant de charger les livres
    const hasAccess = await fetchCurrentUser()
    if (hasAccess) {
      await fetchBooks()
    }
  })
</script>

<style scoped>
  /* Conteneur principal */
  .gestion-page {
    max-width: 900px;
    margin: 2rem auto;
    text-align: center;
  }

  /* Tableau des livres */
  table {
    margin-top: 1rem;
    width: 100%;
    border-collapse: collapse;
  }

  th,
  td {
    padding: 0.5rem;
    border: 1px solid #ccc;
  }

  /* Boutons d'action */
  button {
    margin: 0 0.25rem;
    padding: 0.25rem 0.5rem;
    cursor: pointer;
  }

    button:hover {
      opacity: 0.8;
    }
</style>
