import { reactive } from 'vue'

export const userState = reactive({
  user: null,
  isLoggedIn: false,
  rights: [] // Liste des droits de l'utilisateur
})

export async function fetchUser() {
  try {
    const res = await fetch('/auth/me', { credentials: 'include' })
    if (!res.ok) {
      userState.user = null
      userState.isLoggedIn = false
      userState.rights = []
      return false
    }
    const u = await res.json()
    userState.user = u
    userState.isLoggedIn = true
    userState.rights = u.role?.rights || [] // Récupère les droits

    return true
  } catch (err) {
    console.error('Erreur fetchUser:', err)
    userState.user = null
    userState.isLoggedIn = false
    userState.rights = []
    return false
  }
}

// ⭐ Fonction helper pour vérifier si l'utilisateur a un droit
export function hasRight(rightName) {
  const result = userState.rights.includes(rightName)
  console.log(`hasRight("${rightName}") = ${result}`) // CORRECTION ICI : parenthèses au lieu de backticks
  return result
}

// ⭐ Fonction helper pour vérifier si l'utilisateur a AU MOINS UN des droits
export function hasAnyRight(...rightNames) {
  return rightNames.some(right => userState.rights.includes(right))
}

// ⭐ Fonction helper pour vérifier si l'utilisateur a TOUS les droits
export function hasAllRights(...rightNames) {
  return rightNames.every(right => userState.rights.includes(right))
}
