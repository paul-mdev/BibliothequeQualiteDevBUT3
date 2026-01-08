<template>
  <div class="gestion-emprunts">
    <h1>Gestion des emprunts en cours</h1>

    <!-- État de chargement -->
    <div v-if="loading" class="loading">Chargement des emprunts...</div>

    <!-- Message si aucun emprunt -->
    <div v-else-if="emprunts.length === 0">
      <p>Aucun emprunt en cours actuellement.</p>
    </div>

    <!-- Tableau des emprunts -->
    <table v-else>
      <thead>
        <tr>
          <th>Livre</th>
          <th>Emprunteur</th>
          <th>Date d'emprunt</th>
          <th>Date de retour prévue</th>
          <th>Temps restant</th>
          <th>Action</th>
        </tr>
      </thead>
      <tbody>
        <!-- Boucle sur chaque emprunt avec une clé unique -->
        <tr v-for="emprunt in emprunts" :key="emprunt.id_borrow">
          <td><strong>{{ emprunt.book_name }}</strong></td>
          <td>
            {{ emprunt.user_name }}<br />
            <small>{{ emprunt.user_mail }}</small>
          </td>
          <td>{{ formatDate(emprunt.date_start) }}</td>
          <td>{{ formatDate(emprunt.date_end) }}</td>

          <!-- Affichage du temps restant avec classe conditionnelle en cas de retard -->
          <td :class="{ 'overdue': isOverdue(emprunt.date_end) }">
            {{ tempsRestant(emprunt.date_end) }}
            <span v-if="isOverdue(emprunt.date_end)" class="overdue-text">
              (en retard !)
            </span>
          </td>

          <!-- Bouton de restitution avec état désactivé pendant l'action -->
          <td>
            <button @click="marquerRestitue(emprunt.id_borrow)"
                    class="return-btn"
                    :disabled="returning.includes(emprunt.id_borrow)">
              {{ returning.includes(emprunt.id_borrow) ? 'En cours...' : 'Restitué' }}
            </button>
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
  const emprunts = ref([])        // Liste des emprunts en cours
  const loading = ref(true)       // Indicateur de chargement
  const returning = ref([])       // IDs des emprunts en cours de restitution

  /**
   * Vérifie si l'utilisateur est authentifié
   * Redirige vers /login si non connecté
   */
  const checkAuth = async () => {
    try {
      const res = await fetch('/auth/me', { credentials: 'include' })
      if (!res.ok) throw new Error()
      // TODO: Ajouter une vérification de rôle pour restreindre l'accès
    } catch {
      alert('Vous devez être connecté.')
      router.push('/login')
    }
  }

  /**
   * Récupère tous les emprunts en cours depuis l'API
   * Gère les erreurs d'authentification et serveur
   */
  const fetchEmpruntsEnCours = async () => {
    loading.value = true
    try {
      const res = await fetch('/borrow/current', { credentials: 'include' })

      // Redirection si non authentifié
      if (!res.ok) {
        if (res.status === 401) router.push('/login')
        throw new Error('Erreur serveur')
      }

      // Mise à jour de la liste des emprunts
      emprunts.value = await res.json()
    } catch (err) {
      console.error(err)
      alert('Impossible de charger les emprunts.')
      emprunts.value = []
    } finally {
      loading.value = false
    }
  }

  /**
   * Formate une date ISO en format français (JJ/MM/AAAA)
   * @param {string} dateString - Date au format ISO
   * @returns {string} Date formatée en français
   */
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('fr-FR')
  }

  /**
   * Calcule le temps restant jusqu'à la date de fin
   * Affiche en jours, heures et minutes
   * @param {string} dateFin - Date de fin d'emprunt
   * @returns {string} Temps restant formaté ou "Expiré"
   */
  const tempsRestant = (dateFin) => {
    const maintenant = new Date()
    const fin = new Date(dateFin)
    let diff = fin - maintenant

    // Si la date est dépassée
    if (diff <= 0) return 'Expiré'

    // Calcul des jours restants
    const jours = Math.floor(diff / (1000 * 60 * 60 * 24))
    diff -= jours * 1000 * 60 * 60 * 24

    // Calcul des heures restantes
    const heures = Math.floor(diff / (1000 * 60 * 60))
    diff -= heures * 1000 * 60 * 60

    // Calcul des minutes restantes
    const minutes = Math.floor(diff / (1000 * 60))

    return `${jours}j ${heures}h ${minutes}min`
  }

  /**
   * Vérifie si un emprunt est en retard
   * @param {string} dateFin - Date de fin d'emprunt
   * @returns {boolean} True si en retard
   */
  const isOverdue = (dateFin) => {
    return new Date(dateFin) < new Date()
  }

  /**
   * Marque un emprunt comme restitué
   * Demande confirmation avant l'action
   * Met à jour la liste après succès
   * @param {number} borrowId - ID de l'emprunt à restituer
   */
  const marquerRestitue = async (borrowId) => {
    // Demande de confirmation
    if (!confirm('Confirmer la restitution de ce livre ?')) return

    // Ajout à la liste des retours en cours (désactive le bouton)
    returning.value.push(borrowId)

    try {
      const res = await fetch(`/borrow/${borrowId}/return`, {
        method: 'POST',
        credentials: 'include'
      })

      if (res.ok) {
        // Retrait de l'emprunt de la liste
        emprunts.value = emprunts.value.filter(e => e.id_borrow !== borrowId)
        alert('Livre restitué avec succès !')
      } else {
        const error = await res.text()
        alert(error || 'Erreur lors de la restitution.')
      }
    } catch (err) {
      console.error(err)
      alert('Erreur réseau.')
    } finally {
      // Retrait de la liste des retours en cours (réactive le bouton)
      returning.value = returning.value.filter(id => id !== borrowId)
    }
  }

  // ===== CYCLE DE VIE =====
  // Au montage du composant, vérifie l'auth et charge les emprunts
  onMounted(async () => {
    await checkAuth()
    await fetchEmpruntsEnCours()
  })
</script>

<style scoped>
  .gestion-emprunts {
    max-width: 1100px;
    margin: 2rem auto;
    padding: 1rem;
  }

  h1 {
    text-align: center;
    margin-bottom: 2rem;
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

  tr:hover {
    background-color: #f8fafc;
  }

  .return-btn {
    padding: 8px 16px;
    background: #27ae60;
    color: white;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.95rem;
  }

    .return-btn:hover:not(:disabled) {
      background: #219653;
    }

    .return-btn:disabled {
      background: #95a5a6;
      cursor: not-allowed;
    }

  /* Style pour les emprunts en retard */
  .overdue {
    color: #e74c3c;
    font-weight: bold;
  }

  .overdue-text {
    color: #c0392b;
    font-size: 0.9em;
  }

  .loading {
    text-align: center;
    padding: 40px;
    font-size: 1.2rem;
    color: #666;
  }
</style>
