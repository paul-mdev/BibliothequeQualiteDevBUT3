import { createRouter, createWebHistory } from 'vue-router'
import Accueil from '../pages/PageAccueil.vue'
import Statistiques from '../pages/PageStatistiques.vue'
import Livre from '../pages/PageLivre.vue'
import Login from '../pages/PageLogin.vue'
import Parametres from '../pages/PageParametres.vue'
import Gestion from '../pages/PageGestion.vue'
import AjouterLivre from '../pages/PageAjouterLivre.vue'
import ModifierLivre from '../pages/PageModifierLivre.vue'


const routes = [
  { path: '/', component: Accueil },
  { path: '/statistiques', component: Statistiques },
  { path: '/livre/:id', component: Livre },
  { path: '/login', component: Login },
  { path: '/parametres', component: Parametres },
  { path: '/gestion', component: Gestion },
  { path: '/book/new', component: AjouterLivre },
  { path: '/livre/edit/:id', component: ModifierLivre },

]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, from, next) => {
  // Si on va vers /login → on laisse passer sans aucune vérification
  if (to.path === '/login') {
    return next();
  }

  // Pour toutes les autres routes → on vérifie la session
  try {
    const res = await fetch('/auth/me', { credentials: 'include' });

    // 401 = pas de session valide
    if (res.status === 401) {
      throw new Error('Non connecté');
    }

    if (!res.ok) {
      throw new Error('Erreur serveur');
    }

    const user = await res.json();
    if (!user || !user.user_id) {
      throw new Error('Non connecté');
    }

    // Tout bon → accès autorisé
    next();
  } catch (err) {
    console.log(err)
    // Redirection vers login avec le chemin d'origine
    next({
      path: '/login',
      query: { redirect: to.fullPath }
    });
  }
});
export default router
