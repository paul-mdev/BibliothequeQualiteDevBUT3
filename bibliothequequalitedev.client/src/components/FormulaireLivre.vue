<template>
  <div class="book-form">
    <!-- ===== CHAMPS DU FORMULAIRE ===== -->
    <input v-model="localBook.book_name" placeholder="Nom du livre" required />
    <input v-model="localBook.book_author" placeholder="Auteur" required />
    <input v-model="localBook.book_editor" placeholder="Éditeur" />
    <input type="date" v-model="localBook.book_date" required />

    <!-- ===== CHAMP QUANTITÉ ===== -->
    <!-- Comportement différent selon le mode (ajout/modification) -->
    <div class="quantity-field">
      <label>Nombre d'exemplaires à ajouter :</label>
      <input type="number"
             v-model.number="localBook.quantity"
             min="1"
             :required="isAddMode" />
    </div>

    <!-- ===== UPLOAD D'IMAGE ===== -->
    <input type="file" @change="onFileChange" accept="image/*" />

    <!-- Bouton de soumission avec label dynamique -->
    <button @click="submit">{{ submitLabel }}</button>
  </div>
</template>

<script setup>
  import { ref, watch, computed } from 'vue'

  // ===== PROPS =====
  /**
   * Props reçues du composant parent
   * - book : données du livre à éditer (ou objet vide pour création)
   * - submitLabel : texte du bouton ("Ajouter" ou "Modifier")
   */
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

  // ===== EVENTS =====
  /**
   * Événement émis lors de la soumission du formulaire
   */
  const emit = defineEmits(['submit'])

  /**
   * ===== COMPUTED - DÉTECTION DU MODE =====
   * Détermine si on est en mode ajout ou modification
   * Basé sur le texte du label du bouton
   */
  const isAddMode = computed(() =>
    props.submitLabel.toLowerCase().includes('ajout')
  )

  /**
   * ===== NORMALISATION DE DATE =====
   * Convertit une date en format ISO pour l'input type="date"
   * Format attendu : YYYY-MM-DD
   * @param {string|Date} date - Date à normaliser
   * @returns {string} Date au format YYYY-MM-DD
   */
  const normalizeDate = (date) => {
    if (!date) return ''
    return new Date(date).toISOString().split('T')[0]
  }

  /**
   * ===== ÉTAT LOCAL DU FORMULAIRE =====
   * Copie locale des données pour éviter de modifier directement les props
   */
  const localBook = ref({
    book_name: '',
    book_author: '',
    book_editor: '',
    book_date: '',
    quantity: 1
  })

  // Fichier image sélectionné
  const file = ref(null)

  /**
   * ===== WATCHER SUR LES PROPS =====
   * Synchronise les données locales avec les props
   * Déclenché quand le parent change la prop 'book'
   * Important pour le mode édition
   */
  watch(
    () => props.book,
    (newBook) => {
      localBook.value = {
        book_name: newBook.book_name || '',
        book_author: newBook.book_author || '',
        book_editor: newBook.book_editor || '',
        book_date: normalizeDate(newBook.book_date),
        // Quantité : 1 en mode ajout, 0 en mode modification
        quantity: isAddMode.value ? 1 : 0
      }
    },
    { immediate: true }  // Exécute immédiatement au montage
  )

  /**
   * Gère le changement de fichier image
   * @param {Event} e - Événement de changement
   */
  const onFileChange = (e) => {
    file.value = e.target.files[0] || null
  }

  /**
   * ===== SOUMISSION DU FORMULAIRE =====
   * Émet un événement 'submit' avec toutes les données
   * Le composant parent gère l'envoi à l'API
   */
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
