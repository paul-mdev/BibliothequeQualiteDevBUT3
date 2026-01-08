import { ref } from 'vue'
import { defineStore } from 'pinia'

// Store Pinia gérant les rappels d'emprunts utilisateur
export const useReminderStore = defineStore('reminder', () => {
  const hasReminder = ref(false)
  const isCritical = ref(false)
  const message = ref('')
  const details = ref([])

  // Récupération des rappels depuis l'API
  const fetchReminder = async () => {
    try {
      const res = await fetch('/user/due-reminders', {
        credentials: 'include' // cookies de session
      })

      if (res.ok) {
        const data = await res.json()

        // Mise à jour des états du store
        hasReminder.value = data.hasReminder
        isCritical.value = data.isCritical
        message.value = data.message
        details.value = data.details
      }
    } catch (err) {
      console.error('Erreur chargement rappels', err)
    }
  }

  // Exposition des états et actions
  return {
    hasReminder,
    isCritical,
    message,
    details,
    fetchReminder
  }
})
