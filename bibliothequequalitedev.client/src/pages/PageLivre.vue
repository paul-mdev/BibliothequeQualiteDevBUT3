<template>
  <div class="book-detail">
    <h1>Page Livre Detail</h1>
    <p><strong>ID reçu depuis la route :</strong> {{ routeId }}</p>

    <div v-if="loading">Chargement...</div>

    <div v-else-if="book">
      <h2>{{ book.book_name }}</h2>

      <!-- Image par défaut -->
      <img :src="bookImageUrl"
           alt="Couverture du livre"
           class="book-image"
           @error="onImageError" />


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
  import { defineComponent, ref, onMounted, watch, computed } from 'vue'
  import { useRoute } from 'vue-router'

  export default defineComponent({
    setup() {
      const route = useRoute()
      const routeId = ref(route.params.id)
      const book = ref(null)
      const loading = ref(false)

      const defaultImage = '/images/books/default_book.png'
      console.log('[INIT] defaultImage =', defaultImage)

      const bookImageUrl = computed(() => {
        console.log('[COMPUTED] recalcul image, book =', book.value)

        if (!book.value) {
          console.log('[COMPUTED] book null → default')
          return defaultImage
        }

        if (!book.value.book_image_ext) {
          console.log('[COMPUTED] no ext → default')
          return defaultImage
        }

        const url = `/images/books/${book.value.book_id}${book.value.book_image_ext}`
        console.log('[COMPUTED] image trouvée =', url)
        return url
      })

      const onImageError = (event) => {
        console.log('[IMG ERROR] fallback to default')
        event.target.src = defaultImage
      }

      const fetchBook = async (id) => {
        loading.value = true
        book.value = null

        try {
          const response = await fetch(`/book/${id}`)
          if (!response.ok) throw new Error(`HTTP ${response.status}`)
          book.value = await response.json()
          console.log('[FETCH] book loaded =', book.value)
        } catch (err) {
          console.error('[FETCH ERROR]', err)
          book.value = null
        } finally {
          loading.value = false
        }
      }

      onMounted(() => fetchBook(route.params.id))

      watch(() => route.params.id, (newId) => {
        routeId.value = newId
        fetchBook(newId)
      })

      return {
        routeId,
        book,
        loading,
        bookImageUrl,
        onImageError
      }
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
