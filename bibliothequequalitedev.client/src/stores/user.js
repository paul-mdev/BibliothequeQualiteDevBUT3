import { reactive } from 'vue'

export const userState = reactive({
  user: null,
  isLoggedIn: false,
  isAdmin: false
})

export async function fetchUser() {
  try {
    const res = await fetch('/auth/me', { credentials: 'include' })
    if (!res.ok) {
      userState.user = null
      userState.isLoggedIn = false
      userState.isAdmin = false
      return
    }
    const u = await res.json()
    userState.user = u
    userState.isLoggedIn = true
    userState.isAdmin = u.role?.role_name === 'Administrateur'
  } catch {
    userState.user = null
    userState.isLoggedIn = false
    userState.isAdmin = false
  }
}
