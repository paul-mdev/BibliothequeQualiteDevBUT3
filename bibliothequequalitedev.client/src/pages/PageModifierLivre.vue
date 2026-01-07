<template>
  <div>
    <h1>Modifier le livre</h1>

    <BookForm v-if="book"
              :book="book"
              submit-label="Modifier"
              @submit="updateBook" />
  </div>
</template>

<script setup>
  import { ref, onMounted } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import BookForm from '@/components/FormulaireLivre.vue'

  const route = useRoute()
  const router = useRouter()
  const book = ref(null)

  onMounted(async () => {
    const res = await fetch(`/book/${route.params.id}`)
    if (!res.ok) return router.push('/gestion')
    book.value = await res.json()
  })

  const updateBook = async (data) => {
    const formData = new FormData()
    formData.append('book_name', data.book_name)
    formData.append('book_author', data.book_author)
    formData.append('book_editor', data.book_editor)
    formData.append('book_date', data.book_date)
    if (data.file) formData.append('image', data.file)

    await fetch(`/book/${route.params.id}`, {
      method: 'PUT',
      body: formData,
      credentials: 'include'
    })

    router.push('/gestion')
  }
</script>
