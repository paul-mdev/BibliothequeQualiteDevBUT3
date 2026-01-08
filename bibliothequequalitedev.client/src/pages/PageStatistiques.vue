<template>
  <div class="statistics-page">
    <h1>Tableau de Bord - Statistiques</h1>

    <!-- État de chargement -->
    <div v-if="loading" class="loading">
      Chargement des statistiques...
    </div>

    <!-- Message d'erreur -->
    <div v-else-if="error" class="error">
      Erreur : {{ error }}
    </div>

    <!-- Contenu affiché une fois les données chargées -->
    <div v-else>
      <!-- ============================
           Cartes de statistiques globales
           ============================ -->
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

      <!-- ============================
           Graphique des livres les plus empruntés
           ============================ -->
      <h2>Top 15 des livres les plus empruntés</h2>

      <div class="chart-container">
        <!-- Graphique affiché uniquement s'il y a des données -->
        <Bar v-if="popularBooksData.labels.length > 0"
             :data="popularBooksData"
             :options="chartOptions" />

        <!-- Message si aucune donnée n'est disponible -->
        <p v-else class="no-data">
          Aucun emprunt enregistré.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
  // ============================
  // Imports Vue
  // ============================
  import { ref, computed, onMounted } from 'vue'

  // Composant graphique (Bar chart)
  import { Bar } from 'vue-chartjs'

  // Modules nécessaires de Chart.js
  import {
    Chart as ChartJS,
    Title,
    Tooltip,
    Legend,
    BarElement,
    CategoryScale,
    LinearScale
  } from 'chart.js'

  // Enregistrement des modules Chart.js utilisés
  ChartJS.register(
    Title,
    Tooltip,
    Legend,
    BarElement,
    CategoryScale,
    LinearScale
  )

  // ============================
  // États réactifs
  // ============================
  const stats = ref({})
  const loading = ref(true)
  const error = ref(null)

  // ============================
  // Options du graphique
  // ============================
  const chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top'
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: 1
        }
      }
    }
  }

  // ============================
  // Données du graphique "Top 15 livres"
  // ============================
  const popularBooksData = computed(() => {
    const books = stats.value.popularBooks || []

    return {
      labels: books.map(b => b.bookName),
      datasets: [
        {
          label: 'Nombre d\'emprunts',
          data: books.map(b => b.borrowCount),
          backgroundColor: '#42A5F5',
          borderColor: '#1E88E5',
          borderWidth: 1
        }
      ]
    }
  })

  // ============================
  // Chargement des statistiques au montage
  // ============================
  onMounted(async () => {
    try {
      const res = await fetch('/statistics')

      if (!res.ok) {
        throw new Error('Erreur API')
      }

      // Données reçues depuis l'API
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
  /* Conteneur principal */
  .statistics-page {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }

  /* Grille des cartes statistiques */
  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;
    margin-bottom: 40px;
  }

  /* Carte individuelle */
  .stat-card {
    background: var(--card-bg, #fff);
    color: var(--text-primary, #333);
    padding: 24px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
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

  /* Conteneur du graphique */
  .chart-container {
    position: relative;
    height: 500px;
    margin: 40px 0;
    background: var(--card-bg, #fff);
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  /* Message absence de données */
  .no-data {
    text-align: center;
    color: #888;
    padding: 40px;
    font-style: italic;
  }

  /* Mode sombre */
  @media (prefers-color-scheme: dark) {
    .stat-card,
    .chart-container {
      background: #2d3748;
      color: #f7fafc;
    }
  }
</style>
