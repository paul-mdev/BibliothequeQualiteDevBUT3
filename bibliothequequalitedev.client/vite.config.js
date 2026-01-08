import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue' // ← meilleure pratique : import nommé

export default defineConfig(({ command }) => {
  const isDocker = process.env.DOCKER === 'true'

  return {
    plugins: [vue()],

    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },

    server: {
      host: true,
      port: 5173,
      https: isDocker ? false : true,

      proxy: {
        '^/book': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/images': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/auth': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/users': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/roles': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/statistics': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/borrow': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        },
        '^/user': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true,
          secure: false
        }
      }
    }
  }
})
