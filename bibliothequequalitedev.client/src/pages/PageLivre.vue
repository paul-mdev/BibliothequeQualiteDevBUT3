<template>
  <div class="book-detail">
    <h1>Page Livre Detail</h1>
    <p><strong>ID reçu depuis la route :</strong> {{ routeId }}</p>

    <div v-if="loading">Chargement...</div>

    <div v-else-if="book">
      <h2>{{ book.book_name }}</h2>

      <!-- Image par défaut -->
      <img :src="`/images/books/default_book.png`"
           alt="Couverture par défaut"
           class="book-image" />


      <p><strong>Auteur :</strong> {{ book.book_author }}</p>
      <p><strong>Éditeur :</strong> {{ book.book_editor }}</p>
      <p><strong>Date :</strong> {{ new Date(book.book_date).toLocaleDateString() }}</p>
    </div>

    <div v-else>
      <p>Livre introuvable.</p>
    </div>
  </div>
</template>

<script lang="js">
  import { defineComponent, ref, onMounted, watch } from 'vue'
  import { useRoute } from 'vue-router'

  export default defineComponent({
    setup() {
      const route = useRoute()
      const routeId = ref(route.params.id)
      const book = ref(null)
      const loading = ref(false)

      const fetchBook = async (id) => {
        loading.value = true
        book.value = null
        try {
          const response = await fetch(`/book/${id}`)
          if (!response.ok) throw new Error(`HTTP ${response.status}`)
          book.value = await response.json()
        } catch (err) {
          console.error('Erreur fetch Livre:', err)
          book.value = null
        } finally {
          loading.value = false
        }
      }

      // Chargement initial
      onMounted(() => fetchBook(route.params.id))

      // Recharger si l'ID change
      watch(() => route.params.id, (newId) => {
        routeId.value = newId
        fetchBook(newId)
      })

      return { routeId, book, loading }
    }
  })
</script>

<style scoped>
  .book-detail {
    max-width: 700px;
    margin: 2rem auto;
    text-align: center;
  }

  .book-image {
    max-width: 200px;
    display: block;
    margin: 1rem auto;
  }
</style>
