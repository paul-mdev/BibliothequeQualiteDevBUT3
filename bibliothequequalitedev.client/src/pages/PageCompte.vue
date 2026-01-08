<template>
  <div class="compte-page">
    <h1>Mes livres empruntés</h1>

    <!-- État de chargement -->
    <div v-if="loading" class="loading">
      <p>Chargement...</p>
    </div>

    <!-- Message si aucun emprunt -->
    <div v-else-if="borrowed.length === 0" class="empty-state">
      <p>Vous n'avez aucun livre emprunté actuellement.</p>
    </div>

    <!-- ===== TABLEAU DES EMPRUNTS ===== -->
    <table v-else>
      <thead>
        <tr>
          <th>Titre</th>
          <th>Auteur</th>
          <th>Date d'emprunt</th>
          <th>Date de retour prévue</th>
          <th>Temps restant</th>
          <th>État</th>
        </tr>
      </thead>
      <tbody>
        <!-- Boucle sur les emprunts de l'utilisateur -->
        <!-- Classe 'returned' appliquée aux livres déjà rendus -->
        <tr v-for="b in borrowed"
            :key="b.id_borrow"
            :class="{ 'returned': b.is_returned }">

          <td><strong>{{ b.book_name }}</strong></td>
          <td>{{ b.book_author }}</td>
          <td>{{ formatDate(b.date_start) }}</td>
          <td>{{ formatDate(b.date_end) }}</td>

          <!-- ===== TEMPS RESTANT ===== -->
          <!-- Classe 'overdue' si en retard et non rendu -->
          <td :class="{ 'overdue': !b.is_returned && isOverdue(b.date_end) }">
            <!-- Si rendu, affiche juste un tiret -->
            <span v-if="b.is_returned">-</span>
            <!-- Sinon, affiche le temps restant avec badge si retard -->
            <span v-else>
              {{ tempsRestant(b.date_end) }}
              <span v-if="isOverdue(b.date_end)" class="overdue-badge">
                En retard
              </span>
            </span>
          </td>

          <!-- ===== ÉTAT DE L'EMPRUNT ===== -->
          <td>
            <!-- Badge vert si rendu -->
            <span v-if="b.is_returned" class="badge badge-success">✓ Rendu</span>
            <!-- Badge jaune si en cours -->
            <span v-else class="badge badge-warning">En cours</span>
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

  // ===== ÉTAT RÉACTIF =====
  const borrowed = ref([])      // Liste des emprunts de l'utilisateur
  const loading = ref(true)     // Indicateur de chargement

  /**
   * ===== RÉCUPÉRATION DES EMPRUNTS UTILISATEUR =====
   * Récupère tous les emprunts (en cours et passés) de l'utilisateur connecté
   * Endpoint : /UsersBorrowed/me
   * Nécessite d'être authentifié (credentials: 'include')
   */
  const fetchBorrowed = async () => {
    loading.value = true
    try {
      const res = await fetch('/UsersBorrowed/me', { credentials: 'include' })

      // ===== GESTION DE L'AUTHENTIFICATION =====
      // Si 401, l'utilisateur n'est pas connecté
      if (res.status === 401) {
        alert('Vous devez être connecté pour voir vos emprunts')
        router.push('/login')
        return
      }

      // Autre erreur serveur
      if (!res.ok) {
        throw new Error('Erreur serveur')
      }

      // Mise à jour de la liste des emprunts
      borrowed.value = await res.json()
      console.log('📚 Emprunts chargés:', borrowed.value.length)
    } catch (err) {
      console.error('Erreur fetchBorrowed:', err)
      alert('Erreur lors du chargement de vos emprunts')
      borrowed.value = []
    } finally {
      loading.value = false
    }
  }

  /**
   * Formate une date ISO en format français (JJ/MM/AAAA)
   * @param {string} dateString - Date au format ISO
   * @returns {string} Date formatée
   */
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('fr-FR')
  }

  /**
   * ===== CALCUL DU TEMPS RESTANT =====
   * Calcule le temps restant jusqu'à la date de retour
   * Affiche en jours ou heures selon le temps restant
   * @param {string} dateFin - Date de fin d'emprunt
   * @returns {string} Temps restant formaté
   */
  const tempsRestant = (dateFin) => {
    const maintenant = new Date()
    const fin = new Date(dateFin)
    let diff = fin - maintenant

    // Si dépassé
    if (diff <= 0) return 'Expiré'

    // Calcul en jours
    const jours = Math.floor(diff / (1000 * 60 * 60 * 24))

    // Si plus d'un jour restant, affiche en jours
    if (jours > 0) {
      return `${jours} jour${jours > 1 ? 's' : ''}`
    } else {
      // Sinon affiche en heures
      const heures = Math.floor(diff / (1000 * 60 * 60))
      return `${heures} heure${heures > 1 ? 's' : ''}`
    }
  }

  /**
   * Vérifie si un emprunt est en retard
   * @param {string} dateFin - Date de fin d'emprunt
   * @returns {boolean} True si en retard
   */
  const isOverdue = (dateFin) => {
    return new Date(dateFin) < new Date()
  }

  // ===== CYCLE DE VIE =====
  // Charge les emprunts au montage du composant
  onMounted(() => {
    fetchBorrowed()
  })
</script>

<style scoped>
  .compte-page {
    max-width: 1100px;
    margin: 2rem auto;
    padding: 1rem;
  }

  h1 {
    text-align: center;
    margin-bottom: 2rem;
    color: #2c3e50;
  }

  .loading {
    text-align: center;
    padding: 3rem;
    font-size: 1.1rem;
    color: #666;
  }

  .empty-state {
    text-align: center;
    padding: 3rem;
    background: #f8f9fa;
    border-radius: 8px;
    color: #666;
  }

  /* ===== TABLEAU ===== */
  table {
    width: 100%;
    border-collapse: collapse;
    background: white;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-radius: 8px;
    overflow: hidden;
  }

  th, td {
    padding: 12px 15px;
    text-align: left;
    border-bottom: 1px solid #eee;
  }

  th {
    background-color: #3b82f6;
    color: white;
    font-weight: 600;
    text-align: center;
  }

  td {
    text-align: center;
  }

  tbody tr:hover {
    background-color: #f8fafc;
  }

  /* Style pour les livres rendus (opacité réduite) */
  tbody tr.returned {
    opacity: 0.6;
    background-color: #f0f0f0;
  }

  /* ===== BADGES D'ÉTAT ===== */
  .badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.85rem;
    font-weight: 600;
  }

  /* Badge vert pour "Rendu" */
  .badge-success {
    background-color: #d4edda;
    color: #155724;
  }

  /* Badge jaune pour "En cours" */
  .badge-warning {
    background-color: #fff3cd;
    color: #856404;
  }

  /* ===== STYLE RETARD ===== */
  .overdue {
    color: #e74c3c;
    font-weight: bold;
  }

  .overdue-badge {
    display: inline-block;
    margin-left: 8px;
    padding: 2px 8px;
    background-color: #fee;
    color: #c33;
    border-radius: 4px;
    font-size: 0.8rem;
    font-weight: bold;
  }
</style>
