import './assets/main.css'
import router from './router'
import { createApp } from 'vue'
import App from './App.vue'

const app = createApp(App)
app.use(router)
app.mount('#app')

fetch('/auth/me', { credentials: 'include' })
  .then(r => r.json())
  .then(user => console.log('Mon utilisateur:', user))
