<template>
  <v-container fluid class="pa-0">
    <div class="pagina-notificacoes">
      <!-- Cabeçalho -->
      <div class="header">
        <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
        <div class="usuario">
          <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario"
            @click="$router.push('/editar-perfil')" />
          <span class="saudacao" @click="$router.push('/editar-perfil')">Olá, {{ nomeUsuario }}!</span>
        </div>
      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs class="meus-breadcrumbs px-6" :items="breadcrumbs">
        <template v-slot:item="{ item }">
          <span :class="['breadcrumb-link', { 'breadcrumb-laranja': !item.to }]" @click="item.to && navegar(item.to)"
            style="cursor: pointer;">
            <v-icon left small v-if="item.icon">{{ item.icon }}</v-icon>
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <div v-if="notificacoes.length === 0" class="text-muted py-4">
        Nenhuma notificação encontrada.
      </div>

      <div v-else class="d-flex flex-column gap-4">
        <v-card v-for="notificacao in notificacoes" :key="notificacao.idNotificacao" class="pa-4 elevation-1" rounded>
          <div class="d-flex align-center mb-2">
            <v-icon color="grey-darken-1" class="me-2">mdi-email</v-icon>
            <h5 class="text-orange-darken-2 font-weight-bold mb-0">
              {{ notificacao.titulo }}
            </h5>
          </div>
          <div class="text-body-1 mb-1">{{ notificacao.mensagem }}</div>
          <div class="text-caption text-grey-darken-1">
            Enviado em {{ formatarData(notificacao.dataEnvio) }}
          </div>
        </v-card>
      </div>
    </div>
  </v-container>
</template>
<script>
import axios from 'axios';

export default {
  name: 'HistoricoNotificacoes',
  data() {
    return {
      notificacoes: [],
      nomeUsuario: localStorage.getItem('usuarioNome') || 'Usuário',
      breadcrumbs: [
        { text: 'Serviços e Informações', to: '/home', icon: 'mdi-home' },
        { text: 'Notificações' }
      ]
    };
  },
  methods: {
    formatarData(data) {
      const date = new Date(data);
      if (isNaN(date)) return "Data inválida";
      return date.toLocaleDateString('pt-BR') + ' às ' + date.toLocaleTimeString('pt-BR');
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
    },
    navegar(destino) {
      this.$router.push(destino);
    }
  },
  mounted() {
    this.carregarNotificacoes();
  }
};
</script>
<style scoped>
.pagina-notificacoes {
  margin-left: 95px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem 2rem;
  background-color: white;
  border-bottom: 1px solid #eee;
}

.titulo {
  font-size: 1.8rem;
  font-weight: bold;
  color: #f97316;
}

.usuario {
  display: flex;
  align-items: center;
}

.icone-usuario {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  margin-right: 0.5rem;
}

.saudacao {
  font-weight: 500;
}

.breadcrumb-link {
  color: #6b7280;
  transition: color 0.2s;
  font-size: 1.1rem;
}

.breadcrumb-link:hover {
  color: #f97316;
  text-decoration: underline;
}

.breadcrumb-laranja {
  color: #f97316 !important;
  font-weight: 900;
  font-size: 1.1rem;
}
</style>
