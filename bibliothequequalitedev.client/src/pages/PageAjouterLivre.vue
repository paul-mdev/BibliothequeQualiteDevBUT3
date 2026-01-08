<template>
	<div>
		<h1>Ajouter un livre</h1>

		<!-- ===== COMPOSANT FORMULAIRE RÉUTILISABLE ===== -->
		<!-- Utilise le composant FormulaireLivre avec :
				 - book: objet initial avec valeurs par défaut
				 - submit-label: texte du bouton de soumission
				 - @submit: événement déclenché lors de la soumission -->
		<BookForm :book="book"
							submit-label="Ajouter"
							@submit="addBook" />
	</div>
</template>

<script setup>
	import { ref } from 'vue'
	import { useRouter } from 'vue-router'
	import BookForm from '@/components/FormulaireLivre.vue'

	const router = useRouter()

	/**
	 * ===== DONNÉES INITIALES DU FORMULAIRE =====
	 * Objet réactif avec les valeurs par défaut pour un nouveau livre
	 */
	const book = ref({
		book_name: '',      // Nom du livre (vide)
		book_author: '',    // Auteur (vide)
		book_editor: '',    // Éditeur (vide)
		book_date: '',      // Date de publication (vide)
		quantity: 1         // Quantité par défaut : 1 exemplaire
	})

	/**
	 * ===== FONCTION D'AJOUT DE LIVRE =====
	 * Envoie les données du formulaire à l'API
	 * Utilise FormData pour supporter l'upload d'image
	 * @param {Object} data - Données du formulaire émises par BookForm
	 */
	const addBook = async (data) => {
		// ===== CONSTRUCTION DE FORMDATA =====
		// Nécessaire pour envoyer des fichiers (image)
		const formData = new FormData()
		formData.append('book_name', data.book_name)
		formData.append('book_author', data.book_author)
		formData.append('book_editor', data.book_editor)
		formData.append('book_date', data.book_date)
		formData.append('quantity', data.quantity.toString())

		// Ajout de l'image si présente
		if (data.file) {
			formData.append('image', data.file)
		}

		try {
			const res = await fetch('/book', {
				method: 'POST',
				body: formData,
				credentials: 'include'  // Inclut les cookies de session
			})

			if (!res.ok) {
				const errorText = await res.text()
				alert(errorText || 'Erreur lors de l'ajout du livre')
				return
			}

			// Succès : redirection vers la page de gestion
			router.push('/gestion')
		} catch (err) {
			console.error(err)
			alert('Erreur réseau lors de l'ajout')
		}
	}
</script>

<style scoped>
	div {
		max-width: 600px;
		margin: 2rem auto;
		padding: 1rem;
	}

	h1 {
		text-align: center;
		margin-bottom: 2rem;
	}
</style>
