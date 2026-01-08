<template>
  <div v-if="loading">
    <p>Chargement...</p>
  </div>

  <div v-else-if="hasRight('gerer_livres')" class="gestion-page">
    <h1>Gestion des livres</h1>
    <p>Bienvenue {{ userState.user?.user_name }}, vous pouvez gérer les livres.</p>

    <button @click="addBook">Ajouter un livre</button>

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
        <tr v-for="b in books" :key="b.book_id">
          <td>{{ b.book_id }}</td>
          <td>{{ b.book_name }}</td>
          <td>{{ b.book_author }}</td>
          <td>{{ b.book_editor }}</td>
          <td>{{ new Date(b.book_date).toLocaleDateString() }}</td>
          <td>
            <button @click="editBook(b.book_id)">Modifier</button>
            <button @click="deleteBook(b.book_id)">Supprimer</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>

  <div v-else>
    <p>Vous n'avez pas les droits pour accéder à cette page.</p>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser, hasRight } from '@/stores/user'

  const router = useRouter()
  const books = ref([])
  const loading = ref(true)

  // Récupération de l'utilisateur connecté
  const fetchCurrentUser = async () => {
    try {
      await fetchUser()

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
      loading.value = false
    }
  }

  // Récupération des livres
  const fetchBooks = async () => {
    try {
      const res = await fetch('/book', { credentials: 'include' })
      if (res.ok) {
        books.value = await res.json()
      }
    } catch (err) {
    }
  }

  // Actions sur les livres
  const addBook = () => {
    router.push('/book/new')
  }

  const editBook = (id) => {
    router.push(`/livre/edit/${id}`)
  }

  const deleteBook = async (id) => {
    if (!confirm('Supprimer ce livre ?')) return

    try {
      const res = await fetch(`/book/${id}`, {
        method: 'DELETE',
        credentials: 'include'
      })

      if (res.ok) {
        await fetchBooks()
        console.log('✅ Livre supprimé')
      } else {
        alert('Erreur suppression')
      }
    } catch (err) {
      console.error('Erreur deleteBook:', err)
      alert('Erreur lors de la suppression')
    }
  }

  // Montage du composant
  onMounted(async () => {
    const hasAccess = await fetchCurrentUser()
    if (hasAccess) {
      await fetchBooks()
    }
  })
</script>

<style scoped>
  .gestion-page {
    max-width: 900px;
    margin: 2rem auto;
    text-align: center;
  }

  table {
    margin-top: 1rem;
    width: 100%;
    border-collapse: collapse;
  }

  th, td {
    padding: 0.5rem;
    border: 1px solid #ccc;
  }

  button {
    margin: 0 0.25rem;
    padding: 0.25rem 0.5rem;
    cursor: pointer;
  }

    button:hover {
      opacity: 0.8;
    }
</style>
