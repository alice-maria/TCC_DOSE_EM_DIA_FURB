
<template>
  <div class="container d-flex justify-content-center py-5">
    <div class="home-container p-4 rounded shadow">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 class="text-orange fw-bold">Olá, {{ nomeUsuario }}!</h2>
          <p class="text-muted mb-0">Bem-vindo ao Dose em Dia</p>
        </div>
        <div class="dropdown">
          <button class="btn btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
            Menu
          </button>
          <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="#">Minhas Vacinas</a></li>
            <li><a class="dropdown-item" href="#">Notificações</a></li>
            <li><a class="dropdown-item" href="#">Perfil</a></li>
            <li><a class="dropdown-item" href="#">Sair</a></li>
          </ul>
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
          <div class="card border-0 shadow-sm rounded" :class="vacina.classe">
            <div class="card-body">
              <h5 class="card-title fw-bold">{{ vacina.nome }}</h5>
              <p class="card-text mb-1">Status: {{ vacina.status }}</p>
              <small>Data: {{ vacina.data }}</small>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: "HomeView",
  data() {
    return {
      nomeUsuario: "",
      filtro: "",
      vacinas: [
        {
          id: 1,
          nome: "Hepatite B",
          status: "Aplicada",
          data: "10/02/2023",
          classe: "bg-aplicada"
        },
        {
          id: 2,
          nome: "Tétano",
          status: "A vencer",
          data: "15/08/2024",
          classe: "bg-avencer"
        },
        {
          id: 3,
          nome: "Febre Amarela",
          status: "Vencida",
          data: "01/01/2022",
          classe: "bg-vencida"
        }
      ]
    };
  },
  computed: {
    vacinasFiltradas() {
      if (!this.filtro) return this.vacinas;
      return this.vacinas.filter(v => v.status === this.filtro);
    }
  },
  mounted() {
    const nome = localStorage.getItem("usuarioNome");
    this.nomeUsuario = nome || "Usuário";
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
  max-width: 1400px; /* ficou maior e mais "dashboard" */
  min-height: 80vh;   /* opcional: pra garantir altura */
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
</style>
