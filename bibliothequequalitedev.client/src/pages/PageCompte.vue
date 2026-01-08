<template>
  <div class="compte-page">
    <h1>Mes livres empruntés</h1>

    <div v-if="loading" class="loading">
      <p>Chargement...</p>
    </div>

    <div v-else-if="borrowed.length === 0" class="empty-state">
      <p>Vous n'avez aucun livre emprunté actuellement.</p>
    </div>

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
        <tr v-for="b in borrowed" :key="b.id_borrow" :class="{ 'returned': b.is_returned }">
          <td><strong>{{ b.bookName }}</strong></td>
          <td>{{ b.bookAuthor }}</td>
          <td>{{ formatDate(b.date_start) }}</td>
          <td>{{ formatDate(b.date_end) }}</td>
          <td :class="{ 'overdue': !b.is_returned && isOverdue(b.date_end) }">
            <span v-if="b.is_returned">-</span>
            <span v-else>
              {{ tempsRestant(b.date_end) }}
              <span v-if="isOverdue(b.date_end)" class="overdue-badge">En retard</span>
            </span>
          </td>
          <td>
            <span v-if="b.is_returned" class="badge badge-success">✓ Rendu</span>
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
  const borrowed = ref([])
  const loading = ref(true)

  const fetchBorrowed = async () => {
    loading.value = true
    try {
      const res = await fetch('/UsersBorrowed/me', { credentials: 'include' })

      if (res.status === 401) {
        alert('Vous devez être connecté pour voir vos emprunts')
        router.push('/login')
        return
      }

      if (!res.ok) {
        throw new Error('Erreur serveur')
      }

      borrowed.value = await res.json()
      console.log('📚 Emprunts chargés:', borrowed.value)
    } catch (err) {
      console.error('Erreur fetchBorrowed:', err)
      alert('Erreur lors du chargement de vos emprunts')
      borrowed.value = []
    } finally {
      loading.value = false
    }
  }

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('fr-FR')
  }

  const tempsRestant = (dateFin) => {
    const maintenant = new Date()
    const fin = new Date(dateFin)
    let diff = fin - maintenant

    if (diff <= 0) return 'Expiré'

    const jours = Math.floor(diff / (1000 * 60 * 60 * 24))

    if (jours > 0) {
      return `${jours} jour${jours > 1 ? 's' : ''}`
    } else {
      const heures = Math.floor(diff / (1000 * 60 * 60))
      return `${heures} heure${heures > 1 ? 's' : ''}`
    }
  }

  const isOverdue = (dateFin) => {
    return new Date(dateFin) < new Date()
  }

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
    color: #2c3e50;
  }

  tbody tr:hover {
    background-color: #f8fafc;
  }

  tbody tr.returned {
    opacity: 0.6;
    background-color: #f0f0f0;
  }

  .badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 0.85rem;
    font-weight: 600;
  }

  .badge-success {
    background-color: #d4edda;
    color: #155724;
  }

  .badge-warning {
    background-color: #fff3cd;
    color: #856404;
  }

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
