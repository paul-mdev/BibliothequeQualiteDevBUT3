<template>
  <div class="compte-page">
    <h1>Mes livres empruntés</h1>

    <div v-if="loading">Chargement...</div>

    <div v-else-if="borrowed.length === 0">
      <p>Aucun livre emprunté actuellement.</p>
    </div>

    <table v-else>
      <thead>
        <tr>
          <th>Titre</th>
          <th>Auteur</th>
          <th>Date début</th>
          <th>Date fin</th>
          <th>État</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="b in borrowed" :key="b.id_borrow">
          <td>{{ b.BookName }}</td>
          <td>{{ b.BookAuthor }}</td>
          <td>{{ new Date(b.date_start).toLocaleDateString() }}</td>
          <td>{{ new Date(b.date_end).toLocaleDateString() }}</td>
          <td>{{ b.is_returned ? 'Rendu' : 'En cours' }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'

  const borrowed = ref([])
  const loading = ref(false)

  const fetchBorrowed = async () => {
    loading.value = true
    try {
      const res = await fetch('/UsersBorrowed/me', { credentials: 'include' })
      if (!res.ok) throw new Error('Erreur serveur')
      borrowed.value = await res.json()
    } catch (err) {
      console.error(err)
      borrowed.value = []
    } finally {
      loading.value = false
    }
  }

  onMounted(() => fetchBorrowed())
</script>

<style scoped>
  .compte-page {
    max-width: 900px;
    margin: 2rem auto;
    text-align: center;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
  }

  th, td {
    border: 1px solid #ccc;
    padding: 0.5rem;
    text-align: center;
  }
</style>
