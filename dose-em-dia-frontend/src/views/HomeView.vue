<template>
  <div class="container d-flex justify-content-center py-5">
    <div class="home-container p-4 rounded shadow">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="text-orange fw-bold">Olá, {{ nomeUsuario }}!</h2>
          <p class="text-muted mb-0">Bem-vindo ao Dose em Dia</p>
        </div>
      </div>

      <!-- Filtros -->
      <div class="d-flex gap-2 mb-4">
        <button class="btn btn-outline-dark" :class="{ active: filtro === '' }" @click="filtro = ''">Todas</button>
        <button class="btn btn-outline-success" :class="{ active: filtro === 'Aplicada' }" @click="filtro = 'Aplicada'">Aplicadas</button>
        <button class="btn btn-outline-warning" :class="{ active: filtro === 'A vencer' }" @click="filtro = 'A vencer'">A vencer</button>
        <button class="btn btn-outline-danger" :class="{ active: filtro === 'Vencida' }" @click="filtro = 'Vencida'">Vencidas</button>
      </div>

      <!-- Vacinas -->
      <div class="row">
        <div class="col-md-4 mb-3" v-for="vacina in vacinasFiltradas" :key="vacina.id">
          <div class="vacina-card d-flex flex-column justify-content-center p-3 shadow-sm rounded" :class="definirClasse(mapearStatus(vacina.status))">
            <h5 class="fw-bold mb-1">{{ vacina.nome }}</h5>
            <p class="mb-0 small">Aplicada em: {{ formatarData(vacina.dataAplicacao) }}</p>
            <p class="mb-0 small">Status: {{ mapearStatus(vacina.status) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: "HomeView",
  data() {
    return {
      nomeUsuario: "",
      filtro: "",
      vacinas: []
    };
  },
  computed: {
    vacinasFiltradas() {
      if (!this.filtro) return this.vacinas;
      return this.vacinas.filter(v => v.status === this.filtro);
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
.text-orange {
  color: #f46c20;
}
.home-container {
  background-color: #f8f9fa;
  width: 100%;
  max-width: 2500px;
  min-height: 80vh;
  padding: 2rem;
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
  border: 2px solid #f46c20 !important;
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