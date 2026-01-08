<template>
  <div class="weather-component">
    <h1>Répertoire des livres</h1>

    <!-- ===== BARRE DE RECHERCHE ===== -->
    <!-- Filtre les livres en temps réel -->
    <input type="text"
           v-model="search"
           placeholder="Rechercher un livre..."
           class="search" />

    <!-- État de chargement -->
    <div v-if="loading" class="loading">Chargement...</div>

    <!-- ===== TABLEAU DES LIVRES ===== -->
    <!-- Affiché uniquement si des livres correspondent à la recherche -->
    <table v-else-if="filteredBooks.length > 0">
      <thead>
        <tr>
          <th>Nom</th>
          <th>Auteur</th>
          <th>Éditeur</th>
          <th>Date</th>
          <th>Disponibles</th>
        </tr>
      </thead>
      <tbody>
        <!-- Boucle sur les livres filtrés -->
        <!-- Clic sur une ligne = navigation vers la page détail -->
        <tr v-for="book in filteredBooks"
            :key="book.book_id"
            @click="goToBook(book.book_id)"
            class="row">
          <td>{{ book.book_name }}</td>
          <td>{{ book.book_author }}</td>
          <td>{{ book.book_editor }}</td>
          <td>{{ new Date(book.book_date).toLocaleDateString() }}</td>
          <td>
            <!-- Affichage du nombre d'exemplaires disponibles -->
            <!-- Classe conditionnelle si stock épuisé -->
            <span :class="{ 'out-of-stock': availableCounts[book.book_id] === 0 }">
              {{ availableCounts[book.book_id] ?? '-' }}
            </span>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Message si aucun résultat -->
    <p v-else>Aucun livre trouvé.</p>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { useRouter } from 'vue-router'

  const router = useRouter()

  // ===== ÉTAT RÉACTIF =====
  const loading = ref(true)           // Indicateur de chargement
  const books = ref([])               // Liste complète des livres
  const search = ref('')              // Texte de recherche
  const availableCounts = ref({})     // Dictionnaire { book_id: nombre_disponible }

  /**
   * ===== RÉCUPÉRATION DES DONNÉES =====
   * Charge la liste des livres et leurs stocks disponibles
   * Utilise Promise.all pour paralléliser les requêtes de stock
   */
  const fetchData = async () => {
    loading.value = true
    try {
      // Récupération de la liste des livres
      const response = await fetch('/book')
      if (!response.ok) throw new Error('Erreur réseau')
      books.value = await response.json()

      /**
       * ===== RÉCUPÉRATION PARALLÈLE DES STOCKS =====
       * Crée une promesse par livre pour récupérer son stock
       * Plus rapide que des requêtes séquentielles
       */
      const countPromises = books.value.map(book =>
        fetch(`/book/${book.book_id}/available-count`)
          .then(res => res.ok ? res.json() : 0)  // Retourne 0 si erreur
          .catch(() => 0)                         // Retourne 0 si échec
      )

      // Attend que toutes les promesses se terminent
      const counts = await Promise.all(countPromises)

      // Construction du dictionnaire book_id => count
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

  /**
   * Navigation vers la page détail d'un livre
   * @param {number} id - ID du livre
   */
  const goToBook = (id) => {
    router.push(`/livre/${id}`)
  }

  /**
   * ===== COMPUTED - FILTRAGE DES LIVRES =====
   * Filtre la liste des livres en fonction du texte de recherche
   * Recherche dans le nom et l'auteur du livre
   * @returns {Array} Liste des livres filtrés
   */
  const filteredBooks = computed(() => {
    // Si pas de recherche, retourne tous les livres
    if (!search.value.trim()) return books.value

    // Conversion en minuscules pour recherche insensible à la casse
    const lowerSearch = search.value.toLowerCase()

    // Filtre sur nom et auteur
    return books.value.filter(book =>
      book.book_name.toLowerCase().includes(lowerSearch) ||
      book.book_author.toLowerCase().includes(lowerSearch)
    )
  })

  // ===== CYCLE DE VIE =====
  // Charge les données au montage du composant
  onMounted(fetchData)
</script>

<style scoped>
  .weather-component {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }

  /* ===== BARRE DE RECHERCHE ===== */
  .search {
    margin-bottom: 1.5rem;
    padding: 0.8rem;
    width: 350px;
    max-width: 100%;
    border: 1px solid #ccc;
    border-radius: 8px;
    font-size: 1rem;
  }

  /* ===== TABLEAU ===== */
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

  /* Ligne cliquable avec effet hover */
  .row {
    cursor: pointer;
    transition: background 0.2s;
  }

    .row:hover {
      background-color: #f8fafc;
    }

  /* Style pour stock épuisé */
  .out-of-stock {
    color: #e74c3c;
    font-weight: bold;
  }

  /* ===== MODE SOMBRE ===== */
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
