import { createRouter, createWebHistory } from 'vue-router'
import Accueil from '../pages/PageAccueil.vue'
import Statistiques from '../pages/PageStatistiques.vue'
import Livre from '../pages/PageLivre.vue'
import Login from '../pages/PageLogin.vue'
import Gestion from '../pages/PageGestionLivre.vue'
import AjouterLivre from '../pages/PageAjouterLivre.vue'
import ModifierLivre from '../pages/PageModifierLivre.vue'
import GestionUtilisateur from '../pages/PageGestionUtilisateur.vue'
import Compte from '../pages/PageCompte.vue'
import Emprunts from '../pages/PageGestionEmprunts.vue'

// ✅ Import du store
import { userState, fetchUser, hasRight } from '@/stores/user'

const routes = [
  { path: '/', component: Accueil },
  { path: '/livre/:id', component: Livre },
  { path: '/login', component: Login },
  { path: '/compte', component: Compte },

  // ⭐ Routes protégées par droits
  {
    path: '/gestion/livres',
    component: Gestion,
    meta: { requiresRight: 'gerer_livres' }
  },
  {
    path: '/book/new',
    component: AjouterLivre,
    meta: { requiresRight: 'gerer_livres' }
  },
  {
    path: '/livre/edit/:id',
    component: ModifierLivre,
    meta: { requiresRight: 'gerer_livres' }
  },
  {
    path: '/gestion/utilisateurs',
    component: GestionUtilisateur,
    meta: { requiresRight: 'gerer_utilisateurs' }
  },
  {
    path: '/statistiques',
    component: Statistiques,
    meta: { requiresRight: 'gerer_utilisateurs' }
  },
  {
    path: '/gestion/emprunts',
    component: Emprunts,
    meta: { requiresRight: 'gerer_utilisateurs' }
  },

]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {

  // ✅ Page login toujours accessible
  if (to.path === '/login') {
    return next()
  }

  try {
    // ✅ Récupérer les infos utilisateur
    await fetchUser()

    // ❌ Non connecté → redirection login
    if (!userState.isLoggedIn) {
      return next({
        path: '/login',
        query: { redirect: to.fullPath }
      })
    }

    // ⭐ Vérifier si la route nécessite un droit spécifique
    if (to.meta.requiresRight) {
      const requiredRight = to.meta.requiresRight

      if (!hasRight(requiredRight)) {
        alert(`Accès refusé : vous n'avez pas le droit "${requiredRight}"`)
        return next('/')
      }
    }

    next()
  } catch (err) {
    console.error('[ROUTER GUARD] Erreur :', err)
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }
})

export default router
