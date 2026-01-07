<template>
  <div class="weather-component">
    <h1>Répertoire des livres</h1>

    <input type="text"
           v-model="search"
           placeholder="Rechercher un livre..."
           class="search" />

    <div v-if="loading" class="loading">Chargement...</div>

    <table v-else-if="filteredBooks.length > 0">
      <thead>
        <tr>
          <th>Id</th>
          <th>Nom</th>
          <th>Auteur</th>
          <th>Éditeur</th>
          <th>Date</th>
          <th>Disponibles</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="book in filteredBooks"
            :key="book.book_id"
            @click="goToBook(book.book_id)"
            class="row">
          <td>{{ book.book_id }}</td>
          <td>{{ book.book_name }}</td>
          <td>{{ book.book_author }}</td>
          <td>{{ book.book_editor }}</td>
          <td>{{ new Date(book.book_date).toLocaleDateString() }}</td>
          <td>
            <span :class="{ 'out-of-stock': availableCounts[book.book_id] === 0 }">
              {{ availableCounts[book.book_id] ?? '-' }}
            </span>
          </td>
        </tr>
      </tbody>
    </table>

    <p v-else>Aucun livre trouvé.</p>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()

  const loading = ref(true)
  const books = ref([])
  const search = ref('')
  const availableCounts = ref({}) // { book_id: nombre disponible }

  const fetchData = async () => {
    loading.value = true
    try {
      const response = await fetch('/book')
      if (!response.ok) throw new Error('Erreur réseau')
      books.value = await response.json()

      // Récupération parallèle des stocks (plus rapide)
      const countPromises = books.value.map(book =>
        fetch(`/book/${book.book_id}/available-count`)
          .then(res => res.ok ? res.json() : 0)
          .catch(() => 0)
      )

      const counts = await Promise.all(countPromises)
      books.value.forEach((book, index) => {
        availableCounts.value[book.book_id] = counts[index]
      })
    } catch (error) {
      console.error('Erreur lors du chargement des livres:', error)
      alert('Impossible de charger les livres.')
      books.value = []
    } finally {
      loading.value = false
    }
  }

  const goToBook = (id) => {
    router.push(`/livre/${id}`)
  }

  const filteredBooks = computed(() => {
    if (!search.value.trim()) return books.value

    const lowerSearch = search.value.toLowerCase()
    return books.value.filter(book =>
      book.book_name.toLowerCase().includes(lowerSearch) ||
      book.book_author.toLowerCase().includes(lowerSearch)
    )
  })

  onMounted(fetchData)
</script>

<style scoped>
  .weather-component {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }

  .search {
    margin-bottom: 1.5rem;
    padding: 0.8rem;
    width: 350px;
    max-width: 100%;
    border: 1px solid #ccc;
    border-radius: 8px;
    font-size: 1rem;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    background: var(--color-background-soft, #fff);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
    border-radius: 8px;
    overflow: hidden;
  }

  th, td {
    padding: 12px 15px;
    text-align: left;
    border-bottom: 1px solid #eee;
  }

  th {
    background-color: var(--vt-c-indigo, #3b82f6);
    color: white;
    font-weight: 600;
  }

  .row {
    cursor: pointer;
    transition: background 0.2s;
  }

    .row:hover {
      background-color: #f8fafc;
    }

  .out-of-stock {
    color: #e74c3c;
    font-weight: bold;
  }

  @media (prefers-color-scheme: dark) {
    .row:hover {
      background-color: #334155;
    }

    table {
      background: #1e293b;
    }

    th {
      background-color: #2563eb;
    }

    td {
      border-bottom-color: #334155;
    }
  }

  .loading {
    text-align: center;
    padding: 40px;
    font-size: 1.2rem;
    color: #666;
  }
</style>
