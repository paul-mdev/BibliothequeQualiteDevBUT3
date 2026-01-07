<template>
  <div class="book-detail">
    <h1>Détail du livre</h1>

    <div v-if="loading">Chargement...</div>

    <div v-else-if="book">
      <h2>{{ book.book_name }}</h2>

      <img :src="bookImageUrl"
           alt="Couverture du livre"
           class="book-image"
           @error="onImageError" />

      <p><strong>Auteur :</strong> {{ book.book_author }}</p>
      <p><strong>Éditeur :</strong> {{ book.book_editor }}</p>
      <p><strong>Date :</strong> {{ new Date(book.book_date).toLocaleDateString() }}</p>

      <p>
        <strong>Exemplaires disponibles :</strong>
        <span :class="{ 'out-of-stock': availableCount === 0 }">
          {{ availableCount }}
        </span>
      </p>

      <button @click="borrowBook"
              :disabled="availableCount === 0 || borrowing"
              class="borrow-btn">
        {{
 borrowing
          ? 'Emprunt en cours...'
          : availableCount > 0
            ? 'Emprunter ce livre'
            : 'Hors stock'
        }}
      </button>
    </div>

    <div v-else>
      <p>Livre introuvable.</p>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted, watch } from 'vue'
  import { useRoute, useRouter } from 'vue-router'

  const route = useRoute()
  const router = useRouter()

  const book = ref(null)
  const loading = ref(true)
  const availableCount = ref(0)
  const borrowing = ref(false)

  const defaultImage = '/images/books/default_book.png'

  const bookImageUrl = computed(() => {
    if (!book.value) return defaultImage
    if (!book.value.book_image_ext) return defaultImage
    return `/images/books/${book.value.book_id}${book.value.book_image_ext}`
  })

  const onImageError = (event) => {
    event.target.src = defaultImage
  }

  const fetchBook = async (id) => {
    loading.value = true
    try {
      const response = await fetch(`/book/${id}`)
      if (!response.ok) {
        if (response.status === 404) router.push('/404')
        throw new Error(`HTTP ${response.status}`)
      }
      book.value = await response.json()
    } catch (err) {
      console.error('Erreur chargement livre:', err)
      book.value = null
    } finally {
      loading.value = false
    }
  }

  const fetchAvailableCount = async () => {
    try {
      const res = await fetch(`/book/${route.params.id}/available-count`)
      if (res.ok) {
        availableCount.value = await res.json()
      } else {
        availableCount.value = 0
      }
    } catch (err) {
      console.error('Erreur récupération stock:', err)
      availableCount.value = 0
    }
  }

  const borrowBook = async () => {
    if (availableCount.value === 0 || borrowing.value) return

    borrowing.value = true
    try {
      const res = await fetch(`/book/${route.params.id}/borrow`, {
        method: 'POST',
        credentials: 'include'
      })

      if (res.ok) {
        const data = await res.json()
        alert(data.message || 'Livre emprunté avec succès !')
        await fetchAvailableCount() // Mise à jour du compteur
      } else {
        const error = await res.text()
        alert(error || 'Impossible d\'emprunter le livre.')
      }
    } catch (err) {
      alert('Erreur réseau.')
    } finally {
      borrowing.value = false
    }
  }

  // Chargement initial
  onMounted(() => {
    if (route.params.id) {
      fetchBook(route.params.id)
      fetchAvailableCount()
    }
  })

  // Réaction au changement de route (navigation directe)
  watch(() => route.params.id, (newId) => {
    if (newId) {
      fetchBook(newId)
      fetchAvailableCount()
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
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .out-of-stock {
    color: #e74c3c;
    font-weight: bold;
  }

  .borrow-btn {
    padding: 12px 24px;
    font-size: 1.1rem;
    margin-top: 20px;
    background: #27ae60;
    color: white;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    transition: background 0.2s;
  }

    .borrow-btn:hover:not(:disabled) {
      background: #219653;
    }

    .borrow-btn:disabled {
      background: #95a5a6;
      cursor: not-allowed;
    }
</style>
