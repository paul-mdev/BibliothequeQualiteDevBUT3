import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useReminderStore = defineStore('reminder', () => {
  const hasReminder = ref(false)
  const isCritical = ref(false)
  const message = ref('')
  const details = ref([])

  const fetchReminder = async () => {
    try {
      const res = await fetch('/user/due-reminders', { credentials: 'include' })
      if (res.ok) {
        const data = await res.json()
        hasReminder.value = data.hasReminder
        isCritical.value = data.isCritical
        message.value = data.message
        details.value = data.details
      }
    } catch (err) {
      console.error('Erreur chargement rappels', err)
    }
  }

  return { hasReminder, isCritical, message, details, fetchReminder }
})
