import { createRouter, createWebHistory } from 'vue-router'
import Accueil from '../pages/PageAccueil.vue'
import Statistiques from '../pages/PageStatistiques.vue'
import Livre from '../pages/PageLivre.vue'
import Login from '../pages/PageLogin.vue'
import Parametres from '../pages/PageParametres.vue'

const routes = [
  { path: '/', component: Accueil },
  { path: '/statistiques', component: Statistiques },
  { path: '/livre/:id', component: Livre },
  { path: '/login', component: Login },
  { path: '/parametres', component: Parametres }


]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// Guard global → protège toutes les routes sauf login
router.beforeEach(async (to, from, next) => {
  if (to.path === '/login') return next() // login accessible

  try {
    const res = await fetch('/auth/me', { credentials: 'include' })
    if (!res.ok) throw new Error('Non connecté')
    const user = await res.json()
    if (!user) throw new Error('Non connecté')
    next() // connecté → ok
  } catch {
    next({ path: '/login', query: { redirect: to.fullPath } })
  }
})

export default router
