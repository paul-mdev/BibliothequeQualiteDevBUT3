<template>
  <div class="gestion-emprunts">
    <h1>Gestion des emprunts en cours</h1>

    <div v-if="loading" class="loading">Chargement des emprunts...</div>

    <div v-else-if="emprunts.length === 0">
      <p>Aucun emprunt en cours actuellement.</p>
    </div>

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
        <tr v-for="emprunt in emprunts" :key="emprunt.id_borrow">
          <td><strong>{{ emprunt.book_name }}</strong></td>
          <td>
            {{ emprunt.user_name }}<br />
            <small>{{ emprunt.user_mail }}</small>
          </td>
          <td>{{ formatDate(emprunt.date_start) }}</td>
          <td>{{ formatDate(emprunt.date_end) }}</td>
          <td :class="{ 'overdue': isOverdue(emprunt.date_end) }">
            {{ tempsRestant(emprunt.date_end) }}
            <span v-if="isOverdue(emprunt.date_end)" class="overdue-text">
              (en retard !)
            </span>
          </td>
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
  const emprunts = ref([])
  const loading = ref(true)
  const returning = ref([])

  // Vérification auth (simple pour l'instant)
  const checkAuth = async () => {
    try {
      const res = await fetch('/auth/me', { credentials: 'include' })
      if (!res.ok) throw new Error()
      // Tu peux ajouter une vérification de rôle ici plus tard
    } catch {
      alert('Vous devez être connecté.')
      router.push('/login')
    }
  }

  const fetchEmpruntsEnCours = async () => {
    loading.value = true
    try {
      const res = await fetch('/borrow/current', { credentials: 'include' })
      if (!res.ok) {
        if (res.status === 401) router.push('/login')
        throw new Error('Erreur serveur')
      }
      emprunts.value = await res.json()
    } catch (err) {
      console.error(err)
      alert('Impossible de charger les emprunts.')
      emprunts.value = []
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
    diff -= jours * 1000 * 60 * 60 * 24
    const heures = Math.floor(diff / (1000 * 60 * 60))
    diff -= heures * 1000 * 60 * 60
    const minutes = Math.floor(diff / (1000 * 60))

    return `${jours}j ${heures}h ${minutes}min`
  }

  const isOverdue = (dateFin) => {
    return new Date(dateFin) < new Date()
  }

  const marquerRestitue = async (borrowId) => {
    if (!confirm('Confirmer la restitution de ce livre ?')) return

    returning.value.push(borrowId)

    try {
      const res = await fetch(`/borrow/${borrowId}/return`, {
        method: 'POST',
        credentials: 'include'
      })

      if (res.ok) {
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
      returning.value = returning.value.filter(id => id !== borrowId)
    }
  }

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
