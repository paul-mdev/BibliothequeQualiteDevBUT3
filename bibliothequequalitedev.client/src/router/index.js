import { createRouter, createWebHistory } from 'vue-router'

// ===== IMPORTS DES COMPOSANTS DE PAGES =====
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

// Import du store utilisateur pour la gestion de l'authentification et des droits
import { userState, fetchUser, hasRight } from '@/stores/user'

/**
 * ===== DÉFINITION DES ROUTES =====
 * Chaque route correspond à une page de l'application
 * meta.requiresRight : définit le droit nécessaire pour accéder à la route
 */
const routes = [
  // Routes publiques (accessibles sans authentification spécifique)
  { path: '/', component: Accueil },
  { path: '/livre/:id', component: Livre },
  { path: '/login', component: Login },
  { path: '/compte', component: Compte },

  // ===== ROUTES PROTÉGÉES PAR DROITS =====
  // Nécessitent le droit 'gerer_livres'
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
    path: '/gestion/emprunts',
    component: Emprunts,
    meta: { requiresRight: 'gerer_livres' }
  },

  // Nécessite le droit 'gerer_utilisateurs'
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
]

/**
 * ===== CRÉATION DU ROUTEUR =====
 * Utilise l'historique HTML5 pour des URLs propres (sans #)
 */
const router = createRouter({
  history: createWebHistory(),
  routes
})

/**
 * ===== NAVIGATION GUARD (GARDE DE NAVIGATION) =====
 * Exécuté avant chaque changement de route
 * Gère l'authentification et la vérification des droits
 * 
 * @param {Route} to - Route de destination
 * @param {Route} from - Route d'origine
 * @param {Function} next - Fonction pour continuer/rediriger la navigation
 */
router.beforeEach(async (to, from, next) => {

  // ===== EXCEPTION : Page login toujours accessible =====
  if (to.path === '/login') {
    return next()
  }

  try {
    // ===== RÉCUPÉRATION DES DONNÉES UTILISATEUR =====
    // Appel au store pour charger/rafraîchir les infos utilisateur
    await fetchUser()

    // ===== VÉRIFICATION AUTHENTIFICATION =====
    // Si l'utilisateur n'est pas connecté, redirection vers login
    if (!userState.isLoggedIn) {
      return next({
        path: '/login',
        // Sauvegarde l'URL demandée pour rediriger après connexion
        query: { redirect: to.fullPath }
      })
    }

    // ===== VÉRIFICATION DES DROITS SPÉCIFIQUES =====
    // Si la route nécessite un droit particulier (défini dans meta)
    if (to.meta.requiresRight) {
      const requiredRight = to.meta.requiresRight

      // Vérification si l'utilisateur possède le droit
      if (!hasRight(requiredRight)) {
        alert(`Accès refusé : vous n'avez pas le droit "${requiredRight}"`)
        return next('/')
      }
    }

    // ===== AUTORISATION DE NAVIGATION =====
    console.log('[ROUTER GUARD] Accès autorisé')
    next()

  } catch (err) {
    // ===== GESTION DES ERREURS =====
    // En cas d'erreur (ex: problème réseau), redirection vers login
    console.error('[ROUTER GUARD] Erreur :', err)
    return next({
      path: '/login',
      query: { redirect: to.fullPath }
    })
  }
})

export default router
