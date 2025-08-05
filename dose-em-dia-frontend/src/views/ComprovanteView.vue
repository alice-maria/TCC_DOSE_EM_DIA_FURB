<template>
  <v-container fluid class="pa-0">
    <div class="pagina-comprovante">
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
            <img v-if="item.icon === 'mdi-home'" src="@/assets/icons/home.svg" alt="" class="breadcrumb-home-img" />
            {{ item.text }}
          </span>
        </template>
      </v-breadcrumbs>

      <div class="aviso-comprovante">
        <img src="@/assets/icons/aviso.svg" alt="Ícone de aviso" class="me-3" style="width: 24px; height: 24px;" />
        No certificado de vacinação você terá acesso apenas às vacinas que já foram aplicadas.
      </div>

      <!-- Conteúdo -->
      <div class="conteudo px-6 py-10">
        <h2 class="fw-bold text-orange">Exportar Comprovante</h2>
        <p class="text-muted mb-6">Baixe seu comprovante de vacinação em PDF</p>

        <!-- Frase de incentivo -->
        <p class="frase-reforco">
          Manter sua vacinação em dia é um ato de cuidado com você e com todos ao seu redor.
        </p>

        <!-- Miniatura do comprovante -->
        <div class="preview-comprovante">
          <img src="@/assets/exemplo-comprovante.png" alt="Exemplo de comprovante de vacinação" />
          <p class="legenda-preview">Exemplo de como seu comprovante será gerado em PDF.</p>
        </div>

        <v-btn class="botao-material" color="#f97316" @click="baixarComprovante" :loading="carregando"
          :disabled="carregando">Baixar Comprovante</v-btn>

        <div class="tooltip-container">
          <v-tooltip location="top" open-delay="100" close-delay="100">
            <template v-slot:activator="{ props }">
              <span class="tooltip-legal" v-bind="props">
                * Comprovante informativo
              </span>
            </template>
            <div class="tooltip-detalhe">
              Este comprovante possui caráter informativo e foi gerado automaticamente com base nos registros internos
              do
              sistema <strong>Dose em Dia</strong>. Não possui validade jurídica e não substitui certificados oficiais
              emitidos por autoridades públicas de saúde.
            </div>
          </v-tooltip>
        </div>
        <!-- Overlay de carregamento -->
        <v-overlay v-model="carregando" :persistent="true" class="d-flex align-center justify-center">
          <div class="loading-card">
            <v-progress-circular indeterminate size="36" width="4"></v-progress-circular>
            <div class="mt-3 text-center">
              <div class="fw-600">Gerando seu PDF…</div>
              <div class="text-caption text-medium-emphasis" v-if="progresso > 0 && progresso < 100">
                {{ progresso }}%
              </div>
              <div class="text-caption text-medium-emphasis" v-else>
                Isso pode levar alguns segundos.
              </div>
            </div>
          </div>
        </v-overlay>

      </div>
    </div>
  </v-container>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ComprovanteView',
  data() {
    return {
      nomeUsuario: '',
      mostrarDialogo: false,
      carregando: false,
      progresso: 0,
      breadcrumbs: [
        { text: 'Serviços e Informações', to: '/home', icon: 'mdi-home' },
        { text: 'Exportar comprovante' }
      ]
    };
  },
  mounted() {
    this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
  },
  methods: {
    async baixarComprovante() {
      const usuarioId = localStorage.getItem("usuarioId");
      const usuarioCPF = localStorage.getItem("usuarioCPF"); // você deve armazenar isso no login
      if (!usuarioId || !usuarioCPF) {
        alert("Usuário não identificado. Faça login novamente.");
        return;
      }
      this.carregando = true;
      this.progresso = 0;
      try {
        const response = await axios.get(`http://localhost:5054/api/comprovante/${usuarioId}/gerarComprovante`, {
          responseType: 'blob',
          onDownloadProgress: (e) => {
            if (e.total) {
              const pct = Math.round((e.loaded / e.total) * 100);
              this.progresso = pct;
            }
          }
        });

        // Gera nome do arquivo localmente
        const cpfLimpo = usuarioCPF.replace(/\D/g, '');
        const dataAgora = new Date();
        const dataFormatada = dataAgora.toISOString().slice(0, 10).replace(/-/g, ''); // yyyyMMdd

        const hora = String(dataAgora.getHours()).padStart(2, '0');
        const minutos = String(dataAgora.getMinutes()).padStart(2, '0');
        const segundos = String(dataAgora.getSeconds()).padStart(2, '0');
        const horaFormatada = `${hora}${minutos}${segundos}`;

        const nomeArquivo = `comprovante-vacinacao_${cpfLimpo}_${dataFormatada}_${horaFormatada}.pdf`;

        const blob = new Blob([response.data], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', nomeArquivo);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error("Erro ao gerar comprovante:", error);
        alert("Erro ao gerar comprovante. Tente novamente mais tarde.");
      } finally {
        this.carregando = false;
        this.progresso = 0;
      }
    },
    navegar(rota) {
      this.$router.push(rota);
    }
  }
};
</script>

<style scoped>
.pagina-comprovante {
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

.aviso-comprovante {
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
  font-size: 0.95rem;
}

.text-orange {
  color: #f97316;
}

.conteudo {
  margin: 0 auto;
  max-width: 800px;
  text-align: center;
  padding: 2rem 1rem;
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

.botao-material {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  text-transform: none;
  font-weight: 500;
  font-size: 0.95rem;
  padding: 10px 24px;
  height: 40px;
  line-height: 1;
  letter-spacing: 0.25px;
  border-radius: 999px;
  box-shadow: none;
}

.frase-reforco {
  font-size: 1.05rem;
  font-weight: 500;
  color: #444;
  margin: 1.5rem auto 2rem;
  text-align: center;
  max-width: 600px;
}

.preview-comprovante {
  text-align: center;
  margin-bottom: 2rem;
}

.preview-comprovante img {
  aspect-ratio: 16 / 7;
  width: 100%;
  max-width: 700px;
  height: 300px;
  object-fit: cover;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  margin-bottom: 0.5rem;
}

.legenda-preview {
  font-size: 0.85rem;
  color: #666;
}

.tooltip-container {
  display: flex;
  justify-content: center;
  margin-top: 1.5rem;
}

.tooltip-legal {
  font-size: 0.8rem;
  color: #9ca3af;
  font-style: italic;
  cursor: pointer;
}

.tooltip-detalhe {
  max-width: 320px;
  font-size: 0.85rem;
  line-height: 1.3;
  color: #fff;
}

.loading-card {
  background: rgb(255, 119, 0);
  color: black;
  padding: 16px 20px;
  border-radius: 12px;
  min-width: 220px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.fw-600 {
  font-weight: 600;
}
</style>