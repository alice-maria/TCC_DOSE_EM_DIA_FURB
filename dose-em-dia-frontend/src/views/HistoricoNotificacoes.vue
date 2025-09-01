<template>
  <v-container fluid class="pa-0">
    <div class="pagina-notificacoes">
      <!-- Cabeçalho -->
      <div class="header">
        <h1 class="titulo" @click="$router.push('/home')">Dose em dia</h1>
        <div class="usuario">
          <UsuarioMenu />
        </div>
      </div>

      <!-- Breadcrumbs -->
      <v-breadcrumbs class="meus-breadcrumbs px-6" :items="breadcrumbs">
        <template v-slot:item="{ item }">
          <span v-if="item" :class="['breadcrumb-link', { 'breadcrumb-laranja': !item.to }]"
            @click="item.to && navegar(item.to)" style="cursor: pointer;">
            <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <!-- Nenhuma notificação -->
      <div v-if="Array.isArray(notificacoes) && notificacoes.length === 0" class="mensagem-nenhuma-notificacao">
        Nenhuma notificação encontrada.
      </div>

      <!-- Lista -->
      <div v-else-if="Array.isArray(notificacoes) && notificacoes.length > 0" class="d-flex flex-column gap-4">
        <v-card v-for="(notificacao, i) in notificacoes" :key="notificacao?.idNotificacao ?? `n-${i}`"
          class="outlined-card" variant="outlined" rounded="lg">
          <div class="d-flex align-center mb-2">
            <img src="@/assets/icons/email.svg" alt="Notificação" class="icone-notificacao me-2" />
            <h5 class="text-orange-darken-2 font-weight-bold mb-0">
              {{ notificacao?.titulo ?? "Notificação" }}
            </h5>
          </div>

          <div class="text-body-1 mb-1">
            {{ mensagemCurta(notificacao?.mensagem) }}
          </div>

          <div class="text-caption text-grey-darken-1">
            Enviado em {{ formatarData(notificacao?.dataEnvio) }}
          </div>
        </v-card>
      </div>
    </div>
  </v-container>
</template>

<script>
import axios from "axios";
import UsuarioMenu from "@/views/UsuarioMenu.vue";

export default {
  name: "HistoricoNotificacoes",
  components: { UsuarioMenu },
  data() {
    return {
      notificacoes: [],
      nomeUsuario: localStorage.getItem("usuarioNome") || "Usuário",
      breadcrumbs: [
        { text: "Início", to: "/home", icon: "mdi-home" },
        { text: "Notificações" },
      ],
    };
  },
  methods: {
    formatarData(data) {
      const date = new Date(data);
      if (isNaN(date)) return "Data inválida";
      return (
        date.toLocaleDateString("pt-BR") +
        " às " +
        date.toLocaleTimeString("pt-BR")
      );
    },

    // Fallback simples (usado se DOMParser falhar)
    limparHtml(html) {
      if (!html) return "";
      const div = document.createElement("div");
      div.innerHTML = String(html);
      return (div.textContent || div.innerText || "").replace(/\s+/g, " ").trim();
    },

    // Extrai a frase principal "Identificamos ..." do HTML completo
    mensagemCurta(html) {
      if (!html) return "";

      try {
        const parser = new DOMParser();
        const doc = parser.parseFromString(String(html), "text/html");

        // Textos dos parágrafos
        const paragrafos = Array.from(doc.querySelectorAll("p"))
          .map(p => (p.textContent || "").replace(/\s+/g, " ").trim())
          .filter(Boolean);

        // 1) Preferir parágrafo que começa com "Identificamos"
        let preferida =
          paragrafos.find(t => /^Identificamos/i.test(t)) ||
          paragrafos.find(t => /Identificamos/i.test(t));

        // 2) Se não achou, tentar regex na soma do body
        if (!preferida) {
          const corpo = (doc.body?.textContent || "")
            .replace(/\s+/g, " ")
            .trim();
          const m = corpo.match(/Identificamos[^.!?]*[.!?]/i);
          if (m && m[0]) preferida = m[0].trim();
        }

        // 3) Se ainda não achou, usar o primeiro parágrafo/frase
        if (!preferida) {
          if (paragrafos.length > 0) {
            const primeiraFrase = paragrafos[0].split(/(?<=[.!?])\s+/)[0];
            return (primeiraFrase || paragrafos[0]).trim();
          }
          // fallback final: texto limpo do body
          const txt = (doc.body?.textContent || "").replace(/\s+/g, " ").trim();
          if (txt) {
            const primeiraFrase = txt.split(/(?<=[.!?])\s+/)[0];
            return (primeiraFrase || txt).trim();
          }
        }

        return (preferida || "").trim();
      } catch (e) {
        // Qualquer problema no parser -> fallback simples
        return this.limparHtml(html).split(/(?<=[.!?])\s+/)[0] || "";
      }
    },

    async carregarNotificacoes() {
      const cpf = localStorage.getItem("usuarioCpf");
      if (!cpf) {
        alert("Usuário não identificado. Faça login novamente.");
        return;
      }
      try {
        const { data } = await axios.get(
          `http://localhost:5054/api/notificacoes/usuario/${cpf}/historico`
        );
        this.notificacoes = Array.isArray(data) ? data : [];
      } catch (error) {
        console.error("Erro ao buscar notificações:", error);
        alert("Erro ao carregar notificações.");
      }
    },

    navegar(destino) {
      if (destino) this.$router.push(destino);
    },
  },
  mounted() {
    this.carregarNotificacoes();
  },
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

.breadcrumb-home-img {
  margin-top: -5px;
}

.outlined-card {
  border: 1px solid #CFCFCF;
  background-color: #fdfcff;
  padding: 8px;
  max-width: 1300px;
  margin-left: 20px;
  transition: border-color 0.5s;
  min-height: 140px;
}

.mensagem-nenhuma-notificacao {
  text-align: center;
  color: #6b7280;
}
</style>
