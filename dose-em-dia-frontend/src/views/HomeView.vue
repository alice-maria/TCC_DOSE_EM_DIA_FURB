<template>
  <v-container fluid class="pa-0">
    <div class="pagina-home px-4 py-5">
      <!-- Cabeçalho -->
      <div class="header d-flex justify-content-between align-items-center mb-3">
        <div class="logo-container" @click="$router.push('/home')" style="cursor: pointer;">
          <img src="@/imagens/logo.png" alt="Logo Dose em Dia" class="logo-img" />
          <span class="mensagem-boas-vindas text-muted fw-bold">Seja bem-vindo(a)!</span>
        </div>

        <div class="usuario d-flex align-items-center gap-2">
          <img src="@/imagens/UserPhoto.png" alt="Ícone de usuário" class="icone-usuario"
            @click="$router.push('/editar-perfil')" />
          <span class="saudacao" @click="$router.push('/editar-perfil')">
            Olá, {{ nomeUsuario }}!
          </span>
        </div>
      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs class="meus-breadcrumbs px-1 mb-4" :items="breadcrumbs">
        <template v-slot:item="{ item }">
          <span :class="['breadcrumb-laranja', { 'breadcrumb-laranja': !item.to }]" @click="item.to && navegar(item.to)"
            style="cursor: pointer;">
            <v-icon left small v-if="item.icon">{{ item.icon }}</v-icon>
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <!-- Filtros -->
      <div class="d-flex flex-wrap gap-2 mb-4">
        <button class="btn btn-outline-dark" :class="{ active: filtro === '' }" @click="filtro = ''">Todas</button>
        <button class="btn btn-outline-success" :class="{ active: filtro === 'Aplicada' }" @click="filtro = 'Aplicada'">
          Aplicadas
        </button>
        <button class="btn btn-outline-warning" :class="{ active: filtro === 'A vencer' }" @click="filtro = 'A vencer'">
          A vencer
        </button>
        <button class="btn btn-outline-danger" :class="{ active: filtro === 'Vencida' }" @click="filtro = 'Vencida'">
          Vencidas
        </button>
      </div>

      <!-- Vacinas -->
      <div class="row row-cols-1 row-cols-sm-2 row-cols-md-3 row-cols-lg-4 gx-4">
        <div class="col mb-4" v-for="vacina in vacinasFiltradas" :key="vacina.id">
          <div class="vacina-card d-flex flex-column justify-content-center p-3 shadow-sm rounded h-100"
            :class="definirClasse(mapearStatus(vacina.status))">
            <h5 class="fw-bold mb-1">{{ vacina.nome }}</h5>
            <p class="mb-0 small">Aplicada em: {{ formatarData(vacina.dataAplicacao) }}</p>
            <p class="mb-0 small">Status: {{ mapearStatus(vacina.status) }}</p>
          </div>
        </div>
      </div>
    </div>
  </v-container>
</template>

<script>
import axios from 'axios';

export default {
  name: "HomeView",
  data() {
    return {
      nomeUsuario: "",
      filtro: "",
      vacinas: [],
      breadcrumbs: [
        { text: "Início", to: "/home", icon: "mdi-home" },
      ],
    };
  },
  computed: {
    vacinasFiltradas() {
      if (!this.filtro) return this.vacinas;
      return this.vacinas.filter(v => this.mapearStatus(v.status) === this.filtro);
    }
  },
  mounted() {
    this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
    this.carregarVacinas();
  },
  methods: {
    formatarData(data) {
      const dataObj = new Date(data);
      if (isNaN(dataObj)) return 'Data inválida';
      return dataObj.toLocaleDateString('pt-BR');
    },
    definirClasse(status) {
      switch (status) {
        case 'Aplicada':
          return 'vacina-aplicada';
        case 'A vencer':
          return 'vacina-avencer';
        case 'Vencida':
          return 'vacina-vencida';
        default:
          return '';
      }
    },
    mapearStatus(codigo) {
      switch (codigo) {
        case 0: return 'Aplicada';
        case 1: return 'A vencer';
        case 2: return 'Vencida';
        default: return 'Desconhecido';
      }
    },
    async carregarVacinas() {
      const cpf = localStorage.getItem('usuarioCPF');
      if (!cpf) {
        alert('CPF não encontrado. Faça login novamente.');
        return;
      }

      try {
        const response = await axios.get(`http://localhost:5054/api/vacinas/listaVacinas/${cpf}`);

        // Se as vacinas estiverem dentro de uma propriedade:
        const vacinas = Array.isArray(response.data)
          ? response.data
          : response.data.vacinas || [];

        this.vacinas = vacinas.map(v => ({
          ...v,
          classe: this.definirClasse(this.mapearStatus(v.status))
        }));

        console.log("Vacinas carregadas:", this.vacinas);
      } catch (error) {
        console.error("Erro ao buscar vacinas:", error);
        alert("Erro ao buscar vacinas.");
      }
    },
  }
};
</script>

<style scoped>
.pagina-home {
  margin-left: 95px;
}

.logo-img {
  height: 200px;
  object-fit: contain;
}

.logo-container {
  display: flex;
  flex-direction: column;
  align-items: start;
  margin-top: -45px;
  margin-left: -25px;
}

.mensagem-boas-vindas {
  font-size: 25px;
  margin-top: -110px;
  margin-left: 200px;
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
  margin-top: 40px;
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
  margin-right: 0.1rem;
  margin-top: 37px;
}

.saudacao {
  font-weight: 500;
  margin-top: 38px;
  margin-right: 0.6rem;
}

.home-container {
  background-color: transparent;
  width: 100%;
  padding: 2rem 2rem;
  margin-left: 95px;
}

.bg-aplicada {
  background-color: #d1f7d1;
}

.bg-avencer {
  background-color: #fff9c4;
}

.bg-vencida {
  background-color: #ffcdd2;
}

.btn.active {
  font-weight: bold;
  border: 2px solid !important;
}

.vacina-card {
  border-left: 8px solid transparent;
  background-color: #fff;
  transition: all 0.2s ease-in-out;
  min-height: 120px;
}

/* Barras laterais conforme status */
.vacina-aplicada {
  border-left-color: #4caf50;
}

.vacina-avencer {
  border-left-color: #ffeb3b;
}

.vacina-vencida {
  border-left-color: #f44336;
}
</style>