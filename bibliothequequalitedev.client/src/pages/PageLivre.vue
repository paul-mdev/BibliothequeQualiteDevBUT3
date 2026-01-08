<template>
  <div class="book-detail">
    <h1>Détail du livre</h1>

    <!-- État de chargement -->
    <div v-if="loading">Chargement...</div>

    <!-- Affichage des détails du livre -->
    <div v-else-if="book">
      <h2>{{ book.book_name }}</h2>

      <!-- Image de couverture avec gestion d'erreur -->
      <img :src="bookImageUrl"
           alt="Couverture du livre"
           class="book-image"
           @error="onImageError" />

      <!-- Informations du livre -->
      <p><strong>Auteur :</strong> {{ book.book_author }}</p>
      <p><strong>Éditeur :</strong> {{ book.book_editor }}</p>
      <p><strong>Date :</strong> {{ new Date(book.book_date).toLocaleDateString() }}</p>

      <!-- Affichage du nombre d'exemplaires disponibles -->
      <p>
        <strong>Exemplaires disponibles :</strong>
        <!-- Classe conditionnelle si stock épuisé -->
        <span :class="{ 'out-of-stock': availableCount === 0 }">
          {{ availableCount }}
        </span>
      </p>

      <!-- Bouton d'emprunt avec états dynamiques -->
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

    <!-- Message si livre introuvable -->
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

  // ===== ÉTAT RÉACTIF =====
  const book = ref(null)              // Données du livre
  const loading = ref(true)           // Indicateur de chargement
  const availableCount = ref(0)       // Nombre d'exemplaires disponibles
  const borrowing = ref(false)        // État d'emprunt en cours

  // Image par défaut si aucune image n'est disponible
  const defaultImage = '/images/books/default_book.png'

  /**
   * ===== COMPUTED - URL DE L'IMAGE =====
   * Génère l'URL de l'image en fonction de l'extension du livre
   * Utilise l'image par défaut si aucune extension n'est définie
   */
  const bookImageUrl = computed(() => {
    if (!book.value) return defaultImage
    if (!book.value.book_image_ext) return defaultImage
    // Construction de l'URL : /images/books/{id}{extension}
    return `/images/books/${book.value.book_id}${book.value.book_image_ext}`
  })

  /**
   * Gère l'erreur de chargement d'image
   * Remplace par l'image par défaut si l'image n'existe pas
   * @param {Event} event - Événement d'erreur de l'image
   */
  const onImageError = (event) => {
    event.target.src = defaultImage
  }

  /**
   * Récupère les détails d'un livre depuis l'API
   * Redirige vers /404 si le livre n'existe pas
   * @param {string|number} id - ID du livre à récupérer
   */
  const fetchBook = async (id) => {
    loading.value = true
    try {
      const response = await fetch(`/book/${id}`)

      // Gestion des erreurs HTTP
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

  /**
   * Récupère le nombre d'exemplaires disponibles pour ce livre
   * Met à jour availableCount avec la réponse de l'API
   */
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

  /**
   * ===== ACTION D'EMPRUNT =====
   * Emprunte le livre pour l'utilisateur connecté
   * Vérifie la disponibilité et l'état d'emprunt avant de procéder
   * Rafraîchit le compteur après un emprunt réussi
   */
  const borrowBook = async () => {
    // Vérifications préalables
    if (availableCount.value === 0 || borrowing.value) return

    borrowing.value = true
    try {
      const res = await fetch(`/book/${route.params.id}/borrow`, {
        method: 'POST',
        credentials: 'include'  // Inclut les cookies de session
      })

      if (res.ok) {
        const data = await res.json()
        alert(data.message || 'Livre emprunté avec succès !')
        // Mise à jour du compteur après emprunt
        await fetchAvailableCount()
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

  // ===== CYCLE DE VIE =====
  /**
   * Au montage du composant, charge les données du livre
   * et le nombre d'exemplaires disponibles
   */
  onMounted(() => {
    if (route.params.id) {
      fetchBook(route.params.id)
      fetchAvailableCount()
    }
  })

  /**
   * ===== WATCHER SUR L'ID DE ROUTE =====
   * Surveille les changements d'ID dans l'URL
   * Permet la navigation directe entre différents livres
   * sans rechargement complet de la page
   */
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

  /* Style pour le stock épuisé */
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
