<template>
  <div class="weather-component">
    <h1>Répertoire des livres</h1>

    <input type="text"
           v-model="search"
           placeholder="Rechercher un livre..."
           class="search" />


    <div v-if="loading" class="loading">
      Loading...
    </div>
    <table v-if="filteredBooks">
      <thead>
        <tr>
          <th>Id</th>
          <th>Nom</th>
          <th>Auteur</th>
          <th>Éditeur</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="book in filteredBooks"
            :key="book.book_id"
            @click="goToBook(book.book_id)"
            class="row">
          <td>{{ book.book_id }}</td>
          <td>{{ book.book_name }}</td>
          <td>{{ book.book_author }}</td>
          <td>{{ book.book_editor }}</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="js">
  import { defineComponent } from 'vue'

  export default defineComponent({
    data() {
      return {
        loading: false,
        books: [],
        search: ''
      }
    },
    created() {
      this.fetchData()
    },
    methods: {
      fetchData() {
        this.loading = true
        fetch('book')
          .then(r => r.json())
          .then(json => {
            this.books = json
            this.loading = false
          })
      },
      goToBook(id) {
        this.$router.push(`/livre/${id}`)
      }
    },
    computed: {
      filteredBooks() {
        return this.books.filter(b =>
          b.book_name.toLowerCase().includes(this.search.toLowerCase()) ||
          b.book_author.toLowerCase().includes(this.search.toLowerCase())
        )
      }
    }
  })
</script>

<style scoped>
  .search {
    margin-bottom: 1rem;
    padding: .5rem;
    width: 300px;
  }

  .row {
    cursor: pointer;
  }

    .row:hover {
      background-color: #f2f2f2;
    }
</style>
