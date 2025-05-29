<template>
  <div class="container py-5">
    <h2 class="text-orange fw-bold mb-4">Histórico de Notificações</h2>

    <div v-if="notificacoes.length === 0" class="text-muted">
      Nenhuma notificação encontrada.
    </div>

    <div class="list-group">
      <div
        v-for="notificacao in notificacoes"
        :key="notificacao.idNotificacao"
        class="list-group-item py-3 shadow-sm rounded mb-3"
      >
        <div class="d-flex align-items-center">
          <div class="me-3 d-flex align-items-center">
            <i class="fas fa-envelope fa-lg text-muted"></i>
          </div>
          <div>
            <h5 class="text-orange fw-bold mb-1">{{ notificacao.titulo }}</h5>
            <p class="mb-1">{{ notificacao.mensagem }}</p>
            <small class="text-muted">
              Enviado em {{ formatarData(notificacao.dataEnvio) }}
            </small>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'HistoricoNotificacoes',
  data() {
    return {
      notificacoes: []
    };
  },
  methods: {
    formatarData(data) {
      return new Date(data).toLocaleDateString('pt-BR') + ' às ' + new Date(data).toLocaleTimeString('pt-BR');
    },
    async carregarNotificacoes() {
      const cpf = localStorage.getItem("usuarioCPF");
      if (!cpf) {
        alert("Usuário não identificado. Faça login novamente.");
        return;
      }

      try {
        const response = await axios.get(`http://localhost:5054/api/notificacoes/usuario/${cpf}/historico`);
        this.notificacoes = response.data;
      } catch (error) {
        console.error("Erro ao buscar notificações:", error);
        alert("Erro ao carregar notificações.");
      }
    }
  },
  mounted() {
    this.carregarNotificacoes();
  }
}
</script>

<style scoped>
.text-orange {
  color: #f46c20;
}
</style>