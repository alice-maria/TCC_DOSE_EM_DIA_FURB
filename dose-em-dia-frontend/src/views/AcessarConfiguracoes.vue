<template>
  <v-container class="pa-4">
    <!-- Cabeçalho -->
    <div class="header">
      <h1 class="titulo">Dose em dia</h1>
      <div class="usuario">
        <img src="@/imagens/icone-user-orange.png" alt="Ícone de usuário" class="icone-usuario">
        <span class="saudacao">Olá, {{ nomeUsuario }}!</span>
      </div>
    </div>

    <v-breadcrumbs class="meus-breadcrumbs" :items="breadcrumbs">
      <template v-slot:item="{ item }">
        <span :class="[
          'breadcrumb-link',
          { 'breadcrumb-laranja': !item.to } // se não tem .to, é o item atual
        ]" @click="item.to && navegar(item.to)" style="cursor: pointer;">
          <v-icon left small v-if="item.icon">{{ item.icon }}</v-icon>
          {{ item.text }}
        </span>
      </template>
    </v-breadcrumbs>

    <v-card class="card-ajustado">
      <v-card-text>
        <!-- Notificações -->
        <v-list-item class="hoverable">
          <v-list-item-content>
            <v-list-item-title class="secao-titulo">Notificações</v-list-item-title>
            <v-list-item-subtitle class="secao-descricao">
              Você receberá comunicações por e-mail sobre suas vacinas.
            </v-list-item-subtitle>
          </v-list-item-content>

          <template #append>
            <v-switch v-model="notificacoesAtivas" color="orange" />
          </template>
        </v-list-item>

        <v-divider class="separador"></v-divider>


        <!-- Informações Cadastrais -->
        <v-list-item @click="navegar('informacoes-cadastrais')" class="hoverable">
          <v-list-item-title class="secao-titulo">Informações cadastrais</v-list-item-title>
          <v-list-item-subtitle>Informações sobre seu perfil e dados cadastrais.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Segurança -->
        <v-list-item @click="navegar('redefinir-senha')" class="hoverable">
          <v-list-item-title class="secao-titulo">Segurança</v-list-item-title>
          <v-list-item-subtitle>Altere aqui a sua senha.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Política de Privacidade -->
        <v-list-item @click="navegar('politica-privacidade')" class="hoverable">
          <v-list-item-title class="secao-titulo">Política de Privacidade</v-list-item-title>
          <v-list-item-subtitle>Regras de privacidade de dados pessoais.</v-list-item-subtitle>
          <v-icon>mdi-chevron-right</v-icon>
        </v-list-item>
        <v-divider class="separador"></v-divider>

        <!-- Sair -->
        <v-list-item @click="dialogSair = true" class="hoverable">
          <v-list-item-title class="secao-titulo">Sair</v-list-item-title>
          <v-list-item-subtitle>Desconecte da conta em que você está.</v-list-item-subtitle>
          <v-icon>mdi-logout</v-icon>
        </v-list-item>

        <!-- Diálogo de confirmação -->
        <v-dialog v-model="dialogSair" max-width="320">
          <v-card class="popup-sair">
            <v-card-text class="popup-sair__texto">
              Você confirma a saída da conta?
            </v-card-text>

            <v-card-actions class="popup-sair__botoes">
              <v-btn class="popup-sair__cancelar" @click="dialogSair = false">Cancelar</v-btn>
              <v-btn class="popup-sair__confirmar" @click="confirmarSaida">Confirmar</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>

      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
export default {
  name: "AcessarConfiguracoes",
  data() {
    return {
      nomeUsuario: "",
      dialogSair: false,
      notificacoesAtivas: true,
      breadcrumbs: [
        { text: "Serviços e Informações", to: "/home", icon: "mdi-home" },
        { text: "Configurações" } // este é o item atual, sem link
      ],
    };
  },
  mounted() {
    this.nomeUsuario = localStorage.getItem("usuarioNome") || "Usuário";
  },
  methods: {
    confirmarSaida() {
      localStorage.removeItem("token");
      localStorage.removeItem("usuarioNome");
      this.$router.push("/");
    },
    navegar(rota) {
      if (rota) this.$router.push(rota);
    },
  }
};
</script>

<style scoped>
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 3.0rem;
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

.separador {
  margin: 0.75rem 0;
}

.secao-titulo {
  font-size: 1.5rem;
  font-weight: 600;
  color: #f97316;
}

.secao-descricao {
  font-size: 0.9rem;
  color: #555;
}

.hoverable {
  cursor: pointer;
  transition: background-color 0.2s ease;
  min-height: 12vh;
}

.hoverable:hover {
  background-color: #f9f9f9;
}

.card-ajustado {
  width: 75vw;
  min-height: 77vh;
  margin: 20px auto;
  box-sizing: border-box;
}

.popup-sair {
  background-color: #f97316;
  border-radius: 25px !important;
  /* mais arredondado */
  padding: 32px 24px;
  color: white;
  text-align: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.popup-sair__texto {
  font-weight: bold;
  font-size: 1.3rem !important;
  margin-bottom: 24px;
  line-height: 1.5;
}

.popup-sair__botoes {
  display: flex;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
}

.popup-sair__cancelar,
.popup-sair__confirmar {
  border-radius: 999px;
  text-transform: none;
  font-weight: 600;
  padding: 8px 20px;
  font-size: 0.95rem;
  min-width: 100px;
}

.popup-sair__cancelar {
  background-color: #fb923c;
  color: white;
}

.popup-sair__confirmar {
  background-color: white;
  color: #f97316;
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