<template>
  <v-container fluid class="pa-0">
    <div class="pagina-home px-4 py-5">
      <!-- Cabeçalho -->
      <div class="header d-flex justify-content-between align-items-center mb-3">
        <div class="logo-container" @click="$router.push('/home')" style="cursor: pointer;">
          <img src="@/imagens/logo.png" alt="Logo Dose em Dia" class="logo-img" />
          <h3 class="mensagem-boas-vindas fw-bold">Seja bem-vindo(a)!</h3>
        </div>

        <UsuarioMenu />

      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs class="meus-breadcrumbs px-1 mb-4" :items="breadcrumbs">
        <template v-slot:item="{ item }">
          <span :class="['breadcrumb-laranja', { 'breadcrumb-laranja': !item.to }]" @click="item.to && navegar(item.to)"
            style="cursor: pointer;">
            <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <div class="aviso-vacinas">
        <img src="@/assets/icons/aviso.svg" alt="Ícone de aviso" class="me-3" style="width: 24px; height: 24px;" />
        As vacinas exibidas são fictícias e foram geradas automaticamente pelo sistema. Não substituem documentos oficiais.
      </div>

      <!-- Filtros -->
      <div class="d-flex flex-wrap gap-2 mb-4">
        <button class="btn-filtro" :class="filtro === '' ? 'btn-dark text-white' : 'btn-outline-dark'" @click="filtro = ''">
          Todas
        </button>

        <button class="btn-filtro" :class="filtro === 'Aplicada' ? 'btn-success text-white' : 'btn-outline-success'"
          @click="filtro = 'Aplicada'">
          Aplicadas
        </button>

        <button class="btn-filtro" :class="filtro === 'A vencer' ? 'btn-warning text-white' : 'btn-outline-warning'"
          @click="filtro = 'A vencer'">
          A vencer
        </button>

        <button class="btn-filtro" :class="filtro === 'Vencida' ? 'btn-danger text-white' : 'btn-outline-danger'"
          @click="filtro = 'Vencida'">
          Vencidas
        </button>
      </div>
      <!-- Vacinas -->
      <div class="row row-cols-1 row-cols-md-3 gx-4">
        <template v-for="vacina in vacinasFiltradas" :key="vacina.id">
          <div class="col mb-4">
            <v-card class="vacina-card h-100 d-flex flex-column justify-space-between" variant="elevated"
              :class="definirClasse(mapearStatus(vacina.status))">
              <v-card-title class="d-flex justify-space-between align-center pb-0">
                <span class="text-h6 fw-bold">{{ vacina.nome }}</span>
              </v-card-title>

              <v-card-text class="pt-1">
                <p class="mb-1 text-body-1">Aplicada em: {{ formatarData(vacina.dataAplicacao) }}</p>
                <p class="mb-0 text-body-1">Status: {{ mapearStatus(vacina.status) }}</p>
              </v-card-text>
            </v-card>
          </div>
        </template>
      </div>
    </div>
  </v-container>
</template>

<script>
import axios from 'axios';
import UsuarioMenu from '@/views/UsuarioMenu.vue';

export default {
  components: { UsuarioMenu },
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
      const vacinasValidas = this.vacinas.filter(v => v && typeof v.status !== 'undefined');
      if (!this.filtro) return vacinasValidas;
      return vacinasValidas.filter(v => this.mapearStatus(v.status) === this.filtro);
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
      if (codigo === undefined || codigo === null) return 'Desconhecido';
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
    navegar(to) {
      this.$router.push(to);
    }
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
  font-size: inherit;
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

.breadcrumb-home-img {
    margin-top: -5px;
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

.btn.active {
  font-weight: bold;
  border: 2px solid !important;
}

.vacina-card {
  border-radius: 12px;
  background-color: #fbfbf8;
  min-height: 100px;
}

.vacina-aplicada {
  border-left: 8px solid #4caf50;
}

.vacina-avencer {
  border-left: 8px solid #ffeb3b;
}

.vacina-vencida {
  border-left: 8px solid #f44336;
}

.aviso-vacinas {
  background-color: #fef3e2;
  color: #92400e;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  border: 1px solid #fcd9b6;
  margin-bottom: 2rem;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
  font-size: inherit;
}

.btn-filtro {
  border-radius: 9999px !important;
  text-transform: none !important;
  padding: 6px 16px;              
  line-height: 1.25;
  font-weight: 500;               
}

</style>