<template>
  <div v-if="hasRight('gerer_livres')" class="gestion-page">
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


</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRouter } from 'vue-router'
  import { userState, fetchUser, hasRight } from '../stores/user'

  const router = useRouter()
  const books = ref([])
  const loading = ref(true)

  const fetchCurrentUser = async () => {
    try {
      await fetchUser()

      if (!hasRight('gerer_livres')) {
        alert('Accès refusé : vous n\'avez pas le droit de gérer les livres')
        router.push('/')
      }
    } catch (err) {
      console.error('Erreur récupération utilisateur:', err)
      router.push('/login')
    } finally {
      loading.value = false
    }
  }

  const fetchBooks = async () => {
    const res = await fetch('/book', { credentials: 'include' })
    if (res.ok) books.value = await res.json()
  }

  const deleteBook = async (id) => {
    if (!confirm('Supprimer ce livre ?')) return
    const res = await fetch(`/book/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    })
    if (res.ok) await fetchBooks()
    else alert('Erreur suppression')
  }

  const formatDate = (dateStr) => {
    return new Date(dateStr).toLocaleDateString('fr-FR')
  }

  onMounted(async () => {
    await fetchCurrentUser()
    if (hasRight('gerer_livres')) {
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
  }
</style>
