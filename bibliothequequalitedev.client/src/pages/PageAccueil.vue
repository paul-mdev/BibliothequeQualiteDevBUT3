<template>
  <div class="weather-component">
    <!-- BANDEAU D'ALERTE ÉCHÉANCE -->
    <div v-if="reminder.hasReminder" class="due-alert-banner" :class="{ 'critical': reminder.isCritical }">
      <div class="alert-content">
        <strong>{{ reminder.message }}</strong>
        <button @click="showDetails = !showDetails" class="toggle-btn">
          {{ showDetails ? 'Masquer' : 'Voir les livres concernés' }}
        </button>
      </div>
      <transition name="slide">
        <div v-if="showDetails" class="alert-details">
          <ul>
            <li v-for="item in reminder.details" :key="item.id_borrow">
              <strong>{{ item.bookName }}</strong>
              — à rendre le <strong>{{ formatDate(item.date_end) }}</strong>
              <span class="days-left" :class="{ 'very-soon': item.daysLeft <= 5 }">
                ({{ item.daysLeft }} jour{{ item.daysLeft > 1 ? 's' : '' }} restant{{ item.daysLeft <= 5 ? ' !' : '' }})
              </span>
            </li>
          </ul>
        </div>
      </transition>
    </div>

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
  const reminder = ref({
    hasReminder: false,
    isCritical: false,
    message: '',
    details: []
  })
  const showDetails = ref(false)

  const fetchDueReminder = async () => {
    try {
      const res = await fetch('/users/due-reminders', { credentials: 'include' })
      if (res.ok) {
        const data = await res.json()
        reminder.value = {
          hasReminder: data.hasReminder || false,
          isCritical: data.isCritical || false,
          message: data.message || '',
          details: data.details || []
        }
      }
    } catch (err) {
      console.error('Erreur chargement rappel échéance:', err)
    }
  }

  // === Chargement des livres et stocks ===
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
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('fr-FR')
  }

  onMounted(async () => {
    await fetchData()
    await fetchDueReminder() // Chargement du rappel dès l'ouverture
  })
</script>

<style scoped>
  .weather-component {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }


  /* === Bandeau alerte échéance === */
  .due-alert-banner {
    background: #ff9800;
    color: white;
    padding: 16px 20px;
    text-align: center;
    font-weight: 600;
    border-radius: 8px;
    margin-bottom: 30px;
    box-shadow: 0 4px 10px rgba(0,0,0,0.2);
  }

    .due-alert-banner.critical {
      background: #f44336;
      animation: pulse 2s infinite;
    }

  .alert-content {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 20px;
    flex-wrap: wrap;
  }

  .toggle-btn {
    background: rgba(255,255,255,0.2);
    color: white;
    border: 1px solid white;
    padding: 6px 14px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.9rem;
  }

    .toggle-btn:hover {
      background: rgba(255,255,255,0.3);
    }

  .alert-details {
    margin-top: 12px;
    background: rgba(0,0,0,0.15);
    padding: 12px;
    border-radius: 6px;
    text-align: left;
    max-width: 800px;
    margin-left: auto;
    margin-right: auto;
  }

    .alert-details ul {
      list-style: none;
      padding: 0;
      margin: 0;
    }

    .alert-details li {
      padding: 6px 0;
      border-bottom: 1px solid rgba(255,255,255,0.2);
    }

  .days-left {
    margin-left: 10px;
    font-weight: bold;
  }

  .very-soon {
    color: #ffff00;
  }

  @keyframes pulse {
    0% {
      opacity: 1;
    }

    50% {
      opacity: 0.85;
    }

    100% {
      opacity: 1;
    }
  }

  .slide-enter-active, .slide-leave-active {
    transition: all 0.3s ease;
  }

  .slide-enter-from, .slide-leave-to {
    opacity: 0;
    transform: translateY(-10px);
  }

  /* === Reste du style existant === */
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
    box-shadow: 0 4px 12px rgba(0,0,0,0.05);
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

  .loading {
    text-align: center;
    padding: 40px;
    font-size: 1.2rem;
    color: #666;
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
</style>
