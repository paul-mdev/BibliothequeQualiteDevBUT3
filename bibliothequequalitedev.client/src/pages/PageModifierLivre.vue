<template>
  <div>
    <h1>Modifier le livre</h1>

    <!-- ===== COMPOSANT FORMULAIRE RÉUTILISABLE ===== -->
    <!-- Affiche le formulaire uniquement quand les données sont chargées -->
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

  /**
   * ===== CHARGEMENT DES DONNÉES DU LIVRE =====
   * Au montage, récupère les données du livre à modifier
   * L'ID est extrait de l'URL via route.params.id
   */
  onMounted(async () => {
    const res = await fetch(`/book/${route.params.id}`)

    // Si livre non trouvé, retour à la page de gestion
    if (!res.ok) return router.push('/gestion')

    // Charge les données dans le formulaire
    book.value = await res.json()
  })

  /**
   * ===== FONCTION DE MISE À JOUR =====
   * Envoie les modifications à l'API avec PUT
   * @param {Object} data - Données modifiées du formulaire
   */
  const updateBook = async (data) => {
    // Construction de FormData pour l'upload d'image
    const formData = new FormData()
    formData.append('book_name', data.book_name)
    formData.append('book_author', data.book_author)
    formData.append('book_editor', data.book_editor)
    formData.append('book_date', data.book_date)
    formData.append('quantity', data.quantity)

    // Ajout de la nouvelle image si changée
    if (data.file) formData.append('image', data.file)

    await fetch(`/book/${route.params.id}`, {
      method: 'PUT',
      body: formData,
      credentials: 'include'
    })

    // Retour à la page de gestion après modification
    router.push('/gestion')
  }
</script>
