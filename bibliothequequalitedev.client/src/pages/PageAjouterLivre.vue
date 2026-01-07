<template>
	<div>
		<h1>Ajouter un livre</h1>
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

	const book = ref({
		book_name: '',
		book_author: '',
		book_editor: '',
		book_date: ''
	})

	const addBook = async (data) => {
		const formData = new FormData()
		formData.append('book_name', data.book_name)
		formData.append('book_author', data.book_author)
		formData.append('book_editor', data.book_editor)
		formData.append('book_date', data.book_date)
		if (data.file) formData.append('image', data.file)

		const res = await fetch('/book', {
			method: 'POST',
			body: formData,
			credentials: 'include'
		})

		if (!res.ok) {
			alert('Erreur lors de l’ajout du livre')
			return
		}

		// ✅ REDIRECTION
		router.push('/gestion')
	}
</script>
