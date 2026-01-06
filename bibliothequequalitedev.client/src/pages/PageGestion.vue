<template>
  <div class="gestion-page">
    <h1>Gestion des livres</h1>

    <p v-if="user">Bienvenue {{ user.user_mail }}, vous pouvez gérer les livres.</p>

    <button @click="addBook">Ajouter un livre</button>

    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Nom</th>
          <th>Auteur</th>
          <th>Éditeur</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="b in books" :key="b.book_id">
          <td>{{ b.book_id }}</td>
          <td>{{ b.book_name }}</td>
          <td>{{ b.book_author }}</td>
          <td>{{ b.book_editor }}</td>
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

  const router = useRouter()
  const user = ref(null)
  const books = ref([])

  // Charger l'utilisateur et les livres
  const fetchUser = async () => {
    const res = await fetch('/auth/me', { credentials: 'include' })
    if (!res.ok) return router.push('/login') // pas connecté => redirection
    user.value = await res.json()
  }

  const fetchBooks = async () => {
    const res = await fetch('/book')
    if (!res.ok) return
    books.value = await res.json()
  }

  onMounted(async () => {
    await fetchUser()
    await fetchBooks()
  })

  // Fonctions simples pour gérer les livres
  const addBook = () => router.push('/book/new')
  const editBook = (id) => router.push(`/book/edit/${id}`)
  const deleteBook = async (id) => {
    if (!confirm('Supprimer ce livre ?')) return
    await fetch(`/book/${id}`, { method: 'DELETE' })
    books.value = books.value.filter(b => b.book_id !== id)
  }
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
