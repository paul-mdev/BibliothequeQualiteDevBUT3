<template>
  <div class="statistics-page">
    <h1>Tableau de Bord - Statistiques</h1>

    <div v-if="loading" class="loading">Chargement des statistiques...</div>
    <div v-else-if="error" class="error">Erreur : {{ error }}</div>

    <div v-else>
      <!-- Cartes statistiques -->
      <div class="stats-grid">
        <div class="stat-card">
          <h3>Livres totaux</h3>
          <p>{{ stats.totalBooks }}</p>
        </div>
        <div class="stat-card">
          <h3>Utilisateurs</h3>
          <p>{{ stats.totalUsers }}</p>
        </div>
        <div class="stat-card">
          <h3>Emprunts totaux</h3>
          <p>{{ stats.totalBorrowings }}</p>
        </div>
        <div class="stat-card">
          <h3>Emprunts en cours</h3>
          <p>{{ stats.currentBorrowings }}</p>
        </div>
        <div class="stat-card">
          <h3>Retards</h3>
          <p>{{ stats.totalDelays }}</p>
        </div>
        <div class="stat-card">
          <h3>Taux de retard</h3>
          <p>{{ stats.delayRate.toFixed(2) }} %</p>
        </div>
      </div>

      <!-- Graphique Top 15 livres les plus empruntés -->
      <h2>Top 15 des livres les plus empruntés</h2>
      <div class="chart-container">
        <Bar v-if="popularBooksData.labels.length > 0"
             :data="popularBooksData"
             :options="chartOptions" />
        <p v-else class="no-data">Aucun emprunt enregistré.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { Bar } from 'vue-chartjs' // ← Import correct
  import {
    Chart as ChartJS,
    Title,
    Tooltip,
    Legend,
    BarElement,
    CategoryScale,
    LinearScale
  } from 'chart.js'

  ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale)

  const stats = ref({})
  const loading = ref(true)
  const error = ref(null)

  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'top' }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { stepSize: 1 }
      }
    }
  }

  const popularBooksData = computed(() => {
    const books = stats.value.popularBooks || []
    return {
      labels: books.map(b => b.bookName),
      datasets: [{
        label: 'Nombre d\'emprunts',
        data: books.map(b => b.borrowCount),
        backgroundColor: '#42A5F5',
        borderColor: '#1E88E5',
        borderWidth: 1
      }]
    }
  })

  onMounted(async () => {
    try {
      const res = await fetch('/statistics')
      if (!res.ok) throw new Error()
      stats.value = await res.json()
    } catch (err) {
      console.error(err)
      error.value = 'Impossible de charger les statistiques.'
    } finally {
      loading.value = false
    }
  })
</script>

<style scoped>
  .statistics-page {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;
    margin-bottom: 40px;
  }

  .stat-card {
    background: var(--card-bg, #fff);
    color: var(--text-primary, #333);
    padding: 24px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    text-align: center;
  }

    .stat-card h3 {
      margin: 0 0 12px;
      font-size: 1.1rem;
      opacity: 0.8;
    }

    .stat-card p {
      margin: 0;
      font-size: 2.5rem;
      font-weight: bold;
    }

  .chart-container {
    position: relative;
    height: 500px;
    margin: 40px 0;
    background: var(--card-bg, #fff);
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  }

  .no-data {
    text-align: center;
    color: #888;
    padding: 40px;
    font-style: italic;
  }

  @media (prefers-color-scheme: dark) {
    .stat-card,
    .chart-container {
      background: #2d3748;
      color: #f7fafc;
    }
  }
</style>
