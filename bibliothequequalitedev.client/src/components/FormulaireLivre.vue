<template>
  <div class="book-form">
    <input v-model="localBook.book_name" placeholder="Nom du livre" />
    <input v-model="localBook.book_author" placeholder="Auteur" />
    <input v-model="localBook.book_editor" placeholder="Éditeur" />
    <input type="date" v-model="localBook.book_date" />
    <input type="file" @change="onFileChange" />
    <button @click="submit">{{ submitLabel }}</button>
  </div>
</template>

<script setup>
  // (le script reste exactement le même que précédemment)
  import { ref, watch } from 'vue'

  const props = defineProps({
    book: { type: Object, required: true },
    submitLabel: { type: String, required: true }
  })

  const emit = defineEmits(['submit'])

  const normalizeDate = (date) => {
    if (!date) return ''
    return new Date(date).toISOString().substring(0, 10)
  }

  const localBook = ref({
    book_name: '',
    book_author: '',
    book_editor: '',
    book_date: ''
  })

  const file = ref(null)

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
    gap: 1.2rem;
    max-width: 500px;
    width: 90%;
    margin: 3rem auto;
    padding: 2rem;
    background: var(--color-background-soft);
    border: 1px solid var(--color-border);
    border-radius: 12px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;
  }

    .book-form input {
      padding: 0.9rem 1rem;
      font-size: 1rem;
      background: var(--color-background);
      border: 1px solid var(--color-border);
      border-radius: 8px;
      color: var(--color-text);
      transition: border-color 0.2s;
    }

      .book-form input:focus {
        outline: none;
        border-color: var(--vt-c-indigo);
        box-shadow: 0 0 0 3px rgba(44, 62, 80, 0.15);
      }

      .book-form input::placeholder {
        color: var(--color-text-light-2, #888);
      }

    .book-form button {
      padding: 0.9rem 1rem;
      font-size: 1rem;
      font-weight: 600;
      background: var(--vt-c-indigo);
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      transition: background 0.3s;
      margin-top: 0.5rem;
    }

      .book-form button:hover {
        background: #243444;
      }

  /* Mobile */
  @media (max-width: 480px) {
    .book-form {
      padding: 1.5rem;
      margin: 2rem auto;
    }
  }
</style>
