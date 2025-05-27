<template>
  <div class="container py-5">
    <div class="text-center mb-4">
      <h2 class="fw-bold text-orange">Exportar Comprovante</h2>
      <p class="text-muted">Baixe seu comprovante de vacinação em PDF</p>
    </div>

    <div class="text-center">
      <button class="btn btn-success" @click="baixarComprovante">
        <i class="fas fa-file-pdf me-2"></i> Baixar Comprovante
      </button>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ComprovanteView',
  methods: {
    async baixarComprovante() {
      const usuarioId = localStorage.getItem("usuarioId"); // ou "IdUser"
      if (!usuarioId) {
        alert("Usuário não identificado. Faça login novamente.");
        return;
      }

      try {
        const response = await axios.get(`http://localhost:5054/api/comprovante/${usuarioId}/gerarComprovante`, {
          responseType: 'blob' // importante para arquivos binários
        });

        // Criar um link e baixar o PDF
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', 'comprovante-vacinacao.pdf');
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      } catch (error) {
        console.error("Erro ao gerar comprovante:", error);
        alert("Erro ao gerar comprovante. Tente novamente mais tarde.");
      }
    }
  }
};
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}
</style>
