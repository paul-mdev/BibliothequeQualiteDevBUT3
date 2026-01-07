<template>
  <div class="statistics-page">
    <h1>Tableau de Bord - Statistiques</h1>

    <div v-if="loading">Chargement des statistiques...</div>
    <div v-else-if="error">Erreur : {{ error }}</div>
    <div v-else>
      <!-- Cartes pour stats simples -->
      <div class="stats-grid">
        <div class="stat-card">
          <h3>Livres totaux</h3>
          <p>{{ stats.totalBooks }}</p>
        </div>
        <div class="stat-card">
          <h3>Utilisateurs totaux</h3>
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
          <h3>Retards totaux</h3>
          <p>{{ stats.totalDelays }}</p>
        </div>
        <div class="stat-card">
          <h3>Taux de retard</h3>
          <p>{{ stats.delayRate.toFixed(2) }}%</p>
        </div>
      </div>

      <!-- Graphique pour livres populaires -->
      <h2>Livres les plus populaires</h2>
      <BarChart v-if="popularBooksData.labels.length > 0" :chartData="popularBooksData" :options="chartOptions" />

      <!-- Tableau pour stock par état -->
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { Bar } from 'vue-chartjs';
import { Chart as ChartJS, Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale } from 'chart.js';

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale);

export default {
  components: {
    BarChart: Bar
  },
  data() {
    return {
      stats: {
        totalBooks: 0,
        totalUsers: 0,
        totalBorrowings: 0,
        currentBorrowings: 0,
        totalDelays: 0,
        delayRate: 0,
        popularBooks: []
      },
      loading: true,
      error: null,
      chartOptions: {
        responsive: true,
        maintainAspectRatio: false
      }
    };
  },
  computed: {
    popularBooksData() {
      return {
        labels: this.stats.popularBooks.map(book => book.bookName),
        datasets: [{
          label: 'Nombre d\'emprunts',
          backgroundColor: '#42A5F5',
          data: this.stats.popularBooks.map(book => book.borrowCount)
        }]
      };
    }
  },
  async mounted() {
    try {
      const response = await axios.get('/statistics'); // Adaptez l'URL si proxy ou base URL configurée
      this.stats = response.data;
    } catch (err) {
      this.error = 'Impossible de charger les statistiques.';
      console.error(err);
    } finally {
      this.loading = false;
    }
  },
};
</script>

<style scoped>
  .statistics-page {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
  }

  /* Grille responsive pour les cartes */
  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 24px;
    margin-bottom: 40px;
  }

  /* Cartes avec fond contrasté et texte lisible en dark/light mode */
  .stat-card {
    background-color: var(--card-bg, #ffffff); /* blanc en light mode */
    color: var(--text-primary, #333333); /* texte sombre en light */
    padding: 24px;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    text-align: center;
    transition: transform 0.2s;
  }

    .stat-card:hover {
      transform: translateY(-4px);
    }

  /* En mode sombre : on inverse les couleurs */
  @media (prefers-color-scheme: dark) {
    .stat-card {
      background-color: var(--card-bg-dark, #2d3748); /* gris bleu foncé */
      color: var(--text-primary-dark, #f7fafc); /* texte clair */
    }
  }

  /* Titres et valeurs */
  .stat-card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1rem;
    opacity: 0.8;
    color: inherit;
  }

  .stat-card p {
    margin: 0;
    font-size: 2.5rem;
    font-weight: bold;
    color: inherit;
  }

  /* Tableau */
  table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 20px;
    background-color: var(--card-bg, #ffffff);
    color: var(--text-primary, #333333);
  }

  th, td {
    border: 1px solid #e2e8f0;
    padding: 12px;
    text-align: left;
  }

  th {
    background-color: #edf2f7;
    font-weight: 600;
  }

  /* Adaptation dark mode pour le tableau */
  @media (prefers-color-scheme: dark) {
    table {
      background-color: var(--card-bg-dark, #2d3748);
      color: var(--text-primary-dark, #f7fafc);
    }

    th {
      background-color: #4a5568;
    }

    th, td {
      border-color: #4a5568;
    }
  }

  h2 {
    margin-top: 40px;
    margin-bottom: 20px;
    color: inherit;
  }
</style>
