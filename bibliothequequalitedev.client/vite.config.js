import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import plugin from '@vitejs/plugin-vue'

export default defineConfig(({ command }) => {
  const isDocker = process.env.DOCKER === 'true'

  return {
    plugins: [plugin()],
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
          changeOrigin: true
        },
        '^/images': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        },
        '^/auth': {
          target: isDocker ? 'http://api:5000' : undefined, 
          changeOrigin: true
        },
        '^/users': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        },
        '^/roles': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        }
        ,
        '^/statistics': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        }
        ,
        '^/borrow': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        },
        '^/UsersBorrowed': {
          target: isDocker ? 'http://api:5000' : undefined,
          changeOrigin: true
        }
      }
    }
  }
})
