<template>
  <div class="book-form">
    <input v-model="localBook.book_name" placeholder="Nom du livre" required />
    <input v-model="localBook.book_author" placeholder="Auteur" required />
    <input v-model="localBook.book_editor" placeholder="Éditeur" />
    <input type="date" v-model="localBook.book_date" required />

    <div class="quantity-field">
      <label>Nombre d'exemplaires à ajouter :</label>
      <input type="number"
             v-model.number="localBook.quantity"
             min="1"
             :required="isAddMode" />
    </div>

    <input type="file" @change="onFileChange" accept="image/*" />

    <button @click="submit">{{ submitLabel }}</button>
  </div>
</template>

<script setup>
  import { ref, watch, computed } from 'vue'

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

  const emit = defineEmits(['submit'])

  const isAddMode = computed(() =>
    props.submitLabel.toLowerCase().includes('ajout')
  )

  const normalizeDate = (date) => {
    if (!date) return ''
    return new Date(date).toISOString().split('T')[0]
  }

  const localBook = ref({
    book_name: '',
    book_author: '',
    book_editor: '',
    book_date: '',
    quantity: 1
  })

  const file = ref(null)

  // Synchronisation avec les props (mode édition)
  watch(
    () => props.book,
    (newBook) => {
      localBook.value = {
        book_name: newBook.book_name || '',
        book_author: newBook.book_author || '',
        book_editor: newBook.book_editor || '',
        book_date: normalizeDate(newBook.book_date),
        quantity: isAddMode.value ? 1 : 0  // 1 en ajout, 0 en modification
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
  }

    .book-form input {
      padding: 0.9rem 1rem;
      font-size: 1rem;
      background: var(--color-background);
      border: 1px solid var(--color-border);
      border-radius: 8px;
      color: var(--color-text);
    }

  .quantity-field {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

    .quantity-field label {
      font-weight: 600;
      font-size: 0.95rem;
      color: var(--color-text);
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
    margin-top: 0.5rem;
    transition: background 0.3s;
  }

    .book-form button:hover {
      background: #243444;
    }
</style>
