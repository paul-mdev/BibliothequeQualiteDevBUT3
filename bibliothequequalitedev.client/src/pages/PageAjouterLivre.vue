<template>
  <div class="add-book-page">
    <h1>Ajouter un livre</h1>
    <input v-model="book_name" placeholder="Nom du livre" />
    <input v-model="book_author" placeholder="Auteur" />
    <input v-model="book_editor" placeholder="Éditeur" />
    <input type="date" v-model="book_date" />

    <!-- Fichier image -->
    <input type="file" @change="onFileChange" />

    <button @click="submitBook">Ajouter</button>
  </div>
</template>

<script setup>
  import { ref } from 'vue'

  const book_name = ref('')
  const book_author = ref('')
  const book_editor = ref('')
  const book_date = ref('')
  const file = ref(null)

  const onFileChange = (e) => {
    file.value = e.target.files[0] || null
  }

  const submitBook = async () => {
    const formData = new FormData()
    formData.append('book_name', book_name.value)
    formData.append('book_author', book_author.value)
    formData.append('book_editor', book_editor.value)
    formData.append('book_date', book_date.value)
    if (file.value) formData.append('image', file.value)

    try {
      const res = await fetch('/book', {
        method: 'POST',
        body: formData,
        credentials: 'include'
      })

      if (!res.ok) {
        const text = await res.text()
        alert('Erreur serveur : ' + res.status + '\n' + text)
        console.error('Erreur détaillée:', text)
        return
      }

      alert('Livre ajouté avec succès !')
    } catch (err) {
      alert('Erreur réseau : ' + err.message)
    }
  }
</script>
