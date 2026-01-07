<template>
  <div class="book-form">
    <input v-model="localBook.book_name"
           placeholder="Nom du livre" />

    <input v-model="localBook.book_author"
           placeholder="Auteur" />

    <input v-model="localBook.book_editor"
           placeholder="Éditeur" />

    <input type="date"
           v-model="localBook.book_date" />

    <input type="file"
           @change="onFileChange" />

    <button @click="submit">
      {{ submitLabel }}
    </button>
  </div>
</template>

<script setup>
  import { ref, watch } from 'vue'

  /* props */
  const props = defineProps({
    book: {
      type: Object,
      required: true
    },
    submitLabel: {
      type: String,
      required: true
    }
  })

  /* events */
  const emit = defineEmits(['submit'])

  /* helpers */
  const normalizeDate = (date) => {
    if (!date) return ''
    return date.toString().substring(0, 10) // YYYY-MM-DD
  }

  /* state */
  const localBook = ref({
    book_name: '',
    book_author: '',
    book_editor: '',
    book_date: ''
  })

  const file = ref(null)

  /* sync props → local state */
  watch(
    () => props.book,
    (newBook) => {
      localBook.value = {
        ...newBook,
        book_date: normalizeDate(newBook.book_date)
      }
    },
    { immediate: true }
  )

  /* handlers */
  const onFileChange = (e) => {
    file.value = e.target.files[0] || null
  }

  const submit = () => {
    emit('submit', {
      ...localBook.value,
      file: file.value
    })
  }
</script>

<style scoped>
  .book-form {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    max-width: 400px;
    margin: auto;
  }
</style>
