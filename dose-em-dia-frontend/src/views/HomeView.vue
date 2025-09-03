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
        As vacinas exibidas são fictícias e foram geradas automaticamente pelo sistema. Não substituem documentos
        oficiais.
      </div>

      <!-- Filtros -->
      <div class="d-flex flex-wrap gap-2 mb-4">
        <button class="btn-filtro" :class="filtro === '' ? 'btn-dark text-white' : 'btn-outline-dark'"
          @click="filtro = ''">
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

const STATUS_LABELS = Object.freeze({
  APLICADA: 'Aplicada',
  A_VENCER: 'A vencer',
  VENCIDA: 'Vencida',
  DESCONHECIDO: 'Desconhecido',
});

const STATUS_MAP = Object.freeze({
  0: STATUS_LABELS.APLICADA,
  1: STATUS_LABELS.A_VENCER,
  2: STATUS_LABELS.VENCIDA,
});

const dtfBR = new Intl.DateTimeFormat('pt-BR', { timeZone: 'America/Sao_Paulo' });

const baseURL = process.env.VUE_APP_API_BASE_URL || "https://doseemdiabackend-production.up.railway.app";

export const api = axios.create({
  baseURL: baseURL.replace(/\/+$/, ""), // remove barra(s) finais
  timeout: 20000,
});

export default {
  name: 'HomeView',
  components: { UsuarioMenu },

  data() {
    return {
      nomeUsuario: '',
      filtro: '',
      vacinas: [],
      breadcrumbs: [{ text: 'Início', to: '/home', icon: 'mdi-home' }],
    };
  },

  computed: {
    vacinasFiltradas() {
      const validas = this.vacinas.filter(v => v && v.statusLabel);
      return this.filtro ? validas.filter(v => v.statusLabel === this.filtro) : validas;
    },
  },

  mounted() {
    this.nomeUsuario = localStorage.getItem('usuarioNome') || 'Usuário';
    this.carregarVacinas();
  },

  methods: {
    setFiltro(valor) {
      this.filtro = valor;
    },

    mapearStatus(codigo) {
      if (codigo === undefined || codigo === null) return STATUS_LABELS.DESCONHECIDO;
      return STATUS_MAP[codigo] || STATUS_LABELS.DESCONHECIDO;
    },

    formatarData(data) {
      const d = new Date(data);
      return Number.isNaN(d.getTime()) ? 'Data inválida' : dtfBR.format(d);
    },

    definirClasse(statusLabel) {
      switch (statusLabel) {
        case STATUS_LABELS.APLICADA:
          return 'vacina-aplicada';
        case STATUS_LABELS.A_VENCER:
          return 'vacina-avencer';
        case STATUS_LABELS.VENCIDA:
          return 'vacina-vencida';
        default:
          return '';
      }
    },

    async carregarVacinas() {
      const cpf = localStorage.getItem('usuarioCpf');
      if (!cpf) {
        console.error('CPF não encontrado no localStorage. Redirecionando para login.');
        this.$router.push('/login');
        return;
      }

      try {
        const { data } = await api.get(`/api/vacinas/listaVacinas/${encodeURIComponent(cpf)}`);

        const lista = Array.isArray(data) ? data : (data?.vacinas ?? []);
        this.vacinas = lista
          .filter(Boolean)
          .map(v => {
            const statusLabel = this.mapearStatus(v.status);
            return {
              id: v.id ?? v.Id ?? v.codigo ?? null,
              nome: v.nome ?? v.Nome ?? 'Vacina',
              dataAplicacao: v.dataAplicacao ?? v.DataAplicacao ?? v.data ?? null,
              status: v.status,
              statusLabel,
            };
          });

        console.debug('Vacinas carregadas:', this.vacinas);
      } catch (err) {
        console.error('Erro ao buscar vacinas:', err?.response ?? err);
      }
    },

    navegar(to) {
      this.$router.push(to);
    },
  },

  provide() {
    return { STATUS_LABELS };
  },
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
  align-items: flex-start;
  margin: -45px 0 0 -25px;
}

.mensagem-boas-vindas {
  margin: -110px 0 0 200px;
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
}

.btn-filtro {
  border-radius: 9999px !important;
  text-transform: none !important;
  padding: 6px 16px;
  line-height: 1.25;
  font-weight: 500;
}

@media (max-width: 600px) {
  .pagina-home {
    margin-left: 90px;
    padding: 16px 12px;
    box-sizing: border-box;
  }

  .header {
    gap: 12px;
  }

  .logo-container {
    margin: 0;
    flex-direction: row;
    align-items: center;
  }

  .logo-img {
    height: 56px;
    width: auto;
    object-fit: contain;
  }

  .mensagem-boas-vindas {
    margin: 0 0 0 8px;
    font-size: 1.35rem;
    font-weight: 600;
    line-height: 1.3;
  }

  .meus-breadcrumbs {
    margin-top: 8px !important;
  }

  .breadcrumb-laranja {
    font-size: 0.95rem;
    margin-top: 0;
  }

  .breadcrumb-home-img {
    width: 18px;
    height: 18px;
    margin-top: 0;
    margin-right: 6px;
  }

  .aviso-vacinas {
    padding: 0.75rem;
    font-size: 0.9rem;
    border-radius: 6px;
  }

  .btn-filtro {
    padding: 6px 12px;
    font-size: 0.9rem;
  }

  .row.row-cols-1.row-cols-md-3 {
    --bs-gutter-x: 0.75rem;
    --bs-gutter-y: 0.75rem;
  }

  .vacina-card {
    min-height: 88px;
  }
}
</style>