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
        }
      }
    }
  }
})
