import { createRouter, createWebHistory } from 'vue-router'
import Accueil from '../pages/PageAccueil.vue'
import Statistiques from '../pages/PageStatistiques.vue'
import Livre from '../pages/PageLivre.vue'
import Login from '../pages/PageLogin.vue'
import Parametres from '../pages/PageParametres.vue'
import Gestion from '../pages/PageGestionLivre.vue'
import AjouterLivre from '../pages/PageAjouterLivre.vue'
import ModifierLivre from '../pages/PageModifierLivre.vue'
import GestionUtilisateur from '../pages/PageGestionUtilisateur.vue'
import Compte from '../pages/PageCompte.vue'

// ✅ Import du store
import { userState, fetchUser } from '@/stores/user'

const routes = [
  { path: '/', component: Accueil },
  { path: '/statistiques', component: Statistiques },
  { path: '/livre/:id', component: Livre },
  { path: '/login', component: Login },
  { path: '/parametres', component: Parametres },
  { path: '/gestion/livres', component: Gestion, meta: { requiresAdmin: true } },
  { path: '/book/new', component: AjouterLivre },
  { path: '/livre/edit/:id', component: ModifierLivre },
  { path: '/gestion/utilisateurs', component: GestionUtilisateur, meta: { requiresAdmin: true } },
  { path: '/compte', component: Compte },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  console.log('[ROUTER GUARD] Tentative d\'accès à :', to.path)

  // Pages publiques (login)
  if (to.path === '/login') {
    console.log('[ROUTER GUARD] Page login → accès autorisé')
    return next()
  }

  try {
    // ✅ Met à jour le store une fois
    await fetchUser()
    console.log('[ROUTER GUARD] Store utilisateur :', userState)

    // Non connecté → redirection login
    if (!userState.isLoggedIn) {
      console.log('[ROUTER GUARD] Non connecté → redirection login')
      return next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
    }

    // Vérifier si la route nécessite admin
    if (to.meta.requiresAdmin) {
      console.log(`[ROUTER GUARD] Page admin requise, rôle actuel : ${userState.user?.role?.role_name}`)

      if (!userState.isAdmin) {
        console.log('[ROUTER GUARD] Accès refusé → redirection accueil')
        alert('Accès refusé : vous devez être administrateur')
        return next('/')
      }
    }

    console.log('[ROUTER GUARD] Accès autorisé')
    next()
  } catch (err) {
    console.error('[ROUTER GUARD] Erreur :', err)
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }
})

export default router
